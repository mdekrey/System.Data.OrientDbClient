using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.Data.OrientDbClient
{
    /// <summary>
    /// This test will verify a legitimate connection to a local OrientDB. This test may not pass for you if the local DB does not exist.
    /// </summary>
    public class ConnectionTest
    {

        [Fact]
        public void DefaultStateIsClosed()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=localhost.;Database=UserTest;User=root;Password=root;AttemptCreate=true"))
            {

                // Act

                // Assert
                Assert.Equal(ConnectionState.Closed, connection.State);
            }
        }

        [Fact]
        public void OpenConnectionTest()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=localhost.;Database=UserTest;User=root;Password=root;AttemptCreate=true"))
            {

                // Act
                connection.Open();

                // Assert
                Assert.Equal(ConnectionState.Open, connection.State);
            }
        }

        [Fact]
        public void StateChangeNotifiedTest()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=localhost.;Database=UserTest;User=root;Password=root;AttemptCreate=true"))
            {
                ISet<ConnectionState> states = new HashSet<ConnectionState>();
                connection.StateChange += (subject, args) => states.Add(args.CurrentState);

                // Act
                connection.Open();

                // Assert
                Assert.True(states.Contains(ConnectionState.Connecting));
                Assert.True(states.Contains(ConnectionState.Open));
                Assert.Equal(2, states.Count);
            }
        }

        [Fact]
        public async Task StateChangeNotifiedTestAsync()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=localhost.;Database=UserTest;User=root;Password=root;AttemptCreate=true"))
            {
                ISet<ConnectionState> states = new HashSet<ConnectionState>();
                connection.StateChange += (subject, args) => states.Add(args.CurrentState);

                // Act
                await connection.OpenAsync();

                // Assert
                Assert.True(states.Contains(ConnectionState.Connecting));
                Assert.True(states.Contains(ConnectionState.Open));
                Assert.Equal(2, states.Count);
            }
        }

        [Fact]
        public void StateChangeNotifiedCloseTest()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=localhost.;Database=UserTest;User=root;Password=root;AttemptCreate=true"))
            {
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

        [Fact]
        public void ErrorConnectingTest()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=127.0.0.1;Database=Fake;User=root;Password=root;AttemptCreate=false"))
            {

                // Act
                var exception = Assert.Throws<OrientDbException>(() => connection.Open());

                // Assert
                Assert.NotNull(exception);
                Assert.DoesNotContain(exception.Message, "jToken");
            }
        }


        [Fact]
        public async Task CreateAndDeleteTest()
        {
            // Arrange
            using (var connection = new OrientDbConnection("Server=127.0.0.1;Database=MyTemp_" + System.IO.Path.GetRandomFileName().Replace(".", "") + ";User=root;Password=root;AttemptCreate=True"))
            {

                // Act
                await connection.OpenAsync();
                await Task.Delay(1000);
                var result = await connection.OrientDbHandle.RequestAsync("DELETE", "database");

                // Assert
                Assert.IsType<Newtonsoft.Json.Linq.JValue>(result);
                Assert.Null((result as Newtonsoft.Json.Linq.JValue).Value);
            }
        }
    }
}
