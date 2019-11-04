using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Repository
{
    public class DataProvider
    {
        #region [Private Variables]
        //private static DataProvider instance;
        //private static string connectionString = "";
        #endregion

        #region [Public Properties]
        //public static DataProvider Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new DataProvider();
        //        }
        //        return instance;
        //    }

        //    private set
        //    {
        //        instance = value;
        //    }
        //}

        public NpgsqlTransaction Transaction { get; private set; }
        public NpgsqlConnection Connection
        {
            get { return Transaction.Connection; }
        }
        #endregion

        #region [Public Method]
        public DataProvider(NpgsqlTransaction transaction)
        {
            /*
             *  //20181107
            string dbName = Properties.Settings.Default.DB_NAME;

            if (dbName.ToUpper() == "LIVE")
                connectionString = "Server = 10.203.192.193; Port = 5432; Database = qimlive; User Id = qimlive; Password = qimlive;";
            if (dbName.ToUpper() == "TRAINING")
                connectionString = "Server = 10.203.192.193; Port = 5432; Database = qim_training; User Id = qim_training; Password = qim_training;";
            */

            this.Transaction = transaction;

        }
        public DataTable ExcuteQuery(string query, Dictionary<string, object> parameters = null, 
                                     CommandType commandtype = CommandType.Text)
        {
            #region Comment out
            //using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            //{
            //    try
            //    {
            //        conn.Open();
            //        NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
            //        if (parameters != null && parameters.Count > 0)
            //        {
            //            foreach (var param in parameters)
            //                cmd.Parameters.Add(param.Key, param.Value);
            //        }
            //        cmd.CommandType = commandtype;
            //        cmd.CommandTimeout = 3000;
            //        NpgsqlDataAdapter sda = new NpgsqlDataAdapter(cmd);
            //        DataTable dt = new DataTable();
            //        sda.Fill(dt);
            //        return dt;
            //    }
            //    catch(Exception ex)
            //    {
            //        return new DataTable();
            //    }
            //    finally
            //    {
            //        if (conn.State != ConnectionState.Closed)
            //            conn.Close();
            //    }
            //}
            #endregion

            #region Use Transaction
            try
            {
                NpgsqlCommand cmd;

                if (Transaction == null)
                    cmd = new NpgsqlCommand(query, Connection);
                else
                    cmd = new NpgsqlCommand(query, Connection, Transaction);

                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.Add(param.Key, param.Value);
                }

                cmd.CommandType = commandtype;
                cmd.CommandTimeout = 3000;
                NpgsqlDataAdapter sda = new NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(query, ex.Message, sw);
                }

                throw new Exception(ex.Message);
                //return new DataTable();
            }
            #endregion
        }
        public int ExcuteNonQuery(string query, Dictionary<string, object> parameters = null,
                                     CommandType commandtype = CommandType.Text)
        {
            #region Comment Out
            //using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            //{
            //    try
            //    {
            //        conn.Open();
            //        NpgsqlCommand cmd = new NpgsqlCommand(query, conn, transaction);
            //        if (parameters != null && parameters.Count > 0)
            //        {
            //            foreach (var param in parameters)
            //                cmd.Parameters.Add(param.Key, param.Value);
            //        }
            //        cmd.CommandType = commandtype;
            //        cmd.CommandTimeout = 3000;
            //        return cmd.ExecuteNonQuery();
            //    }
            //    catch
            //    {
            //        return -1;
            //    }
            //    finally
            //    {
            //        if (conn.State != ConnectionState.Closed)
            //            conn.Close();
            //    }
            //}
            #endregion

            #region Use Transaction
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(query, Connection, Transaction);
                if (parameters != null && parameters.Count > 0)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.Add(param.Key, param.Value);
                }
                cmd.CommandType = commandtype;
                cmd.CommandTimeout = 3000;
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
              
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(query,  ex.Message, sw);
                }

                throw new Exception(ex.Message);
               //return -1;
            }
            #endregion
        }

        #endregion
    }
}
