using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Npgsql;

namespace BcrServer
{
    class Program
    {
        static TcpServer server;
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                server.Disconnect();
            }
            return false;
        }

        private static void Init()
        {
            Header();

            //Set Path Execute
            string temp = System.Reflection.Assembly.GetExecutingAssembly().Location;
            BcrServer_Repository.Properties.Settings.Default.PATH_EXECUTE = temp.Substring(0, temp.LastIndexOf("\\"));
           
            //Set DB Name for Repository DLL
            BcrServer_Repository.Properties.Settings.Default.DB_NAME = Properties.Settings.Default.DB_NAME;
            BcrServer_Repository.Properties.Settings.Default.Save();

            Properties.Settings.Default.PATH_EXECUTE = temp.Substring(0, temp.LastIndexOf("\\"));
        }

        private static void Header()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Title = Properties.Settings.Default.TITLE;

            Console.WriteLine();
            Console.WriteLine("***************************************************************");
            Console.WriteLine("**                     VNN BARCODE SERVER                    **");
            Console.WriteLine("**                   Copyright © 2018 - VNN                  **");
            Console.WriteLine("**                      Version: {0}                    **", Properties.Settings.Default.VERSION);
            Console.WriteLine("***************************************************************");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            Init();
            //string lotNo = "94-20", lotNo2 = "94-20";

            //int ret = lotNo.CompareTo(lotNo2);

            if (CheckConnection() == false)
                Environment.Exit(0);

            string t = System.Reflection.Assembly.GetExecutingAssembly().Location;
       

            //ShowTcpConnectionStatistics();
            //ShowTcpTimeouts();
            //ShowTcpSegmentData();
            //GetTcpConnections();

            //20181202
            //TcpServer server = new TcpServer();
            server = new TcpServer();
            server.StartListening();
        }

        static bool CheckConnection()
        {
            string connectionString = string.Empty;

            try
            {
                NpgsqlConnection conn;
                string dbName = Properties.Settings.Default.DB_NAME;

                if (dbName.ToUpper() == "LIVE")
                    connectionString = "Server = 10.203.192.193; Port = 5432; Database = qimlive; User Id = qimlive; Password = qimlive;";
                else if (dbName.ToUpper() == "TRAINING")
                    connectionString = "Server = 10.203.192.193; Port = 5432; Database = qim_training; User Id = qim_training; Password = qim_training;";
                else
                {
                    MessageBox.Show("DB Name incorrect. Please check...", "Thong bao");
                    return false;
                }

                //Open Connection and pass Transaction to Repository.
                conn = new NpgsqlConnection(connectionString);
                conn.Open();

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                    NpgsqlConnection.ClearAllPools();
                    conn.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Check Connection Error \n" + ex.Message, "Thong bao");
                return false;
            }
        }

        #region Check Tcp Parameters
        public static void ShowTcpTimeouts()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpStatistics tcpstat = properties.GetTcpIPv4Statistics();

            Console.WriteLine("  Minimum Transmission Timeout............. : {0}",
                tcpstat.MinimumTransmissionTimeout);
            Console.WriteLine("  Maximum Transmission Timeout............. : {0}",
                tcpstat.MaximumTransmissionTimeout);
            Console.WriteLine("  Maximum connections ............. : {0}",
                tcpstat.MaximumConnections);
            Console.WriteLine();
        }
        public static void ShowTcpSegmentData()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpStatistics tcpstat = properties.GetTcpIPv4Statistics();

            Console.WriteLine("  Segment Data:");
            Console.WriteLine("      Received  ........................... : {0}",
                tcpstat.SegmentsReceived);
            Console.WriteLine("      Sent ................................ : {0}",
                tcpstat.SegmentsSent);
            Console.WriteLine("      Retransmitted ....................... : {0}",
                tcpstat.SegmentsResent);
            Console.WriteLine("      Resent with reset ................... : {0}",
                tcpstat.ResetsSent);
            Console.WriteLine();
        }
        public static void ShowTcpConnectionStatistics()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpStatistics tcpstat = properties.GetTcpIPv4Statistics();

            Console.WriteLine("  Connection Data:");
            Console.WriteLine("      Current  ............................ : {0}",
                tcpstat.CurrentConnections);
            Console.WriteLine("      Cumulative .......................... : {0}",
                tcpstat.CumulativeConnections);
            Console.WriteLine("      Initiated ........................... : {0}",
                tcpstat.ConnectionsInitiated);
            Console.WriteLine("      Accepted ............................ : {0}",
                tcpstat.ConnectionsAccepted);
            Console.WriteLine("      Failed Attempts ..................... : {0}",
                tcpstat.FailedConnectionAttempts);
            Console.WriteLine("      Reset ............................... : {0}",
                tcpstat.ResetConnections);
            Console.WriteLine("      Errors .............................. : {0}",
                tcpstat.ErrorsReceived);
            Console.WriteLine();
        }
        public static void GetTcpConnections()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            //            TcpConnectionInformation connections = properties.GetActiveTcpConnections();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation t in connections)
            {
                Console.Write("Local endpoint: {0} ", t.LocalEndPoint.Address);
                Console.Write("Remote endpoint: {0} ", t.RemoteEndPoint.Address);
                Console.WriteLine("{0}", t.State);
            }
            Console.WriteLine();
        }
        #endregion
    }
}
