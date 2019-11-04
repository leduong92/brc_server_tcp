using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Windows.Forms;

namespace BcrServer_Repository
{
    public class UnitOfWork : IDisposable
    {
        string connectionString = string.Empty;
        NpgsqlTransaction transaction;
        NpgsqlConnection conn;

        public UnitOfWork()
        {
            try
            {
                string dbName = Properties.Settings.Default.DB_NAME;

                if (dbName.ToUpper() == "LIVE")
                    connectionString = "Server = 10.203.192.193; Port = 5432; Database = qimlive; User Id = qimlive; Password = qimlive;";
                else if (dbName.ToUpper() == "TRAINING")
                    connectionString = "Server = 10.203.192.193; Port = 5432; Database = qim_training; User Id = qim_training; Password = qim_training;";
                else
                {
                    MessageBox.Show("DB Name incorrect. Please check...", "Thong bao");
                    Application.Exit();
                }

                //Open Connection and pass Transaction to Repository.
                conn = new NpgsqlConnection(connectionString);
                conn.Open();
                transaction = conn.BeginTransaction();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private ICommonRepository commonRepo;
        private IApp200Repository app200Repo;
        private IApp300Repository app300Repo;
        private IApp400Repository app400Repo;
        private IApp500Repository app500Repo;

        public ICommonRepository CommonRepo => (commonRepo ?? new CommonRepository(transaction));
        public IApp200Repository App200Repo => (app200Repo ?? new App200Repository(transaction));

        public IApp300Repository App300Repo => (app300Repo ?? new App300Repository(transaction));
        public IApp400Repository App400Repo => (app400Repo ?? new App400Repository(transaction));
        public IApp500Repository App500Repo => (app500Repo ?? new App500Repository(transaction));

        public void Dispose()
        {
            if (transaction != null)
                transaction.Dispose();

            if (conn != null)
            {
                conn.Close();
                NpgsqlConnection.ClearAllPools();
                conn.Dispose();
            }
        }

        public int Commit()
        {
            try
            {
                transaction.Commit();                
            }
            catch
            {
                transaction.Rollback();
                return -1;
                throw;
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();

                //if (conn != null)
                //    conn.Close();

                transaction = conn.BeginTransaction();

                ResetRepositories();
            }
            return 1;
        }
        public void Rollback()
        {
            try
            {
                transaction.Rollback();
                //transaction.Connection.BeginTransaction();
                ResetRepositories();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();

                //if (conn != null)
                //    conn.Close();

                transaction = conn.BeginTransaction();

                ResetRepositories();
            }
        }

        //Set private interface equal null.
        private void ResetRepositories()
        {
            commonRepo = null;
            app200Repo = null;
            app300Repo = null;
            app400Repo = null;
            app500Repo = null;
        }
    }
}
