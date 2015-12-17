using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbBatchCommand : DbCommand
    {
        OrientDbConnection _connection;
        UpdateRowSource m_updatedRowSource = UpdateRowSource.None;
        OrientDbParameterCollection m_parameters = new OrientDbParameterCollection();
        private int _commandTimeout;

        // Implement the default constructor here.
        public OrientDbBatchCommand()
        {
        }

        // Implement other constructors here.
        public OrientDbBatchCommand(string[] cmdsText)
        {
            this.CommandsText = new List<string>(cmdsText);
        }

        public OrientDbBatchCommand(string[] cmdsText, OrientDbConnection connection)
        {
            this.CommandsText = new List<string>(cmdsText);
            _connection = connection;
        }

        public OrientDbBatchCommand(string[] cmdsText, OrientDbConnection connection, bool isTransaction)
        {
            this.CommandsText = new List<string>(cmdsText);
            _connection = connection;
            this.IsTransaction = isTransaction;
        }

        public override string CommandText
        {
            get { throw new InvalidOperationException(OrientDbStrings.BatchCommandUsesCommandsText); }
            set { throw new InvalidOperationException(OrientDbStrings.BatchCommandUsesCommandsText); }
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

        protected override DbTransaction DbTransaction
        {
            get { throw new InvalidOperationException(OrientDbStrings.BatchCommandUsesIsTransaction); }
            set { throw new InvalidOperationException(OrientDbStrings.BatchCommandUsesIsTransaction); }
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

        public List<string> CommandsText { get; } = new List<string>();
        public bool IsTransaction { get; set; }

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
            _connection.OrientDbHandle.Request("POST", "batch", arguments: "sql", body: RequestBody());

        private Task<Newtonsoft.Json.Linq.JToken> InternalExecuteAsync() =>
            _connection.OrientDbHandle.RequestAsync("POST", "batch", arguments: "sql", body: RequestBody());

        private object RequestBody() => new
        {
            transaction = IsTransaction,
            operations = new[]
                {
                    new
                    {
                        type = "script",
                        language = "sql",
                        script = CommandsText.Select(ActualSql)
                    }
                }
        };

        // (?<=^([^""']|""[^""]*""|'[^']*')*) = Must be preceded by either no quotes or a complete double-quote string or a complete single-quote string (as many times as we want)
        // (?<!\\)(\\\\)* = Must be preceded by an even number (including 0) of backslashes and not one more
        // (?<!\\)(\\\\)*\\ = Must be preceded by an odd number of backslashes and not one more
        internal static readonly Regex ParameterReplace = new Regex(@"(?<=^([^""']|""([^""]|(?<!\\)(\\\\)*\\"")*(?<!\\)(\\\\)*""|'([^']|(?<!\\)(\\\\)*\\')*(?<!\\)(\\\\)*')*)(?<parameter>\:[a-zA-Z0-9_]+)");

        internal string ActualSql(string inCommand)
        {
            if (Parameters.Count == 0)
            {
                return inCommand;
            }
            var values = new OrientDbRawValues();

            return ParameterReplace.Replace(inCommand, (Match match) =>
            {
                var parameterName = match.Groups["parameter"].Value;
                if (!Parameters.Contains(parameterName))
                {
                    return match.Groups[0].Value;
                }
                return values.Escape(Parameters[parameterName].Value);
            });
        }
    }
}