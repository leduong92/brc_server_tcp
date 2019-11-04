using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer
{
    public class PlcConnection
    {
        #region DLL Import
        [DllImport(@"\dll\CMelP3E.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int P3ERead(string ipaddr, uint port, string buf, int bufsize
       , int devtype, string devaddr, short counts);

        [DllImport(@"\dll\CMelP3E.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int P3EWrite(string ipaddr, uint port, int devtype, string devaddr, short counts, string data);
        #endregion

        #region Propeties
        private static PlcConnection instance;

        public static PlcConnection Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlcConnection();

                return instance;
            }
        }
        #endregion

        #region Method
        public PlcConnection() { }

        /// <summary>
        /// Gui tin hieu toi PLC
        /// </summary>
        /// <param name="ip">Dia chi IP cua PLC</param>
        /// <param name="port">Port cur PLC PLC</param>
        /// <param name="data">Vung du lieu muon doc o PLC</param>
        /// <returns></returns>
        public int SendToPlc(string ip, uint port, string data)
        {
            int ret;
            string signal = string.Format("M*00{0}", data);

            //ret = P3EWrite("10.203.83.81", 25884, 1, "M*000302", 1, "1");
            ret = P3EWrite(ip, port, 1, signal, 1, "1");

            return ret;
        }

        /// <summary>
        /// Nhan tin hieu tra ve tu PLC
        /// </summary>
        /// <param name="ip">Dia chi IP cua PLC</param>
        /// <param name="port">Port cur PLC PLC</param>
        /// <param name="data">Vung du lieu muon doc o PLC</param>
        /// <returns></returns>
        public int ReceiveFromPlc(string ip, uint port, string data)
        {
            int ret;
            string buf = string.Empty;
            string signal = string.Format("W*00{0}", data);

            ret = P3ERead(ip, port, buf, buf.Length, 0, signal, 1);

            return ret;
        }
        #endregion
    }
}
