using BcrServer_Helper;
using BcrServer_Model;
using BcrServer_Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Globalization;

namespace BcrServer
{
    public class App300
    {
        #region [Initialize]
        private static App300 instance;
        public static App300 Instance
        {
            get
            {
                if (instance == null)
                    instance = new App300();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        UnitOfWork uow;
        public App300()
        {
            uow = new UnitOfWork();
        }

        ~App300()
        {
            uow = null;
        }

        #endregion  [Initialize]

      
        /// <summary>
        /// Nhan hang thanh pham -
        /// M301
        /// Lay tat ca cac thung theo list pcl
        /// 
        /// </summary>
        /// <param name="pclList">Danh sach PCL da quet</param>
        /// <returns></returns>
        public string GetAllBoxByPCL(List<string> pclList, string userId)
        {
            try
            {
                if (pclList == null)
                    return AppCommon.ShowErrorMsg(11);

                if (pclList.Count < 1)
                    return AppCommon.ShowErrorMsg(11);

                string strPcl = Utility.Instance.ToOneRow(pclList);

                string result = uow.App300Repo.CheckingPclReceived(strPcl);

                if (!string.IsNullOrEmpty(result))
                    return AppCommon.ShowErrorMsg(3, "Warning!!!", result);

                DataTable dtBoxes = uow.App300Repo.GetAllBoxByPCL(strPcl);

                if (dtBoxes.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(29, "Warning!!!", "M301");

                return Utility.Instance.ToOneRow(dtBoxes);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetAllBoxByPCL", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M301");
            }
        }

        public string GetBoxOfMitsuba(List<string> pclList)
        {
            try
            {
                if (pclList == null)
                    return AppCommon.ShowErrorMsg(11);

                if (pclList.Count < 1)
                    return AppCommon.ShowErrorMsg(11);

                string strPcl = Utility.Instance.ToOneRow(pclList);

                string t = Utility.Instance.IniReadValue("MITSUBA", "ITEMS");

                string items = Utility.Instance.AddQuoteFromString(t);

                DataTable dt = uow.App300Repo.GetAllBoxMitsubaByPCL(strPcl, items);

                //DataTable dt = uow.App300Repo.GetAllBoxMitsubaByPCL("'LOR1201907030013'", items);

                return Utility.Instance.ToOneRow(dt);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetBoxOfMitsuba", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M301");
            }
        }

        public string GetMissingBox(string boxes)
        {
            string strBoxes = string.Empty;
  
            try
            {
                if (string.IsNullOrEmpty(boxes))
                    return AppCommon.ShowErrorMsg(14);

                strBoxes = boxes.Replace("VC", "','VC").Substring(2) + "'";

                DataTable dt = uow.App300Repo.FindBoxNotScannedYet(strBoxes);

                return Utility.Instance.JoinWithSpecialCharater(dt, "item", "\n\n");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetMissingBox", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M301");
            }
        }

        public string GetAllBoxORWithoutPCL()
        {
            try
            {
                DataTable dtBoxes = uow.App300Repo.GetAllBoxORWithoutPCL();

                if (dtBoxes.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(29, "Warning!!!", "M301");

                return Utility.Instance.ToOneRow(dtBoxes);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetAllBoxORWithoutPCL", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M301");
            }
        }

        public string GetAllPCLNotReceiveYetForOR()
        {
            try
            {
                DataTable dataPcl = uow.App300Repo.GetPlcRecorded();

                if (dataPcl.Rows.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(11);
                }

                return Utility.Instance.JoinWithSpecialCharater(dataPcl, "pcl_no");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M300", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M300");
            }

        }


        /// <summary>
        /// Nhan hang thanh pham -
        /// M301S01
        /// </summary>
        /// <param name="pclList"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string ReceiveBoxByPCL(List<string> pclList, string userId)
        {
            try
            {
                int ret = 0;
                string strPCLIn = string.Empty;
                string instockDate = DateTime.Now.ToString("yyyyMMdd");

                if (pclList == null)
                    return AppCommon.ShowErrorMsg(11);

                if (pclList.Count < 1)
                    return AppCommon.ShowErrorMsg(11);

                strPCLIn = Utility.Instance.ToOneRow(pclList);

                ret = uow.App300Repo.UpdateIncomeStatusOfBox(strPCLIn, userId, instockDate);

                if (ret > 0)
                {
                    //Incoming Protection 
                    ret = uow.App300Repo.InsertUnpostedJobBySelect(strPCLIn, userId);
                    if (ret > 0)
                        uow.Commit();
                    else
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(27);
                    }
                }
                else
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(28);
                }

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "ReceiveBoxByPCL", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M301S01");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho - 
        /// M306
        /// </summary>
        /// <param name="pallete"></param>
        /// <returns></returns>
        public string GetAllBoxByPalleteNo(string pallete)
        {
            try
            {
                if (string.IsNullOrEmpty(pallete))
                    return AppCommon.ShowErrorMsg(2);

                DataTable dtBoxes = uow.App300Repo.GetAllBoxByPallete(pallete);
                if (dtBoxes.Rows.Count == 0)
                    return AppCommon.ShowErrorMsg(29);

                return Utility.Instance.ToOneRow(dtBoxes);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M306");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306S01
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public string HandleDataForInventoryChecking(List<string> boxes, string userId)
        {
            try
            {
                if (boxes.Count == 0)
                    return AppCommon.ShowErrorMsg(29, "M306S01");

                string palleteNo = boxes[0];

                string strBoxesIn = string.Empty;

                DataTable dtPallete = uow.App300Repo.GetGroupNumberByPallete(palleteNo);

                if (dtPallete.Rows.Count < 0)
                    return AppCommon.ShowErrorMsg(04, null, palleteNo);

                int groups = Convert.ToInt32(dtPallete.Rows[0]["groups"]);

                string symbol = dtPallete.Rows[0]["symbol"] == null ? "" : dtPallete.Rows[0]["symbol"].ToString().Trim();

                boxes.RemoveAt(0);

                boxes = boxes.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();

                strBoxesIn = Utility.Instance.ToOneRow(boxes);

                /*Lay thong tin box info theo danh sach cac box duoc gui len.
                 *Neu so luong box tra ve != so luong box scan len thi thong bao loi.
                 */
                DataTable dtBoxInfo = uow.App300Repo.GetBoxInfoByManyBox(strBoxesIn, groups);

                if (dtBoxInfo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(888, "M306S01");

                //boxes.Add("vcdddddddd");
                if (dtBoxInfo.Rows.Count != boxes.Count())
                {
                    //Convert from datatable to list<string>
                    List<string> rows = Utility.Instance.ConvertToList(dtBoxInfo, "BOX_NO");

                    //Get list of string not exist in boxes
                    List<string> exceptList = boxes.Except(rows).ToList<string>();

                    if (exceptList.Count > 0)
                        return AppCommon.ShowErrorMsg(38, "M306S01", exceptList[0]);
                }

                ////Insert du lieu vao td_check_instock
                int ret = uow.App300Repo.DeleteTdCheckStockByBoxNo(strBoxesIn);

                ret = uow.App300Repo.InsertTdCheckStockBySelect(palleteNo, symbol, strBoxesIn, userId);

                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(33, null, strBoxesIn);
                }

                uow.Commit();
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306S01", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M306S01");
            }

            return "";
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306S02
        /// Lay tat ca cac pallete da duoc in shipping to nhung chua scan Reser/ship.
        /// </summary>
        /// <param name="pallete"></param>
        /// <returns></returns>
        public string GetAllPalletePrintedShippingTo(string palleteNo)
        {
            try
            {
                if (string.IsNullOrEmpty(palleteNo))
                    return AppCommon.ShowErrorMsg(02);

                DataTable dtGroupInfo = uow.App300Repo.GetGroupNumberByPallete(palleteNo);

                if (dtGroupInfo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(04, null, palleteNo);

                int groups = Convert.ToInt32(dtGroupInfo.Rows[0]["groups"]);

                DataTable dtPallete = uow.App300Repo.GetAllPalletebyGroup(groups);
                if (dtPallete.Rows.Count == 0)
                    return "";

                return Utility.Instance.ToOneRow(dtPallete);

            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306S02", sw);
                }

                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M306S02", "Catch Exception");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306S03
        /// </summary>
        /// <param name="data">Danh sach shipping to duoc quet va gui tu tay scan</param>
        /// <returns></returns>
        public string HandlePalleteForInventoryChecking(List<string> mnPallete, string userId)
        {
            try
            {
                int groups, groups2;
                string symbol;

                if (mnPallete.Count < 1)
                    return AppCommon.ShowErrorMsg(777, "M306S03");

                string palleteNo = mnPallete[0];

                mnPallete.RemoveAt(0);

                mnPallete = mnPallete.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();

                string strData = Utility.Instance.ToOneRow(mnPallete);

                DataTable dtPallete = uow.App300Repo.GetGroupNumberByPallete(palleteNo);

                if (dtPallete.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(04, null, palleteNo);

                groups = Convert.ToInt32(dtPallete.Rows[0]["groups"]);

                symbol = dtPallete.Rows[0]["symbol"] == null ? "" : dtPallete.Rows[0]["symbol"].ToString().Trim();

                DataTable dtPalleteWh = uow.App300Repo.GetGroupNumberByPallete(strData, 1);
                if (dtPalleteWh.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(04, "M306S03", "dtPalleteWh");

                if (dtPalleteWh.Rows.Count > 1)
                    return AppCommon.ShowErrorMsg(37, "M306S03", "Check Groups");

                groups2 = Convert.ToInt32(dtPalleteWh.Rows[0]["groups"]);

                if (groups != groups2)
                {
                    return AppCommon.ShowErrorMsg(37, "M306S03", "Check Groups");
                }

                foreach (string str in mnPallete)
                {
                    string strBoxes = string.Empty;

                    DataTable dtBoxes = uow.App300Repo.GetBoxInfoNotShipByPallete(str);
                    if (dtBoxes.Rows.Count < 1)
                        return AppCommon.ShowErrorMsg(29, "M306S03", "dtBoxes");

                    strBoxes = Utility.Instance.ToOneRow(dtBoxes, 1, "box_no");

                    int ret = uow.App300Repo.DeleteTdCheckStockByBoxNo(strBoxes);

                    ret = uow.App300Repo.InsertTdCheckStockBySelect(palleteNo, symbol, strBoxes, userId);

                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(33, "M306S03");
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306S03", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M306S03", "Catch Exception");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306R01
        /// </summary>
        /// <param name="boxNo"></param>
        /// <returns></returns>
        public string HandleDataForInventoryCheckingForOR(string boxNo, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(boxNo))
                    return AppCommon.ShowErrorMsg(14, "M306R01", "CHECK BOX");

                DataTable dt = uow.App300Repo.CheckBoxStatusByBoxNo(boxNo);
                if (dt.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(38, "M306R01", boxNo);

                //Neu box_no1(td_box_info) co du lieu => thung hang da duoc scan sap xep.
                string box1 = dt.Rows[0]["box_no1"].ToString();

                if (!string.IsNullOrEmpty(box1))
                {
                    switch (Convert.ToInt32(dt.Rows[0]["status"]))
                    {
                        case 0:
                            return AppCommon.ShowErrorMsg(58, null, box1);
                        case 1:
                            return AppCommon.ShowErrorMsg(59, null, box1);
                        case 4:
                            return AppCommon.ShowErrorMsg(61, null, box1);
                        default:
                            return AppCommon.ShowOkMsg(888);
                    };
                }

                ////Insert du lieu vao td_check_instock
                int ret = uow.App300Repo.DeleteTdCheckStockByBoxNo(string.Format("'{0}'", boxNo));

                ret = uow.App300Repo.InsertTdCheckStockBySelect("", "", string.Format("'{0}'", boxNo), userId);

                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(33, "M306R01", boxNo);
                }

                uow.Commit();

                //return AppCommon.ShowOkMsg(999);
                return "";
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306R01", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M306R01", "Catch Exception");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306R02
        /// </summary>
        /// <returns></returns>
        public string GetAllPalletePrintedShippingToForOR()
        {
            try
            {
                DataTable dtPallete = uow.App300Repo.GetPalletePrintedShippingToForOR();
                if (dtPallete.Rows.Count == 0)
                    return "";

                return Utility.Instance.ToOneRow(dtPallete);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306R02", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M306R02", "Catch Exception");
            }
        }

        /// <summary>
        /// Kiem Hang Ton Kho -
        /// M306R03
        /// </summary>
        /// <param name="mnPallete"></param>
        /// <returns></returns>
        public string HandlePalleteForInventoryCheckingForOR(List<string> mnPallete, string userId)
        {
            try
            {
                if (mnPallete.Count < 1)
                    return AppCommon.ShowErrorMsg(777, "M306R03");

                foreach(string pallete in mnPallete)
                {
                    string strBoxes = string.Empty;

                    DataTable dtBoxes = uow.App300Repo.GetBoxInfoNotShipByPallete(pallete);
                    if (dtBoxes.Rows.Count < 1)
                        return AppCommon.ShowErrorMsg(29, "M306R03", "dtBoxes");

                    strBoxes = Utility.Instance.ToOneRow(dtBoxes, 1, "box_no");

                    int ret = uow.App300Repo.DeleteTdCheckStockByBoxNo(strBoxes);

                    ret = uow.App300Repo.InsertTdCheckStockBySelect("", "", strBoxes, userId);

                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(33, "M306R03");
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M306R03", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M306R03", "Catch Exception");
            }
        }
    
        public string PrintPCLOR()
        {
            try
            {
                DataTable dataPcl = uow.App300Repo.GetPlcRecorded();

                if (dataPcl.Rows.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(11);
                }

                foreach (DataRow dr in dataPcl.Rows)
                {
                    bool IsBoxNoRecordedFI = false;
                    bool IsBoxNoReceived = false;
                    List<PclOring> printPclList = new List<PclOring>();

                    DataTable dt = uow.App300Repo.GetAllBoxByPclNoOR(dr["pcl_no"].ToString());

                    string boxTemp = string.Empty;
                    int index = 1;
                    //data box trong box_delivery
                    DataTable dtBoxDelivery = uow.App300Repo.GetCountBoxInTdBoxDelivery(dr["pcl_no"].ToString());
                    //data box trong tt_pcl_print
                    DataTable dtPclPrint = uow.App300Repo.GetCountBoxInTtPclPrint(dr["pcl_no"].ToString());

                    //kiem tra so box trong 2 bang co bang nhau khong
                    if (dtBoxDelivery.Rows.Count != dtPclPrint.Rows.Count)
                        continue;

                    foreach (DataRow dtRow in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(boxTemp))
                        {
                            int ret = string.Compare(boxTemp, dtRow["box_no"].ToString());

                            if (ret != 0) //neu 2 box khac nhau
                            {
                                index += 1;

                                //check box da FI chua
                                IsBoxNoRecordedFI = uow.App300Repo.IsBoxNoRecoredFI(dtRow["box_no"].ToString());

                                if (!IsBoxNoRecordedFI)
                                {
                                    return AppCommon.ShowErrorMsg(30, "", dtRow["box_no"].ToString());
                                }

                                //check PCL da nhan hang vao kho chua
                                IsBoxNoReceived = uow.App300Repo.IsBoxNoReceived(dtRow["box_no"].ToString());

                                if (!IsBoxNoReceived)
                                {
                                    return AppCommon.ShowErrorMsg(31);
                                }
                            }

                        }

                        PclOring eachRow = new PclOring()
                        {
                            Item = dtRow["item"] == null ? "" : dtRow["item"].ToString(),
                            JobOrderNo = dtRow["job_order_no"] == null ? "" : dtRow["job_order_no"].ToString(),
                            LotNo = dtRow["lot_no"] == null ? "" : dtRow["lot_no"].ToString(),
                            Wc = dtRow["wc"] == null ? "" : dtRow["wc"].ToString(),
                            Qty = dtRow["qty"] == null ? 0 : Convert.ToInt32(dtRow["qty"].ToString()),
                            MocQc = dtRow["moc_qc"] == null ? "" : dtRow["moc_qc"].ToString(),
                            BoxNum = dtRow["box_num"] == null ? "" : dtRow["box_num"].ToString(),
                            IssueDate = dtRow["waiting_wh_date"] == null ? "" : dtRow["waiting_wh_date"].ToString().Substring(6, 2) + "/" + dtRow["waiting_wh_date"].ToString().Substring(4, 2) + "/" + dtRow["waiting_wh_date"].ToString().Substring(2, 2),
                            IssueBy = dtRow["waiting_wh_user"] == null ? "" : dtRow["waiting_wh_user"].ToString(),
                            IssueTime = dtRow["waiting_wh_time"] == null ? "" : dtRow["waiting_wh_time"].ToString().Substring(0, 2) + ":" + dtRow["waiting_wh_time"].ToString().Substring(2, 2) + ":" + dtRow["waiting_wh_time"].ToString().Substring(4, 2),
                            PclNo = dtRow["pcl_no"] == null ? "" : dtRow["pcl_no"].ToString(),
                            BoxNo = dtRow["box_no"] == null ? "" : dtRow["box_no"].ToString(),
                            Index = index
                        };

                        boxTemp = eachRow.BoxNo;

                        printPclList.Add(eachRow);
                    }

                    if(printPclList.Count > 0)
                        Printer<PclOring>.PrintData("PCLORING", printPclList);
                }

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M300", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M300");
            }

        }

        public int checkFiFoForShip(string shippingTos)
        {
            int checkFiFo = 0;
            int ret = 999;
            try
            {
                DataTable maxLotByItemOfShippingReserved = uow.App300Repo.OS1GetALLDataReservedPallete(shippingTos);

                if (maxLotByItemOfShippingReserved.Rows.Count <= 0)
                    return 42;

                foreach (DataRow dr in maxLotByItemOfShippingReserved.Rows) //reserved
                {
                    DataTable maxLotReceivedInStockByItem = uow.App300Repo.OS1GetAllDataReceivedInStockNotInPallete(dr["item"].ToString());

                    if (maxLotReceivedInStockByItem.Rows.Count > 0)
                    {
                        foreach (DataRow drCheckFiFo in maxLotReceivedInStockByItem.Rows) //incoming stock in, waiting for sort in pallete
                        {
                            if (dr["entry_date"].ToString().Contains(drCheckFiFo["entry_date"].ToString()) == false)
                            {
                                checkFiFo = AppCommon.CheckFifo(drCheckFiFo["lot_no"].ToString(), dr["lot_no"].ToString());
                                if (checkFiFo < 0) //lot tren pallete < lot dang cho chuan bi len pallete
                                {
                                    //insert or update fifof
                                    ret = uow.App300Repo.InsertOrUpdateFifo(shippingTos, drCheckFiFo["pallete_no"].ToString(), dr["lot_no"].ToString(), drCheckFiFo["lot_no"].ToString(), dr["item"].ToString(), 1, 0);

                                    if (ret != 0)
                                        uow.Rollback();

                                    uow.Commit();
                                    return ret;

                                }
                            }
                        }
                    }

                    DataTable dtItemNotInPallete = uow.App300Repo.OS1GetItemNotInPalleteAndNotShipByDate(dr["item"].ToString(), shippingTos, dr["entry_date"].ToString());

                    if (dtItemNotInPallete.Rows.Count > 0)
                    {
                        foreach (DataRow drCheck in dtItemNotInPallete.Rows)
                        {
                            if (shippingTos.Contains(drCheck["pallete_no"].ToString()) == false || string.IsNullOrEmpty(drCheck["pallete_no"].ToString()))
                            {
                                if (dr["entry_date"].ToString().Contains(drCheck["entry_date"].ToString()) == false)
                                {
                                    checkFiFo = AppCommon.CheckFifo(drCheck["lot_no"].ToString(), dr["lot_no"].ToString());
                                    if (checkFiFo < 0) //lot tren pallete < lot dang cho chuan bi len pallete
                                    {
                                        //insert or update fifo
                                        ret = uow.App300Repo.InsertOrUpdateFifo(shippingTos, drCheck["pallete_no"].ToString(), dr["lot_no"].ToString(), drCheck["lot_no"].ToString(), dr["item"].ToString(), 3, 3);

                                        if (ret != 0)
                                            uow.Rollback();

                                        uow.Commit();

                                        return ret;
                                    }
                                }
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "CHECKFIFOSHIP", sw);
                }

                uow.Rollback();
                return ret = 87;
            }

            return ret;
        }

        /// <summary>
        /// M305OS1
        /// </summary>
        /// <param name="listShippingTo"></param>
        /// <returns></returns>
        public string ShipOS(string location, string userId, List<string> listShippingTo)
        {
            try
            {
                int ret = 0;
                int flag = 0;

                string shippingTos = Utility.Instance.ToOneRow(listShippingTo);

                ret = checkFiFoForShip(shippingTos);

                if (ret == 0)
                {
                    return AppCommon.ShowErrorMsg(46);
                }
                else if (ret > 0 && ret < 999)
                {
                    return AppCommon.ShowErrorMsg(ret);
                }

                for (int i = 0; i < listShippingTo.Count; i++)
                {
                    string stock = string.Empty;
                    string palleteTypeNo = string.Empty;
                    string palleteNo = string.Empty;
                    string deliveryPlace = string.Empty;
                    string numberShipSeq = string.Empty;
                    string countSeq = string.Empty;

                    int shipSeq;
                    int seqNo = 0;

                    DataTable dtEachShippingTo = uow.App300Repo.OS1GetDataPallete(listShippingTo[i]);

                    if (dtEachShippingTo.Rows.Count <= 0)
                        return AppCommon.ShowErrorMsg(52);

                    //20190410 check null this properties
                    seqNo = Convert.ToInt32(dtEachShippingTo.Rows[0]["seq_no"] == null || dtEachShippingTo.Rows[0]["seq_no"].ToString() == ""? "0": dtEachShippingTo.Rows[0]["seq_no"].ToString());
                    deliveryPlace = string.Concat(dtEachShippingTo.Rows[0]["symbol"], "-", seqNo);

                    DataTable dtGetSeq = uow.App300Repo.GetSeq("SOS1", DateTime.Now.ToString("yyyyMMdd"));

                    if (flag > 0)
                    {
                        ret = uow.App300Repo.GetSeqForAll(1, "td_shipping_seq_os"); //mode 1: tang sequence
                    }
                    else
                    {
                        if (dtGetSeq.Rows.Count <= 0)
                        {
                            ret = uow.App300Repo.GetSeqForAll(0, "td_shipping_seq_os"); //mode 0: reset
                                                                                        //ret = 100; // test, khi xong thi thay doi ret theo thu tu tang dan
                            flag = 1;
                        }
                        else
                        {
                            ret = uow.App300Repo.GetSeqForAll(1, "td_shipping_seq_os"); //mode 1: tang sequence
                        }
                    }

                    countSeq = String.Format("{0:000}", ret);

                    numberShipSeq = string.Concat("SOS1", DateTime.Now.ToString("yyyyMMdd"), countSeq);

                    DataTable dtTdBoxInfo = uow.App300Repo.OS1GetShippingToInTdBoxInfo(listShippingTo[i]);

                    if (dtTdBoxInfo.Rows.Count < 0)
                    {
                        return AppCommon.ShowErrorMsg(42, "ShipOS");
                    }

                    for (int index = 0; index < dtTdBoxInfo.Rows.Count; index++)
                    {
                        string valuesInsert = string.Empty;
                        string valuesUpdate = string.Empty;

                        DataTable dtBoxJobTag = uow.App300Repo.GetBoxJobTagByBoxInShippingTo(dtTdBoxInfo.Rows[index]["box_no"].ToString());

                        if (dtBoxJobTag.Rows.Count < 0)
                        {
                            return AppCommon.ShowErrorMsg(38, "ShipOS");
                        }
                        //20190410 check null those properties
                        shipSeq = Convert.ToInt32(dtTdBoxInfo.Rows[0]["ship_seq"] == null || dtTdBoxInfo.Rows[0]["ship_seq"].ToString() == "" ? "0" : dtTdBoxInfo.Rows[0]["ship_seq"].ToString());
                        stock = dtTdBoxInfo.Rows[0]["stock"].ToString() == null ? "" : dtTdBoxInfo.Rows[0]["stock"].ToString();
                        palleteTypeNo = dtTdBoxInfo.Rows[0]["palletetype_no"].ToString() == null ? "" : dtTdBoxInfo.Rows[0]["palletetype_no"].ToString();
                        palleteNo = dtTdBoxInfo.Rows[0]["pallete_no"].ToString() == null ? "" : dtTdBoxInfo.Rows[0]["pallete_no"].ToString();

                        foreach (DataRow dr in dtBoxJobTag.Rows)
                        {
                            //ENTRY_DATE, ENTRY_USER, SHIPPING_NO, BOX_NO, ITEM, QTY_BOX, LOT_NO, JOB_NO, SUFFIX, BOX_SEQ, PARENTPALLETE, PALLETE_NO, LOC, DELIVERY_PLACE

                            valuesInsert += string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, {9}, '{10}', '{11}', '{12}', '{13}'),", DateTime.Now.ToString("yyyyMMdd"), userId, numberShipSeq, dr["box_no"].ToString(), dr["item"].ToString(), dr["qty"], dr["lot_no"].ToString(), dr["job_no"].ToString(), dr["suffix"], dr["box_seq"] == null ? "0" : dr["box_seq"].ToString(), palleteTypeNo, palleteNo, stock, deliveryPlace);

                            valuesUpdate += string.Format("UPDATE TD_CHECK_STOCK SET status = 1 where box_no = '{0}' and job_no = '{1}' and suffix = '{2}' and lot_no = '{3}' ;", dr["box_no"].ToString(), dr["job_no"].ToString(), dr["suffix"], dr["lot_no"].ToString());
                        }

                        if (!string.IsNullOrEmpty(valuesInsert))
                        {
                            valuesInsert = valuesInsert.Substring(0, valuesInsert.LastIndexOf(","));

                            ret = uow.App300Repo.InsertTtShippingPrint(valuesInsert);

                            if (ret <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(54, "ShipOS");
                            }

                            ret = uow.App300Repo.UpdTdCheckStock(valuesUpdate);

                            if (ret < 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(79, "ShipOS");
                            }
                        }
                    }

                    //update box_info

                    ret = uow.App300Repo.UpdTdBoxInfo(numberShipSeq, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), userId, listShippingTo[i]);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(56, "ShipOS");
                    }

                    //insert box trace send to AS400
                    ret = uow.App300Repo.InsertBoxTraceBySelect(listShippingTo[i]);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(57, "ShipOS");
                    }
                }

                uow.Commit();
                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "numberShipSeq", sw);
                }

                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M305OS1");
            }
        }

        public string ShipOR(string location, string userId, List<string> listShippingTo)
        {
            try
            {
                int ret = 0, flag = 0;
                string shippingTos = "'" + listShippingTo.Aggregate((a, b) => a + "', '" + b) + "'";

                DataTable dtTdBoxInfo = uow.App300Repo.ORGetDataPalleteInBoxInfo(shippingTos);

                if (dtTdBoxInfo.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(42);
                }
                //20190627 add var to check fifo
                string boxIsInSertFiFo = string.Empty;
                string curBox = string.Empty;
                bool flagFiFo = false;

                for (int i = 0; i < listShippingTo.Count; i++)
                {
                    string deliveryPlace = string.Empty;
                    string numberShipSeq = string.Empty;
                    string countSeq = string.Empty;
                    string stock = string.Empty;
                    string palleteTypeNo = string.Empty;
                    string palleteNo = string.Empty;
                    int shipSeq;     

                    DataTable dtTdPalleteWh = uow.App300Repo.ORGetDataPallete(listShippingTo[i]);

                    if (dtTdPalleteWh.Rows.Count <= 0)
                    {
                        return AppCommon.ShowErrorMsg(36, "ShipOR");
                    }

                    deliveryPlace = string.Concat(dtTdPalleteWh.Rows[0]["symbol"], "-", dtTdPalleteWh.Rows[0]["seq_no"]);

                    DataTable dtGetSeq = uow.App300Repo.GetSeq("SOR1", DateTime.Now.ToString("yyyyMMdd"));

                    if (flag > 0)
                    {
                        ret = uow.App300Repo.GetSeqForAll(1, "td_shipping_seq_or"); //mode 1: tang sequence
                    }
                    else
                    {
                        if (dtGetSeq.Rows.Count <= 0)
                        {
                            ret = uow.App300Repo.GetSeqForAll(0, "td_shipping_seq_or"); //mode 0: reset
                                                                                        //ret = 100; // test, khi xong thi thay doi ret theo thu tu tang dan
                            flag = 1;
                        }
                        else
                        {
                            ret = uow.App300Repo.GetSeqForAll(1, "td_shipping_seq_or"); //mode 1: tang sequence
                        }
                    }

                    countSeq = String.Format("{0:000}", ret);

                    numberShipSeq = string.Concat("SOR1", DateTime.Now.ToString("yyyyMMdd"), countSeq);

                    DataTable dtTdBoxInfoCheck = uow.App300Repo.ORGetDataByPallete(listShippingTo[i]);

                    if (dtTdBoxInfoCheck.Rows.Count <= 0)
                    {
                        return AppCommon.ShowErrorMsg(42, "ShipOR");
                    }


                    
                    for (int index = 0; index < dtTdBoxInfoCheck.Rows.Count; index++)
                    {
                        string valuesInsert = string.Empty;
                        string valuesUpdate = string.Empty;

                        //20190626 check fifo
                        string boxno = dtTdBoxInfoCheck.Rows[index]["box_no"].ToString();
                        string item = dtTdBoxInfoCheck.Rows[index]["item"].ToString();
                        //1. Check fifo voi hang da ship - lot cua boxno >= lot cua item da ship
                        if (uow.App300Repo.CheckFiFoScanStoringOR(boxno, item))
                        {
                            //insert vao td_box_fifo
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(46, "M305OR1", boxno);
                        }

                        //2. Lay min & max lot cua boxno
                        DataTable boxMinMaxLot = uow.App300Repo.GetMinAndMaxLotOfBoxOR(boxno);
                        if(boxMinMaxLot.Rows.Count <= 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(38, "M305OR1", boxno);
                        }
                        string boxMinLot = boxMinMaxLot.Rows[0]["min_lot"].ToString();
                        string boxMaxLot = boxMinMaxLot.Rows[0]["max_lot"].ToString();

                        //3. max lot cua boxno <= min lot cua item da nhan hang chua sx
                        string minLotItemIsNotStored = string.Empty;
                        boxIsInSertFiFo = string.Empty;
                        DataTable dtTemp = uow.App300Repo.MinLotOfItemIsNotStored(item);
                        if(dtTemp.Rows.Count > 0)
                        {
                            minLotItemIsNotStored = dtTemp.Rows[0]["lot_no"].ToString();
                            boxIsInSertFiFo = dtTemp.Rows[0]["box_no"].ToString();
                        }

                        if (!string.IsNullOrEmpty(minLotItemIsNotStored) && string.Compare(boxMaxLot, minLotItemIsNotStored) > 0)
                        {
                            flagFiFo = true;
                            curBox = boxno;
                            uow.Rollback();
                            break;
                            //return AppCommon.ShowErrorMsg(46, "M305OR1", boxno);
                        }

                        //4. max lot cua boxno <= min lot cua item da sx
                        string minLotItemIsStored = string.Empty;
                        dtTemp = uow.App300Repo.MinLotOfItemIsStored(item, listShippingTo[i]);
                        if (dtTemp.Rows.Count > 0)
                        {
                            minLotItemIsStored = dtTemp.Rows[0]["lot_no"].ToString();
                            boxIsInSertFiFo = dtTemp.Rows[0]["box_no"].ToString();
                        }
                        
                        if (!string.IsNullOrEmpty(minLotItemIsStored) && string.Compare(boxMaxLot, minLotItemIsStored) > 0)
                        {
                            flagFiFo = true;
                            curBox = boxno;
                            uow.Rollback();
                            break;
                            //return AppCommon.ShowErrorMsg(46, "M305OR1", boxno);
                        }

                        //20190626 end

                        DataTable dtBoxJobTag = uow.App300Repo.GetBoxJobTagByBoxInShippingTo(dtTdBoxInfoCheck.Rows[index]["box_no"].ToString());

                        if (dtBoxJobTag.Rows.Count < 0)
                        {
                            return AppCommon.ShowErrorMsg(38, "ShipOR");
                        }

                        shipSeq = Convert.ToInt32(dtTdBoxInfo.Rows[i]["ship_seq"]);
                        stock = dtTdBoxInfo.Rows[i]["stock"].ToString();
                        palleteTypeNo = dtTdBoxInfo.Rows[i]["palletetype_no"].ToString();
                        palleteNo = dtTdBoxInfo.Rows[i]["pallete_no"].ToString();

                        foreach (DataRow dr in dtBoxJobTag.Rows)
                        {
                            //ENTRY_DATE, ENTRY_USER, SHIPPING_NO, BOX_NO, ITEM, QTY_BOX, LOT_NO, JOB_NO, SUFFIX, BOX_SEQ, PARENTPALLETE, PALLETE_NO, LOC, DELIVERY_PLACE
                            valuesInsert += string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, {9}, '{10}', '{11}', '{12}', '{13}'),", DateTime.Now.ToString("yyyyMMdd"), userId, numberShipSeq, dr["box_no"].ToString(), dr["item"].ToString(), dr["qty"], dr["lot_no"].ToString(), dr["job_no"].ToString(), dr["suffix"], dr["box_seq"], palleteTypeNo, palleteNo, stock, deliveryPlace);

                            valuesUpdate += string.Format("UPDATE TD_CHECK_STOCK SET status = 1 where box_no = '{0}' and job_no = '{1}' and suffix = '{2}' and lot_no = '{3}';", dr["box_no"].ToString(), dr["job_no"].ToString(), dr["suffix"], dr["lot_no"].ToString());
                        }

                        if (!string.IsNullOrEmpty(valuesInsert))
                        {
                            valuesInsert = valuesInsert.Substring(0, valuesInsert.LastIndexOf(","));

                            ret = uow.App300Repo.InsertTtShippingPrint(valuesInsert);

                            if (ret <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(54, "ShipOR");
                            }

                            ret = uow.App300Repo.UpdTdCheckStock(valuesUpdate);

                            if (ret < 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(79, "ShipOR");
                            }

                        }
                    }
                    //20190627 add check fifo
                    if (flagFiFo)
                        break;
                    //end

                    //update box_info
                    ret = uow.App300Repo.UpdTdBoxInfoOR(numberShipSeq, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), userId, listShippingTo[i]);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(56, "ShipOR");
                    }

                    //insert box trace send to AS400
                    ret = uow.App300Repo.InsertBoxTraceBySelect(listShippingTo[i]);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(57, "ShipOR");
                    }
                }
                //20190627 check fifo
                if(flagFiFo)
                {
                    bool result = uow.App300Repo.InsertOrUpdateBoxFiFo(curBox, boxIsInSertFiFo);
                    if(!result)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(88, "ShipOR");
                    }
                    uow.Commit();
                    //lay du lieu tra ve tay scan 
                    //yeu cau Item - Lot
                    DataTable temp = uow.App300Repo.GetMinAndMaxLotOfBoxOR(curBox);
                    string item = temp.Rows[0]["item"].ToString().Trim();
                    string lotNo = temp.Rows[0]["max_lot"].ToString();
                    string strConcat = item.PadRight(20) + lotNo.PadRight(20);
                    return AppCommon.ShowErrorMsg(46, "M305OR1", strConcat);
                }
                //end
                uow.Commit();
                return AppCommon.ShowOkMsg(0);
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M305OS1", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M305OS1");

            }
        }

        public string InsOrUpFF(string palleteNo, string palleteNoff, string lot, string lotFf, string item, int status, int types)
        {
            try
            {
                DataTable dtCheck = new DataTable();
                string strWhere = "", boxNoFf, boxNoTemp, strSet;
                string date = DateTime.Now.ToString("yyyyMMdd");
                string time = DateTime.Now.ToString("hhmmss");

                strWhere = string.Format("bi.pallete_no in ({0}) and bj.item='{1}' and bj.lot_no='{2}'", palleteNo, item, lot);
                dtCheck = uow.App300Repo.SelectBoxTop1(strWhere);

                if (dtCheck.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(39);
                }

                boxNoTemp = dtCheck.Rows[0]["box_no"].ToString();

                switch (types)
                {
                    case 0:
                        strWhere = string.Format("item='{0}' and income_status='1' and store_status='0' and entry_date >= (TO_CHAR(CURRENT_DATE - INTERVAL '1 Years', 'YYYY') || '0101')", item);
                        break;
                    case 1:
                        strWhere = string.Format("bi.pallete_no='{0}' and bj.item='{1}' and bj.lot_no='{2}' and bi.status<= 1 ", palleteNoff, item, lotFf);
                        break;
                    case 2:
                        strWhere = string.Format("bi.pallete_no='{0}' and bj.item='{1}' and bj.lot_no='{2}' and bi.status>=3", palleteNoff, item, lotFf);
                        break;
                    case 3:
                        strWhere = string.Format("bi.pallete_no='{0}' and bj.item='{1}' and bj.lot_no='{2}' and bi.status<4", palleteNoff, item, lotFf);
                        break;
                }

                if (types > 0)
                {
                    dtCheck = uow.App300Repo.SelectBoxTop1(strWhere);

                    if (dtCheck.Rows.Count <= 0)
                    {
                        return AppCommon.ShowErrorMsg(39);
                    }

                    boxNoFf = dtCheck.Rows[0]["box_no"].ToString();
                }
                else
                {
                    dtCheck = uow.App300Repo.SelectBoxDelivery(strWhere);

                    if (dtCheck.Rows.Count <= 0)
                    {
                        return AppCommon.ShowErrorMsg(39);
                    }

                    boxNoFf = dtCheck.Rows[0]["box_no"].ToString();
                }


                strWhere = string.Format("box_no = '{0}' and box_no_ff = '{1}' and status = {2}", boxNoTemp, boxNoFf, status);
                dtCheck = uow.App300Repo.SelectBoxFifo(strWhere);

                if (dtCheck.Rows.Count > 0)
                {
                    //update
                    strSet = string.Format("update_date = '{0}', update_time = '{1}'", date, time);
                    strWhere = string.Format("box_no= '{0}' and box_no_ff='{1}'", boxNoTemp, boxNoFf);

                    int ret = uow.App300Repo.UpdBoxFifo(strSet, strWhere);

                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(41);
                    }
                    uow.Commit();
                }
                else
                {
                    //insert
                    int ret = uow.App300Repo.InsertBoxFifo(boxNoTemp, boxNoFf, "", status, date, time, types);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(41);
                    }
                    uow.Commit();

                }

                return "0";
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M304OS1", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M304OS1");
            }
        }

        //==CheckFiFoBoxInPallete==
        public string CheckFiFoBoxInPallete(string arrPallete)
        {
            try
            {
                DataTable dtCheck = new DataTable();
                DataTable dtCheck2 = new DataTable();
                int ret;

                dtCheck = uow.App300Repo.SelectGroupItem_BJBI(arrPallete);

                if (dtCheck.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(38);
                }

                foreach (DataRow dr in dtCheck.Rows)
                {
                    dtCheck2 = uow.App300Repo.SelectWithoutFrom(dr["item"].ToString());
                    if (dtCheck2.Rows.Count > 0)
                    {
                        //so sanh
                        foreach (DataRow dr2 in dtCheck2.Rows)
                        {
                            if (string.Compare(dr["entry_date"].ToString(), dr2["entry_date"].ToString()) != 0)
                            {
                                ret = AppCommon.CheckFifo(dr2["lot_no"].ToString(), dr["lot_no"].ToString());

                                if (ret > 0)
                                {
                                    //InsOrUpFF
                                    if (InsOrUpFF(arrPallete, dr2["pallete_no"].ToString(), dr["lot_no"].ToString(), dr2["lot_no"].ToString(), dr["item"].ToString(), 1, 0) != "0")
                                    {
                                        return AppCommon.ShowErrorMsg(38);
                                    }
                                    //cff++;
                                    return AppCommon.ShowErrorMsg(46);
                                }
                            }
                        }
                    }

                    //select lot info
                    //kiem tra lan 1
                    dtCheck2 = uow.App300Repo.SelectLotInfo(dr["item"].ToString(), arrPallete, dr["entry_date"].ToString(), 1);
                    if (dtCheck2.Rows.Count > 0)
                    {
                        //so sanh
                        foreach (DataRow dr2 in dtCheck2.Rows)
                        {
                            if (arrPallete.Contains(dr2["pallete_no"].ToString()) == false || string.IsNullOrEmpty(dr2["pallete_no"].ToString()))
                            {
                                if (string.Compare(dr["entry_date"].ToString(), dr2["entry_date"].ToString()) != 0)
                                {
                                    ret = AppCommon.CheckFifo(dr2["lot_no"].ToString(), dr["lot_no"].ToString());

                                    if (ret > 0)
                                    {
                                        //InsOrUpFF
                                        if (InsOrUpFF(arrPallete, dr2["pallete_no"].ToString(), dr["lot_no"].ToString(), dr2["lot_no"].ToString(), dr["item"].ToString(), 1, 1) != "0")
                                        {
                                            return AppCommon.ShowErrorMsg(38);
                                        }
                                        //cff++;
                                        return AppCommon.ShowErrorMsg(46);
                                    }
                                }
                            }
                        }
                    }

                    //kiem tra lan 2
                    dtCheck2 = uow.App300Repo.SelectLotInfo(dr["item"].ToString(), arrPallete, dr["entry_date"].ToString(), 2);
                    if (dtCheck2.Rows.Count > 0)
                    {
                        //so sanh
                        foreach (DataRow dr2 in dtCheck2.Rows)
                        {
                            if (arrPallete.Contains(dr2["pallete_no"].ToString()) == false)
                            {
                                if (string.Compare(dr["entry_date"].ToString(), dr2["entry_date"].ToString()) != 0)
                                {
                                    ret = AppCommon.CheckFifo(dr2["lot_no"].ToString(), dr["lot_no"].ToString());

                                    if (ret > 0)
                                    {
                                        //InsOrUpFF
                                        if (InsOrUpFF(arrPallete, dr2["pallete_no"].ToString(), dr["lot_no"].ToString(), dr2["lot_no"].ToString(), dr["item"].ToString(), 1, 2) != "0")
                                        {
                                            return AppCommon.ShowErrorMsg(38);
                                        }
                                        //cff++;
                                        return AppCommon.ShowErrorMsg(46);
                                    }
                                }
                            }
                        }
                    }   //end if (dtCheck2.Rows.Count > 0) kiem tra lan 2
                }
                return "0"; //ket qua ok
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M304OS1", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M304OS1");
            }
        }

        public string CheckFifoForReser(string arrPallete)
        {
            try
            {
                DataTable dtGetMaxLotOfItem = uow.App300Repo.SelectGroupItem_BJBI(arrPallete);
                if (dtGetMaxLotOfItem.Rows.Count <= 0)
                    return AppCommon.ShowErrorMsg(38);

               
                foreach(DataRow row in dtGetMaxLotOfItem.Rows)
                {
                    string item = row["item"].ToString().Trim();
                    string entryDate = row["entry_date"].ToString().Trim();
                    string lotNo = row["lot_no"].ToString().Trim();

                    //Kiem tra voi ma hang dang o trang thai nhan hang (Chua scan sap xep)
                    DataTable dtItemOnReceive = uow.App300Repo.GetMaxLotByItemOnReceive(item);

                    foreach(DataRow row2 in dtItemOnReceive.Rows)
                    {
                        string entryDate2 = row["entry_date"].ToString().Trim();
                        string lotNo2 = row["lot_no"].ToString().Trim();

                        if (!entryDate.Equals(entryDate2))
                        {
                            //lotNo = Lot cua thung can reser
                            //lotNo2 =  Lot cua nhung thung chua scan sap xep
                            //lotNo < lotNo2: Lot cua thung can reser phai nho hon tat ca cac lot da duoc nhap kho.
                            if (lotNo.CompareTo(lotNo2) < 0)
                            {
                                if (InsOrUpFF(arrPallete, row2["pallete_no"].ToString(), lotNo, lotNo2, item, 1, 0) != "0")
                                    return AppCommon.ShowErrorMsg(88);
                              
                                return AppCommon.ShowErrorMsg(46);
                            }
                        }
                    }

                    //Kiem tra voi ma hang da duoc sap xep hoac da duoc in shipping to
                    DataTable dtItemReceivedOrArrangedToPallete = uow.App300Repo.SelectLotInfo(item, arrPallete, entryDate, 1);

                    foreach(DataRow row3 in dtItemReceivedOrArrangedToPallete.Rows)
                    {
                        string palleteNo3 = row3["pallete_no"] == null ? "" : row3["pallete_no"].ToString();
                        string entryDate3 = row3["entry_date"].ToString().Trim();
                        string lotNo3 = row3["lot_no"].ToString().Trim();

                        if (arrPallete.Contains(palleteNo3) || string.IsNullOrEmpty(palleteNo3))
                            continue;

                        if (!entryDate.Equals(entryDate3))
                        {
                            //lotNo = Lot cua thung can reser
                            //lotNo3 =  Lot cua nhung thung da duoc sx hoac da in shipping to

                            //lotNo < lotNo3: Lot cua thung can reser phai nho hon tat ca cac thung da sap xep hoac da in shipping to
                            if (lotNo.CompareTo(lotNo3) > 0)
                            {
                                if (InsOrUpFF(arrPallete, palleteNo3, lotNo, lotNo3, item, 1, 1) != "0")
                                    return AppCommon.ShowErrorMsg(88);

                                return AppCommon.ShowErrorMsg(46);
                            }
                        }
                    }

                    //Kiem tra voi ma hang da duoc scan reser hoac scan ship
                    DataTable dtItemReservedOrShipped = uow.App300Repo.SelectLotInfo(item, arrPallete, entryDate, 2);
                    foreach(DataRow row4 in dtItemReservedOrShipped.Rows)
                    {
                        string palleteNo4 = row4["pallete_no"] == null ? "" : row4["pallete_no"].ToString();
                        string entryDate4 = row4["entry_date"].ToString().Trim();
                        string lotNo4 = row4["lot_no"].ToString().Trim();

                        if (arrPallete.Contains(palleteNo4) || string.IsNullOrEmpty(palleteNo4))
                            continue;

                        if (!entryDate.Equals(entryDate4))
                        {
                            //lotNo = Lot cua thung can reser
                            //lotNo4 =  Lot cua nhung thung da duoc scan reser hoac scan ship
                            // lotNo < lotNo4: Lot cua thung can reser phai lon hon tat ca cac lot da duoc reser hoac ship
                            if (lotNo.CompareTo(lotNo4) < 0)
                            {
                                if (InsOrUpFF(arrPallete, palleteNo4, lotNo, lotNo4, item, 1, 2) != "0")
                                    return AppCommon.ShowErrorMsg(88);

                                return AppCommon.ShowErrorMsg(46);
                            }
                        }
                    }
                }
                return "0"; //ket qua ok
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M304OS1", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M304OS1");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Reservation(string userId, List<string> data)
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");
                string time = DateTime.Now.ToString("hhmmss");
                string arrPallete = Utility.Instance.ToOneRow(data);    //noi chuoi pallete thanh 1 dong de select
                DataTable dt = new DataTable();
                string strWhere, strSet;

                strWhere = string.Format(" pallete_no in ({0}) and status = 1", arrPallete);
                dt = uow.App300Repo.SelectBoxInfo(strWhere);

                if (dt.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(36);
                }

                //check fifo box in pallete
                //20190404 - khoa
                //if (CheckFiFoBoxInPallete(arrPallete) != "0")
                //{
                //    return CheckFiFoBoxInPallete(arrPallete);
                //}

                if (CheckFifoForReser(arrPallete) != "0")
                    return AppCommon.ShowErrorMsg(46);

                //update box info data to database
                strWhere = string.Format(" pallete_no in ({0}) and status = 1 order by box_no asc", arrPallete);
                dt = uow.App300Repo.SelectBoxInfo(strWhere);

                if (dt.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(47);
                }

                strSet = string.Format("status = 3 , reserved_user = '{0}', reserved_date = '{1}', reserved_time = '{2}'", userId, date, time);
                strWhere = string.Format("pallete_no in({0}) and Status=1", arrPallete);

                int ret = uow.App300Repo.UpdBoxInfo(strSet, strWhere);

                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(48);
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M304OS1", sw);
                } 

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M304OS1");
            }
        }

        //ham lay tat ca cac thung tren pallete
        public string GetTotalBoxOS(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Trim()))
                    return AppCommon.ShowErrorMsg(888, "M303SO1-OS-1");

                DataTable dtTotalBox = uow.App300Repo.GetTotalBoxOS(data.Trim());
                if (dtTotalBox.Rows.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(69);
                }

                //20190907 Kiem tra tren pallete co hang cho hay khong?
                //neu co khong cho in shipping to
                DataTable dt = uow.App300Repo.GetWaitingboxInPalleteType(data.Trim());
                if(dt.Rows.Count > 0)
                {
                    string mess = dt.Rows[0]["item"].ToString().Trim().PadRight(20) + dt.Rows[0]["lot_no"].ToString().Trim().PadRight(20);
                    return AppCommon.ShowErrorMsg(102, "M303SO1-OS-2", mess);
                }

                return Utility.Instance.ToOneRow(dtTotalBox);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M303SO1-OS", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M303SO1-OS");
            }
        }

        //ham lay  tat ca cac thung dua vao 1 thung bat ky
        public string GetTotalBoxOR(string data, string loc, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Trim()))
                    return AppCommon.ShowErrorMsg(888, "M303SO1 - OR");

                DataTable dtTotalPallete = uow.App300Repo.GetPalleteOR(data.Trim());
                if (dtTotalPallete.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M303SO1 - OR");
                }

                string palleteNo = dtTotalPallete.Rows[0]["pallete_no"].ToString().Trim();
                DataTable dtTotalBox = uow.App300Repo.GetTotalBoxOR(palleteNo);

                //20190702 Kiem tra so luong theo item so voi WH Plan OR truoc khi in shipping to 
                DataTable dtItemWHPlan = uow.App300Repo.GetBalanceAndItemInWHPlan(palleteNo);
                foreach(DataRow row in dtItemWHPlan.Rows)
                {
                    string itemCheck = row["item"].ToString();
                    double balanceWH = Convert.ToDouble(row["balance"].ToString());
                    double qtyActual = uow.App300Repo.GetQtyActualOfItemInPallete(palleteNo, itemCheck);
                    if (qtyActual != balanceWH)
                    {
                        return AppCommon.ShowErrorMsg(98, "M303SO1 - OR", itemCheck.PadRight(20));
                    }
                }

                //End 20190702

                if (dtTotalBox.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M303SO1 - OR");
                }

                string box_no = dtTotalBox.Rows[0]["box_no"] == null ? "" : dtTotalBox.Rows[0]["box_no"].ToString().Trim();

                //20191904 - Truong hop PCL chi co 1 box duy nhat thi chuyen qua in shipping to luon.
                //if (dtTotalBox.Rows.Count == 1 && !string.IsNullOrEmpty(box_no) && data.Equals(box_no))
                //{
                //    SetShippingToOR(loc, userId, data);
                //}

                return Utility.Instance.ToOneRow(dtTotalBox);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M303SO1-OR", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M303SO1 - OR");
            }
        }

        //check va luu du lieu shipping to OS
        public string SetShippingToOS(string Loc, string UserId, List<string> data)
        {
            string dateEDTVN, dateEDTHCM, palleteNo, symbol, typeName, detail, groupsName, item;
            string PalletecodeH, cPalleteNo, date, year, time;
            int lanXuat, groups, status, ret, seqNo = 0, seqNo1 = 0, seqNo2 = 0;

            try
            {
                year = DateTime.Now.ToString("yyyy");
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("HHmmss");

                dateEDTVN = data[0].ToString().Substring(0, 8);
                dateEDTHCM = data[0].ToString().Substring(8, 8);
                palleteNo = data[1].ToString();
                lanXuat = Convert.ToInt32(data[2].ToString());

                data.RemoveRange(0, 3);

                //Kiem tra tat ca cac thung co tren pallete khong, lay item
                DataTable dtBoxInfo = uow.App300Repo.GetCheckBoxInfoOS(palleteNo, data[data.Count - 1]);
                if (dtBoxInfo.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(47, "M303-OS", data[0].ToString().Trim());
                }

                item = dtBoxInfo.Rows[0]["item"].ToString().Trim();

                //du vao pallete lay ma khach hang
                DataTable dtPalleteWH = uow.App300Repo.GetCheckPallete(palleteNo);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M303-OS", "ShippingToOS");
                }

                symbol = dtPalleteWH.Rows[0]["symbol"].ToString().Trim();
                typeName = dtPalleteWH.Rows[0]["name_type"].ToString().Trim();
                //lay ten khac hang
                DataTable dtMaterSymbolWH = uow.App300Repo.GetCheckMaterSymbol(symbol);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(52, "M303-OS", "ShippingToOS");
                }

                detail = dtMaterSymbolWH.Rows[0]["detail"].ToString().Trim();
                groups = Convert.ToInt32(dtMaterSymbolWH.Rows[0]["groups"].ToString().Trim());
                groupsName = dtMaterSymbolWH.Rows[0]["group_name"].ToString().Trim();
                status = Convert.ToInt32(dtMaterSymbolWH.Rows[0]["status"].ToString().Trim());

                //so luong pallete di trong 1 nam
                DataTable dtCtrlGroupSeq = uow.App300Repo.GetCheckCtrlGroupSeq(groups, year);

                if (dtCtrlGroupSeq.Rows.Count == 0)
                {
                    ret = uow.App300Repo.SetInsertCtrlGroupSeq(groups, groupsName, year, 1, Loc);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(62, "M303-OS", "ShippingToOS");
                    }
                }
                else
                {
                    seqNo = Convert.ToInt32(dtCtrlGroupSeq.Rows[0]["seq_no"].ToString().Trim()) + 1;
                    ret = uow.App300Repo.SetUpdateCtrlGroupSeq(groups, year, seqNo, Loc);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(63, "M303-OS", "ShippingToOS");
                    }
                }

                cPalleteNo = "T" + Loc;
                //insert so lan tao shipping to
                DataTable dtRecCodeWH = uow.App300Repo.GetRecCodeWH(cPalleteNo, date);

                if (dtRecCodeWH.Rows.Count == 0)
                {
                    ret = uow.App300Repo.SetResetSeq("td_shipping_to_seq_os");
                    DataTable dtSetSeq = uow.App300Repo.GetSeq("td_shipping_to_seq_os");
                    seqNo1 = Convert.ToInt32(dtSetSeq.Rows[0]["nextval"].ToString().Trim());
                    ret = uow.App300Repo.SetInsertRecCodeWH(cPalleteNo, date, 0, date);

                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(64, "M303-OS", "ShippingToOS");
                    }
                }
                else
                {
                    DataTable dtSetSeq = uow.App300Repo.GetSeq("td_shipping_to_seq_os");
                    seqNo1 = Convert.ToInt32(dtSetSeq.Rows[0]["nextval"].ToString().Trim());
                    //ret = uow.App300Repo.SetUpdateRecCodeWH(seqNo1, cPalleteNo, date);
                    //if (ret < 0)
                    //{
                    //    uow.Rollback();
                    //    return AppCommon.ShowErrorMsg(65, "M303-OS", "ShippingToOS");
                    //}
                }

                PalletecodeH = cPalleteNo + date + String.Format("{0:000}", seqNo1);

                foreach (string box in data)
                {
                    ret = uow.App300Repo.SetUpdateBoxInfoWH(PalletecodeH, date, time, UserId, lanXuat, dateEDTHCM, dateEDTVN, palleteNo, box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M303-OS", box);
                    }
                }

                // inset td_pallete_wh 
                ret = uow.App300Repo.SetInsertPalleteWH(PalletecodeH, palleteNo, item, seqNo, symbol, "0", date, UserId);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(67, "M303-OS", palleteNo);
                }

                //insert so lan in shipping to
                DataTable dtMaxSeqPrint = uow.App300Repo.GetMaxPalletePrint(PalletecodeH);
                if (dtMaxSeqPrint.Rows.Count <= 0)
                {
                    seqNo2 = 1;
                }
                else
                {
                    seqNo2 = Convert.ToInt32(dtMaxSeqPrint.Rows[0]["seq_no"].ToString().Trim()) + 1;
                }

                ret = uow.App300Repo.SetInsertPalletePrint(PalletecodeH, seqNo2, date, time);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(68, "M303-OS", "ShippingToOS");
                }

                uow.Commit();

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(PalletecodeH, 0, Loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M303 - OS fn: PrintSMAndST");
                //In shipping to
                PrintSMAndST(PalletecodeH, Loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M303-OS", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M303-OS", "ShippingToOS");
            }
        }
        //luu du lieu shipping to OR
        public string SetShippingToOR(string Loc, string UserId, string data)
        {
            string   palleteNo, symbol, typeName, detail, groupsName, item;
            string PalletecodeH, cPalleteNo, date, year, time;
            int groups, status, ret, seqNo = 0, seqNo1 = 0;

            try
            {
                year = DateTime.Now.ToString("yyyy");
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("HHmmss");

                DataTable dtTotalPallete = uow.App300Repo.GetPalleteOR(data);
                if (dtTotalPallete.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M303-OR", "ShippingToOR");
                }
                PalletecodeH = dtTotalPallete.Rows[0]["pallete_no"].ToString().Trim();
                item = dtTotalPallete.Rows[0]["item"].ToString().Trim();
                palleteNo = dtTotalPallete.Rows[0]["palletetype_no"].ToString().Trim();

                //du vao pallete lay ma khach hang
                DataTable dtPalleteWH = uow.App300Repo.GetCheckPallete(palleteNo);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M303-OR", "ShippingToOR");
                }

                symbol = dtPalleteWH.Rows[0]["symbol"].ToString().Trim();
                typeName = dtPalleteWH.Rows[0]["name_type"].ToString().Trim();
                //lay ten khac hang
                DataTable dtMaterSymbolWH = uow.App300Repo.GetCheckMaterSymbol(symbol);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(52, "M303-OR", "ShippingToOR");
                }

                detail = dtMaterSymbolWH.Rows[0]["detail"].ToString().Trim();
                groups = Convert.ToInt32(dtMaterSymbolWH.Rows[0]["groups"].ToString().Trim());
                groupsName = dtMaterSymbolWH.Rows[0]["group_name"].ToString().Trim();
                status = Convert.ToInt32(dtMaterSymbolWH.Rows[0]["status"].ToString().Trim());
                //so luong pallete di trong 1 nam
                DataTable dtCtrlGroupSeq = uow.App300Repo.GetCheckCtrlGroupSeq(groups, year);
                if (dtCtrlGroupSeq.Rows.Count == 0)
                {
                    ret = uow.App300Repo.SetInsertCtrlGroupSeq(groups, groupsName, year, 1, Loc);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(62, "M303-OR", "ShippingToOR");
                    }
                }
                else
                {
                    seqNo = Convert.ToInt32(dtCtrlGroupSeq.Rows[0]["seq_no"].ToString().Trim()) + 1;
                    ret = uow.App300Repo.SetUpdateCtrlGroupSeq(groups, year, seqNo, Loc);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(63, "M303-OR", "ShippingToOR");
                    }
                }
                
                cPalleteNo = "T" + Loc;
                //insert so lan tao shipping to
                DataTable dtRecCodeWH = uow.App300Repo.GetRecCodeWH(cPalleteNo, date);
                if (dtRecCodeWH.Rows.Count == 0)
                {
                    ret = uow.App300Repo.SetResetSeq("td_shipping_to_seq_or");
                    DataTable dtSetSeq = uow.App300Repo.GetSeq("td_shipping_to_seq_or");
                    seqNo = Convert.ToInt32(dtSetSeq.Rows[0]["nextval"].ToString().Trim());
                    ret = uow.App300Repo.SetInsertRecCodeWH(cPalleteNo, date, 0, date);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(64, "M303-OR", "ShippingToOR");
                    }
                }
                else
                {
                    DataTable dtSetSeq = uow.App300Repo.GetSeq("td_shipping_to_seq_or");
                    seqNo = Convert.ToInt32(dtSetSeq.Rows[0]["nextval"].ToString().Trim());
                }

                DataTable dtMaxSeqPrint = uow.App300Repo.GetMaxPalletePrint(PalletecodeH);
                if (dtMaxSeqPrint.Rows.Count <= 0)
                {
                    seqNo1 = 1;
                }
                else
                {
                    seqNo1 = Convert.ToInt32(dtMaxSeqPrint.Rows[0]["seq_no"].ToString().Trim()) + 1;
                }

                ret = uow.App300Repo.SetInsertPalletePrint(PalletecodeH, seqNo1, date, time);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(68, "M303-OR", "ShippingToOR");
                }

                ret = uow.App300Repo.UpdatePackingUser(PalletecodeH, UserId);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(92, "M303-OR", "ShippingToOR");
                }

                uow.Commit();

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(PalletecodeH, 0, Loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M303 - OR fn: PrintSMAndST");

                //In shipping to
                PrintSMAndST(PalletecodeH, Loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M303-OR", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M303-OR", "ShippingToOR");
            }
        }
        //Print shipping to (OS)
        public string PrintSMAndST(string palleteNo, string loc, DataTable dtShippingTo)
        {
            try
            {
                List<ShippingTo> printListShippingTo = new List<ShippingTo>();
                List<ShippingMark> printListShippingMark = new List<ShippingMark>();

                foreach (DataRow row in dtShippingTo.Rows)
                {
                    ShippingTo shippingTo = new ShippingTo()
                    {
                        PalleteNo = row["pallete_no"] == null ? "" : row["pallete_no"].ToString().Trim(),
                        ShipSeq = row["ship_seq"] == null ? "0" : row["ship_seq"].ToString().Trim(),
                        PalleteNo1 = row["pallete_no1"] == null ? "0" : row["pallete_no1"].ToString().Trim(),
                        Detail = row["symbol"].ToString().Trim().Substring(0, 2) == "DY" ? row["symbol"].ToString().Trim() : row["detail"] == null ? "" : row["detail"].ToString().Trim(),
                        User = row["packing_user"] == null ? "" : row["packing_user"].ToString().Trim(),
                        Item = row["item"] == null ? "" : row["item"].ToString().Trim(),
                        Jobtag = row["job_order_no"] == null ? "" : row["job_order_no"].ToString().Trim(),
                        BoxNum = row["box_num"] == null ? "" : row["box_num"].ToString().Trim(),
                        Qty = Convert.ToInt32(row["qty"] == null || row["qty"].ToString() == "" ? "0" : row["qty"].ToString().Trim()),
                        Rev = Convert.ToInt32(row["rev"] == null || row["rev"].ToString() == "" ? "0" : row["rev"].ToString().Trim()),
                    };
                    string pallete1;
                    if (loc.Equals("OS1"))
                    {
                        pallete1 = row["pallete_no1"] == null ? "" : (row["groups"].ToString() == "5" || row["groups"].ToString() == "6") ? row["detail"].ToString().Trim().Substring(0, 1) + row["pallete_no1"].ToString().Trim().PadLeft(4, '0') : row["pallete_no1"].ToString().Trim();
                    }
                    else
                    {
                        pallete1 = "K" + row["pallete_no1"].ToString().Trim();
                    }
                    string detail = string.Empty;
                    string symbol1 = row["symbol1"].ToString().Trim();

                    if (loc.Trim().Equals("OS1"))
                    {
                        if (row["symbol"].ToString().Trim().Substring(0, 2) == "DY")
                        {
                            detail = row["symbol"].ToString().Trim();
                        }
                        else
                        {
                            detail = row["detail"] == null ? "" : row["groups"].ToString() == "6" ? row["symbol"].ToString().Trim() + " (FUKUSHIMA)" : row["detail"].ToString().Trim();
                        }
                    }
                    else if (loc.Trim().Equals("OR1"))
                    {
                        if (row["detail"].ToString().Trim().Equals("KUMAMOTO"))
                        {
                            detail = "NAGOYA";

                            if (symbol1.Contains("JYB"))
                            {
                                detail = "NAGOYA (JYOUBI)";
                            }
                            else
                            {
                                if (symbol1.Length != 4)
                                {
                                    if (symbol1.Contains("MB"))
                                    {
                                        detail = "NAGOYA (M-B)";
                                    }
                                }
                                else
                                {
                                    if (symbol1.Substring(0) == "M" && symbol1.Substring(2, 1) == "B")
                                    {
                                        detail = "NAGOYA (M-B)";
                                    }
                                }
                            }
                        }
                        else if (row["detail"].ToString().Trim().Equals("JYOUBI"))
                        {
                            if (symbol1.Contains("JYB"))
                            {
                                detail = "NAGOYA (JYOUBI)";
                            }
                        }
                        else
                        {
                            detail = symbol1;
                        }
                    }

                    string Datetmp;
                    if (dtShippingTo.Rows[0]["groups"].ToString() == "2")
                    {
                        DateTime tmp = DateTime.ParseExact(
                                           s: row["etd_date"].ToString().Trim(),
                                           format: "ddMMyyyy", provider: System.Globalization.CultureInfo.InvariantCulture);
                        tmp = tmp.AddDays(4);

                        Datetmp = tmp.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        Datetmp = row["etd_date"] == null ? "" : row["etd_date"].ToString().Trim().Substring(0, 2) + "/" + row["etd_date"].ToString().Trim().Substring(2, 2) + "/" + row["etd_date"].ToString().Trim().Substring(4, 4);
                    }
                    ShippingMark shippingMark = new ShippingMark()
                    {
                        PalleteNo = row["pallete_no"] == null ? "" : row["pallete_no"].ToString().Trim(),
                        PalleteNo1 = pallete1,
                        //Detail = row["detail"] == null ? "" : row["groups"].ToString() == "6" ? "PAA (FUKUSHIMA)" : row["detail"].ToString().Trim(),
                        Detail = detail,
                        Rev = Convert.ToInt32(row["rev"] == null || row["rev"].ToString() == "" ? "0" : row["rev"].ToString().Trim()),
                        TotalBox = row["count_box"] == null ? "" : row["count_box"].ToString().Trim(),
                        ETDVNNDate = row["etdvn_date"] == null ? "" : row["etdvn_date"].ToString().Trim().Substring(0, 2) + "/" + row["etdvn_date"].ToString().Trim().Substring(2, 2) + "/" + row["etdvn_date"].ToString().Trim().Substring(4, 4),
                        ETDHCMDate = Datetmp,
                        TotalItem = row["count_item"] == null ? "" : row["count_item"].ToString().Trim(),
                        TotalQty = Convert.ToInt32(row["qty"] == null || row["qty"].ToString() == "" ? "" : row["qty"].ToString().Trim()),
                    };

                    printListShippingTo.Add(shippingTo);

                    printListShippingMark.Add(shippingMark);
                }

                Printer<ShippingTo>.PrintData("SHIPPINGTO", printListShippingTo);

                if (loc == "OS1")
                {
                    Printer<ShippingMark>.PrintData("SHIPPINGMARK", printListShippingMark);
                    //20190702 In 2 shippng Mark doi voi hang OS
                    Printer<ShippingMark>.PrintData("SHIPPINGMARK", printListShippingMark);
                }
                else
                {
                    Printer<ShippingMark>.PrintData("SHIPPINGMARK", printListShippingMark);
                    if (dtShippingTo.Rows[0]["groups"].ToString() == "2")
                        Printer<ShippingMark>.PrintData("SHIPPINGMARK", printListShippingMark);
                }

                return AppCommon.ShowOkMsg(0);
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "PrintSMAndST", sw);
                }
                return AppCommon.ShowErrorMsg(777, "PrintSMAndST");
            }
        }
        public string CheckPlaceInPallete(string palletetypeNo)
        {
            try
            {
                DataTable dt = uow.App300Repo.GetPlaceOfPallete(palletetypeNo);

                if (dt.Rows.Count <= 0)
                    return AppCommon.ShowErrorMsg(4, "M302S01", palletetypeNo);

                //20190407 My mod de tach Nagoya va DY tranh nham pallete
                if (dt.Rows[0]["symbol"].ToString().Trim().Contains("DY-"))
                    return dt.Rows[0]["symbol"].ToString();
                return dt.Rows[0]["detail"].ToString();
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M302S01", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M302S01");
            }
        }
        public string StoringOS(string location, string userId, List<string> listData)
        {
            string userName = string.Empty;
            string palleteTypeNo = listData[0];
            string symbol;
            long? currentNoTrn = 0;
            int ret;
            DateTime date;

            try
            {
                listData.RemoveAt(0);
                Dictionary<string, long> dicStockWH = new Dictionary<string, long>();

                DataTable dt = uow.App300Repo.GetPlaceOfPallete(palleteTypeNo);
                symbol = dt.Rows[0]["symbol"].ToString().Trim();
                string itemGroup = dt.Rows[0]["item_group"].ToString().Trim();
                foreach (string box in listData)
                {
                    //if (!uow.App300Repo.CheckBoxNoConditionalsToStored(box))
                    ret = uow.App300Repo.CheckBoxNoConditionalsToStored(box);
                    switch (ret)
                    {
                        case -2:
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(91, "M302S02", box);   
                        case -1:
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(89, "M302S02", box);
                        case 0:
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(86, "M302S02", box);
                    }

                    if (uow.App300Repo.CheckBoxNoIsExistInPallete(box))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(35, "M302S02", box);
                    }

                    if (!uow.App300Repo.CheckDestination(box, symbol, location, itemGroup))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(37, "M302S02", box);
                    }

                    date = DateTime.Now;
                    if (uow.App300Repo.UpdateBoxDeliveryAfterStored(box, palleteTypeNo, userId, date) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(28, "M302S02", box);
                    }

                    DataTable boxDelivery = uow.App300Repo.GetInfoBoxDelivery(box);
                    TdBoxInfo boxInfo = new TdBoxInfo()
                    {
                        box_no = box,
                        local_po_line = 0,
                        item = boxDelivery.Rows[0]["item"].ToString(),
                        co_line = 0,
                        co_release = 0,
                        cust_num = boxDelivery.Rows[0]["cust_num"].ToString(),
                        cust_seq = Convert.ToInt32(boxDelivery.Rows[0]["cust_seq"].ToString()),
                        cust_item = boxDelivery.Rows[0]["cust_item"].ToString(),
                        cust_po = boxDelivery.Rows[0]["cust_po"].ToString(),
                        qty = 0,
                        qty_shipped = 0,
                        status = 0,
                        box_type = "",
                        qty_box = Convert.ToInt32(boxDelivery.Rows[0]["qty_box"].ToString()),
                        qty_pack = Convert.ToInt32(boxDelivery.Rows[0]["qty_pack"].ToString()),
                        send_sign = 0,
                        palletetype_no = palleteTypeNo,
                        ship_date = date.ToString("yyyyMMdd"),
                        shipmark_date = date.ToString("yyyyMMdd"),
                        exp_date = date.ToString("yyyyMMdd"),
                        stock = "STOCK",
                        pallete_no = "",
                        ship_seq = 0,
                        etdvn_date = null,
                        etd_date = null,
                        packing_user = null,
                        packing_date = null,
                        packing_time = null,
                        product_code = "FG00"
                    };

                    if (uow.App300Repo.InsertToBoxInfoAfterSotred(boxInfo) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(40, "M302S02", box);
                    }

                    DataTable boxJobtag = uow.App300Repo.GetQtyStockByBoxJobtag(box);
                    if (boxJobtag.Rows.Count <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(29, "M302S02", box);
                    }

                    if (dicStockWH.ContainsKey(boxJobtag.Rows[0]["item"].ToString()))
                    {
                        dicStockWH[boxJobtag.Rows[0]["item"].ToString()]
                            += (long)Convert.ToDouble(boxJobtag.Rows[0]["qty"].ToString());
                    }
                    else
                    {
                        dicStockWH.Add(boxJobtag.Rows[0]["item"].ToString(),
                            (long)Convert.ToDouble(boxJobtag.Rows[0]["qty"].ToString()));
                    }
                }

                //do khi dung transaction, khong lay duoc gia tri cap nhat truoc do tren table
                //nen sau khi tinh toan xong cac du lieu can insert va cap nhat
                //thi tien hanh cap nhat 1 lan
                foreach (KeyValuePair<string, long> item in dicStockWH)
                {
                    if (uow.App300Repo.CheckItemIsExistInStockWh(item.Key))
                    {
                        if (uow.App300Repo.UpdateTdStockWh(item.Key, item.Value) <= 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(44, "M302S02", item.Key);
                        }
                    }
                    else
                    {
                        if (uow.App300Repo.InsertTdStockWH(new TdStockWH()
                        {
                            stock = "STOCK",
                            item = item.Key,
                            qty_onhand = 0,
                            qty_ship = 0,
                            qty_reserve = 0,
                            qty_complete = item.Value
                        }) <= 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(44, "M302S02", item.Key);
                        }
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "StoringOS", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M302S02");
            }
        }
        public string StoringOR(string location, string userId, string box)
        {
            string item = "";
            int ret;
            long? currentNoTrn;
            string palleteNo = string.Empty, palleteType = string.Empty;
            string edtDate = string.Empty, etdVNDate = string.Empty;
            string strPLNo = string.Empty, detail = string.Empty;

            try
            {
                //if (!uow.App300Repo.CheckBoxNoConditionalsToStored(box))
                ret = uow.App300Repo.CheckBoxNoConditionalsToStored(box);
                switch (ret)
                {
                    case -1: //Chua nhan hang vao kho
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(101, "M302S03", box);
                    case -2: //Chua in PCL
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(91, "M302S03", box);
                    case 0: //Da scan sap xep
                        DataTable dtTemp = uow.App300Repo.GetPalleteDetailByBox(box);
                        if (dtTemp.Rows.Count > 0)
                        {
                            strPLNo = dtTemp.Rows[0]["symbol"].ToString().Trim() + " - " + dtTemp.Rows[0]["seq_no"].ToString().Trim();
                            palleteType = dtTemp.Rows[0]["PARENTPALLETE_NO"].ToString().Trim();
                            detail = strPLNo.PadRight(20) + edtDate.PadRight(20);
                            uow.Rollback();
                            return AppCommon.ShowOkMsg(86, detail, box);
                        }
                        else
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(86, "M302S03", box);
                        }
                }
                //20190813
                //if (ret == -1)
                //{
                //    uow.Rollback();
                //    return AppCommon.ShowErrorMsg(89, "M302S02", box);
                //}
                //if (ret == 0)
                //{
                //    DataTable dtTemp = uow.App300Repo.GetPalleteDetailByBox(box);
                //    if (dtTemp.Rows.Count > 0)
                //    {
                //        strPLNo = dtTemp.Rows[0]["symbol"].ToString().Trim() + " - " + dtTemp.Rows[0]["seq_no"].ToString().Trim();
                //        palleteType = dtTemp.Rows[0]["PARENTPALLETE_NO"].ToString().Trim();
                //        detail = strPLNo.PadRight(20) + edtDate.PadRight(20);
                //        return AppCommon.ShowOkMsg(86, detail, box);
                //    }
                //    else
                //        return AppCommon.ShowErrorMsg(86, "M302S03", box);
                //}

                DataTable dt = uow.App300Repo.GetInfoOfBoxByBoxJobtag(box);
                if (dt.Rows.Count <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(10, "M302S03", box);
                }

                item = dt.Rows[0]["item"].ToString().Trim();

                ret = uow.App300Repo.CheckBoxCanStoreOR(box, item);
                if (ret != -1)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(ret, "M302S03", box);
                }

                if (uow.App300Repo.CheckFiFoScanStoringOR(box, item))
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(46, "M302S03", box);
                }

                dt = uow.App300Repo.GetInfoWareHousePlanByItem(item);
                if (dt.Rows.Count <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(49, "M302S03", box);
                }

                palleteNo = dt.Rows[0]["pallete_no"].ToString();
                edtDate = dt.Rows[0]["due_date"].ToString().Trim();
                edtDate = edtDate.Replace('-', '/');
                etdVNDate = string.Join("", edtDate.Split('/'));

                dt = uow.App200Repo.GetPalleteInfoByPalleteNo(palleteNo);
                if (dt.Rows.Count <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(4, "M302S03", palleteNo);
                }

                strPLNo = dt.Rows[0]["symbol"].ToString().Trim() + " - " + dt.Rows[0]["seq_no"].ToString().Trim();
                palleteType = dt.Rows[0]["PARENTPALLETE_NO"].ToString().Trim();
                detail = strPLNo.PadRight(20) + edtDate.PadRight(20);

                ////20190907 Kiem tra noi den khi sap xep
                //dt = uow.App300Repo.GetPlaceOfPallete(palleteType);
                //string symbol = dt.Rows[0]["symbol"].ToString().Trim();
                //if (!uow.App300Repo.CheckDestination(box, symbol, location, null))
                //{
                //    uow.Rollback();
                //    return AppCommon.ShowErrorMsg(37, "M302S02", box);
                //}
                ////end 20190907

                DateTime date = DateTime.Now;

                if (uow.App300Repo.UpdateBoxDeliveryAfterStored(box, palleteType, userId, date) <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(28, "M302S03", box);
                }

                TdBoxInfo boxInfo = new TdBoxInfo()
                {
                    box_no = box,
                    local_po_line = 0,
                    item = item,
                    co_line = 0,
                    co_release = 0,
                    cust_num = "",
                    cust_seq = 0,
                    cust_item = "",
                    cust_po = "",
                    qty = 0,
                    qty_shipped = 0,
                    box_type = "",
                    qty_box = 0,
                    qty_pack = 0,
                    send_sign = 0,
                    palletetype_no = palleteType,
                    ship_date = date.ToString("yyyyMMdd"),
                    shipmark_date = date.ToString("yyyyMMdd"),
                    exp_date = date.ToString("yyyyMMdd"),
                    stock = "STOCK",
                    pallete_no = palleteNo,
                    etdvn_date = etdVNDate,
                    etd_date = etdVNDate,
                    packing_user = userId,
                    packing_date = date.ToString("yyyyMMdd"),
                    packing_time = date.ToString("HHmmss"),
                    ship_seq = 0,
                    status = 1,
                    product_code = "FG10"
                };

                if (uow.App300Repo.InsertToBoxInfoAfterSotred(boxInfo) <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(40, "M302S03", box);
                }
                //uow.Commit();

                //DataTable dtBoxJobtag = uow.App300Repo.GetInfoOfBoxByBoxJobtag(box);
                //currentNoTrn = uow.App300Repo.GetNextNo("TRN");
                //if (currentNoTrn < 0)
                //{
                //    return AppCommon.ShowErrorMsg((int)(currentNoTrn * (-1)), "M302S03", currentNoTrn.ToString());
                //}

                //if (uow.App300Repo.InsertTsStockResult(new tsStockResult()
                //{
                //    box_no = box,
                //    data_sign = "030",
                //    item = item,
                //    job_no = dtBoxJobtag.Rows[0]["job_no"].ToString(),
                //    qty = 0,
                //    status = "1",
                //    suffix = 0,
                //    update_user = userId,
                //    //trn_no = uow.App300Repo.GetTrnNoOfTsStockResult()
                //    trn_no = currentNoTrn
                //}) <= 0)
                //{
                //    return AppCommon.ShowErrorMsg(43, "M302S03", box);
                //}

                uow.Commit();

                return AppCommon.ShowOkMsg(999, detail, box);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M302S03 - StoringOR", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M302S03");
            }
        }

        //Sap xep OS
        public string CheckWaitingData(string location, string userId, List<string> listData)
        {
            string palleteTypeNo = listData[0];
            string box = listData[1];
            string symbol;
            int ret;
            DateTime date;

            try
            {
                listData.RemoveAt(0);
                
                DataTable dt = uow.App300Repo.GetPlaceOfPallete(palleteTypeNo);
                symbol = dt.Rows[0]["symbol"].ToString().Trim();
                string itemGroup = dt.Rows[0]["item_group"].ToString().Trim();
                ret = uow.App300Repo.CheckBoxNoConditionalsToStored(box);
                int currentWaitingQty = 0;
                switch (ret)
                {
                    case -2:
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(91, "M302S04", box);
                    case -1:
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(89, "M302S04", box);
                    case 0:
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(86, "M302S04", box);
                }
                if (uow.App300Repo.CheckBoxNoIsExistInPallete(box))
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(35, "M302S04", box);
                }

                if (!uow.App300Repo.CheckDestination(box, symbol, location, itemGroup))
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(37, "M302S04", box);
                }
                //Kiem tra hang cho
                DataTable dataWaiting = uow.App300Repo.GetInfoItemOfBoxIsInWaiting(box);
                //Khong phai hang cho
                if (dataWaiting.Rows.Count <= 0)
                {
                    return UpdateStoringOS(location, userId, palleteTypeNo, box);
                }
                //hang cho
                else
                {
                    string item = dataWaiting.Rows[0]["item"].ToString().Trim();
                    string shippingTo = dataWaiting.Rows[0]["shipping_to"].ToString().Trim();
                    string duedate = dataWaiting.Rows[0]["due_date"].ToString().Trim();
                    int waitingQty = uow.App300Repo.GetWaitingQty(item, duedate);
                    int qtyStored = uow.App300Repo.GetWaitingQtyStored(item, duedate);                    
                    currentWaitingQty = waitingQty - qtyStored;
                    //Da du so luong hang cho
                    if(currentWaitingQty <= 0)
                    {
                        //20190907 Check fifo voi hang da sap xep de kiem tra thung hang cho hien tai co lot nho hon hang da sx hay ko
                        string curLotOfBox = uow.App400Repo.GetMinMaxLotOfBox(box).Rows[0]["max_lot"].ToString().Trim();
                        string maxLotOfItemStored = uow.App300Repo.GetMaxLofOfItemStored(item);
                        if(string.Compare(curLotOfBox, maxLotOfItemStored) < 0)
                        {
                            return AppCommon.ShowErrorMsg(46, "M302S04", maxLotOfItemStored);
                        }
                        //20190907 end
                        return UpdateStoringOS(location, userId, palleteTypeNo, box);
                    }                    

                    dt = uow.App300Repo.GetAllboxCanStoredByItem(item);
                    foreach (DataRow dr in dt.Rows)
                    {
                        currentWaitingQty = currentWaitingQty - Convert.ToInt32(dr["qty"].ToString());
                        if (currentWaitingQty >= 0 && box.Trim().Equals(dr["box_no"].ToString().Trim()))
                        {
                            //insert vao td_cal_waiting_plan
                            ret = uow.App300Repo.InsertIntoWaitingPlanLog(userId, item, duedate, box, uow.App300Repo.GetBoxQtyOfBox(box));
                            string result = UpdateStoringOS(location, userId, palleteTypeNo, box);
                            if (result == "")                                
                                return AppCommon.ShowOkMsg(999, "Hang cho", "");
                            return result;
                        }
                        if (currentWaitingQty <= 0)
                        {
                            return UpdateStoringOS(location, userId, palleteTypeNo, box);
                        }
                    }

                }
                return UpdateStoringOS(location, userId, palleteTypeNo, box); 
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M302S04 - CheckWaitingData", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M302S04");
            }
        }

        public string UpdateStoringOS(string location, string userId, string palleteTypeNo, string box)
        {
            string userName = string.Empty;
            string symbol;
            DateTime date;

            try
            {
                Dictionary<string, long> dicStockWH = new Dictionary<string, long>();

                DataTable dt = uow.App300Repo.GetPlaceOfPallete(palleteTypeNo);
                symbol = dt.Rows[0]["symbol"].ToString().Trim();               

                date = DateTime.Now;
                if (uow.App300Repo.UpdateBoxDeliveryAfterStored(box, palleteTypeNo, userId, date) <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(28, "UpdateStoringOS", box);
                }

                DataTable boxDelivery = uow.App300Repo.GetInfoBoxDelivery(box);
                TdBoxInfo boxInfo = new TdBoxInfo()
                {
                    box_no = box,
                    local_po_line = 0,
                    item = boxDelivery.Rows[0]["item"].ToString(),
                    co_line = 0,
                    co_release = 0,
                    cust_num = boxDelivery.Rows[0]["cust_num"].ToString(),
                    cust_seq = Convert.ToInt32(boxDelivery.Rows[0]["cust_seq"].ToString()),
                    cust_item = boxDelivery.Rows[0]["cust_item"].ToString(),
                    cust_po = boxDelivery.Rows[0]["cust_po"].ToString(),
                    qty = 0,
                    qty_shipped = 0,
                    status = 0,
                    box_type = "",
                    qty_box = Convert.ToInt32(boxDelivery.Rows[0]["qty_box"].ToString()),
                    qty_pack = Convert.ToInt32(boxDelivery.Rows[0]["qty_pack"].ToString()),
                    send_sign = 0,
                    palletetype_no = palleteTypeNo,
                    ship_date = date.ToString("yyyyMMdd"),
                    shipmark_date = date.ToString("yyyyMMdd"),
                    exp_date = date.ToString("yyyyMMdd"),
                    stock = "STOCK",
                    pallete_no = "",
                    ship_seq = 0,
                    etdvn_date = null,
                    etd_date = null,
                    packing_user = null,
                    packing_date = null,
                    packing_time = null,
                    product_code = "FG00"
                };
                if (uow.App300Repo.InsertToBoxInfoAfterSotred(boxInfo) <= 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(40, "M302S02", box);
                }
                string itemBox = boxDelivery.Rows[0]["item"].ToString();

                if (uow.App300Repo.CheckItemIsExistInStockWh(itemBox))
                {
                    if (uow.App300Repo.UpdateTdStockWh(itemBox, 
                        (long)Convert.ToDouble(uow.App300Repo.GetBoxQtyOfBox(box))) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(44, "UpdateStoringOS", itemBox);
                    }
                }
                else
                {
                    if (uow.App300Repo.InsertTdStockWH(new TdStockWH()
                    {
                        stock = "STOCK",
                        item = itemBox,
                        qty_onhand = 0,
                        qty_ship = 0,
                        qty_reserve = 0,
                        qty_complete = (long)Convert.ToDouble(uow.App300Repo.GetBoxQtyOfBox(box))
                    }) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(44, "UpdateStoringOS", itemBox);
                    }
                }

                uow.Commit();

                return "";
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "UpdateStoringOS", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "UpdateStoringOS");
            }
        }
    }
}
