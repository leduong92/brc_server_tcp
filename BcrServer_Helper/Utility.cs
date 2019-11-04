using BcrServer_Helper;
using BcrServer_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Helper
{
    public class Utility
    {
        private static Utility instance;

        public static Utility Instance
        {
            get
            {
                if (instance == null)
                    instance = new Utility();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        public Utility() { }

        public string MenuId { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }

        public string ToOneRow(List<string> data)
        {
            return "'" + data.Aggregate((a, b) => a + "', '" + b) + "'";
        }

        public string ToOneRow(DataTable data, int type = 0, string colName = null)
        {
            string val = string.Empty;

            if (type == 0)
                val = string.Join("", data.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));
            else if (type == 1)
                val = ToOneRow(ConvertToList(data, colName));

            return val;
        }

        public List<string> ConvertToList(DataTable dt, string column_name)
        {
            return dt.AsEnumerable().Select(t => t.Field<string>(column_name).Trim()).ToList();
        }

        public string JoinWithSpecialCharater(DataTable data, string colName, string spChar = "@")
        {
            return ConvertToList(data, colName).Aggregate((a, b) => a + spChar + b);
        }

        public string AddQuoteFromString(string data)
        {
            string value = string.Empty;
            foreach(string str in data.Split(','))
            {
                value += string.Format("'{0}',", str.Trim());
            }

            return value.Substring(0, value.LastIndexOf(','));
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section,
           string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        public string IniReadValue(string Section, string Key)
        {
            string temp = System.Reflection.Assembly.GetExecutingAssembly().Location;

            string path = temp.Substring(0, temp.LastIndexOf("\\")) + "\\vnn.ini";

            StringBuilder sb = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", sb, 255, path);
            return sb.ToString();

        }

    }
}
