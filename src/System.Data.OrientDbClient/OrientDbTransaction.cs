using System.Data.Common;

namespace System.Data.OrientDbClient
{
    public class OrientDbTransaction : DbTransaction
    {
        private OrientDbConnection orientDbConnection;

        public OrientDbTransaction(OrientDbConnection orientDbConnection)
        {
            this.orientDbConnection = orientDbConnection;
        }

        public override IsolationLevel IsolationLevel => IsolationLevel.Chaos;

        protected override DbConnection DbConnection => orientDbConnection;

        public override void Commit()
        {
        }

        public override void Rollback()
        {
            throw new NotSupportedException();
        }
    }
}