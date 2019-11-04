using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer
{
    public static class AppCommon
    {
        public static string ShowErrorMsg(int msgCode, string msgTitle = null, string msgData = null, string msg1 = null)
        {
            string title = "NG" + (msgTitle == null ? "Thong Bao" : msgTitle);
            string msg = msg1 == null ? BcrServer_Helper.Message.MessageDictionary[msgCode] : msg1;
            string space = "";
            int t = 0;
            if (msg.Length > 20)
            {
                t = msg.Length - 20;
                space = ("").PadRight(20 + (20 - t));
            }

            return (title.PadRight(22) + msg.PadRight(20) + space + (msgData == null ? "" : msgData.PadRight(20)));
        }

        public static string ShowOkMsg(int msgCode, string msgTitle = null, string msgData = null, string msg1 = null)
        {
            string title = "OK" + (msgTitle == null ? "Thong Bao" : msgTitle);
            string msg = msg1 == null ? BcrServer_Helper.Message.MessageDictionary[msgCode] : msg1;
            string space = "";
            int t = 0;

            if (msg.Length > 20)
            {
                t = msg.Length - 20;
                space = ("").PadRight(20 + (20 - t));
            }

            string ret = title.PadRight(22) + msg.PadRight(20) + space + (msgData == null ? "" : msgData.PadRight(20));

            return ret;
        }

        public static int CheckFifo(string lot2, string lot1)
        {
            if (string.Compare(lot2, lot1) < 0)
            {
                return -1;
            }

            return 0;

            //return string.Compare(lot2, lot1);
        }
    }
}
