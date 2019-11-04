using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{

    public static class ReceiveData<T> where T : new()
    {
        public static string MenuId { get; set; }
        public static string Location { get; set; }
        public static string UserId { get; set; }
        public static T GenericObject { get; set; }

        public static bool SplitByModel(string data)
        {
            try
            {
                string temp = string.Empty;

                if (data.Split('$').Length < 4)
                {
                    MenuId = (data.Split('$'))[0].ToString();
                    UserId = (data.Split('$'))[1].ToString();
                }
                else
                {
                    MenuId = (data.Split('$'))[0].ToString();
                    Location = (data.Split('$'))[1].ToString();
                    UserId = (data.Split('$'))[2].ToString();
                    temp = (data.Split('$'))[3];
                }

                if (UserId.Contains("*"))
                    UserId = UserId.Replace("*", "");

                GenericObject = new T();

                foreach (var prop in GenericObject.GetType().GetProperties())
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        string t = temp.Split('@')[0];
                        prop.SetValue(GenericObject, t);
                        temp = temp.Remove(temp.IndexOf(t), temp.IndexOf(t) + t.Length + 1);
                    }
                    else
                    {
                        prop.SetValue(GenericObject, temp.Split('@').ToList<string>());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                GenericObject = new T();
                return false;
            }
        }
    }

    //public class ReceiveModel
    //{
    //    public string MenuId { get; set; }
    //    public string Location { get; set; }
    //    public string UserId { get; set; }
    //    public List<string> Data { get; set; }
    //    //public List<T> GenericObject { get; set; }
    //}


    //public class ReceiveData
    //{ 
    //    public List<ReceiveModel> RecvList;

    //    public void SplitDataByModel(string data)
    //    {
    //        try
    //        {
    //            RecvList = new List<ReceiveModel>();

    //            string temp = string.Empty;

    //            if (data.Split('$').Length < 4)
    //            {
    //                RecvList[0].MenuId = (data.Split('$'))[0].ToString();
    //                RecvList[0].UserId = (data.Split('$'))[1].ToString().Replace("*","");
    //            }
    //            else
    //            {
    //                RecvList[0].MenuId = (data.Split('$'))[0].ToString();
    //                RecvList[0].Location = (data.Split('$'))[1].ToString();
    //                RecvList[0].UserId = (data.Split('$'))[2].ToString().Replace("*", "");
    //                temp = (data.Split('$'))[3];
    //            }

    //            RecvList[0].Data.AddRange(temp.Split('@').ToList<string>());
    //        }
    //        catch (Exception ex)
    //        {
    //            RecvList[0].Data = new List<string>();
    //            return;
    //        }
    //    }
    //}
}
