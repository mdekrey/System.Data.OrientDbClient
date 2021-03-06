﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.Data.OrientDbClient
{
    /// <summary>
    /// This test will verify a legitimate connection to a local OrientDB. This test may not pass for you if the local DB does not exist.
    /// 
    /// This also relies on an existing "GratefulDeadConcerts" database; the demo db. TODO - build our own database and drop it.
    /// </summary>
    public class CommandTest
    {
        private readonly OrientDbConnection connection;

        public CommandTest()
        {
            connection = new OrientDbConnection("Server=127.0.0.1;Database=GratefulDeadConcerts;User=root;Password=root;");
            connection.Open();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        [Fact]
        public void NonQueryTest()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM V";

            // Act
            var result = cmd.ExecuteNonQuery();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task NonQueryTestAsync()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM V";

            // Act
            var result = await cmd.ExecuteNonQueryAsync();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void ScalarTest()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM V";

            // Act
            var result = cmd.ExecuteScalar();

            // Assert
            Assert.IsAssignableFrom<long>(result);
            Assert.True((long)result > 800);
        }

        [Fact]
        public async Task ScalarTestAsync()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM V";

            // Act
            var result = await cmd.ExecuteScalarAsync();

            // Assert
            Assert.IsAssignableFrom<long>(result);
            Assert.True((long)result > 800);
        }

        [Fact]
        public void ScalarRidTest()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "CREATE VERTEX V";

            // Act
            var result = cmd.ExecuteScalar();

            // Assert
            Assert.IsAssignableFrom<string>(result);

            cmd.CommandText = "DELETE VERTEX " + result;
            var deleteResult = cmd.ExecuteNonQuery();
            Assert.Equal(1, deleteResult);
        }

        [Fact]
        public void ReaderTest()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT FROM V";

            // Act
            var reader = cmd.ExecuteReader();

            // Assert
            Assert.True(reader.Read());
            Assert.True(reader.GetOrdinal("@rid") >= 0);
            Assert.True(reader.FieldCount >= 4);
        }

        [Fact]
        public async Task ReaderTestAsync()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT FROM V";

            // Act
            var reader = await cmd.ExecuteReaderAsync();

            // Assert
            Assert.True(reader.Read());
            Assert.True(reader.GetOrdinal("@rid") >= 0);
            Assert.True(reader.FieldCount >= 4);
        }

        // TODO - test parameters

        [Fact]
        public void ParameterTest()
        {
            // Arrange
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT \":temp\" as s, out(:temp) FROM V WHERE @rid='#9:1'";
            cmd.Parameters.Add(":temp", "written_by");

            // Act
            var reader = cmd.ExecuteReader();

            // Assert
            Assert.True(reader.Read());
            Assert.Equal(":temp", reader["s"]);
            Assert.IsType<JArray>(reader["out"]);
            Assert.Equal(1, ((JArray)reader["out"]).Count);
        }

        [Fact]
        public void BatchScalarRidTest()
        {
            // Arrange
            var cmd = connection.CreateBatchCommand();
            cmd.CommandsText.Add("LET $v1 = CREATE VERTEX V");
            cmd.CommandsText.Add("LET $v2 = CREATE VERTEX V");
            cmd.CommandsText.Add("return { 'v1': $v1.@rid, 'v2': $v2 }");

            // Act
            using (var reader = cmd.ExecuteReader())
            {
                // Assert
                Assert.True(reader.Read());
                Assert.IsType<string>(reader["v1"]);
                Assert.IsType<string>(reader["v2"]);
                Assert.False(reader.Read());
            }
        }

    }
}
