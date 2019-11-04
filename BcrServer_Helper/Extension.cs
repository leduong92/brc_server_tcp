using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace BcrServer_Helper
{
    public static class Extension
    {
        public static string AddSingleQuotationMark(this string str)
        {
            return string.Format("'{0}'", str);
        }
        public static List<T> DataTableToList<T>(this DataTable dt) where T : new()
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}
