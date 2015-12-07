using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbCommand : DbCommand
    {
        OrientDbConnection _connection;
        OrientDbTransaction m_txn;
        string m_sCmdText;
        UpdateRowSource m_updatedRowSource = UpdateRowSource.None;
        OrientDbParameterCollection m_parameters = new OrientDbParameterCollection();
        private int _commandTimeout;
        private OrientDbTransaction _transaction;

        // Implement the default constructor here.
        public OrientDbCommand()
        {
        }

        // Implement other constructors here.
        public OrientDbCommand(string cmdText)
        {
            m_sCmdText = cmdText;
        }

        public OrientDbCommand(string cmdText, OrientDbConnection connection)
        {
            m_sCmdText = cmdText;
            _connection = connection;
        }

        public OrientDbCommand(string cmdText, OrientDbConnection connection, OrientDbTransaction txn)
        {
            m_sCmdText = cmdText;
            _connection = connection;
            m_txn = txn;
        }

        public override string CommandText
        {
            get { return m_sCmdText; }
            set { m_sCmdText = value; }
        }

        public override int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Text; }
            set { if (value != CommandType.Text) throw new NotSupportedException(); }
        }

        public new OrientDbParameterCollection Parameters
        {
            get { return m_parameters; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return m_parameters;
            }
        }

        new public OrientDbTransaction Transaction
        {
            get
            {
                // if the transaction object has been zombied, just return null
                if ((null != _transaction) && (null == _transaction.Connection))
                {
                    _transaction = null;
                }
                return _transaction;
            }
            set
            {
                // OrientDb doesn't support long-running connections... this is kinda a joke
                _transaction = value;
            }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                return _transaction;
            }

            set
            {
                _transaction = (OrientDbTransaction)value;
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return m_updatedRowSource; }
            set { m_updatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }

            set { _connection = (OrientDbConnection)value; }
        }

        public override bool DesignTimeVisible { get; set; }

        /****
         * IMPLEMENT THE REQUIRED METHODS.
         ****/
        public override void Cancel()
        {
            // The sample does not support canceling a command
            // once it has been initiated.
            throw new NotSupportedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            return new OrientDbParameter();
        }
        
        public override void Prepare()
        {
            // The sample Prepare is a no-op.
        }
        
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            EnforceOpenConnection();
            return ResultTransforms.ToReaderResult(InternalExecute());
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            EnforceOpenConnection();
            return ResultTransforms.ToReaderResult(await InternalExecuteAsync());
        }

        public override int ExecuteNonQuery()
        {
            EnforceOpenConnection();
            return ResultTransforms.ToNonQueryResult(InternalExecute());
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            EnforceOpenConnection();
            return ResultTransforms.ToNonQueryResult(await InternalExecuteAsync());
        }

        public override object ExecuteScalar()
        {
            EnforceOpenConnection();
            return ResultTransforms.ToScalarResult(InternalExecute());
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            EnforceOpenConnection();
            return ResultTransforms.ToScalarResult(await InternalExecuteAsync());
        }


        private void EnforceOpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Connection must valid and open");
        }

        private Newtonsoft.Json.Linq.JToken InternalExecute() =>
            _connection.OrientDbHandle.Request("POST", "batch", body: RequestBody());

        private Task<Newtonsoft.Json.Linq.JToken> InternalExecuteAsync() =>
            _connection.OrientDbHandle.RequestAsync("POST", "batch", body: RequestBody());

        private object RequestBody() => new
        {
            transaction = Transaction,
            operations = new[]
                {
                    new
                    {
                        type = "script",
                        language = "sql",
                        script = ActualSql()
                    }
                }
        };

        // (?<=^([^""']|""[^""]*""|'[^']*')*) = Must be preceded by either no quotes or a complete double-quote string or a complete single-quote string (as many times as we want)
        // (?<!\\)(\\\\)* = Must be preceded by an even number (including 0) of backslashes and not one more
        // (?<!\\)(\\\\)*\\ = Must be preceded by an odd number of backslashes and not one more
        internal static readonly Regex ParameterReplace = new Regex(@"(?<=^([^""']|""([^""]|(?<!\\)(\\\\)*\\"")*(?<!\\)(\\\\)*""|'([^']|(?<!\\)(\\\\)*\\')*(?<!\\)(\\\\)*')*)(?<parameter>\$[a-zA-Z0-9_]+)");
        internal string ActualSql()
        {
            if (Parameters.Count == 0)
            {
                return CommandText;
            }

            return ParameterReplace.Replace(CommandText, (Match match) =>
            {
                var parameterName = match.Groups["parameter"].Value;
                if (!Parameters.Contains(parameterName))
                {
                    return match.Groups[0].Value;
                }
                return new OrientDbRawValues().Escape(Parameters[parameterName].Value);
            });
        }
    }
}