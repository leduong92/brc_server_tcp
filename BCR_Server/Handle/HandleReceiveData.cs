using BcrServer_Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer
{
    public class ReceiveModel
    {
        public string MenuId { get; set; }
        public string SubMenu { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
        public List<string> Data { get; set; }
    }

    public class HandleReceiveData
    {
        public List<ReceiveModel> RecvList;
        public void SplitDataByModel(string data)
        {
            try
            {
                RecvList = new List<ReceiveModel>();

                string temp = string.Empty;
                string menu = string.Empty;

                if (data.Split('$').Length < 4)
                {
                    RecvList.Add(new ReceiveModel()
                    {
                        MenuId = (data.Split('$'))[0].ToString().Trim(),
                        UserId = (data.Split('$'))[1].ToString().Replace("*", "")
                    });
                }
                else
                {
                    menu = (data.Split('$'))[0].ToString().Trim();

                    RecvList.Add(new ReceiveModel()
                    {
                        //MenuId = (data.Split('$'))[0].ToString(),
                        MenuId = menu.Length == 7 ? menu.Substring(0, 4) : menu,
                        SubMenu = menu.Length == 7 ? menu.Substring(4, 3) : "",
                        Location = (data.Split('$'))[1].ToString(),
                        UserId = (data.Split('$'))[2].ToString().Replace("*", ""),
                    });

                    temp = (data.Split('$'))[3];

                    RecvList[0].Data = new List<string>();

                    RecvList[0].Data.AddRange((temp.Split('@').ToList<string>()).Where(t => !string.IsNullOrWhiteSpace(t)).ToList());
                }
            }
            catch (Exception ex)
            {
                RecvList[0].Data = new List<string>();
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SplitDataByModel", sw);
                }
                return;
            }
        }
    }
}
