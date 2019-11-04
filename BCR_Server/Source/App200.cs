using BcrServer_Model;
using BcrServer_Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BcrServer
{
    public class App200
    {
        private static App200 instance;
        public static App200 Instance
        {
            get
            {
                if (instance == null)
                    instance = new App200();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        UnitOfWork uow;

        public App200()
        {
            uow = new UnitOfWork();
        }

        ~App200()
        {
            uow = null;
        }

        /// <summary>
        /// Thay Doi Pallete Cua Thung - M200
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public string ChangePalleteByBoxNo(List<string> boxes)
        {
            try
            {
                string palleteNo = string.Empty;
                string palleteTypeNo = string.Empty;
                int stBoxInfo;

                foreach (string box in boxes)
                {
                    if (!box.StartsWith("V"))
                        continue;

                    //01-Kiem tra item cua box duoc scan co phai ma hang can thay doi pallete hay khong.
                    DataTable dtWHPlan = uow.App200Repo.GetChangePalleteInfoByBoxNo(box);

                    if (dtWHPlan.Rows.Count == 0)
                    {
                        return AppCommon.ShowOkMsg(1);
                    }

                    //Kiem tra box duoc scan co trong td_box_info khong.
                    DataTable dtBoxInfo = uow.App200Repo.GetBoxInfoByBoxNo(box);
                    if (dtBoxInfo.Rows.Count == 0)
                    {
                        return AppCommon.ShowErrorMsg(8);
                    }

                    stBoxInfo = Convert.ToInt32(dtBoxInfo.Rows[0]["status"]);
                    if (stBoxInfo >= 3)
                    {
                        return AppCommon.ShowErrorMsg(9);
                    }

                    palleteNo = dtWHPlan.Rows[0]["PALLETE_NO"] == null ? "" : dtWHPlan.Rows[0]["PALLETE_NO"].ToString().Trim();
                    if (string.IsNullOrEmpty(palleteNo))
                    {
                        return AppCommon.ShowErrorMsg(2);
                    }

                    //Lay thong tin cua pallete
                    DataTable dtPallete = uow.App200Repo.GetPalleteInfoByPalleteNo(palleteNo);
                    if (dtPallete.Rows.Count == 0)
                    {
                        return AppCommon.ShowErrorMsg(4, null, palleteNo);
                    }

                    palleteTypeNo = dtPallete.Rows[0]["PARENTPALLETE_NO"] == null ? "" : dtPallete.Rows[0]["PARENTPALLETE_NO"].ToString().Trim();
                    if (string.IsNullOrEmpty(palleteTypeNo))
                    {
                        return AppCommon.ShowErrorMsg(6, null, palleteNo);
                    }

                    //Neu pallete trong box_info = pallete duoc scan => Box da duoc doi pallete.
                    string palleteTemp = dtBoxInfo.Rows[0]["PALLETE_NO"] == null ? "" : dtBoxInfo.Rows[0]["PALLETE_NO"].ToString().Trim();
                    if (string.IsNullOrEmpty(palleteTemp))
                    {
                        return AppCommon.ShowErrorMsg(2, null, box);
                    }

                    if (palleteTemp.Equals(palleteNo))
                    {
                        return AppCommon.ShowOkMsg(5, null, palleteNo);
                    }

                    if (uow.App200Repo.UpdatePalleteNoByBoxNo(box, palleteTypeNo, palleteNo) <= 0)
                    {
                        return AppCommon.ShowErrorMsg(7, null, box);
                    }

                    uow.Commit();

                    string symSeq = (dtPallete.Rows[0]["SEQ_NO"].ToString().Trim() + " - " + dtPallete.Rows[0]["SYMBOL"].ToString().Trim()).PadRight(20);

                    //TcpConnection.DataToSend = AppCommon.ShowOkMsg(999, null, symSeq + palleteNo);
                    return (symSeq + palleteNo);
                }
                return AppCommon.ShowErrorMsg(999, null, null, "No Data");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }

        /// <summary>
        /// M201
        /// </summary>
        /// <param name="pclList"></param>
        /// <returns></returns>
        public string GetBoxAndReceiveByPclNo(List<string> pclList)
        {
            try
            {
                string listBox = string.Empty;

                if (pclList == null)
                {
                    return AppCommon.ShowErrorMsg(11);
                }

                if (pclList.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(11);
                }

                foreach (string pcl in pclList)
                {
                    if (!pcl.StartsWith("LOR1") && !pcl.StartsWith("LOS1"))
                    {
                        return AppCommon.ShowErrorMsg(12); ;
                    }

                    DataTable dt = uow.App200Repo.GetBoxAndReceiveByPclNo(pcl);

                    if (dt.Rows.Count == 0)
                    {
                        return AppCommon.ShowErrorMsg(3, pcl);
                    }

                    //Make data from column to 1 row
                    if (string.IsNullOrEmpty(listBox))
                        listBox = string.Join("", dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));
                    else
                    {
                        string tmp = string.Join("", dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));
                        listBox += tmp;
                    }
                }

                return listBox;
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }

        /// <summary>
        /// M203
        /// </summary>
        /// <param name="boxNo"></param>
        /// <returns></returns>
        public string GetAllBoxInPalleteByBoxNo(string boxNo)
        {
            try
            {
                string boxes = string.Empty;

                if (string.IsNullOrEmpty(boxNo))
                    return AppCommon.ShowErrorMsg(14);

                if (!boxNo.StartsWith("V"))
                    return AppCommon.ShowErrorMsg(15);

                DataTable dbBoxes = uow.App200Repo.GetAllBoxInPalleteByBoxNo(boxNo);

                if (dbBoxes.Rows.Count == 0)
                    return AppCommon.ShowErrorMsg(16, null, boxNo);

                boxes = string.Join("", dbBoxes.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.ItemArray)));

                return boxes;
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }

        /// <summary>
        /// Tim thung hang bi thieu tren pallete - M203S01
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public string PrintBoxFoundInPallete(List<string> boxes)
        {
            try
            {
                List<BoxNotExist> printList = new List<BoxNotExist>();
                string date = DateTime.Now.ToString("yyyyMMdd");
                string time = DateTime.Now.ToString("hhmmss");
                string strNotIn = "'" + boxes.Aggregate((a, b) => a + "', '" + b) + "'";

                DataTable data = uow.App200Repo.GetAllBoxNotInPallete(strNotIn, boxes.FirstOrDefault());
                uow.App200Repo.DeleteBoxNotInPallete(boxes.FirstOrDefault());

                foreach (DataRow row in data.Rows)
                {
                    DataTable dt = uow.App200Repo.GetBoxInfoByBoxJob(row["box_no"].ToString());
                    string destination = dt.Rows[0]["cust_user_main_stk_loc"].ToString().Trim();

                    BoxNotExist temp = new BoxNotExist()
                    {
                        BoxNo = row["box_no"].ToString(),
                        Item = dt.Rows[0]["item"].ToString(),
                        LotNo = dt.Rows[0]["lot"].ToString(),
                        Qty = Convert.ToInt32(dt.Rows[0]["qty"].ToString()),
                        PalleteNo = dt.Rows[0]["pallete_no"].ToString(),
                        EntryDate = date,
                        EntryTime = time,
                        Destination = destination.Length == 4 ? destination.Substring(1, 1) + destination.Substring(3, 1) : destination
                    };

                    printList.Add(temp);

                    int ret = uow.App200Repo.InsertToBoxNotInPallete(temp);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(17);
                    }
                }

                uow.Commit();
                List<BoxNotExist> printTemp = printList.OrderBy(p => p.Item).ThenBy(p => p.LotNo).ThenBy(p => p.BoxNo).ToList<BoxNotExist>();

                Printer<BoxNotExist>.PrintData("BOXNOTINPALLETE", printTemp);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }

        /// <summary>
        /// Dua pallete len Container - M204
        /// </summary>
        /// <param name="location"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string LoadPalleteToContainer(string location, string userId, List<string> data)
        {
            try
            {
                bool bret = false;
                int ret = 0;
                //Checking
                string place = data[0].ToString().Substring(3).Trim();
                string containerNo = data[1].ToString().Trim();
                //string shippingSeq = data[2].ToString().Trim
                string shippingSeq = string.Empty;
                string containerDate = data[4].ToString().Trim();
                string pallete = data[5].ToString().Trim();

                //Kiem tra pallete da duoc scan ship hay chua
                bret = uow.App200Repo.IsPalleteExists(pallete);
                if (bret == false)
                    return AppCommon.ShowErrorMsg(23);

                bret = uow.App200Repo.IsPalleteCanExport(pallete);
                if (!bret)
                    return AppCommon.ShowErrorMsg(22);

                if (containerNo.Length < 11)
                    return AppCommon.ShowErrorMsg(19);

                DataTable dt = uow.App200Repo.GetBoxInfoByContainerNoAndDate(containerNo, containerDate);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["container_lock_sign"].ToString().Trim() == "*")
                    {
                        return AppCommon.ShowErrorMsg(26);
                    }
                }

                if (location.Equals("OR1"))
                {
                    if (!place.ToUpper().Equals("NAGOYA"))
                        return AppCommon.ShowErrorMsg(18);

                    shippingSeq = data[2].ToString().Trim();
                }

                if (location.Equals("OS1"))
                {
                    if (!place.ToUpper().Equals("NAGOYA") && !place.ToUpper().Equals("YOKOHAMA"))
                        return AppCommon.ShowErrorMsg(18);

                    string symbol = uow.App200Repo.GetSymbolOfPalleteNo(pallete);
                    if (symbol.Substring(0, 3).Equals("PAA"))
                    {
                        if (!place.ToUpper().Equals("YOKOHAMA"))
                            return AppCommon.ShowErrorMsg(18);
                    }
                    else
                    {
                        if (!place.ToUpper().Equals("NAGOYA"))
                            return AppCommon.ShowErrorMsg(18);
                    }

                    shippingSeq = data[3].ToString().Trim();
                }

                ret = uow.App200Repo.UpdateContainerByPallete(pallete, containerNo, shippingSeq, containerDate, location, userId, DateTime.Now.ToString("HHmmss"));

                if (ret <= 0)
                {
                    uow.Rollback();
                }

                uow.Commit();
                //If check ok and update the data no problem.
                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }

        /// <summary>
        /// Khoa pallete khi da len container xong - M205
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SetLockSignForPallete(List<string> data)
        {
            try
            {
                string place = data[0].ToString().Substring(3).Trim();
                string containerNo = data[1].ToString().Trim();
                string shippingSeq = data[2].ToString().Trim();
                string shippingSeqOS = data[3].ToString().Trim();
                string containerDate = data[4].ToString().Trim();
                string containerLockSign = string.Empty;

                int ret = 0;

                DataTable dt = uow.App200Repo.GetBoxInfoByContainerNoAndDate(containerNo, containerDate);

                if (dt.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(888);

                foreach (DataRow row in dt.Rows)
                {
                    containerLockSign = row["container_lock_sign"] == null ? "" : row["container_lock_sign"].ToString().Trim();

                    if (containerLockSign == "*")
                        return AppCommon.ShowErrorMsg(24);
                }

                ret = uow.App200Repo.UpdateContainerLockSign(containerNo, containerDate);

                if (ret < 1)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(25);
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    BcrServer_Helper.WriteLog.Instance.Write(ex.Message, "", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, " ");
            }
        }
    }
}
