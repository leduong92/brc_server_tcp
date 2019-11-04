using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Repository
{
    public class CommonRepository : DataProvider, ICommonRepository 
    {
        NpgsqlTransaction transaction;
        public CommonRepository(NpgsqlTransaction _transaction) : base(_transaction)
        {
            this.transaction = _transaction;
        }

        public DataTable GetDataByUserId(string userId)
        {
            string query = string.Format("select last_name_en, first_name_en, author, location_cd from tm_user_packing where ( user_cd = '{0}' or user_cd = substr('{0}', 1, 3)|| substr('{0}', 5, 4))", userId);

            DataTable dt = ExcuteQuery(query);

            return dt;
        }

        public int Insert()
        {
            string query = "INSERT INTO tm_user_packing(location_cd, user_cd, first_name, last_name, author, entry_date, first_name_en, last_name_en) VALUES ('OS1', '12345678', 'TRANSACTION', 'TRANSACTION', 'A', '20181107', 'TRANSACTION', 'TRANSACTION');";
            int ret;

            ret = ExcuteNonQuery(query);

            return ret;
        }

        public int Insert2()
        {
            string query = "INSERT INTO tm_user_packing(location_cd, user_cd, first_name, last_name, author, entry_date, first_name_en, last_name_en) VALUES ('OS1', '12345679', 'TRANSACTION', 'TRANSACTION', 'A', '20181107', 'TRANSACTION', 'TRANSACTION');";
            int ret;

            ret = ExcuteNonQuery(query);

            return ret;
        }
    }
}
