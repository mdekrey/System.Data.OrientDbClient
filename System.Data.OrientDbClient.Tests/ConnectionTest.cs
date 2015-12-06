﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.Data.OrientDbClient
{
    /// <summary>
    /// This test will verify a legitimate connection to a local OrientDB. This test may not pass for you if the local DB does not exist.
    /// </summary>
    public class ConnectionTest : IDisposable
    {
        private readonly OrientDbConnection connection;

        public ConnectionTest()
        {
            connection = new OrientDbConnection("Server=127.0.0.1;Database=UserTest;User=root;Password=root;AttemptCreate=true");
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        [Fact]
        public void DefaultStateIsClosed()
        {
            // Arrange

            // Act

            // Assert
            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [Fact]
        public void OpenConnectionTest()
        {
            // Arrange

            // Act
            connection.Open();

            // Assert
            Assert.Equal(ConnectionState.Open, connection.State);
        }

        [Fact]
        public void StateChangeNotifiedTest()
        {
            // Arrange
            ISet<ConnectionState> states = new HashSet<ConnectionState>();
            connection.StateChange += (subject, args) => states.Add(args.CurrentState);

            // Act
            connection.Open();

            // Assert
            Assert.True(states.Contains(ConnectionState.Connecting));
            Assert.True(states.Contains(ConnectionState.Open));
            Assert.Equal(2, states.Count);
        }

        [Fact]
        public async Task StateChangeNotifiedTestAsync()
        {
            // Arrange
            ISet<ConnectionState> states = new HashSet<ConnectionState>();
            connection.StateChange += (subject, args) => states.Add(args.CurrentState);

            // Act
            await connection.OpenAsync();

            // Assert
            Assert.True(states.Contains(ConnectionState.Connecting));
            Assert.True(states.Contains(ConnectionState.Open));
            Assert.Equal(2, states.Count);
        }

        [Fact]
        public void StateChangeNotifiedCloseTest()
        {
            // Arrange
            connection.Open();
            ISet<ConnectionState> states = new HashSet<ConnectionState>();
            connection.StateChange += (subject, args) => states.Add(args.CurrentState);

            // Act
            connection.Close();

            // Assert
            Assert.True(states.Contains(ConnectionState.Closed));
            Assert.Equal(1, states.Count);
        }

    }
}