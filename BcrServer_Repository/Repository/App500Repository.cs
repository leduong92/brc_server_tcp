using Npgsql;

namespace BcrServer_Repository
{
    public class App500Repository : DataProvider, IApp500Repository
    {
        #region [Constructor]
        NpgsqlTransaction transaction;
        public App500Repository(NpgsqlTransaction _transaction) : base(_transaction)
        {
            this.transaction = _transaction;
        }
        #endregion


    }
}
