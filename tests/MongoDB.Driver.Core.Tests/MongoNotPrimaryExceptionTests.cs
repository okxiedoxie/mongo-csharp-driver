﻿/* Copyright 2013-present MongoDB Inc.
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

using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using Xunit;

namespace MongoDB.Driver
{
    public class MongoNotPrimaryExceptionTests
    {
        private readonly ConnectionId _connectionId = new ConnectionId(new ServerId(new ClusterId(1), new DnsEndPoint("localhost", 27017)), 2).WithServerValue(3);
        private readonly BsonDocument _command = new BsonDocument("command", 1);
        private readonly BsonDocument _serverResult = new BsonDocument("result", 1);

        [Fact]
        public void constructor_should_initialize_subject()
        {
            var subject = new MongoNotPrimaryException(_connectionId, _command, _serverResult);

            subject.ConnectionId.Should().BeSameAs(_connectionId);
            subject.InnerException.Should().BeNull();
            subject.Message.Should().Be("Server returned not primary error.");
            subject.Command.Should().BeSameAs(_command);
            subject.Result.Should().Be(_serverResult);
        }

        [Fact]
        public void Serialization_should_work()
        {
            var subject = new MongoNotPrimaryException(_connectionId, _command, _serverResult);

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, subject);
                stream.Position = 0;
                var rehydrated = (MongoNotPrimaryException)formatter.Deserialize(stream);

                rehydrated.ConnectionId.Should().Be(subject.ConnectionId);
                rehydrated.InnerException.Should().BeNull();
                rehydrated.Message.Should().Be(subject.Message);
                rehydrated.Command.Should().Be(subject.Command);
                rehydrated.Result.Should().Be(subject.Result);
            }
        }
    }
}
