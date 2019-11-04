using BcrServer_Helper;
using BcrServer_Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer
{
    public class App100
    {
        #region Properties
        private static App100 instance;
        public static App100 Instance
        {
            get
            {
                if (instance == null)
                    instance = new App100();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }
        #endregion
        UnitOfWork uow;
        public App100()
        {
            uow = new UnitOfWork();
        }
        ~App100()
        {
            uow = null;
        }

        #region Method
        public string GetUserProfileByUserId(string userId)
        {
            string dataToSend = string.Empty;
            DataTable dt = uow.CommonRepo.GetDataByUserId(userId);

            if (dt.Rows.Count > 0)
            {
                //dataToSend = dt.Rows[0]["last_name_en"].ToString().Trim() + " " + dt.Rows[0]["first_name_en"].ToString().Trim();
                string name = dt.Rows[0]["last_name_en"].ToString().Trim() + " " + dt.Rows[0]["first_name_en"].ToString().Trim();
                string adminSign = dt.Rows[0]["author"].ToString().Trim();
                string location = dt.Rows[0]["location_cd"].ToString().Trim();

                dataToSend = name.PadRight(30) + adminSign + location;
                //TcpConnection.DataToSend = dataToSend;

                return dataToSend;
            }
            else
            {
                return AppCommon.ShowErrorMsg(666, "Login", userId);
            }
        }

        public void Insert()
        {
            int i = uow.CommonRepo.Insert();

            if (i > 0)
            {
                int j = uow.CommonRepo.Insert2();

                if (j > 0)
                    uow.Commit();
                else
                    uow.Rollback();
            }
        }

        #endregion
    }
}
