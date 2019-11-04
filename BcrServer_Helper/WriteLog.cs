using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Helper
{
    public class WriteLog
    {
        private static WriteLog instance;

        public static WriteLog Instance
        {
            get
            {
                if (instance == null)
                    instance = new WriteLog();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        public WriteLog() {}

        public void CheckFileSize(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                long sizeOfFile = fi.Length;

                if (sizeOfFile > 10000)
                {
                    File.Delete(path);
                }
            }
        }

        public void Write(string msg, string msg2 , TextWriter tw)
        {
            tw.Write("\r\n  Log Entry: ");
            tw.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            tw.WriteLine("Log Message: {0}", msg);
            tw.WriteLine("             {0}", msg2);
            tw.WriteLine("============================================================================");
        }
    }
}
