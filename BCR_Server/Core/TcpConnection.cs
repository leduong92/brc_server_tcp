using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BcrServer
{
    public static class TcpConnection
    {
        static IPAddress localAdd;
        static TcpListener listener;
        static NetworkStream nwStream;
        static TcpClient client;
        public static string DataToSend;
        public static void Disconnect()
        {
            try
            {
                if (listener != null)
                    listener.Stop();

                if (nwStream != null)
                    nwStream.Dispose();

                if (client != null)
                    client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't disconnect barcode server.");
                Console.WriteLine("Error: \n" + ex.Message);
            }
        }
        public static bool Init(string ipAddr, int portNo, string dbName)
        {
            try
            {
                localAdd = IPAddress.Parse(ipAddr);
                listener = new TcpListener(localAdd, portNo);
                //listener = new TcpListener(IPAddress.Any, Properties.Settings.Default.PORT_NO);
                listener.Start();
                //Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");         
                Console.WriteLine(string.Format(">> Started Date: {0}", DateTime.Today.ToLongDateString()));
                Console.WriteLine(string.Format(">> Started Time: {0}", DateTime.Today.ToLongTimeString()));
                Console.WriteLine(string.Format(">> Connected to DB: {0}", dbName));
                Console.WriteLine(">> Barcode Server already started!!!\n");
                //Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> Can't initialize connection.");
                Console.WriteLine(">> Error: \n>> " + ex.Message);
                return false;
            }
        }

        public static string WaitRequestFromHT()
        {
            string dataReceived = string.Empty;
            string result = string.Empty;

            try
            {
                client = listener.AcceptTcpClient();
                nwStream = client.GetStream();
               
                nwStream.ReadTimeout = BcrServer.Properties.Settings.Default.READ_TIME_OUT;
                //nwStream.WriteTimeout = Properties.Settings.Default.WRITE_TIME_OUT;
                client.SendBufferSize = BcrServer.Properties.Settings.Default.SEND_BUFFER_SIZE; //10Mb
                client.ReceiveBufferSize = BcrServer.Properties.Settings.Default.RECEIVE_BUFFER_SIZE; //10Mb

                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                if (bytesRead > 0)
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                else
                    dataReceived = "";

                Console.WriteLine(">> Received : " + dataReceived);

                result = dataReceived.Substring(5);
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> WaitRequestFromHT \n" + ex.Message);

                if(nwStream != null)
                    nwStream.Close();

                if(client != null)
                    client.Close();

                if(listener != null)
                    listener.Stop();
            }

            return result;
        }

        public static void SendBackToHT(string dataSend)
        {
            try
            {
                string tmp = dataSend;
                string textToSend = dataSend.Length.ToString("D5") + dataSend;

                Console.WriteLine(">> Sending back: " + textToSend);
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                nwStream.Close();
                client.Close();

                Console.WriteLine(string.Format(">> Send OK ({0} bytes)\n", dataSend.Length.ToString("D5")));
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> SendBackToHT \n" + ex.Message);

                if (nwStream != null)
                    nwStream.Close();

                if (client != null)
                    client.Close();
            }
        }
    }
}
