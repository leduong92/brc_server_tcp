using BcrServer_Helper;
using BcrServer_Model;
using BcrServer_Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BcrServer
{
    public class App400
    {
        #region[Initialize]
        private static App400 instance;
        public static App400 Instance
        {
            get
            {
                if (instance == null)
                    instance = new App400();
                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        UnitOfWork uow;
        public App400()
        {
            uow = new UnitOfWork();
        }

        ~App400()
        {
            uow = null;
        }
        #endregion

        #region [FUNCTION'S KHOA]
        /// <summary>
        /// M400-S01 : TRA HANG VE SAN XUAT
        /// </summary>
        /// <param name="boxes"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public string ReturnBoxToProduction(List<string> boxes, string user)
        {
            try
            {
                foreach (string box in boxes)
                {
                    DataTable dtBoxInfo = uow.App400Repo.GetBoxInfo(box);

                    //if (dtBoxInfo.Rows.Count < 1)
                    //    return AppCommon.ShowErrorMsg(38, "M400S01", box);

                    if (dtBoxInfo.Rows.Count > 0)
                    {
                        if (Convert.ToInt32(dtBoxInfo.Rows[0]["status_return"]) == 1)
                            return AppCommon.ShowErrorMsg(77, "M400S01", box);
                    }

                    int ret = uow.App400Repo.UpdateFinishedLotQty(box);

                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(70, "M400S01", box);
                    }

                    //Khi tra ve san xuat, neu du lieu trong td_incoming_box.status ='0' thi update lai.
                    //Du lieu trong bang nay co khi ghi nhan PCL thanh cong.

                    //20190906 Do item_sign = 3 thi box se ko insert vao incomming_box 
                    if (uow.App400Repo.CheckIsExistInInComingBox(box))
                    {
                        ret = uow.App400Repo.UpdateStatusIncomingBox(box);
                        if (ret < 1)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(71, "M400S01", box);
                        }
                    }
                    //ret = uow.App400Repo.UpdateStatusIncomingBox(box);
                    //if (ret < 1)
                    //{
                    //    uow.Rollback();
                    //    return AppCommon.ShowErrorMsg(71, "M400S01", box);
                    //}

                    ret = uow.App400Repo.InsertBoxReturn(box, user);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(72, "M400S01", box);
                    }

                    ret = uow.App400Repo.DeleteBoxDelivery(box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(73, "M400S01", box);
                    }

                    ret = uow.App400Repo.DeleteBoxInfo(box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(74, "M400S01", box);
                    }
                    //20190702 neu la thung hang cho phai xoa trong waiting_plan
                    DataTable boxWaiting = uow.App300Repo.CheckBoxInWatingLog(box);                    
                    if (boxWaiting.Rows.Count > 0)
                    {
                        string item = boxWaiting.Rows[0]["item"].ToString();
                        if (uow.App300Repo.DeleteWaitingBox(box) < 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(99, "M400S01");
                        }
                        

                        if (uow.App400Repo.GetStatusBoxInfoOfBox(box) >= 1)
                        {
                            DataTable boxInfo = uow.App400Repo.GetBoxInfo(box);

                            int qtyBox = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                            if(uow.App400Repo.UpdateActualQtyWaitingPlan(qtyBox * (-1), item, boxInfo.Rows[0]["pallete_no"].ToString()) < 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(94, "M400S01", box);
                            }
                        }
                    }
                    //20190702

                    ret = uow.App400Repo.DeletePclPrint(box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(75, "M400S01", box);
                    }

                    ret = uow.App400Repo.DeleteDailyBoxRec(box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(76, "M400S01", box);
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M400-S01: ReturnBoxToProduction", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M400S01");
            }
        }

        /// <summary>
        /// M400-S02 : CHO SAN XUAT MUON LAI THUNG HANG
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public string BorrowBoxForOS(List<string> boxes)
        {
            try
            {
                foreach (string box in boxes)
                {
                    DataTable dtBoxInfo = uow.App400Repo.GetBoxInfo(box);

                    if (dtBoxInfo.Rows.Count < 1)
                        return AppCommon.ShowErrorMsg(38, "M400S02", box);

                    if (Convert.ToInt32(dtBoxInfo.Rows[0]["status_return"]) == 1)
                        return AppCommon.ShowErrorMsg(77, "M400S02", box);

                    int ret = uow.App400Repo.UpdateBoxInfoStatusReturn(0, box);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M400S02", box);
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "BorrowBoxForOS", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M400S02");
            }
        }

        /// <summary>
        /// M400 - S03 : NHAN LA THUNG HANG TU SAN XUAT
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetBoxFromPalleteOrShippingTo(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Trim()))
                    return AppCommon.ShowErrorMsg(888, "M400S03");

                DataTable dt = new DataTable();

                if (data.StartsWith("TOS") || data.StartsWith("TOR"))
                    dt = uow.App400Repo.GetBoxInfoReturnWithCondition(null, data, 0);
                else if (data.StartsWith("POS") || data.StartsWith("POR"))
                    dt = uow.App400Repo.GetBoxInfoReturnWithCondition(data, null, 1);

                if (dt.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(29, "M400S03");

                return Utility.Instance.ToOneRow(dt, 0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetBoxFromPalleteOrShippingTo", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M400S03");
            }
        }

        /// <summary>
        /// M400 - S04 : NHAN LA THUNG HANG TU SAN XUAT
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string TakeBoxBackForOS(List<string> data)
        {
            try
            {
                int ret = 0;

                if (data.Count < 1)
                    return AppCommon.ShowErrorMsg(888, "M400S04");

                string firstElement = data[0];

                data.RemoveAt(0);

                foreach (string box in data)
                {
                    if (firstElement.StartsWith("TOS") || firstElement.StartsWith("TOR"))
                        ret = uow.App400Repo.UpdateBoxInfoStatusReturn(2, box, firstElement);
                    else if (firstElement.StartsWith("POS") || firstElement.StartsWith("POR"))
                        ret = uow.App400Repo.UpdateBoxInfoStatusReturn(1, box, firstElement);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M400S04");
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "TakeBoxBackForOS", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M400S04");
            }
        }

        /// <summary>
        /// M404 - S01 : NHAN HANG THNAH PHAM NGAY KIEM KE
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string InstockWhenCheckInventory(List<string> data, string userId)
        {
            try
            {
                int ret = 0;
                string strPCLIn = string.Empty;
                string instockDate = string.Empty;

                if (data == null)
                    return AppCommon.ShowErrorMsg(11);

                if (data.Count < 1)
                    return AppCommon.ShowErrorMsg(11);

                //instockDate = data[0].Substring(4, 4) + data[0].Substring(2, 2) + data[0].Substring(0, 2);
                instockDate = data[0];

                data.RemoveAt(0);

                strPCLIn = Utility.Instance.ToOneRow(data);

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
                    WriteLog.Instance.Write(ex.Message, "M404S01", sw);
                }
               
                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M404S01");
            }
        }

        /// <summary>
        /// M406 : HOAN DOI THUNG - PALLETE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string CheckPalleteForSwitchBox(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return AppCommon.ShowErrorMsg(888, "M406");

                DataTable dtBoxInfo = uow.App400Repo.GetBoxForSwitchPallete(data);
                if (dtBoxInfo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(4, "M406", data);

                return Utility.Instance.ToOneRow(dtBoxInfo, 0, "BOX_NO");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "CheckPalleteForSwitchBox", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M406");
            }
        }

        /// <summary>
        /// M406 - S01 : HOAN DOI THUNG - PALLETE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SwitchBoxInPallete(List<string> data, string userId)
        {
            try
            {
                if (data.Count < 1)
                    return AppCommon.ShowErrorMsg(888, "M406S01");

                string fromPallete = data[0];
                string toPallete = data[1];
                string symbol = string.Empty;

                data.RemoveRange(0, 2);

                DataTable dtPallete = uow.App300Repo.GetGroupNumberByPallete(toPallete, 0);
                if (dtPallete.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(4, "M406S01", toPallete);

                symbol = dtPallete.Rows[0]["SYMBOL"] == null ? "" : dtPallete.Rows[0]["SYMBOL"].ToString();
                string itemGroup = dtPallete.Rows[0]["ITEM_GROUP"].ToString().Trim();
                if (string.IsNullOrEmpty(symbol.Trim()))
                    return AppCommon.ShowErrorMsg(52, "M406S01", toPallete);

                foreach (string box in data)
                {
                    bool bret = uow.App400Repo.IsBoxStored(fromPallete, box);
                    if (!bret)
                        return AppCommon.ShowErrorMsg(47, "M406S01", box);

                    if (!uow.App300Repo.CheckDestination(box, symbol.Trim(), "OS1", itemGroup))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(37, "M406S01", box);
                    }

                    int ret = uow.App400Repo.UpdatePalleteInBoxInfo(toPallete, box);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M406S01", box);
                    }

                    ret = uow.App400Repo.UpdatePalleteInBoxDelivery(toPallete, box);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(28, "M406S01", box);
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SwitchBoxInPallete", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M406S01");
            }
        }

        /// <summary>
        /// M414 - S01 : THEM THUNG TU KHU VUC NGOAI VAO SHIPPING TO
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userId"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public string AddBoxFromPalleteIntoShippingTo(List<string> data, string userId, string loc)
        {
            try
            {
                if (data.Count < 1)
                    return AppCommon.ShowErrorMsg(888, "M414S01");

                string pallete = data[0];
                string shippingTo = data[1];

                data.RemoveRange(0, 2);

                foreach (string box in data)
                {                 
                    DataTable dtBoxInfo = uow.App400Repo.GetEtdBoxInfo(shippingTo);
                    if (dtBoxInfo.Rows.Count < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(81, "M414S01", shippingTo);
                    }

                    string etd = dtBoxInfo.Rows[0]["etd_date"] == null ? "" : dtBoxInfo.Rows[0]["etd_date"].ToString();
                    string etdvn = dtBoxInfo.Rows[0]["etdvn_date"] == null ? "" : dtBoxInfo.Rows[0]["etdvn_date"].ToString();
                    int shipSeq = dtBoxInfo.Rows[0]["ship_seq"] == null ? 0 : Convert.ToInt32(dtBoxInfo.Rows[0]["ship_seq"]);
                    string symbol = dtBoxInfo.Rows[0]["symbol"] == null ? "" : dtBoxInfo.Rows[0]["symbol"].ToString();
                    string palleteTypeNo = dtBoxInfo.Rows[0]["palletetype_no"] == null ? "" : dtBoxInfo.Rows[0]["palletetype_no"].ToString().Trim();
                    DataTable dtBoxInfo2 = uow.App300Repo.GetCheckBoxInfoOS(pallete, box);
                    if (dtBoxInfo2.Rows.Count < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(38, "M414S01", box);
                    }
                    //20190702 check BRQ0008,9
                    string itemGroup = uow.App300Repo.GetPlaceOfPallete(palleteTypeNo).Rows[0]["item_group"].ToString().Trim();
                    if (!uow.App300Repo.CheckDestination(box, symbol.Trim(), loc, itemGroup))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(37, "M414S01", box);
                    }

                    //20190530 Kiem tra xu ly hang cho
                    string item = dtBoxInfo2.Rows[0]["item"].ToString();
                    //20190704 kiem tra item tren shipping dang lam co phai hang cho ko
                    if(uow.App300Repo.CheckItemIsWaitingItem(item))
                    {
                        if (uow.App300Repo.CheckShippingToIsWaiting(shippingTo))
                        {
                            if (uow.App300Repo.CheckBoxInWatingLog(box).Rows.Count <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(93, "M414S01", box);
                            }
                            DataTable qtyWaiting = uow.App300Repo.GetQtyWaitingByShippingTo(shippingTo, item);
                            if (qtyWaiting.Rows.Count <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(96, "M414S01", box);
                            }
                            int actualQty = Convert.ToInt32(qtyWaiting.Rows[0]["qty"].ToString());
                            int boxQty = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                            if (actualQty - boxQty >= 0)
                            {
                                if (uow.App400Repo.UpdateActualQtyWaitingPlan(boxQty, item, shippingTo) <= 0)
                                {
                                    uow.Rollback();
                                    return AppCommon.ShowErrorMsg(94, "M414S01", box);
                                }
                                if (uow.App400Repo.UpdateShippingToForWaitingBox(shippingTo, box) <= 0)
                                {
                                    uow.Rollback();
                                    return AppCommon.ShowErrorMsg(94, "M414S01", box);
                                }

                            }
                            else
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(94, "M414S01", box);
                            }

                            //20190718 Check Fifo
                            //1. Get Maxlot of Box
                            DataTable dt = uow.App400Repo.GetMinMaxLotOfBox(box);
                            if (dt.Rows.Count <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(38, "M414S01", box);
                            }
                            string curMaxLotBox = dt.Rows[0]["max_lot"].ToString().Trim();

                            //2. curMaxLotBox > max(hang da reser, ship)
                            string maxLotOfReserShip = uow.App400Repo.GetMaxLotOfReserShipOS(item);
                            if(maxLotOfReserShip != "" && string.Compare(curMaxLotBox, maxLotOfReserShip) < 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(46, "M414S02", box);
                            }

                            //3. curMaxLotBox < All hang o nhan hang min(max_lof of box)
                            string minLotOfReceive = uow.App400Repo.GetMinLotOfReceiveByItem(item);
                            if(minLotOfReceive != "" && string.Compare(curMaxLotBox, minLotOfReceive) > 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(46, "M414S02", box);
                            }

                            //4. curMaxLotBox < All hang da sx chua in shipping to (status = 0) min(max_lot of box)
                            string maxLotOfItemBeforeStore = uow.App400Repo.GetMinLotStoredByItem(item, shippingTo).Trim();
                            if (maxLotOfItemBeforeStore != "" && string.Compare(curMaxLotBox, maxLotOfItemBeforeStore) > 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(46, "M414S02", box);
                            }           
                            
                            //end 20190718 Check fifo

                        }
                        //20190704
                        else
                        {
                            if (uow.App300Repo.CheckBoxInWatingLog(box).Rows.Count > 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(96, "M414S01", box);
                            }
                        }
                    }
                    //End kiem tra xu ly hang cho

                    int ret = uow.App300Repo.SetUpdateBoxInfoWH(shippingTo, "", "", userId, shipSeq, etd, etdvn, pallete, box);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M414S01", box);
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M414S01-AddBoxFromPalleteIntoShippingTo", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M414S01");
            }
        }
        
        /// <summary>
        /// M415 - S01: LAY TAT CA THUNG HANG NAM TRONG SHIP (td_box_info.status = 4)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetBoxFromShipping(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return AppCommon.ShowErrorMsg(888, "M415S01");

                DataTable dt = uow.App400Repo.GetAllBoxFromShipping(data);
                if (dt.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(42, "M415S01");

                return Utility.Instance.ToOneRow(dt);
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M415S01-GetBoxFromShipping", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M415S01");
            }
        }

        /// <summary>
        /// M415 - S02: CAP NHAT DU LIEU CHO CAC THUNG TRA VE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string ReturnBoxFromShippingToReceived(List<string> boxes, string user)
        {
            try
            {
                if (boxes.Count < 1)
                    return AppCommon.ShowErrorMsg(888, "M415S02");

                string shippingNo = boxes[0].ToString().Trim();
                boxes.RemoveAt(0);

                string date = DateTime.Now.ToString("yyyyMMdd");
                string time = DateTime.Now.ToString("hhmmss");

                foreach (string box in boxes)
                {
                    //20190717 neu la thung hang cho phai xoa trong waiting_plan
                    DataTable boxWaiting = uow.App300Repo.CheckBoxInWatingLog(box);
                    if (boxWaiting.Rows.Count > 0)
                    {
                        string item = boxWaiting.Rows[0]["item"].ToString();

                        if (uow.App400Repo.GetStatusBoxInfoOfBox(box) >= 1)
                        {
                            DataTable boxInfo = uow.App400Repo.GetBoxInfo(box);

                            int qtyBox = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                            if (uow.App400Repo.UpdateActualQtyWaitingPlan(qtyBox * (-1), item, boxInfo.Rows[0]["pallete_no"].ToString()) < 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(94, "M415S02", box);
                            }
                        }
                    }
                    //20190717
                    int ret = uow.App400Repo.UpdateStatusOfBoxReturnShippingToReceived(shippingNo, box, date, time, user);

                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M415S02");
                    }

                    ret = uow.App400Repo.DeleteTTShipingPrint(box);
                    if (ret < 1)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(90, "M415S02");
                    }
                }

                uow.Commit();
                return AppCommon.ShowOkMsg(0);
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M415S02-ReturnBoxFromShippingToReceived", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M415S02");
            }
        }

        #endregion [FUNCTION'S KHOA]


        #region [FUNCTION'S DUONG]

        #region [M401 - OS] TRA HANG TU INSTOCK VE KHU VUC RECEIVE 
        public string GetBoxByPallete(string palleteNo)
        {
            try
            {
                string boxesSendHT = string.Empty;
                DataTable dtBoxesByPallete = uow.App400Repo.GetBoxFromPallete(palleteNo);

                if (dtBoxesByPallete.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(10);
                }

                boxesSendHT = Utility.Instance.ToOneRow(dtBoxesByPallete, 0);

                return boxesSendHT;
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M401-GetBoxByPallete", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M401S01");
            }
        }

        public string OSReturnBoxFromInstockToReceive(string userId, string location, List<string> boxes)
        {
            try
            {
                string boxesReceive = Utility.Instance.ToOneRow(boxes);
                string valuesInsert = string.Empty;
                int ret = 0;
                long? trn;

                string palleteNo = boxes[0];

                for (int i = 1; i < boxes.Count; i++)
                {
                    DataTable dtBoxInfo = uow.App400Repo.GetDataBoxInfo(palleteNo, boxes[i]);
                    if (dtBoxInfo.Rows.Count <= 0)
                        return AppCommon.ShowErrorMsg(47, "M401");

                    //update td_box_delivery
                    ret = uow.App400Repo.UpdBoxDelivery(dtBoxInfo.Rows[0]["box_no"].ToString(), userId);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(28, "M401");
                    }

                    //delete box_info

                    ret = uow.App400Repo.DelBoxInfo(palleteNo, dtBoxInfo.Rows[0]["box_no"].ToString());

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(56, "M401");
                    }

                    trn = uow.App300Repo.GetTrnNoOfTsStockResult("TRN");

                    if (trn < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(53, "M401");
                    }
                    else
                    {
                        trn++;
                    }

                    //20190702 Xoa hang cho trong td_waiting_plan_log de insert lai
                    if (uow.App300Repo.CheckBoxInWatingLog(boxes[i]).Rows.Count > 0)
                    {
                        if (uow.App300Repo.DeleteWaitingBox(boxes[i]) < 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(99, "M401");
                        }
                    }
                    //20190702

                    valuesInsert += string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', {5}, '{6}'),", dtBoxInfo.Rows[0]["box_no"].ToString(), "060", dtBoxInfo.Rows[0]["item"].ToString(), "1", userId, trn, "");

                }
                if (!string.IsNullOrEmpty(valuesInsert))
                {
                    valuesInsert = valuesInsert.Substring(0, valuesInsert.LastIndexOf(","));

                    ret = uow.App400Repo.InsertTsStockResult(valuesInsert);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(43, "M401");
                    }
                }
                uow.Commit();
                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "OSReturnBoxFromInstockToReceive", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M401");
            }
        }
        #endregion

        /// <summary>
        /// [M401 - OR] TRA HANG TU INSTOCK VE KHU VUC RECEIVE
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="location"></param>
        /// <param name="boxes"></param>
        /// <returns></returns>
        public string ORReturnBoxFromInstockToReceive(string userId, string location, List<string> boxes)
        {
            try
            {
                string boxesReceive = Utility.Instance.ToOneRow(boxes);
                string valuesInsert = string.Empty;
                int ret = 0;
                long? trn;

                for (int i = 0; i < boxes.Count; i++)
                {
                    DataTable dtBoxInfo = uow.App400Repo.GetDataBoxInfo(boxes[i]);
                    if (dtBoxInfo.Rows.Count <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(47, "M401OR1");
                    }

                    //update td_box_delivery
                    ret = uow.App400Repo.UpdBoxDelivery(dtBoxInfo.Rows[0]["box_no"].ToString());

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(28, "M401OR1");
                    }

                    //delete box_info

                    ret = uow.App400Repo.DelBoxInfo(dtBoxInfo.Rows[0]["box_no"].ToString());

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(56, "M401OR1");
                    }

                    trn = uow.App300Repo.GetTrnNoOfTsStockResult("TRN");

                    if (trn < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(53, "M401OR1");
                    }
                    else
                    {
                        trn++;
                    }
                    
                    valuesInsert += string.Format("('{0}', '{1}', '{2}', '{3}', '{4}', {5}, '{6}'),", dtBoxInfo.Rows[0]["box_no"].ToString(), "030", dtBoxInfo.Rows[0]["item"].ToString(), "1", userId, trn, "Tra ve Receiving");
                }

                if (!string.IsNullOrEmpty(valuesInsert))
                {
                    valuesInsert = valuesInsert.Substring(0, valuesInsert.LastIndexOf(","));

                    ret = uow.App400Repo.InsertTsStockResult(valuesInsert);

                    if (ret <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(43, "M401OR1");
                    }
                }

                uow.Commit();

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "ORReturnBoxFromInstockToReceive", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M401OR1");
            }
        }
 
        /// <summary>
        /// [M409 - OS] CHINH SUA NGAY ETD VA SO LAN XUAT
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="location"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string ChangeEtdAndNOT(string userID, string location, List<string> data)
        {
            try
            {
                string setQuery = string.Empty;
                int ret = 0;

                int numberOfTime = Convert.ToInt32(data[0]);
                string dateEtdVn = data[1];
                string dateEtdHcm = data[2];
                string palleteNo = data[3];

                DataTable dtBoxInfo = uow.App400Repo.GetDataPallete(data[3]);

                if (dtBoxInfo.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(4, "M409");
                }

                DataTable dtPalleteType = uow.App400Repo.GetDataPalleteTypeNo(dtBoxInfo.Rows[0]["palletetype_no"].ToString());

                if (dtPalleteType.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(80, "M409");
                }

                if (numberOfTime != 0)
                {
                    setQuery = string.Format("ship_seq = {0}, etdvn_date = '{1}', etd_date = '{2}'", numberOfTime, dateEtdVn, dateEtdHcm);
                }
                else
                {
                    setQuery = string.Format("etdvn_date = '{0}', etd_date = '{1}'", dateEtdVn, dateEtdHcm);
                }

                ret = uow.App400Repo.UpdBoxInFo(setQuery, palleteNo);

                if (ret <= 0)
                {
                    uow.Rollback();
                    AppCommon.ShowErrorMsg(0);
                }

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(palleteNo, 0, location);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M409 - OS1 fn: PrintSMAndST");
                //In shipping to
                App300.Instance.PrintSMAndST(palleteNo, location, dtShippingTo);

                uow.Commit();
                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "ChangeEtdAndNOT", sw);
                }

                uow.Rollback();
                return AppCommon.ShowErrorMsg(78, "M409 - OS1 fn: PrintSMAndST");
            }

        }

        #endregion [FUNCTION'S DUONG]


        #region [FUNCTION'S HIEU]
        public string GetTotalBoxOS(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Trim()))
                    return AppCommon.ShowErrorMsg(888, "M403SO1 - OS", "TotalBoxOS");

                DataTable dtTotalBox = uow.App400Repo.GetTotalBoxOS(data.Trim());
                if (dtTotalBox.Rows.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(69);
                }

                return Utility.Instance.ToOneRow(dtTotalBox);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetTotalBoxOS", sw);
                }

                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M403SO1-OS", "TotalBoxOS");
            }
        }
        /// <summary>
        /// M403 - IN MAT PHIEU SHIPPING TO
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SetLostShippingToOR(string loc, string userId, string data)
        {
            string palleteNo, date, time;
            int ret, seqNo = 0;
            try
            {
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("HHmmss");

                DataTable dtTotalPallete = uow.App300Repo.GetPalleteOR(data);
                if (dtTotalPallete.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M403-OS", "LostShippingToO");
                }

                palleteNo = dtTotalPallete.Rows[0]["pallete_no"].ToString().Trim();

                DataTable dtMaxSeqPrint = uow.App300Repo.GetMaxPalletePrint(palleteNo);
                if (dtMaxSeqPrint.Rows.Count <= 0)
                {
                    seqNo = 1;
                }
                else
                {
                    seqNo = Convert.ToInt32(dtMaxSeqPrint.Rows[0]["seq_no"].ToString().Trim()) + 1;
                }

                ret = uow.App300Repo.SetInsertPalletePrint(palleteNo, seqNo, date, time);

                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(68, "M403-OS", "LostShippingToO");
                }

                if (loc.Equals("OR1"))
                {
                    ret = uow.App300Repo.UpdatePackingUser(palleteNo, userId);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(92, "M403-OS", "LostShippingToO");
                    }
                }

                uow.Commit();

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(palleteNo, 0, loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M403 - OS1 fn: PrintSMAndST");

                //In shipping to
                App300.Instance.PrintSMAndST(palleteNo, loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SetLostShippingToOR", sw);
                }

                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M403-OR", "LostShippingToO");
            }
        }

        /// <summary>
        /// M405- IN PHIEU SHIPPING TO CHI DINH
        /// </summary>
        /// <param name="Loc"></param>
        /// <param name="UserId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string PrintShippingToSpecify(string Loc, string UserId, List<string> data)
        {
            int i = 1, j;
            string dateEDTVN, dateEDTHCM, palleteNo, symbol, typeName, detail, groupsName, item;
            string PalletecodeH, cPalleteNo, date, year, time;
            int lanXuat, groups, status, ret, seqNo = 0, seqNo1 = 0, seqNo2 = 0;
            try
            {
                year = DateTime.Now.ToString("yyyy");
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("hhmmss");

                dateEDTVN = data[0].ToString().Substring(0, 8);
                dateEDTHCM = data[0].ToString().Substring(8, 8);
                palleteNo = data[1].ToString();
                lanXuat = Convert.ToInt32(data[2].ToString());

                data.RemoveRange(0, 3);

                //Kiem tra tat ca cac thung co tren pallete khong, lay item
                DataTable dtBoxInfo = uow.App300Repo.GetCheckBoxInfoOS(palleteNo, data[data.Count - 1]);
                if (dtBoxInfo.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(47, "M405-OS", data[0].ToString().Trim());
                }

                item = dtBoxInfo.Rows[0]["item"].ToString().Trim();

                //du vao pallete lay ma khach hang
                DataTable dtPalleteWH = uow.App300Repo.GetCheckPallete(palleteNo);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M405-OS", "ShippingToSpecify");
                }

                symbol = dtPalleteWH.Rows[0]["symbol"].ToString().Trim();
                typeName = dtPalleteWH.Rows[0]["name_type"].ToString().Trim();
                //lay ten khac hang
                DataTable dtMaterSymbolWH = uow.App300Repo.GetCheckMaterSymbol(symbol);
                if (dtPalleteWH.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(52);
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
                        return AppCommon.ShowErrorMsg(62, "M405-OS", "ShippingToSpecify");
                    }
                }
                else
                {
                    seqNo = Convert.ToInt32(dtCtrlGroupSeq.Rows[0]["seq_no"].ToString().Trim()) + 1;
                    ret = uow.App300Repo.SetUpdateCtrlGroupSeq(groups, year, seqNo, Loc);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(63, "M405-OS", "ShippingToSpecify");
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
                        return AppCommon.ShowErrorMsg(64, "M405-OS", "ShippingToSpecify");
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
                    //    return AppCommon.ShowErrorMsg(65, "M405-OS", "ShippingToSpecify");
                    //}
                }

                PalletecodeH = cPalleteNo + date + String.Format("{0:000}", seqNo1);

                foreach (string box in data)
                {
                    ret = uow.App300Repo.SetUpdateBoxInfoWH(PalletecodeH, date, time, UserId, lanXuat, dateEDTHCM, dateEDTVN, palleteNo, box);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M405-OS", box);
                    }
                }

                // inset td_pallete_wh 
                ret = uow.App300Repo.SetInsertPalleteWH(PalletecodeH, palleteNo, item, seqNo, symbol, "0", date, UserId);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(67, "M405-OS", data[i].ToString().Trim());
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
                    return AppCommon.ShowErrorMsg(68, "M405-OS", "ShippingToSpecify");
                }

                uow.Commit();

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(PalletecodeH, 0, Loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M405 - OS1 fn: PrintSMAndST");
                //In shipping to
                App300.Instance.PrintSMAndST(PalletecodeH, Loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M405-OS1", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M405-OS", "ShippingToSpecify");
            }
        }
        //Reprint shipping to / shipping mark
        public string PrintReprintShippingTo(string Loc, string UserId, string data)
        {
            int i = 1, j;

            string palleteNo, date, year, time;
            int ret, seqNo = 0;
            try
            {
                year = DateTime.Now.ToString("yyyy");
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("hhmmss");

                DataTable dtTotalPallete = uow.App400Repo.GetPalleteOS(data);
                if (dtTotalPallete.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(69, "M407-OS", "ReprintShippingTo");
                }
                palleteNo = dtTotalPallete.Rows[0]["pallete_no"].ToString().Trim();

                //insert so lan in shipping to
                DataTable dtMaxSeqPrint = uow.App300Repo.GetMaxPalletePrint(palleteNo);
                if (dtMaxSeqPrint.Rows.Count <= 0)
                {
                    seqNo = 1;
                }
                else
                {
                    seqNo = Convert.ToInt32(dtMaxSeqPrint.Rows[0]["seq_no"].ToString().Trim()) + 1;
                }

                ret = uow.App300Repo.SetInsertPalletePrint(palleteNo, seqNo, date, time);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(68, "M407-OS", "ReprintShippingTo");
                }

                uow.Commit();

                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(data, 1, Loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M407 - OS fn: PrintSMAndST");
                //In shipping to
                App300.Instance.PrintSMAndST(palleteNo, Loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "PrintReprintShippingTo", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M407-OS", "ReprintShippingTo");
            }
        }

        //Reprint shipping to / shipping mark (The goods has ship)
        public string SetLostShippingToOS(string Loc, string UserId, string data)
        {
            try
            {
                DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(data, 4, Loc);
                if (dtShippingTo.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(78, "M411 - OS fn: PrintSMAndST");
                //In shipping to
                App300.Instance.PrintSMAndST(data, Loc, dtShippingTo);

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SetLostShippingToOS", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M411-OS", "LostShippingToOS");
            }
        }

        public string SetTranferNumPLT(string Loc, string UserId, List<string> data)
        {

            string palleteNo1, palleteNo2, date, time;
            int ret, seqNo = 0, seqNo1 = 0, seqNo2 = 0;

            try
            {
                date = DateTime.Now.ToString("yyyyMMdd");
                time = DateTime.Now.ToString("hhmmss");

                palleteNo1 = data[0].ToString();
                palleteNo2 = data[1].ToString();
                // check 2 pallete o trang thai chua ship
                foreach (string pallete in data)
                {
                    DataTable dtpallete = uow.App400Repo.GetCheckPallete(pallete);
                    if (dtpallete.Rows.Count < 1)
                        return AppCommon.ShowErrorMsg(04, "M408-OS", pallete);
                }
                //doi so thu tu cua 2 pallete
                DataTable dtSeq1 = uow.App400Repo.GetSeq(palleteNo1);
                if (dtSeq1.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(53, "M408-OS", "TranferNumPLT");

                seqNo1 = Convert.ToInt32(dtSeq1.Rows[0]["Seq_no"].ToString().Trim());
                DataTable dtSeq2 = uow.App400Repo.GetSeq(palleteNo2);
                if (dtSeq2.Rows.Count < 1)
                    return AppCommon.ShowErrorMsg(53, "M408-OS", "TranferNumPLT");

                seqNo2 = Convert.ToInt32(dtSeq2.Rows[0]["Seq_no"].ToString().Trim());
                ret = uow.App400Repo.SetUpdatePalleteWHSeq(palleteNo1, seqNo2);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(82, "M408-OS", palleteNo1);
                }
                ret = uow.App400Repo.SetUpdatePalleteWHSeq(palleteNo2, seqNo1);
                if (ret < 0)
                {
                    uow.Rollback();
                    return AppCommon.ShowErrorMsg(82, "M408-OS", palleteNo2);
                }
                //insert lai so lan xuat
                foreach (string pallete in data)
                {
                    DataTable dtMaxSeqPrint = uow.App300Repo.GetMaxPalletePrint(pallete);
                    if (dtMaxSeqPrint.Rows.Count <= 0)
                    {
                        seqNo = 1;
                    }
                    else
                    {
                        seqNo = Convert.ToInt32(dtMaxSeqPrint.Rows[0]["seq_no"].ToString().Trim()) + 1;
                    }

                    ret = uow.App300Repo.SetInsertPalletePrint(pallete, seqNo, date, time);
                    if (ret < 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(68, "M408-OS", "TranferNumPLT");
                    }
                }

                uow.Commit();

                foreach (string pallete in data)
                {
                    DataTable dtShippingTo = uow.App300Repo.GetShippingToPrint(pallete, 0, Loc);
                    if (dtShippingTo.Rows.Count < 1)
                        return AppCommon.ShowErrorMsg(78, "M408 - OS1 fn: PrintSMAndST");
                    //In shipping to
                    App300.Instance.PrintSMAndST(pallete, Loc, dtShippingTo);
                }

                return AppCommon.ShowOkMsg(0);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SetTranferNumPLT", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M408-OS", "TranferNumPLT");
            }
        }
        #endregion [FUNCTION'S HIEU]


        #region [FUNCTION'S MY]
        public string GetAllBoxInPalleteExceptBorrowedBox(string pallete)
        {
            try
            {
                DataTable dt = uow.App400Repo.GetAllBoxInPalleteExceptBorrowedBox(pallete);
                if (dt.Rows.Count <= 0)
                {
                    AppCommon.ShowErrorMsg(38, "M402S01", pallete);
                }
                //return string.Join("", dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.Field<string>("box_no"))));
                return Utility.Instance.ToOneRow(dt, 1, "box_no");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetAllBoxInPalleteExceptBorrowedBox", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M402S01", "GetBoxInPallete");
            }
        }
        public string TranferPalleteTemp(string location, string userId, List<string> listData)
        {
            try
            {
                string palleteOld = listData[0];
                string palleteNew = listData[1];
                string symbol = string.Empty, etdDate = null, etdvnDate = null;
                long? shipSeq = 0;
                string itemGroup = "";
                int status; //status = 0 chuyển về chưa in shipping to, 1: pallete khác đã sx, 3: pallete đã reser, 4: pallete đã scan ship

                listData.RemoveAt(0);
                listData.RemoveAt(0);

                if (!palleteNew.Equals("0"))
                {
                    DataTable dt = uow.App400Repo.GetInfoOfPalleteInBoxInfo(palleteNew);
                    shipSeq = Convert.ToInt32(dt.Rows[0]["ship_seq"].ToString());
                    etdvnDate = dt.Rows[0]["etdvn_date"].ToString();
                    etdDate = dt.Rows[0]["etd_date"].ToString();
                    status = Convert.ToInt32(dt.Rows[0]["status"].ToString());

                    //20190703 check BRQ0008,9
                    DataTable dtPalleteType = uow.App400Repo.GetDestinationInfoOfPallete(palleteNew);
                    symbol = dtPalleteType.Rows[0]["symbol"].ToString().Trim();
                    itemGroup = dtPalleteType.Rows[0]["item_group"].ToString().Trim();
                    if (status == 0)
                    {
                        return AppCommon.ShowErrorMsg(78, "M402S02", palleteNew);
                    }
                    if (string.IsNullOrEmpty(symbol))
                    {
                        return AppCommon.ShowErrorMsg(52, "M402S02", palleteNew);
                    }
                }
                else
                {
                    status = 0;
                }
                if (status == 3 || status == 4)
                {
                    string check = App300.Instance.CheckFiFoBoxInPallete("'" + palleteOld + "'");
                    if (check != "0")
                    {
                        return check;
                    }
                }
                foreach (string box in listData)
                {
                    if (!uow.App400Repo.CheckBoxIsStoredInPallete(palleteOld, box))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(38, "M402S02", box);
                    }
                    if (!palleteNew.Equals("0"))
                    {
                        if (!uow.App300Repo.CheckDestination(box, symbol, location, itemGroup))
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(37, "M402S02", box);
                        }
                    }
                    TdBoxInfo boxinfo = new TdBoxInfo()
                    {
                        box_no = box
                    };
                    switch (status)
                    {
                        case 0:
                            //20190807 neu la thung hang cho phai xoa trong waiting_plan
                            DataTable boxWaiting = uow.App300Repo.CheckBoxInWatingLog(box);
                            if (boxWaiting.Rows.Count > 0)
                            {
                                string item = boxWaiting.Rows[0]["item"].ToString();
                                                                
                                if (uow.App400Repo.GetStatusBoxInfoOfBox(box) >= 1)
                                {
                                    DataTable boxInfo = uow.App400Repo.GetBoxInfo(box);

                                    int qtyBox = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                                    if (uow.App400Repo.UpdateActualQtyWaitingPlan(qtyBox * (-1), item, boxInfo.Rows[0]["pallete_no"].ToString()) < 0)
                                    {
                                        uow.Rollback();
                                        return AppCommon.ShowErrorMsg(94, "M402S02", box);
                                    }
                                }
                            }
                            //20190807
                            boxinfo.pallete_no = "";
                            boxinfo.packing_date = "";
                            boxinfo.packing_time = "";
                            boxinfo.packing_user = "";
                            boxinfo.reserved_date = "";
                            boxinfo.reserved_time = "";
                            boxinfo.reserved_user = "";
                            boxinfo.move_stock_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_stock_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_stock_user = userId;
                            boxinfo.status = 0;
                            boxinfo.move_status = 1;
                            boxinfo.ship_seq = 0;
                            boxinfo.etd_date = "";
                            break;
                        case 1:
                            boxinfo.pallete_no = palleteNew;
                            boxinfo.reserved_date = "";
                            boxinfo.reserved_time = "";
                            boxinfo.reserved_user = "";
                            boxinfo.move_pltemp_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_pltemp_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_pltemp_user = userId;
                            boxinfo.status = 1;
                            boxinfo.move_status = 2;
                            boxinfo.ship_seq = shipSeq;
                            boxinfo.etd_date = etdDate;
                            boxinfo.etdvn_date = etdvnDate;

                            break;
                        case 3:
                            //Check fifo

                            boxinfo.pallete_no = palleteNew;
                            boxinfo.reserved_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.reserved_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.reserved_user = userId;
                            boxinfo.move_plreser_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_plreser_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_plreser_user = userId;
                            boxinfo.status = 3;
                            boxinfo.move_status = 3;
                            boxinfo.ship_seq = shipSeq;
                            boxinfo.etd_date = etdDate;
                            boxinfo.etdvn_date = etdvnDate;
                            break;
                        case 4:
                            //Check fifo
                            boxinfo.pallete_no = palleteNew;
                            boxinfo.reserved_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.reserved_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.reserved_user = userId;
                            boxinfo.move_plreser_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_plreser_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_plreser_user = userId;
                            boxinfo.status = 4;
                            boxinfo.move_status = 4;
                            boxinfo.ship_seq = shipSeq;
                            boxinfo.etd_date = etdDate;
                            boxinfo.etdvn_date = etdvnDate;
                            break;
                    }
                    if (uow.App400Repo.UpdateBoxInfoWhenChangePallete(boxinfo) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M402S02", box);
                    }
                }

                uow.Commit();
                return AppCommon.ShowOkMsg(0, null, null);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "TranferPalleteTemp", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M402S02", "SwapShippingTo");
            }
        }
        public string TransferInstockToReserShip(string location, string userId, List<string> listData)
        {
            try
            {
                string palleteOld = listData[0];
                int status = Convert.ToInt32(listData[1]), statusOfShippingTo = 0;
                string shippingToNew = listData[2];
                string symbol = string.Empty, etdDate = null, etdvnDate = null, item = null;
                long? shipSeq = 0;
                string curMaxLotBox = "", curMinLotBox = "";
                string shipNo = "";
                string itemGroup = "";
                listData.RemoveRange(0, 3);

                DataTable dt = uow.App400Repo.GetInfoOfPalleteInBoxInfo(shippingToNew);
                if (dt.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(81, "M412S02", shippingToNew);
                }
                statusOfShippingTo = Convert.ToInt32(dt.Rows[0]["status"].ToString());
                if (statusOfShippingTo >= 3)
                {
                    if (statusOfShippingTo != 3 && status == 1)
                    {
                        return AppCommon.ShowErrorMsg(84, "M412S02", shippingToNew);
                    }
                    if (statusOfShippingTo != 4 && status == 2)
                    {
                        return AppCommon.ShowErrorMsg(85, "M412S02", shippingToNew);
                    }
                }
                else
                {
                    return AppCommon.ShowErrorMsg(83, "M412S02", shippingToNew);
                }
                if (statusOfShippingTo == 4)
                {
                    shipNo = dt.Rows[0]["ship_no"].ToString();
                }
                shipSeq = Convert.ToInt32(dt.Rows[0]["ship_seq"].ToString());
                etdvnDate = dt.Rows[0]["etdvn_date"].ToString();
                etdDate = dt.Rows[0]["etd_date"].ToString();
                //20190703 check BRQ0008,9
                DataTable dtPalleteType = uow.App400Repo.GetDestinationInfoOfPallete(shippingToNew);
                symbol = dtPalleteType.Rows[0]["symbol"].ToString().Trim();
                itemGroup = dtPalleteType.Rows[0]["item_group"].ToString().Trim();

                //symbol = uow.App400Repo.GetDestinationInfoOfPallete(shippingToNew).Trim();
                foreach (string box in listData)
                {
                    dt = uow.App300Repo.GetCheckBoxInfoOS(palleteOld, box);
                    if (dt.Rows.Count <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(38, "M412S02", box);
                    }
                    if (!uow.App300Repo.CheckDestination(box, symbol, location, itemGroup))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(37, "M412S02", box);
                    }

                    //Check fifo
                    //1 Lay ra max lot cua box
                    dt = uow.App400Repo.GetMinMaxLotOfBox(box);
                    if (dt.Rows.Count <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(38, "M412S02", box);
                    }
                    curMaxLotBox = dt.Rows[0]["max_lot"].ToString().Trim();
                    //curMinLotBox = dt.Rows[0]["min_lot"].ToString().Trim();

                    //2. curMaxLotBox > all box reser or ship
                    item = dt.Rows[0]["item"].ToString().Trim();
                    string minLotOfItemReserOrShipped = uow.App400Repo.GetMinLotBeforeReserOrShipByItem(item, statusOfShippingTo).Trim();
                    if (minLotOfItemReserOrShipped != "" && string.Compare(curMaxLotBox, minLotOfItemReserOrShipped) > 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(46, "M412S02", box);
                    }

                    //3. curMaxLotBox < All hang o nhan hang min(max_lof of box)
                    string maxLotOfItemBeforeStore = uow.App400Repo.GetMinLotOfReceiveByItem(item).Trim();
                    if (maxLotOfItemBeforeStore != "" && string.Compare(curMaxLotBox, maxLotOfItemBeforeStore) > 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(46, "M412S02", box);
                    }

                    //4. curMaxLotBox < All hang da sx chua in shipping to (status = 0) min(max_lot of box)
                    string maxLotOfItemStored = uow.App400Repo.GetMinLotStoredByItem(item, shippingToNew).Trim();
                    if (maxLotOfItemStored != "" && string.Compare(curMaxLotBox, maxLotOfItemStored) > 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(46, "M412S02", box);
                    }

                    //201900808 kiem tra item tren shipping dang lam co phai hang cho ko
                    if (uow.App300Repo.CheckItemIsWaitingItem(item))
                    {
                        if (uow.App300Repo.CheckShippingToIsWaiting(shippingToNew))
                        {
                            if (uow.App300Repo.CheckBoxInWatingLog(box).Rows.Count <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(93, "M412S02", box);
                            }
                            DataTable qtyWaiting = uow.App300Repo.GetQtyWaitingByShippingTo(shippingToNew, item);
                            if (qtyWaiting.Rows.Count <= 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(96, "M412S02", box);
                            }
                            int actualQty = Convert.ToInt32(qtyWaiting.Rows[0]["qty"].ToString());
                            int boxQty = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                            if (actualQty - boxQty >= 0)
                            {
                                if (uow.App400Repo.UpdateActualQtyWaitingPlan(boxQty, item, shippingToNew) <= 0)
                                {
                                    uow.Rollback();
                                    return AppCommon.ShowErrorMsg(94, "M412S02", box);
                                }
                                if (uow.App400Repo.UpdateShippingToForWaitingBox(shippingToNew, box) <= 0)
                                {
                                    uow.Rollback();
                                    return AppCommon.ShowErrorMsg(94, "M412S02", box);
                                }
                            }
                            else
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(94, "M412S01", box);
                            }

                        }
                        else
                        {
                            if (uow.App300Repo.CheckBoxInWatingLog(box).Rows.Count > 0)
                            {
                                uow.Rollback();
                                return AppCommon.ShowErrorMsg(96, "M412S01", box);
                            }
                        }
                    }
                    //End 20190808 kiem tra xu ly hang cho


                    if (statusOfShippingTo == 3)
                    {
                        TdBoxInfo boxInfo = new TdBoxInfo()
                        {
                            pallete_no = shippingToNew,
                            reserved_date = DateTime.Now.ToString("yyyyMMdd"),
                            reserved_time = DateTime.Now.ToString("HHmmss"),
                            reserved_user = userId,
                            status = 3,
                            ship_seq = shipSeq,
                            etd_date = etdDate,
                            etdvn_date = etdvnDate,
                            box_no = box
                        };
                        if (uow.App400Repo.UpdateBoxInfoWhenChangePallete(boxInfo) <= 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(66, "M412S02", box);
                        }
                    }
                    else
                    {             
                        TdBoxInfo boxInfo = new TdBoxInfo()
                        {
                            pallete_no = shippingToNew,
                            ship_no = shipNo,
                            ship_date = DateTime.Now.ToString("yyyyMMdd"),
                            ship_time = DateTime.Now.ToString("HHmmss"),
                            ship_user = userId,
                            status = 4,
                            ship_seq = shipSeq,
                            etd_date = etdDate,
                            etdvn_date = etdvnDate,
                            box_no = box
                        };
                        if (uow.App400Repo.UpdateBoxInfoWhenChangePallete(boxInfo) <= 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(66, "M412S02", box);
                        }

                        dt = uow.App300Repo.GetInfoOfBoxByBoxJobtag(box);
                        if (uow.App400Repo.InsertTTShippingPrintByModel(new TtShippingPrint()
                        {
                            shipping_no = shipNo,
                            box_no = box,
                            item = item,
                            qty_box = Convert.ToInt32(dt.Rows[0]["qty"].ToString().Trim()),
                            job_no = dt.Rows[0]["job_no"].ToString().Trim(),
                            suffix = Convert.ToInt32(dt.Rows[0]["suffix"].ToString().Trim()),
                            lot_no = dt.Rows[0]["lot_no"].ToString().Trim(),
                            box_seq = Convert.ToInt32(dt.Rows[0]["box_seq"].ToString().Trim()),
                            delivery_place = symbol,
                            parentpallete = palleteOld,
                            pallete_no = shippingToNew,
                            entry_date = DateTime.Now.ToString("yyyyMMdd"),
                            entry_user = userId,
                            ship_seq = shipSeq,

                        }) < 0)
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(54, "M412S02", box);
                        }
                    }
                }
                uow.Commit();
                return AppCommon.ShowOkMsg(0, null, null);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "TransferInstockToReserShip", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M412S02", "SwapInStock");
            }
        }
        public string GetAllBoxInPalleteResered(string pallete)
        {
            try
            {
                DataTable dt = uow.App400Repo.GetAllBoxInPalleteReser(pallete);
                if (dt.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(84, "M417S01", pallete);
                }
                //return string.Join("", dt.Rows.OfType<DataRow>().Select(x => string.Join(" ; ", x.Field<string>("box_no"))));
                return Utility.Instance.ToOneRow(dt, 1, "box_no");
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "GetAllBoxInPalleteResered", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M417S01", "GetBoxInPallete");
            }
        }
        public string TranferAfterResered(string location, string userId, List<string> listData)
        {
            try
            {
                string palleteOld = listData[0];
                string palleteNew = listData[1];
                string symbol = string.Empty, etdDate = null, etdvnDate = null;
                long? shipSeq = 0;
                int status; //status = 0 chuyển về chưa in shipping to, 1: pallete khác đã sx, 3: pallete đã reser
                string itemGroup = "";

                listData.RemoveAt(0);
                listData.RemoveAt(0);

                if (!palleteNew.Equals("0"))
                {
                    DataTable dt = uow.App400Repo.GetInfoOfPalleteInBoxInfo(palleteNew);
                    shipSeq = Convert.ToInt32(dt.Rows[0]["ship_seq"].ToString());
                    etdvnDate = dt.Rows[0]["etdvn_date"].ToString();
                    etdDate = dt.Rows[0]["etd_date"].ToString();
                    status = Convert.ToInt32(dt.Rows[0]["status"].ToString());
                    //symbol = uow.App400Repo.GetDestinationInfoOfPallete(palleteNew).Trim();
                    //20190703 check BRQ0008,9
                    DataTable dtPalleteType = uow.App400Repo.GetDestinationInfoOfPallete(palleteNew);
                    symbol = dtPalleteType.Rows[0]["symbol"].ToString().Trim();
                    itemGroup = dtPalleteType.Rows[0]["item_group"].ToString().Trim();

                    if (status == 0)
                    {
                        return AppCommon.ShowErrorMsg(78, "M417S02", palleteNew);
                    }
                    if (string.IsNullOrEmpty(symbol))
                    {
                        return AppCommon.ShowErrorMsg(52, "M417S02", palleteNew);
                    }
                }
                else
                {
                    status = 0;
                }
                foreach (string box in listData)
                {
                    if (!uow.App400Repo.CheckBoxIsReseredInPallete(palleteOld, box))
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(38, "M417S02", box);
                    }
                    if (!palleteNew.Equals("0"))
                    {
                        if (!uow.App300Repo.CheckDestination(box, symbol, location, itemGroup))
                        {
                            uow.Rollback();
                            return AppCommon.ShowErrorMsg(37, "M417S02", box);
                        }
                    }
                    TdBoxInfo boxinfo = new TdBoxInfo()
                    {
                        box_no = box
                    };
                    switch (status)
                    {
                        case 0:
                            boxinfo.pallete_no = "";
                            boxinfo.packing_date = "";
                            boxinfo.packing_time = "";
                            boxinfo.packing_user = "";
                            boxinfo.reserved_date = "";
                            boxinfo.reserved_time = "";
                            boxinfo.reserved_user = "";
                            boxinfo.move_stock_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_stock_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_stock_user = userId;
                            boxinfo.status = 0;
                            boxinfo.move_status = 1;
                            boxinfo.ship_seq = 0;
                            boxinfo.etd_date = "";
                            break;
                        case 1:
                            boxinfo.pallete_no = palleteNew;
                            boxinfo.reserved_date = "";
                            boxinfo.reserved_time = "";
                            boxinfo.reserved_user = "";
                            boxinfo.move_pltemp_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_pltemp_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_pltemp_user = userId;
                            boxinfo.status = 1;
                            boxinfo.move_status = 2;
                            boxinfo.ship_seq = shipSeq;
                            boxinfo.etd_date = etdDate;
                            boxinfo.etdvn_date = etdvnDate;

                            break;
                        case 3:
                            boxinfo.pallete_no = palleteNew;
                            boxinfo.reserved_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.reserved_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.reserved_user = userId;
                            boxinfo.move_plreser_date = DateTime.Now.ToString("yyyyMMdd");
                            boxinfo.move_plreser_time = DateTime.Now.ToString("HHmmss");
                            boxinfo.move_plreser_user = userId;
                            boxinfo.status = 3;
                            boxinfo.move_status = 3;
                            boxinfo.ship_seq = shipSeq;
                            boxinfo.etd_date = etdDate;
                            boxinfo.etdvn_date = etdvnDate;
                            break;
                    }
                    if (uow.App400Repo.UpdateBoxInfoWhenChangePallete(boxinfo) <= 0)
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(66, "M417S02", box);
                    }
                }
                uow.Commit();
                return AppCommon.ShowOkMsg(0, null, null);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "TranferAfterResered", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M417S02", "SwapAfterReser");
            }
        }
        public string CheckBoxCanInsertToShippingTo(string box)
        {
            string result = "";
            try
            {
                bool flag = false;
                DataTable dt = uow.App300Repo.CheckBoxInWatingLog(box);
                if (dt.Rows.Count <= 0)
                {
                    return AppCommon.ShowErrorMsg(93, "M416", box);
                }
                if(!string.IsNullOrEmpty(dt.Rows[0]["shipping_to"].ToString().Trim()))
                {
                    return AppCommon.ShowErrorMsg(107, "M416", box);
                }
                string item = dt.Rows[0]["item"].ToString();
                dt = uow.App300Repo.GetQtyWaitingByItem(item);
                int qtyBox = Convert.ToInt32(uow.App300Repo.GetBoxQtyOfBox(box));
                foreach (DataRow row in dt.Rows)
                {
                    string pallete = "";
                    int waitingQty = Convert.ToInt32(row["qty"].ToString());
                    if (waitingQty - qtyBox >= 0)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        DataTable temp = uow.App300Repo.GetInfoOfShippingTo(row["shipping_to"].ToString().Trim());
                        if (temp.Rows.Count > 0)
                        {
                            string groups = temp.Rows[0]["groups"].ToString().Trim();
                            string detail = temp.Rows[0]["detail"].ToString().Trim();
                            string seqno = temp.Rows[0]["seq_no"].ToString().Trim();
                            pallete = seqno == null ? "" : (groups == "5" || groups == "6") ? detail.Substring(0, 1) + seqno.PadLeft(4, '0') : seqno;
                        }
                        //result += row["shipping_to"].ToString().PadRight(20, ' ');
                        result += pallete.PadRight(20, ' ');
                    }
                }
                return AppCommon.ShowOkMsg(999, "Duoc sap len Pallete", result);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M416", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M416");
            }
        }

        public string CheckStatusShippingTo(string shippingTo)
        {
            string result = "";
            try
            {
                int ret = uow.App400Repo.CheckWaitingShippingToIsOK(shippingTo);
                string palleteNo = uow.App400Repo.GetNumberPallete(shippingTo);
                if (ret > 0) //Thieu
                {
                    DataTable data = uow.App400Repo.GetAllWaitingBoxOfShippingTo(shippingTo);
                    if (data.Rows.Count > 0)
                    {
                        List<BoxNotExist> printList = new List<BoxNotExist>();
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
                                PalleteNo = string.Format("{0} - {1}", shippingTo, palleteNo),
                                Destination = destination
                            };

                            printList.Add(temp);

                        }

                        uow.Commit();
                        List<BoxNotExist> printTemp = printList.OrderBy(p => p.Item).ThenBy(p => p.LotNo).ThenBy(p => p.BoxNo).ToList<BoxNotExist>();

                        Printer<BoxNotExist>.PrintData("BOXNOTINPALLETEANDTITLE", printTemp);

                        return AppCommon.ShowOkMsg(999, "Hang cho da duoc in", result);
                    }
                    int numBoxInComplete = uow.App400Repo.GetNumberBoxIncompleteInPallete(shippingTo);
                    result = numBoxInComplete + " thung";
                    return AppCommon.ShowOkMsg(106, "M419", result);
                }
                if (ret < 0) //Khong phai pallete hang cho
                {
                    return AppCommon.ShowOkMsg(104, "M419", result);
                }
                return AppCommon.ShowOkMsg(103, "M419", result); //Du
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M419", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M419");
            }
        }


        #endregion [FUNCTION'S MY]

        public string FindBox(string boxNo)
        {
            try
            {
                bool ret = uow.App400Repo.FindBox(boxNo);

                if (ret)
                {
                    if(uow.App400Repo.UpdateFlagOfFindBox(boxNo) > 0)
                    {
                        uow.Commit();
                        return AppCommon.ShowErrorMsg(999, "Lay thung nay ra.", boxNo);
                    }
                    else
                    {
                        uow.Rollback();
                        return AppCommon.ShowErrorMsg(999, "Update loi.", boxNo);
                    }
                    
                }
                else
                    return "";

            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M418-Find box", sw);
                }

                uow.Rollback();

                return AppCommon.ShowErrorMsg(777, "M418");
            }

        }
        public string GetTotalBoxInPallete(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.Trim()))
                    return AppCommon.ShowErrorMsg(888, "M414");

                DataTable dtTotalBox = uow.App300Repo.GetTotalBoxOS(data.Trim());
                if (dtTotalBox.Rows.Count < 1)
                {
                    return AppCommon.ShowErrorMsg(69);
                }
                return Utility.Instance.ToOneRow(dtTotalBox);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "M414", sw);
                }
                uow.Rollback();
                return AppCommon.ShowErrorMsg(777, "M414");
            }
        }
    }
}
