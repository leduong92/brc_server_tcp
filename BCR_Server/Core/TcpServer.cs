using BcrServer_Helper;
using BcrServer_Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BcrServer
{
    public class TcpServer
    {
        TcpListener listener;
        public void StartListening()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse(BcrServer.Properties.Settings.Default.SERVER_IP), BcrServer.Properties.Settings.Default.PORT_NO);

                TcpClient client = null;

                listener.Start();

                Console.WriteLine(">> Started date: {0}", DateTime.Today.ToLongDateString());
                Console.WriteLine(">> Started time: {0}", DateTime.Now.ToLongTimeString());
                Console.WriteLine(">> Connected to DB: {0}", BcrServer.Properties.Settings.Default.DB_NAME);
                Console.WriteLine(">> Barcode server already started!!!\n");

                while (true)
                {
                    client = listener.AcceptTcpClient();
                    client.Client.DontFragment = true;
                    client.NoDelay = true;
                    client.Client.NoDelay = true;
                    client.SendBufferSize = BcrServer.Properties.Settings.Default.SEND_BUFFER_SIZE; //20Mb
                    client.ReceiveBufferSize = BcrServer.Properties.Settings.Default.RECEIVE_BUFFER_SIZE; //20Mb

                    //Multithread
                    //Each connection will be separate threads.
                    ThreadPool.QueueUserWorkItem(ThreadProc, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> Can't initialize connection.");
                Console.WriteLine(">> Error: \n>> " + ex.Message);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Moi mot connection se goi toi ThreadProc de xu ly
        /// </summary>
        /// <param name="obj">TCP Client accepted</param>
        private void ThreadProc(object obj)
        {
            try
            {
                string dataReceived = string.Empty;
                var client = (TcpClient)obj;

                NetworkStream nwStream = client.GetStream();

                if (nwStream.CanRead)
                {
                    nwStream.ReadTimeout = BcrServer.Properties.Settings.Default.READ_TIME_OUT;
                    //nwStream.ReadTimeout = 20000;

                    int numberOfBytesRead = 0;
                    byte[] buffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();

                    //Incoming message may be larger than the buffer size.
                    int rcvLen = 0;
                    int i = 0;

                    /* Do TCP Segment = 1460 => neu chuoi gui tu H/T len Bcr Server > 1460 thi se bi split package.
                     * Can kiem tra do dai chuoi nhan duoc sau moi lan loop co bang voi do dai chuoi gui di hay khong( 5 byte dau cua chuoi gui di la do dai chuoi duoc tinh tu tay scan */
                    do
                    {
                        do
                        {
                            //Moi lan chi doc 1024 bytes
                            numberOfBytesRead = nwStream.Read(buffer, 0, buffer.Length);
                            //Console.WriteLine(numberOfBytesRead.ToString());

                            myCompleteMessage.AppendFormat("{0}", ASCIIEncoding.ASCII.GetString(buffer, 0, numberOfBytesRead));
                            //CountTcpConnections();
                        }
                        while (nwStream.DataAvailable);

                        dataReceived = myCompleteMessage.ToString();

                        //Do dai thuc te nhan duoc moi lan 
                        rcvLen = dataReceived.Trim().Length;

                        //Do dai cua goi tin = 5 byte dau tien + 5.
                        i = Convert.ToInt32(dataReceived.Substring(0, 5)) + 5;
                    } while (rcvLen < i); //Loop cho den khi nhan day du du lieu tu network stream.

                    Console.WriteLine(">> Received From: {0}", client.Client.RemoteEndPoint.ToString());
                    Console.WriteLine(">> Received Time: {0}", DateTime.Now);
                    Console.WriteLine(">> Received Data: \n{0}\n", dataReceived);

                    HandleCommand handler = new HandleCommand();

                    handler.CommandDistribution(ref client, ref nwStream, dataReceived.Substring(5));
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "TCP Server", sw);
                }                               
            }
        }

        /// <summary>
        /// Khi close chuong trinh, se close TCP Listener de lan sau khi mo chuong trinh khong bi dung port
        /// </summary>
        public void Disconnect()
        {
            if (listener != null)
            {
                listener.Stop();
            }
        }

        /// <summary>
        /// Hien thi trang thai cua goi tin (Time Wait, FIN, Established)
        /// </summary>
        private  void CountTcpConnections()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            int establishedConnections = 0;
            
            foreach (TcpConnectionInformation t in connections)
            {
                if (t.RemoteEndPoint.Address.ToString()== "10.203.83.232")
                {
                    //if (t.State == TcpState.Established)
                    //{
                    //    establishedConnections++;
                    //}
                    Console.WriteLine("Connection state: {0}", t.State);
                    Console.Write("Local endpoint: {0} ", t.LocalEndPoint.Address);
                    Console.WriteLine("Remote endpoint: {0} ", t.RemoteEndPoint.Address);
                }

            }
            Console.WriteLine("There are {0} established TCP connections.",
               establishedConnections);
        }
    }
}
