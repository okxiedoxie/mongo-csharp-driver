﻿/* Copyright 2018-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Misc;
using Xunit;

namespace MongoDB.Driver.Core.Operations
{
    public class BulkDeleteOperationTests : OperationTestBase
    {
        [Theory]
        [ParameterAttributeData]
        public void Execute_with_collation_should_throw_when_collation_is_not_supported(
            [Values(false, true)] bool async)
        {
            var collation = new Collation("en_US");
            var requests = new List<DeleteRequest>
            {
                new DeleteRequest(new BsonDocument("x", 1)),
                new DeleteRequest(new BsonDocument("x", 1)) { Collation = collation }
            };
            var subject = new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings);

            var exception = Record.Exception(() => ExecuteOperation(subject, async));

            exception.Should().BeNull();
        }

        [Theory]
        [ParameterAttributeData]
        public void Execute_with_hint_should_throw_when_hint_is_not_supported(
            [Values(0, 1)] int w,
            [Values(false, true)] bool async)
        {
            var writeConcern = new WriteConcern(w);
            var requests = new List<DeleteRequest>
            {
                new DeleteRequest(new BsonDocument("x", 1))
                {
                    Hint = new BsonDocument("_id", 1)
                }
            };
            var subject = new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                WriteConcern = writeConcern
            };

            var exception = Record.Exception(() => ExecuteOperation(subject, async, useImplicitSession: true));

            if (!writeConcern.IsAcknowledged)
            {
                exception.Should().BeOfType<NotSupportedException>();
            }
#pragma warning disable CS0618 // Type or member is obsolete
            else if (Feature.HintForDeleteOperations.IsSupported(CoreTestConfiguration.MaxWireVersion))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                exception.Should().BeNull();
            }
            else
            {
                exception.Should().BeOfType<MongoCommandException>();
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Let_get_and_set_should_work(
            [Values(null, "{ name : 'name' }")] string let)
        {
            var requests = new List<DeleteRequest>
            {
                new DeleteRequest(new BsonDocument("x", 1))
                {
                    Hint = new BsonDocument("_id", 1)
                }
            };
            var subject = new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings);
            var value = let != null ? BsonDocument.Parse(let) : null;

            subject.Let = value;
            var result = subject.Let;

            result.Should().Be(value);
        }
    }
}
