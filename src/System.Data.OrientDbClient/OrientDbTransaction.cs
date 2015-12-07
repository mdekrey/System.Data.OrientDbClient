using System.Data.Common;

namespace System.Data.OrientDbClient
{
    public class OrientDbTransaction : DbTransaction
    {
        private OrientDbConnection orientDbConnection;

        public OrientDbTransaction(OrientDbConnection orientDbConnection)
        {
            this.orientDbConnection = orientDbConnection;
            throw new NotSupportedException(OrientDbStrings.TransactionsNotSupported);
        }

        public override IsolationLevel IsolationLevel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override DbConnection DbConnection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}