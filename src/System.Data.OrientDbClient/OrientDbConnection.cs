using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbConnection : DbConnection
    {
        private string connectionString;
        private ConnectionState _connectionState = ConnectionState.Closed;
        public readonly OrientDbHandle OrientDbHandle = new OrientDbHandle();

        public OrientDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool AttemptCreate => OrientDbHandle.AttemptCreate;

        public override string ConnectionString
        {
            get { return this.connectionString; }
            set
            {
                this.connectionString = value;

                var connectionParameters = new NameValueCollection();
                foreach (var entry in from entry in connectionString.Split(';')
                                      let parts = entry.Split('=')
                                      where parts.Length == 2
                                      select new { key = parts[0], value = parts[1] })
                {
                    connectionParameters[entry.key] = entry.value;
                }
                OrientDbHandle.Server = connectionParameters["Server"];
                OrientDbHandle.Port = int.Parse(connectionParameters["Port"] ?? "2480");
                OrientDbHandle.Database = connectionParameters["Database"];
                OrientDbHandle.User = connectionParameters["User"];
                OrientDbHandle.Password = connectionParameters["Password"];
                OrientDbHandle.UseSsl = Convert.ToBoolean(connectionParameters["UseSsl"] ?? "False");
                OrientDbHandle.AttemptCreate = Convert.ToBoolean(connectionParameters["AttemptCreate"] ?? "False");
            }
        }

        public override string Database => OrientDbHandle.Database;

        public override string DataSource => OrientDbHandle.Server;

        public string Password => OrientDbHandle.Password;
        public int Port => OrientDbHandle.Port;

        public override string ServerVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ConnectionState State => _connectionState;

        public string User => OrientDbHandle.User;
        public bool UseSsl => OrientDbHandle.UseSsl;

        public override void ChangeDatabase(string databaseName)
        {
            OrientDbHandle.Database = databaseName;
        }

        public override void Close()
        {
            if (_connectionState != ConnectionState.Closed)
            {
                NotifyAndUpdateState(ConnectionState.Closed);
            }
        }

        public override void Open()
        {
            NotifyAndUpdateState(ConnectionState.Connecting);
            try
            {
                OrientDbHandle.Request("GET", "connect");
            }
            catch (OrientDbException)
            {
                if (AttemptCreate)
                {
                    OrientDbHandle.Request("POST", "database", "plocal/graph");
                }
                else
                {
                    throw;
                }
            }

            NotifyAndUpdateState(ConnectionState.Open);
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            NotifyAndUpdateState(ConnectionState.Connecting);
            try
            {
                await OrientDbHandle.RequestAsync("GET", "connect");
            }
            catch (OrientDbException)
            {
                if (AttemptCreate)
                {
                    await OrientDbHandle.RequestAsync("POST", "database", "plocal/graph");
                }
            }

            NotifyAndUpdateState(ConnectionState.Open);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new OrientDbTransaction(this);
        }

        public new OrientDbCommand CreateCommand()
        {
            return new OrientDbCommand() { Connection = this };
        }

        protected override DbCommand CreateDbCommand()
        {
            return new OrientDbCommand() { Connection = this };
        }

        private void NotifyAndUpdateState(ConnectionState state)
        {
            var oldState = _connectionState;
            _connectionState = state;
            OnStateChange(new StateChangeEventArgs(oldState, state));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                base.Dispose(disposing);
            }
        }
    }
}
