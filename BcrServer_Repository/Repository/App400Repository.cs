using System;
using System.Data;
using Npgsql;
using BcrServer_Model;
using System.Collections.Generic;

namespace BcrServer_Repository
{
    public class App400Repository : DataProvider, IApp400Repository
    {
        #region [Constructor]
        NpgsqlTransaction transaction;
        public App400Repository(NpgsqlTransaction _transaction) : base(_transaction)
        {
            this.transaction = _transaction;
        }
        #endregion

        #region [METHOD'S KHOA]
        public int UpdateFinishedLotQty(string boxNo)
        {
            string query = string.Format("UPDATE TR_MPS_INFO_NBCS D SET FINISHED_LOT_QTY = B.FINISHED_LOT_QTY - B.TOTAL_QTY FROM (SELECT AA.MPS_NO, SUM(AA.QTY) TOTAL_QTY, AA.MPS_QTY2, AA.FINISHED_LOT_QTY FROM ( SELECT A.QTY,  A.FINAL_JOB, A.BOX_NO , B.MPS_NO, C.MPS_QTY2, C.FINISHED_LOT_QTY FROM TD_BOX_JOBTAG A INNER JOIN TR_CUR_JOB_NBCS B ON B.JOB_ORDER_NO = A.FINAL_JOB INNER JOIN TR_MPS_INFO_NBCS C ON B.MPS_NO = C.MPS_NO  WHERE BOX_NO = '{0}' ) AA GROUP BY AA.MPS_NO, AA.MPS_QTY2, AA.FINISHED_LOT_QTY) B WHERE D.MPS_NO = B.MPS_NO", boxNo);

            return ExcuteNonQuery(query);
        }

        public int UpdateStatusIncomingBox(string boxNo)
        {
            string query = string.Format("UPDATE TD_INCOMING_BOX A SET STATUS = '1' FROM (SELECT BOX_NO FROM TD_INCOMING_BOX WHERE STATUS = '0' AND BOX_NO = '{0}') B WHERE B.BOX_NO = A.BOX_NO", boxNo);

            return ExcuteNonQuery(query);
        }

        public int InsertBoxReturn(string boxNo, string user)
        {
            string query = string.Format("INSERT INTO TD_BOX_RETURN SELECT TO_CHAR(NOW(), 'YYYYMMDD'), TO_CHAR(NOW(), 'HHMISS'), '{1}' ,  A.BOX_NO, A.PCL_NO, A.QTY, B.RECEIVE_DATE, C.STATUS FROM (SELECT BOX_NO, PCL_NO, SUM(QTY) AS QTY FROM TT_PCL_PRINT WHERE BOX_NO = '{0}' GROUP BY BOX_NO, PCL_NO) A LEFT JOIN TD_BOX_DELIVERY B ON B.BOX_NO = A.BOX_NO LEFT JOIN TD_BOX_INFO C ON C.BOX_NO = A.BOX_NO WHERE A.BOX_NO = '{0}'", boxNo, user);

            return ExcuteNonQuery(query);
        }

        public int DeleteBoxDelivery(string boxNo)
        {
            string query = string.Format("DELETE FROM TD_BOX_DELIVERY WHERE BOX_NO = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }

        public int DeleteBoxInfo(string boxNo)
        {
            string query = string.Format("DELETE FROM TD_BOX_INFO WHERE BOX_NO = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }

        public int DeletePclPrint(string boxNo)
        {
            string query = string.Format("DELETE FROM TT_PCL_PRINT WHERE BOX_NO = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }

        public int DeleteDailyBoxRec(string boxNo)
        {
            string query = string.Format("DELETE FROM TD_DAILY_BOX_REC WHERE BOX_NO = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }

        public DataTable GetBoxInfo(string boxNo)
        {
            string query = string.Format("SELECT * FROM TD_BOX_INFO WHERE BOX_NO = '{0}'; ", boxNo);
            return ExcuteQuery(query);
        }

        /// <summary>
        /// Cap nhat trang thai box dang cho muon hay da lay lai tu san xuat
        /// </summary>
        /// <param name="type">0: cho san xuat muon - 1: nhan lai tu san xuat</param>
        /// <param name="boxNo"></param>
        /// <param name="pallete"></param>
        /// <returns></returns>
        public int UpdateBoxInfoStatusReturn(int type = 0, string boxNo = null, string pallete = null)
        {
            string query = string.Empty;

            if (type == 0)
            {
                query = string.Format("UPDATE TD_BOX_INFO SET STATUS_RETURN = 1, BORROWED_DATE = TO_CHAR(NOW(), 'YYYYMMDD'),BORROWED_TIME = TO_CHAR(NOW(), 'HHMISS') WHERE BOX_NO = '{0}'  AND STATUS_RETURN <> 1", boxNo);
            }
            else if (type == 1) //UPDATE BY PALLETE_NO
            {
                query = string.Format("UPDATE TD_BOX_INFO SET STATUS_RETURN = 2, RETURNWH_DATE = TO_CHAR(NOW(), 'YYYYMMDD'), RETURNWH_TIME = TO_CHAR(NOW(), 'HHMISS') WHERE PALLETETYPE_NO= '{0}' AND BOX_NO = '{1}' AND STATUS = 0 AND STATUS_RETURN = 1", pallete, boxNo);
            }
            else if (type == 2) //UPDATE BY SHIPPING TO
            {
                query = string.Format("UPDATE TD_BOX_INFO SET STATUS_RETURN = 2, RETURNWH_DATE = TO_CHAR(NOW(), 'YYYYMMDD'), RETURNWH_TIME = TO_CHAR(NOW(), 'HHMISS') WHERE PALLETE_NO= '{0}' AND BOX_NO  = '{1}' AND STATUS_RETURN = 1", pallete, boxNo);
            }

            return ExcuteNonQuery(query);
        }

        /// <summary>
        /// Get box no with condition
        /// </summary>
        /// <param name="pallete">So Pallete (POS - POR)</param>
        /// <param name="shippingTo">So Shipping To (TOS - TOR)</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable GetBoxInfoReturnWithCondition(string pallete = null, string shippingTo = null, int type = 0)
        {
            string query = string.Empty;

            switch(type)
            {
                case 0:
                    query = string.Format("SELECT TRIM(BOX_NO) FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS_RETURN = 1", shippingTo);
                    break;
                case 1:
                    query = string.Format("SELECT TRIM(BOX_NO) FROM TD_BOX_INFO WHERE PALLETETYPE_NO = '{0}' AND STATUS = 0 AND STATUS_RETURN = 1", pallete);
                    break;
            }

            return ExcuteQuery(query);
        }

        public DataTable GetBoxForSwitchPallete(string pallete)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_INFO WHERE PALLETETYPE_NO = '{0}' AND STATUS = '0' AND STATUS_RETURN <> 1", pallete);
            return ExcuteQuery(query);
        }

        public bool IsBoxStored(string pallete, string boxNo)
        {
            string query = string.Format("SELECT BOX_NO FROM TD_BOX_DELIVERY WHERE PALLET_NO= '{0}' AND BOX_NO= '{1}' AND STORE_STATUS='1'", pallete, boxNo);
            return ExcuteQuery(query).Rows.Count > 0;
        }

        public int UpdatePalleteInBoxDelivery(string pallete, string boxNo)
        {
            string query = string.Format("UPDATE TD_BOX_DELIVERY SET PALLET_NO = '{0}' WHERE BOX_NO = '{1}'", pallete, boxNo);
            return ExcuteNonQuery(query);
        }

        public int UpdatePalleteInBoxInfo(string pallete, string boxNo)
        {
            string query = string.Format("UPDATE TD_BOX_INFO SET PALLETETYPE_NO = '{0}' WHERE BOX_NO = '{1}'", pallete, boxNo);
            return ExcuteNonQuery(query);
        }

        public DataTable GetEtdBoxInfo(string shippingTo)
        {
            //string query = string.Format("SELECT A.PALLETE_NO, A.ETD_DATE, A.ETDVN_DATE, A.SHIP_SEQ , B.SYMBOL FROM TD_BOX_INFO  A LEFT JOIN TD_PALLETE_WH B ON B.PALLETE_NO = A.PALLETE_NO WHERE A.PALLETE_NO = '{0}' AND A.STATUS = 1 LIMIT 1", shippingTo);
            string query = string.Format("SELECT A.PALLETE_NO, A.ETD_DATE, A.ETDVN_DATE, A.SHIP_SEQ , B.SYMBOL, A.PALLETETYPE_NO FROM TD_BOX_INFO  A LEFT JOIN TD_PALLETE_WH B ON B.PALLETE_NO = A.PALLETE_NO WHERE A.PALLETE_NO = '{0}' AND A.STATUS = 1 LIMIT 1", shippingTo);
            return ExcuteQuery(query);
        }

        public DataTable GetAllBoxFromShipping(string shippingTo)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS = '4'", shippingTo);

            return ExcuteQuery(query);
        }

        public int UpdateStatusOfBoxReturnShippingToReceived(string shipping, string box, string date, string time, string user)
        {

            string query = string.Format("UPDATE TD_BOX_INFO SET STATUS= '0',PALLETE_NO='',SHIP_SEQ=0,ETD_DATE='',ETDVN_DATE='',MOVE_STOCK_DATE='{0}',MOVE_STOCK_TIME='{1}',MOVE_STOCK_USER='{2}',MOVE_STATUS=5 WHERE PALLETE_NO = '{3}' AND BOX_NO= '{4}' AND STATUS='4'", date, time, user, shipping, box);

            return ExcuteNonQuery(query);
        }

        public int DeleteTTShipingPrint(string boxNo)
        {
            string query = string.Format("DELETE FROM TT_SHIPPING_PRINT WHERE BOX_NO = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }

        #endregion [METHOD'S KHOA]


        #region [METHOD'S DUONG]

        #region [OS] TRA THUNG HANG TU INSTOCK VE KHU VUC RECEIVE

        public DataTable GetBoxFromPallete(string pallete)
        {
            string query = string.Format("SELECT trim(box_no) FROM td_box_info WHERE palletetype_no = '{0}' and status = 0 group by box_no", pallete);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetDataBoxInfo(string palleteTypeNo, string boxNo)
        {
            string query = string.Format("SELECT box_no, item, product_code, status, qty_box, palletetype_no, ship_no, pallete_no, etd_date, etdvn_date FROM td_box_info WHERE palletetype_no = '{0}' and box_no = '{1}' and status = '0'", palleteTypeNo, boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdBoxDelivery(string boxNo, string userId)
        {
            string query = string.Format("UPDATE td_box_delivery SET pallet_no = '', store_status = '0', store_date = '', store_user = '', returnreceive_status = '1', returnreceive_date = '{0}',returnreceive_time = '{1}', returnreceive_user = '{2}' WHERE box_no = '{3}' ", DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), userId, boxNo);

            return ExcuteNonQuery(query, null, CommandType.Text);
        }

        public int DelBoxInfo(string palleteTypeNo, string boxNo)
        {
            string query = string.Format("DELETE FROM td_box_info WHERE palletetype_no= '{0}' and box_no = '{1}' and status = '0'", palleteTypeNo, boxNo);

            return ExcuteNonQuery(query, null, CommandType.Text);
        }

        public int InsertTsStockResult(string values)
        {
            int ret = 0;

            string query = string.Format("INSERT INTO TS_STOCK_RESULT (box_no, data_sign, item, status, update_user, trn_no, memo) VALUES {0} ", values);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }
        #endregion

        #region [OR] TRA THUNG HANG TU INSTOCK VE KHU VUC RECEIVE
        public DataTable GetDataBoxInfo(string boxNo)
        {
            string query = string.Format("SELECT box_no, item, product_code, status, qty_box, palletetype_no, ship_no, pallete_no, etd_date, etdvn_date FROM td_box_info WHERE box_no = '{0}' and status <= '4'", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdBoxDelivery(string boxNo)
        {
            string query = string.Format("UPDATE td_box_delivery SET pallet_no = '', store_status = '0', store_date = '' WHERE box_no = '{0}' ", boxNo);

            return ExcuteNonQuery(query, null, CommandType.Text);
        }

        public int DelBoxInfo(string boxNo)
        {
            string query = string.Format("DELETE FROM td_box_info WHERE box_no = '{0}'", boxNo);

            return ExcuteNonQuery(query, null, CommandType.Text);
        }
        #endregion

        #region THAY DOI NGAY ETD VA SO LAN XUAT
        public DataTable GetDataPallete(string pallete)
        {
            string query = string.Format("SELECT box_no, item, product_code, status, qty_box, palletetype_no, stock, ship_no, pallete_no, ship_seq, etd_date, etdvn_date, shipping_sequence FROM td_box_info WHERE pallete_no = '{0}' AND status > 0", pallete);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetDataPalleteTypeNo(string palleteTypeNo)
        {
            string query = string.Format("SELECT * FROM td_palletetype_wh where palletetype_no = '{0}'", palleteTypeNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdBoxInFo(string setQuery, string palleteNo)
        {
            string query = string.Format("UPDATE td_box_info set {0} WHERE pallete_no = '{1}' AND status > 0", setQuery, palleteNo);

            return ExcuteNonQuery(query, null, CommandType.Text);
        }
        #endregion

        #endregion [METHOD'S DUONG]

        #region [METHOD'S HIEU]
        public DataTable GetTotalBoxOS(string palleteNo)
        {
            string query = string.Format("SELECT trim(box_no) as box_no FROM TD_BOX_INFO WHERE pallete_no='{0}' and status = 1 and status_return <> 1  ", palleteNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetPalleteOS(string palleteNo)
        {
            string query = string.Format("SELECT item, pallete_no FROM TD_BOX_INFO WHERE pallete_no = '{0}' and status = 1 and status_return <> 1 ", palleteNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetCheckPallete(string palleteNo)
        {
            string query = string.Format("SELECT * FROM TD_BOX_INFO WHERE pallete_no = '{0}' and status < 4 ", palleteNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetSeq(string palleteNo)
        {
            string query = string.Format("SELECT * FROM td_pallete_wh WHERE pallete_no = '{0}' ", palleteNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetLostShippingToOS(string palltete)
        {
            string query = string.Format("SELECT a.pallete_no, ship_seq, c.seq_no as pallete_no1, detail, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,(SELECT max(seq_no) seq_no FROM td_pallete_print WHERE pallete_no = '{0}') rev, a.etd_date, etdvn_date,count_item,count_box,d.groups ,c.symbol  FROM td_box_info a left join (SELECT a.box_no,max(job_no || '-' || to_char(suffix, 'FM000')) job_order_no ,max(lot_no || ' ' || box_seq) box_num, sum(b.qty) qty FROM td_box_info a left join td_box_jobtag b on a.box_no = b.box_no WHERE a.pallete_no = '{0}' group by a.box_no )b on a.box_no = b.box_no left join td_pallete_wh c on a.pallete_no = c.pallete_no left join td_mastersymbol_wh d on c.symbol = d.symbol left join(SELECT  pallete_no, count(aa.item) count_item, sum(ctn) count_box FROM (SELECT pallete_no, item, count(a.item) as ctn FROM td_box_info a WHERE pallete_no = '{0}' group by pallete_no, item) aa group by pallete_no)bb on a.pallete_no = bb.pallete_no WHERE a.pallete_no = '{0}' and a.status = 4 group by a.pallete_no,c.seq_no, ship_seq, c.symbol, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,  a.etd_date, etdvn_date,count_item,count_box,d.groups ,d.detail order by item,box_no,box_num desc", palltete);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public int SetUpdatePalleteWHSeq(string palleteNo, int seqNo)
        {
            string query = string.Format("update td_pallete_wh set seq_no = {0} WHERE pallete_no = '{1}'", seqNo, palleteNo);
            return ExcuteNonQuery(query);
        }
        #endregion [METHOD'S HIEU]


        #region [METHOD'S MY]
        public bool CheckBoxIsStoredInPallete(string pallete, string box)
        {
            string query = string.Format("SELECT * FROM td_box_info WHERE pallete_no = '{0}' AND box_no = '{1}' AND status = '1'", pallete, box);
            DataTable dt = ExcuteQuery(query);
            return (dt.Rows.Count > 0);
        }

        public DataTable GetAllBoxInPalleteExceptBorrowedBox(string pallete)
        {
            string query = string.Format("SELECT * FROM td_box_info WHERE pallete_no = '{0}' AND status=1 AND status_return <> 1 ", pallete);
            return ExcuteQuery(query);
        }

        public DataTable GetDestinationInfoOfPallete(string pallete)
        {
            string query = string.Format("SELECT a.* FROM td_palletetype_wh a INNER JOIN td_pallete_wh b on a.palletetype_no = b.parentpallete_no WHERE pallete_no = '{0}'", pallete);
            return ExcuteQuery(query);
        }

        public DataTable GetInfoOfPalleteInBoxInfo(string pallete)
        {
            string query = string.Format("SELECT ship_no, status, ship_seq, etd_date, etdvn_date FROM td_box_info WHERE pallete_no = '{0}' GROUP BY 1, 2, 3, 4, 5", pallete);
            return ExcuteQuery(query);
        }

        public int UpdateBoxInfoWhenChangePallete(TdBoxInfo box, List<string> listBox = null)
        {
            string addBox = "";
            if (listBox == null)
            {
                addBox = string.Format(" = '{0}'", box.box_no);
            }
            else
            {
                addBox = string.Format(" IN ('{0}')", string.Join("','", listBox.ToArray()));
            }
            string query = string.Format("UPDATE td_box_info SET pallete_no=COALESCE(@pallete_no, pallete_no), packing_date=COALESCE(@packing_date, packing_date), packing_time=COALESCE(@packing_time, packing_time), packing_user=COALESCE(@packing_user, packing_user), reserved_date=COALESCE(@reserved_date, reserved_date), reserved_time=COALESCE(@reserved_time, reserved_time), reserved_user=COALESCE(@reserved_user, reserved_user),move_stock_date=COALESCE(@move_stock_date, move_stock_date),move_stock_time=COALESCE(@move_stock_time, move_stock_time),move_stock_user=COALESCE(@move_stock_user, move_stock_user), status=COALESCE(@status, status),move_status=COALESCE(@move_status, move_status),ship_seq=COALESCE(@ship_seq, ship_seq),etd_date=COALESCE(@etd_date, etd_date), move_pltemp_date=COALESCE(@move_pltemp_date, move_pltemp_date), move_pltemp_time=COALESCE(@move_pltemp_time, move_pltemp_time), move_pltemp_user=COALESCE(@move_pltemp_user, move_pltemp_user),move_plreser_date=COALESCE(@move_plreser_date, move_plreser_date), move_plreser_time=COALESCE(@move_plreser_time, move_plreser_time), move_plreser_user=COALESCE(@move_plreser_user, move_plreser_user), etdvn_date=COALESCE(@etdvn_date, etdvn_date), ship_no=COALESCE(@ship_no, ship_no), ship_date=COALESCE(@ship_date, ship_date), ship_time=COALESCE(@ship_time, ship_time), ship_user=COALESCE(@ship_user, ship_user) WHERE box_no " + addBox);


            return ExcuteNonQuery(query, new System.Collections.Generic.Dictionary<string, object>()
            {
                {"pallete_no", box.pallete_no},
                {"reserved_date", box.reserved_date},
                {"reserved_time", box.reserved_time},
                {"reserved_user", box.reserved_user},
                {"move_pltemp_date", box.move_pltemp_date},
                {"move_pltemp_time", box.move_pltemp_time},
                {"move_pltemp_user", box.move_pltemp_user},
                {"status", box.status},
                {"move_status", box.move_status},
                {"ship_seq", box.ship_seq},
                {"etd_date", box.etd_date},
                {"etdvn_date", box.etdvn_date},
                {"packing_date", box.packing_date},
                {"packing_time", box.packing_time},
                {"packing_user", box.packing_user},
                {"move_stock_date", box.move_stock_date},
                {"move_stock_time", box.move_stock_time},
                {"move_stock_user", box.move_stock_user},
                {"move_plreser_date", box.move_plreser_date },
                {"move_plreser_time", box.move_plreser_time },
                {"move_plreser_user", box.move_plreser_user },
                {"ship_no", box.ship_no },
                {"ship_date", box.ship_date },
                {"ship_time", box.ship_time },
                {"ship_user", box.ship_user }
            });
        }

        public DataTable GetMinMaxLotOfBox(string box)
        {
            string query = string.Format("SELECT item, MAX(lot_no) max_lot, MIN(lot_no) min_lot FROM td_box_jobtag WHERE box_no = '{0}' GROUP BY item", box);
            return ExcuteQuery(query);
        }
        public string GetMinLotBeforeReserOrShipByItem(string item, int status)
        {
            //20190424 - MY
            //string query = string.Format("SELECT MIN(lot_no) lot_no FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.item = '{0}' AND a.status < {1}", item, status);

            string query = string.Format("SELECT MIN(C.LOT_NO) LOT_NO FROM (SELECT A.BOX_NO, MAX(B.LOT_NO) LOT_NO FROM TD_BOX_INFO A INNER JOIN TD_BOX_JOBTAG B ON A.BOX_NO = B.BOX_NO WHERE B.ITEM = '{0}' AND A.STATUS < '{1}' GROUP BY A.BOX_NO) C", item, status);
            return ExcuteQuery(query).Rows[0]["lot_no"].ToString().Trim();
        }
        public string GetMinLotOfReceiveByItem(string item)
        {
            string query = string.Format("SELECT MIN(C.LOT_NO) LOT_NO FROM (SELECT a.box_no, MAX(lot_no) lot_no FROM td_box_jobtag a INNER JOIN td_box_delivery b ON a.box_no = b.box_no WHERE a.item = '{0}' AND b.store_status = '0' AND b.income_status = '1' GROUP BY a.box_no) C", item);
            return ExcuteQuery(query).Rows[0]["lot_no"].ToString().Trim();
        }
        public int InsertTTShippingPrintByModel(TtShippingPrint shipping)
        {
            string query = string.Format("INSERT INTO tt_shipping_print(entry_date, entry_time, entry_user, box_no, item, qty_box, qty, lot_no, job_no, suffix, parentpallete, delivery_place, box_seq, shipping_no, pallete_no, ship_seq, loc) VALUES (@entry_date, @entry_time, @entry_user, @box_no, @item, @qty_box, @qty, @lot_no, @job_no, @suffix, @parentpallete, @delivery_place, @box_seq, @shipping_no, @pallete_no, @ship_seq, @loc)");
            return ExcuteNonQuery(query, new System.Collections.Generic.Dictionary<string, object>()
            {
                {"entry_date", shipping.entry_date},
                {"entry_time", shipping.entry_time},
                {"entry_user", shipping.entry_user},
                {"box_no", shipping.box_no},
                {"item", shipping.item},
                {"qty_box", shipping.qty_box},
                {"qty", shipping.qty},
                {"lot_no", shipping.lot_no},
                {"job_no", shipping.job_no},
                {"suffix", shipping.suffix},
                {"parentpallete", shipping.parentpallete},
                {"delivery_place", shipping.delivery_place},
                {"box_seq", shipping.box_seq},
                {"shipping_no", shipping.shipping_no},
                {"pallete_no", shipping.pallete_no},
                {"ship_seq", shipping.ship_seq},
                {"loc", shipping.loc}
            });
        }
        public DataTable GetAllBoxInPalleteReser(string pallete)
        {
            string query = string.Format("SELECT * FROM td_box_info WHERE pallete_no = '{0}' AND status < 4 AND status_return <> 1 ", pallete);
            return ExcuteQuery(query);
        }
        public bool CheckBoxIsReseredInPallete(string pallete, string box)
        {
            string query = string.Format("SELECT * FROM td_box_info WHERE pallete_no = '{0}' AND box_no = '{1}' AND status < 4", pallete, box);
            DataTable dt = ExcuteQuery(query);
            return (dt.Rows.Count > 0);
        }
        public int UpdateActualQtyWaitingPlan(int boxQty, string item, string ShippingTo)
        {
            string query = string.Format("UPDATE td_wh_waiting_plan SET actual_qty = actual_qty + {0} WHERE item = '{1}' AND shipping_to = '{2}' AND due_date >= TO_CHAR(current_date, 'YYYYMMDD')", boxQty, item, ShippingTo);
            return ExcuteNonQuery(query);
        }
        public int GetStatusBoxInfoOfBox(string boxno)
        {
            string query = string.Format("SELECT status FROM td_box_info WHERE box_no = '{0}'", boxno);
            return Convert.ToInt32(ExcuteQuery(query).Rows[0]["status"].ToString());
        }

        public string GetMinLotStoredByItem(string item, string exceptShippingTo)
        {
            string query = string.Format("SELECT MIN(C.LOT_NO) LOT_NO FROM (SELECT A.BOX_NO, MAX(B.LOT_NO) LOT_NO FROM TD_BOX_INFO A INNER JOIN TD_BOX_JOBTAG B ON A.BOX_NO = B.BOX_NO WHERE B.ITEM = '{0}' AND A.STATUS = '0' AND A.PALLETE_NO NOT IN ('{1}') GROUP BY A.BOX_NO) C", item, exceptShippingTo);
            return ExcuteQuery(query).Rows[0]["lot_no"].ToString().Trim();
        }
        public string GetMaxLotOfReserShipOS(string item)
        {
            string query = string.Format("SELECT max(lot_no) lot_no FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.status > '3'  AND product_code = 'FG00' AND a.item = '{0}'", item);

            return ExcuteQuery(query).Rows[0]["lot_no"].ToString().Trim();
        }
        #endregion [METHOD'S MY]

        public bool FindBox(string boxNo)
        {
            string query = string.Format("select * from td_find_box where box_no = '{0}'", boxNo);
            return ExcuteQuery(query).Rows.Count > 0;
        }

        public int UpdateFlagOfFindBox(string boxNo)
        {
            string query = string.Format("UPDATE td_find_box SET FLAG = '1' WHERE box_no = '{0}'", boxNo);
            return ExcuteNonQuery(query);
        }
        public bool CheckIsExistInInComingBox(string boxno)
        {
            string query = string.Format("SELECT * FROM td_incoming_box WHERE box_no = '{0}'", boxno);
            return ExcuteQuery(query).Rows.Count > 0;
        }
        public int CheckWaitingShippingToIsOK(string shippingTo)
        {
            string query = string.Format("select (sum(waiting_qty) - sum(actual_qty)) balance from td_wh_waiting_plan where  shipping_to = '{0}' and due_date >= '{1}'", shippingTo, DateTime.Now.ToString("yyyyMMdd"));
            DataTable dt = ExcuteQuery(query);
            int balanceQty = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[0]["balance"].ToString()) ? "-1" : dt.Rows[0]["balance"].ToString());
            return balanceQty;
        }
        public int UpdateShippingToForWaitingBox(string shippingTo, string box)
        {
            string query = string.Format("UPDATE td_waiting_plan_log SET shipping_to = '{0}' WHERE box_no = '{1}'", shippingTo, box);
            return ExcuteNonQuery(query);
        }
        public DataTable GetAllWaitingBoxOfShippingTo(string shippingTo)
        {
            string query = string.Format("SELECT a.box_no, a.item, max(lot_no) lot_no, sum(b.qty) qty FROM td_waiting_plan_log a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.item IN (SELECT item FROM td_wh_waiting_plan WHERE (SELECT sum(waiting_qty) - sum(actual_qty) FROM td_wh_waiting_plan) > 0 AND shipping_to = '{0}' AND due_date >= '{1}' ) AND (shipping_to IS NULL OR shipping_to = '') AND due_date >= '{1}' GROUP BY a.box_no, a.item", shippingTo, DateTime.Now.ToString("yyyyMMdd"));
            return ExcuteQuery(query);
        }
        public string GetNumberPallete(string shippingTo)
        {
            string query = string.Format("SELECT CASE WHEN d.groups = 5 OR d.groups = 6 THEN (SUBSTRING(d.detail, 1, 1) || to_char(b.seq_no, 'FM0000')) ELSE b.seq_no::text END pallete FROM td_pallete_wh b INNER JOIN td_palletetype_wh c ON b.parentpallete_no = c.palletetype_no LEFT JOIN td_mastersymbol_wh d ON c.symbol = d.symbol WHERE b.pallete_no  = '{0}'", shippingTo);
            DataTable dt = ExcuteQuery(query);
            return dt.Rows.Count > 0 ? dt.Rows[0]["pallete"].ToString() : "";
        }
        public int GetNumberBoxIncompleteInPallete(string shippingto)
        {
            string query = string.Format("SELECT sum((waiting_qty - actual_qty)/box_qty) numbox FROM td_wh_waiting_plan where shipping_to = '{0}'", shippingto);
            DataTable dt = ExcuteQuery(query);
            return string.IsNullOrEmpty(dt.Rows[0]["numbox"].ToString()) ? 0 : Convert.ToInt32(dt.Rows[0]["numbox"].ToString());
        }

        //public DataTable GetStatusOfBox(string boxno)
        //{
        //    string query = string.Format("SELECT status FROM td_box_info WHERE box_no = '{0}'");
        //}
    }
}
