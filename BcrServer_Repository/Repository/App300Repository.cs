using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using BcrServer_Model;

namespace BcrServer_Repository
{
    public class App300Repository : DataProvider, IApp300Repository
    {
        #region [Constructor]
        NpgsqlTransaction transaction;
        public App300Repository(NpgsqlTransaction _transaction) : base(_transaction)
        {
            this.transaction = _transaction;
        }
        #endregion

  
        public string CheckingPclReceived(string pclNo)
        {
            string result = string.Empty;

            string query = string.Format("SELECT PCL_NO FROM TD_BOX_DELIVERY WHERE WAITING_WH_STATUS = '1' AND INCOME_STATUS = '1' AND PCL_NO in ({0})", pclNo);

            DataTable dt = ExcuteQuery(query);

            if (dt.Rows.Count > 0)
                result = dt.Rows[0] == null ? "" : dt.Rows[0]["PCL_NO"].ToString();

            return result;
        }

        public DataTable GetAllBoxByPCL(string pclNo)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_DELIVERY WHERE WAITING_WH_STATUS = '1' AND INCOME_STATUS = '0' AND PCL_NO in ({0}); ", pclNo);
           
            //for test
            //string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_DELIVERY WHERE PCL_NO in ({0}); ", pclNo);
            return ExcuteQuery(query);
        }

        public DataTable GetAllBoxORWithoutPCL()
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO  FROM TD_BOX_DELIVERY A WHERE A.WAITING_WH_STATUS = '1' AND A.INCOME_STATUS = '0' AND A.PCL_NO LIKE 'LOR1%' GROUP BY BOX_NO ");
            return ExcuteQuery(query);
        }

        public DataTable GetAllBoxMitsubaByPCL(string pclNo, string items)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_DELIVERY WHERE WAITING_WH_STATUS = '1' AND INCOME_STATUS = '0' AND PCL_NO in ({0}) and item in ({1}); ", pclNo, items);
            
            //for test
            //string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_DELIVERY WHERE PCL_NO in ({0}) and item in ({1}); ", pclNo, items);
            return ExcuteQuery(query);
        }

        public DataTable FindBoxNotScannedYet(string boxes)
        {
            string query = string.Format("select (ROW_NUMBER () OVER (ORDER BY aa.item, aa.lot) || '.' || aa.item || ' ' || aa.lot) as item from ( select a.box_no, b.item, max(b.lot_no || TRIM(b.box_seq_or)) as lot from td_box_delivery a left join td_box_jobtag b on b.box_no = a.box_no where  a.WAITING_WH_STATUS = '1' AND a.INCOME_STATUS = '0' and a.box_no in ({0}) group by a.box_no, b.item) aa", boxes);
            //string query = string.Format("select (ROW_NUMBER () OVER (ORDER BY aa.item, aa.lot) || '.' || aa.item || ' ' || aa.lot) as item from ( select a.box_no, b.item, max(b.lot_no || trim(b.box_seq_or)) as lot from td_box_delivery a left join td_box_jobtag b on b.box_no = a.box_no where a.box_no in ({0}) group by a.box_no, b.item) aa", boxes);

            return ExcuteQuery(query);
        }

        public int UpdateIncomeStatusOfBox(string pclList, string userId, string instockDate)
        {
            string incomeTime = DateTime.Now.ToString("HHmmss");

            string query = string.Format("UPDATE TD_BOX_DELIVERY SET INCOME_STATUS='1', INCOME_DATE = '{2}', INCOME_TIME = '{3}', RECEIVE_DATE = '{2}', INCOME_USER = '{1}', WAITING_WH_USER = '{1}' WHERE WAITING_WH_STATUS='1' AND INCOME_STATUS='0' AND PCL_NO IN ({0})", pclList, userId, instockDate, incomeTime);

            return ExcuteNonQuery(query);
        }

        public int InsertUnpostedJobBySelect(string pclList, string userId)
        {
            string insTemplate = "INSERT INTO TT_UNPOSTEDJOB_NBCS ";
            string selTemplate = string.Format("SELECT NEXTVAL('seq_unpostedjob_nbcs') AS  SEQ_ID, TO_CHAR(NOW(), 'YYYYMMDD')::int AS JOB_DATE, TO_CHAR(NOW(), 'HH24MISS')::int AS ENTRY_TIME, '{1}' AS USER_ID, A.FINAL_JOB,  A.ITEM,A.QTY, 'crUnpostedJob' AS UPDATE_PGM, 0 AS ACTIVE_SIGN, A.PCL_NO_ORG, (CASE WHEN SUBSTR(A.PCL_NO_ORG,1,4) = 'LOS1' THEN 'S' ELSE 'R' END) || SUBSTR(A.PCL_NO_ORG,8,9) AS PCL_NO, B.TOTAL_RECORDS FROM (SELECT PCL_NO AS PCL_NO_ORG, FINAL_JOB, ITEM, SUM(QTY) AS QTY FROM TT_PCL_PRINT WHERE PCL_NO IN ({0}) GROUP BY PCL_NO, FINAL_JOB, ITEM ORDER BY PCL_NO) A INNER JOIN (SELECT PCL_NO, COUNT(*) AS TOTAL_RECORDS FROM (SELECT PCL_NO, FINAL_JOB FROM TT_PCL_PRINT WHERE PCL_NO IN ({0}) GROUP BY PCL_NO, FINAL_JOB) AA GROUP BY PCL_NO) B ON B.PCL_NO = A.PCL_NO_ORG", pclList, userId);

            string query = string.Format("{0} {1}", insTemplate, selTemplate);

            return ExcuteNonQuery(query);
        }

        public DataTable GetAllBoxByPallete(string pallete)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) FROM TD_BOX_INFO A INNER JOIN TD_PALLETETYPE_WH B ON B.PALLETETYPE_NO = A.PALLETETYPE_NO INNER JOIN TD_MASTERSYMBOL_WH C ON C.SYMBOL = B.SYMBOL INNER JOIN (SELECT D.PALLETETYPE_NO, D.SYMBOL, E.GROUPS FROM TD_PALLETETYPE_WH D INNER JOIN TD_MASTERSYMBOL_WH E ON E.SYMBOL = D.SYMBOL WHERE D.PALLETETYPE_NO = '{0}' AND D.STATUS = '0') F ON F.GROUPS = C.GROUPS WHERE A.STATUS = 0 GROUP BY A.BOX_NO", pallete);

            return ExcuteQuery(query);
        }
        public DataTable GetGroupNumberByPallete(string pallete, int type = 0)
        {
            string query = string.Empty;

            switch (type)
            {
                case 0:
                    query = string.Format("SELECT B.GROUPS, B.SYMBOL, A.ITEM_GROUP FROM TD_PALLETETYPE_WH A INNER JOIN TD_MASTERSYMBOL_WH B ON B.SYMBOL = A.SYMBOL WHERE A.PALLETETYPE_NO  = '{0}'", pallete);
                    break;
                case 1:
                    query = string.Format("SELECT B.GROUPS, MAX(C.ITEM_GROUP) AS ITEM_GROUP FROM TD_PALLETE_WH A INNER JOIN TD_MASTERSYMBOL_WH B ON B.SYMBOL = A.SYMBOL INNER JOIN TD_PALLETETYPE_WH C ON C.PALLETETYPE_NO = A.PARENTPALLETE_NO WHERE A.PALLETE_NO IN ({0}) GROUP BY  B.GROUPS", pallete);
                    break;
            }

            return ExcuteQuery(query);
        }

        public DataTable GetBoxInfoByManyBox(string boxes, int groups)
        {
            string query = string.Format("SELECT A.BOX_NO FROM TD_BOX_INFO A INNER JOIN TD_PALLETETYPE_WH B ON B.PALLETETYPE_NO = A.PALLETETYPE_NO INNER JOIN TD_MASTERSYMBOL_WH C ON C.SYMBOL = B.SYMBOL WHERE A.STATUS = 0 AND C.GROUPS = {0} AND A.BOX_NO IN ({1}) GROUP BY A.BOX_NO", groups, boxes);

            return ExcuteQuery(query);
        }

        public int InsertTdCheckStockBySelect(string pallete, string symbol, string box, string userId)
        {
            string _time = DateTime.Now.ToString("HHmmss");

            string query = string.Format("INSERT INTO TD_CHECK_STOCK SELECT TO_CHAR(NOW(), 'YYYYMMDD'), '{4}', '{3}' AS USER_ID,  A.BOX_NO, A.ITEM, A.QTY , A.LOT_NO, A.JOB_NO, A.SUFFIX, '{0}' AS PALLETETYPE_NO, '{1}' AS SYMBOL, A.BOX_SEQ, 0, 0, B.PALLETE_NO  FROM TD_BOX_JOBTAG A LEFT JOIN TD_BOX_INFO B ON B.BOX_NO = A.BOX_NO WHERE A.BOX_NO IN ({2})", pallete, symbol, box, userId, _time);

            return ExcuteNonQuery(query);
        }

        public int DeleteTdCheckStockByBoxNo(string boxNo)
        {
            string query = string.Format("DELETE FROM TD_CHECK_STOCK WHERE BOX_NO IN  ({0})", boxNo);

            return ExcuteNonQuery(query);
        }

        public DataTable GetAllPalletebyGroup(int groups)
        {
            string query = string.Format("SELECT PALLETE_NO FROM TD_BOX_INFO A INNER JOIN TD_PALLETETYPE_WH B ON B.PALLETETYPE_NO = A.PALLETETYPE_NO INNER JOIN TD_MASTERSYMBOL_WH C ON C.SYMBOL = B.SYMBOL WHERE C.GROUPS = {0} AND A.STATUS = '1' AND A.PRODUCT_CODE ='FG00' GROUP BY PALLETE_NO", groups);

            return ExcuteQuery(query);
        }

        public DataTable GetBoxInfoNotShipByPallete(string pallete)
        {
            string query = string.Format("SELECT BOX_NO, PALLETE_NO, PALLETETYPE_NO FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS < 4", pallete);
            return ExcuteQuery(query);
        }

        public DataTable CheckBoxStatusByBoxNo(string boxNo)
        {
            string query = string.Format("SELECT A.BOX_NO, A.PALLET_NO, A.INCOME_STATUS, b.BOX_NO, B.PALLETE_NO, B.PALLETETYPE_NO, B.STATUS FROM TD_BOX_DELIVERY A LEFT JOIN TD_BOX_INFO B ON B.BOX_NO = A.BOX_NO WHERE A.BOX_NO = '{0}'", boxNo);
            return ExcuteQuery(query);
        }

        public DataTable GetPalletePrintedShippingToForOR()
        {
            string query = "SELECT PALLETE_NO  FROM TD_BOX_INFO A WHERE A.PRODUCT_CODE = 'FG10' AND  A.STATUS = 1 GROUP BY PALLETE_NO ";

            return ExcuteQuery(query);
        }


        public DataTable GetAllBoxByPclNoOR(string pclNo)
        {
            string query = string.Format("SELECT b.item, b.job_no ||'-'|| to_char(b.suffix,'FM000') as job_order_no, (b.lot_no ||' '||b.box_seq_or) as lot_no, (b.work_center || '/' || b.machine) as wc,  b.qty, b.box_no, c.moc_qc, c.so_thung as box_num, a. waiting_wh_date, a.waiting_wh_user, a.waiting_wh_time, b.pcl_no FROM td_box_delivery a LEFT JOIN td_daily_box_rec c on a.box_no = c.box_no INNER JOIN tt_pcl_print b on a.pcl_no = b.pcl_no and a.box_no = b.box_no WHERE a.pcl_no = '{0}' and a.waiting_wh_status='1' and a.income_status='0'  ORDER BY b.item, b.lot_no || b.box_seq_or, b.pcl_no, b.box_no ", pclNo);

            return ExcuteQuery(query);
        }

        public DataTable GetPlcRecorded()
        {
            string query = string.Format("SELECT pcl_no FROM td_box_delivery WHERE waiting_wh_status = '1' AND income_status = '0' AND pcl_no like 'LOR1%' GROUP BY pcl_no");
            //string query = string.Format("SELECT pcl_no FROM td_box_delivery WHERE pcl_no = 'LOR1201910150018' GROUP BY pcl_no");

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetCountBoxInTdBoxDelivery(string pclNo)
        {
            string query = string.Format("SELECT box_no FROM td_box_delivery WHERE pcl_no = '{0}'", pclNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetCountBoxInTtPclPrint(string pclNo)
        {
            string query = string.Format("SELECT box_no FROM tt_pcl_print WHERE pcl_no = '{0}' group by box_no", pclNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public bool IsBoxNoRecoredFI(string boxNo)
        {
            string query = string.Format("SELECT * FROM td_daily_box_rec WHERE box_no = '{0}'", boxNo);
            return ExcuteQuery(query).Rows.Count > 0;
        }

        public bool IsBoxNoReceived(string boxNo)
        {
            string query = string.Format("SELECT * FROM tt_pcl_print WHERE box_no = '{0}' order by lot_no || box_seq_or asc", boxNo);
            return ExcuteQuery(query).Rows.Count > 0;
        }

        public long? GetTrnNoOfTsStockResult(string tableName)
        {
            string query = string.Format("SELECT * FROM TD_REC_COUNTER WHERE table_name = '{0}'", tableName);
            DataTable dt = ExcuteQuery(query);

            return (long?)Convert.ToDouble(dt.Rows[0]["current_no"].ToString());
        }

        public DataTable OS1GetALLDataReservedPallete(string shippingTos)
        {
            string query = string.Format("SELECT B.item, max(B.lot_no) as lot_no, B.entry_date FROM td_box_info A INNER JOIN td_box_jobtag B ON A.box_no = B.box_no WHERE pallete_no IN ({0}) AND A.status < 4 GROUP BY B.item, B.entry_date ORDER BY item asc", shippingTos);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable OS1GetAllDataReceivedInStockNotInPallete(string item)
        {
            string query = string.Format("SELECT distinct '' as pallete_no, max(lot_no) as lot_no, B.entry_date FROM td_box_delivery A INNER JOIN td_box_jobtag B ON A.box_no = B.box_no WHERE B.item= '{0}' AND income_status = '1' AND store_status = '0' AND B.entry_date >= (TO_CHAR(CURRENT_DATE - INTERVAL '1 Years', 'YYYY') || '0101') GROUP BY pallete_no, B.entry_date ORDER BY lot_no asc", item);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int InsertOrUpdateFifo(string dataCurrent, string dataToCheck, string lotPrevious, string LotCurrent, string item, int status, int types)
        {
            int ret = 0;
            string whereQuery = string.Empty;
            string boxTemp = string.Empty;
            string boxFiFo = string.Empty;

            string getBoxByItemAndLot = string.Format("SELECT A.box_no , B.item, lot_no, A.status, entry_date FROM td_box_info A INNER JOIN td_box_jobtag B ON A.box_no = B.box_no WHERE A.pallete_no IN ({0}) AND B.item = '{1}' AND B.lot_no = '{2}' limit 1", dataCurrent, item, lotPrevious);

            DataTable dtGetItemByLot = ExcuteQuery(getBoxByItemAndLot, null, CommandType.Text);

            if (dtGetItemByLot.Rows.Count <= 0)
                return 38; // khong tim thay thung
            boxTemp = dtGetItemByLot.Rows[0]["box_no"].ToString();

            switch (types)
            {
                case 0:
                    whereQuery = string.Format("item = '{0}' AND income_status = '1' AND store_status = '0' AND entry_date >= (TO_CHAR(CURRENT_DATE - INTERVAL '1 Years', 'YYYY') || '0101')", item);
                    break;
                case 1:
                    whereQuery = string.Format("A.pallete_no = '{0}' AND B.item = '{1}' AND B.lot_no = '{2}' AND A.status <= 1", dataToCheck, item, LotCurrent);
                    break;
                case 2:
                    whereQuery = string.Format("A.pallete_no = '{0}' AND B.item = '{1}' AND B.lot_no = '{2}' AND A.status >= 3", dataToCheck, item, LotCurrent);
                    break;
                case 3:
                    whereQuery = string.Format("A.pallete_no = '{0}' AND B.item = '{1}' AND B.lot_no = '{2}' AND A.status < 4", dataToCheck, item, LotCurrent);
                    break;
                default:
                    break;
            }

            if (types > 0)
            {
                string query = string.Format("SELECT A.box_no , B.item, lot_no, A.status, entry_date FROM td_box_info A INNER JOIN td_box_jobtag B ON A.box_no = B.box_no WHERE {0} limit 1", whereQuery);

                DataTable dtBoxFiFo = ExcuteQuery(query, null, CommandType.Text);

                if (dtBoxFiFo.Rows.Count <= 0)
                    return 38; // khong tim thay thung

                boxFiFo = dtBoxFiFo.Rows[0]["box_no"].ToString();
            }
            else
            {
                string query = string.Format("SELECT entry_date, box_no, item FROM td_box_delivery WHERE {0}", whereQuery);
                DataTable dtBoxDelevery = ExcuteQuery(query, null, CommandType.Text);

                if (dtBoxDelevery.Rows.Count <= 0)
                    return 38; // khong tim thay thung

                boxFiFo = dtBoxDelevery.Rows[0]["box_no"].ToString();
            }

            DataTable dtCheckBoxExistsBoxFiFo = ExcuteQuery(string.Format("SELECT * FROM td_box_fifo WHERE box_no = '{0}' and box_no_ff = '{1}' and status = {2}", boxTemp, boxFiFo, status), null, CommandType.Text);

            if (dtCheckBoxExistsBoxFiFo.Rows.Count <= 0)
            {
                //insert
                string query = "INSERT INTO td_box_fifo(box_no, box_no_ff, status, entry_date, entry_time, types) VALUES (@box_no, @box_no_ff, @status, @entry_date, @entry_time, @types); ";
                ret = ExcuteNonQuery(query, new Dictionary<string, object> {
                    {"box_no", boxTemp },
                    {"box_no_ff", boxFiFo },
                    {"status", status },
                    {"entry_date", DateTime.Now.ToString("yyyyMMdd") },
                    {"entry_time", DateTime.Now.ToString("HHmmss") },
                    {"types", types }
                });

                if (ret <= 0)
                {
                    return 40; // Khong the them vao td_box_info
                }
            }
            else
            {
                //update
                string query = string.Format("UPDATE td_box_fifo SET update_date = '{0}', update_time = '{1}'  WHERE box_no = '{2}' and box_no_ff = '{3}' ", DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), boxTemp, boxFiFo);

                ret = ExcuteNonQuery(query, null, CommandType.Text);
                if (ret <= 0)
                {
                    return 48; //Khong the cap nhap du lieu Box Info
                }
            }

            return 0;
        }

        public DataTable OS1GetItemNotInPalleteAndNotShipByDate(string item, string shippingTos, string date)
        {
            string query = string.Format("SELECT DISTINCT A.pallete_no, max(lot_no) as lot_no, B.entry_date FROM td_box_info A INNER JOIN td_box_jobtag B ON B.box_no = A.box_no LEFT JOIN td_pallete_wh C ON C.pallete_no = A.pallete_no WHERE B.item = '{0}' AND A.pallete_no NOT IN ({1}) AND A.status < 4 AND B.entry_date < '{2}' GROUP BY A.pallete_no, B.entry_date  ORDER BY lot_no asc", item, shippingTos, date);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable OS1GetDataPallete(string pallete)
        {
            string query = string.Format("SELECT * FROM TD_PALLETE_WH WHERE pallete_no = '{0}'", pallete);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetSeq(string columnsName, string entryDate)
        {
            string query = string.Format("SELECT ship_no FROM td_box_info WHERE ship_no > '{0}{1}001' limit 1", columnsName, entryDate);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int GetSeqForAll(int mode, string seqName)
        {
            DataTable dtSeqNo;
            int ret = 0;
            string whereQuery = string.Empty;

            if (mode == 1)
            {
                whereQuery = string.Format("SELECT NEXTVAL('{0}') as nextval", seqName); //raise
            }
            else if (mode == 0)
            {
                //whereQuery = string.Format("ALTER SEQUENCE {0} RESTART WITH 100", seqName); //20190406 comment out
                //whereQuery = string.Format("SELECT NEXTVAL('{0}')", seqName); //20190406 comment out
                whereQuery = string.Format("SELECT setval('{0}', 100, TRUE) as nextval", seqName); //Reset
            }

            dtSeqNo = ExcuteQuery(whereQuery, null, CommandType.Text);

            ret = Convert.ToInt32(dtSeqNo.Rows[0]["nextval"]);

            return ret;
        }

        public DataTable OS1GetShippingToInTdBoxInfo(string palleteNo)
        {
            string query = string.Format("SELECT * FROM td_box_info WHERE pallete_no = '{0}' AND status < 4", palleteNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetBoxJobTagByBoxInShippingTo(string boxNo)
        {
            string query = string.Format("SELECT * FROM td_box_jobtag WHERE box_no = '{0}'", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int InsertTtShippingPrint(string value)
        {
            int ret = 0;
            string query = string.Format("INSERT INTO TT_SHIPPING_PRINT (ENTRY_DATE, ENTRY_USER, SHIPPING_NO, BOX_NO, ITEM, QTY_BOX, LOT_NO, JOB_NO, SUFFIX, BOX_SEQ, PARENTPALLETE, PALLETE_NO, LOC, DELIVERY_PLACE) VALUES {0}", value);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public int UpdTdCheckStock(string query)
        {
            int ret = 0;

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public int UpdTdBoxInfo(string numberShipSeq, string entryDate, string entryTime, string userId, string pallete)
        {
            int ret = 0;

            string query = string.Format("UPDATE TD_BOX_INFO SET ship_no = '{0}', ship_time = '{1}', ship_date = '{2}', ship_user = '{3}', status = 4 WHERE pallete_no = '{4}' AND status < 4", numberShipSeq, entryTime, entryDate, userId, pallete);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public int InsertBoxTraceBySelect(string pallete)
        {
            int ret = 0;

            string query = string.Format("INSERT INTO TS_BOX_TRACE( COMPANY_CODE, PLANT_CODE, BOX_NO, LINE_GROUP, LINENO, JOB_ORDER_NO, MPS_NO, FINISHED_GOODS_CODE, CUSTOMER_CODE, CUSTOMER_ORDER_NO, CENTER_CD, SHIP_TO_STOCK_LOCATION, STOCK_ACCOUNT_NO, BOX_QTY, STAFF_ID, STOCKIN_DATE, MAIN_LOT_NO, LOT_NO_01, LOT_QTY_01, LOT_NO_02, LOT_QTY_02, LOT_NO_03, LOT_QTY_03, LOT_NO_04, LOT_QTY_04, LOT_NO_05, LOT_QTY_05, LOT_NO_06, LOT_QTY_06, LOT_NO_07, LOT_QTY_07, LOT_NO_08, LOT_QTY_08, ENTRY_DATE, ENTRY_TIME) SELECT AA.COMPANY_CODE, AA.PLANT_CODE, AA.BOX_NO, AA.LINE_GROUP, AA.LINE_NO, AA.JOB_ORDER_NO, AA.MPS_NO, AA.FINISHED_GOODS_CODE, AA.CUSTOMER_CODE, AA.CUSTOMER_ORDER_NO, AA.CENTER_CD, AA.SHIP_TO_STOCK_LOCATION, AA.STOCK_ACCOUNT_NO, AA.BOX_QTY, AA.STAFF_ID, AA.STOCKIN_DATE, AA.MAIN_LOT_NO, SPLIT_PART(CC.ARR[1], '*', 1) AS LOT_NO_01, COALESCE(SPLIT_PART(CC.ARR[1], '*', 2), '0')::INTEGER AS LOT_QTY_01, SPLIT_PART(CC.ARR[2], '*', 1) AS LOT_NO_02, COALESCE(SPLIT_PART(CC.ARR[2], '*', 2), '0')::INTEGER AS LOT_QTY_02, SPLIT_PART(CC.ARR[3], '*', 1) AS LOT_NO_03, COALESCE(SPLIT_PART(CC.ARR[3], '*', 2), '0')::INTEGER AS LOT_QTY_03, SPLIT_PART(CC.ARR[4], '*', 1) AS LOT_NO_04, COALESCE(SPLIT_PART(CC.ARR[4], '*', 2), '0')::INTEGER AS LOT_QTY_04, SPLIT_PART(CC.ARR[5], '*', 1) AS LOT_NO_05, COALESCE(SPLIT_PART(CC.ARR[5], '*', 2), '0')::INTEGER AS LOT_QTY_05, SPLIT_PART(CC.ARR[6], '*', 1) AS LOT_NO_06, COALESCE(SPLIT_PART(CC.ARR[6], '*', 2), '0')::INTEGER AS LOT_QTY_06, SPLIT_PART(CC.ARR[7], '*', 1) AS LOT_NO_07, COALESCE(SPLIT_PART(CC.ARR[7], '*', 2), '0')::INTEGER AS LOT_QTY_07, SPLIT_PART(CC.ARR[8], '*', 1) AS LOT_NO_08, COALESCE(SPLIT_PART(CC.ARR[8], '*', 2), '0')::INTEGER AS LOT_QTY_08, TO_CHAR(CURRENT_DATE, 'YYYYMMDD')::INTEGER AS ENTRY_DATE, TO_CHAR(NOW(), 'HH24MISS')::INTEGER AS ENTRY_TIME FROM (SELECT B.COMPANY_CODE, B.PLANT_CODE, A.BOX_NO, B.LINE_GROUP, B.LINE_NO, A.STARTING_JOB AS JOB_ORDER_NO, B.MPS_NO, B.FINISHED_GOODS_CODE, B.CUSTOMER_CODE, B.CUSTOMER_ORDER_NO, C.MAIN_STOCK_LOCATION AS CENTER_CD, C.SHIP_TO_STOCK_LOCATION, C.STOCK_ACCOUNT_NO, E.SHIP_USER AS STAFF_ID, D.RECEIVE_DATE::INTEGER AS STOCKIN_DATE, F.MAIN_LOT_NO, SUM(A.QTY) AS BOX_QTY FROM TD_BOX_JOBTAG A INNER JOIN TR_CUR_JOB_NBCS B ON B.JOB_ORDER_NO = A.STARTING_JOB INNER JOIN TR_MPS_INFO_NBCS C ON C.MPS_NO = B.MPS_NO INNER JOIN TD_BOX_DELIVERY D ON D.BOX_NO = A.BOX_NO INNER JOIN TD_BOX_INFO E ON E.BOX_NO = A.BOX_NO AND E.STATUS = '4' INNER JOIN (SELECT Z.BOX_NO, FN_MAKE_LOTDUC(PACK_DATE) || (CASE WHEN LOCATION_CD = 'OS1' THEN ' ' || BOX_SEQ::TEXT ELSE BOX_SEQ_OR END) AS MAIN_LOT_NO FROM TD_BOX_JOBTAG Z INNER JOIN (SELECT BOX_NO, STARTING_JOB FROM TD_BOX_JOBTAG JJ WHERE EXISTS (SELECT 'X' FROM TD_BOX_INFO WHERE BOX_NO = JJ.BOX_NO AND PALLETE_NO = '{0}')  GROUP BY BOX_NO, STARTING_JOB  ) FF ON Z.JOB_NO || '-' || TO_CHAR(Z.SUFFIX, 'FM000') = FF.STARTING_JOB AND Z.BOX_NO = FF.BOX_NO) F ON F.BOX_NO = A.BOX_NO WHERE E.PALLETE_NO = '{0}' GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15, 16) AA INNER JOIN (SELECT BB.BOX_NO, ARRAY_AGG(LOT_NO || '*' || QTY) ARR FROM (SELECT BOX_NO, FN_MAKE_LOTDUC(PACK_DATE) || (CASE WHEN LOCATION_CD = 'OS1' THEN ' ' || BOX_SEQ::TEXT ELSE BOX_SEQ_OR END) AS LOT_NO,QTY FROM TD_BOX_JOBTAG X WHERE EXISTS (SELECT 'X' FROM TD_BOX_INFO Y WHERE Y.BOX_NO = X.BOX_NO AND Y.PALLETE_NO = '{0}') ORDER BY PACK_DATE) BB GROUP BY 1) CC ON CC.BOX_NO = AA.BOX_NO;", pallete);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public DataTable ORGetDataPallete(string shippingTos)
        {
            string query = string.Format("SELECT * FROM TD_PALLETE_WH WHERE pallete_no = '{0}'", shippingTos);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable ORGetDataPalleteInBoxInfo(string shippingTos)
        {
            string query = string.Format("SELECT box_no, item, status, palletetype_no, pallete_no, stock, ship_seq, ship_no FROM td_box_info WHERE pallete_no IN ({0}) AND status < 4", shippingTos);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable ORGetDataByPallete(string shippingTo)
        {
            //string query = string.Format("SELECT box_no, item, status, palletetype_no, pallete_no, stock, ship_seq, ship_no FROM td_box_info WHERE pallete_no = '{0}' AND status < 4", shippingTo);
            string query = string.Format("SELECT a.box_no, item, status, palletetype_no, pallete_no, stock, ship_seq, ship_no FROM td_box_info a INNER JOIN (SELECT x.box_no, max(lot_no || box_seq_or) lot_no FROM td_box_info x INNER JOIN td_box_jobtag y ON x.box_no = y.box_no WHERE x.status < 4 AND pallete_no = '{0}' GROUP BY x.box_no) b ON a.box_no = b.box_no WHERE pallete_no = '{0}' AND status < 4 ORDER BY item, b.lot_no DESC", shippingTo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdTdBoxInfoOR(string numberShipSeq, string entryDate, string entryTime, string userId, string pallete)
        {
            int ret = 0;

            string query = string.Format("UPDATE TD_BOX_INFO SET ship_no = '{0}', ship_time = '{1}', ship_date = '{2}', ship_user = '{3}', status = 4 WHERE pallete_no = '{4}' AND status < 4 ", numberShipSeq, entryTime, entryDate, userId, pallete);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public DataTable SelectBoxFifo(string where)
        {
            string query = string.Format("select * from td_box_fifo where {0}", where);

            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable SelectBoxInfo(string where)
        {
            string query = string.Format("select * from td_box_info where {0}", where);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable SelectGroupItem_BJBI(string palleteNo)
        {
            string query = string.Format("SELECT B.ITEM,MAX(B.LOT_NO) AS LOT_NO, B.ENTRY_DATE FROM TD_BOX_INFO A INNER JOIN TD_BOX_JOBTAG B ON A.BOX_NO = B.BOX_NO WHERE PALLETE_NO IN ({0}) GROUP BY B.ITEM,B.ENTRY_DATE ORDER BY ITEM ASC", palleteNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable SelectWithoutFrom(string item)
        {
            string query = string.Format("select distinct '' as pallete_no,max(lot_no) as lot_no,bj.entry_date from td_box_delivery bd inner join td_box_jobtag bj on bj.box_no = bd.box_no where bj.item='{0}' and income_status='1' and store_status='0' and bj.entry_date >= (TO_CHAR(CURRENT_DATE - INTERVAL '1 Years', 'YYYY') || '0101') group by pallete_no,bj.entry_date order by lot_no asc", item);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetMaxLotByItemOnReceive(string item)
        {
            string query = string.Format("SELECT '' AS PALLETE_NO, MAX(LOT_NO) AS LOT_NO, B.ENTRY_DATE FROM TD_BOX_DELIVERY A INNER JOIN TD_BOX_JOBTAG B ON B.BOX_NO = A.BOX_NO WHERE B.ITEM = '{0}' AND INCOME_STATUS='1' AND STORE_STATUS='0' AND B.ENTRY_DATE >= (TO_CHAR(CURRENT_DATE - INTERVAL '1 YEARS', 'YYYY') || '0101') GROUP BY PALLETE_NO, B.ENTRY_DATE ORDER BY LOT_NO ASC", item);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="palleteNo"></param>
        /// <param name="date"></param>
        /// <param name="count">so lan kiem tra</param>
        /// <returns></returns>
        public DataTable SelectLotInfo(string item, string palleteNo, string date, int count)
        {
            string query = "";

            if (count == 1)
                query = string.Format("select distinct bi.pallete_no,max(lot_no) as lot_no,bj.entry_date from td_box_info bi inner join td_box_jobtag bj on bj.box_no = bi.box_no left join td_pallete_wh pw on pw.pallete_no = bi.pallete_no where bj.item='{0}' and bi.pallete_no not in ({1}) and bi.status<=1 and bj.entry_date<'{2}' group by bi.pallete_no,bj.entry_date  order by lot_no asc", item, palleteNo, date);
            else
                query = string.Format("select distinct bi.pallete_no,max(lot_no) as lot_no,bj.entry_date from td_box_info bi inner join td_box_jobtag bj on bj.box_no = bi.box_no left join td_pallete_wh pw on pw.pallete_no = bi.pallete_no where bj.item='{0}' and bi.pallete_no not in ({1}) and bi.status >= 3 and bj.entry_date >='{2}' group by bi.pallete_no,bj.entry_date  order by lot_no asc", item, palleteNo, date);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable SelectBoxTop1(string where)
        {
            string query = string.Format("select * from td_box_info bi inner join td_box_jobtag bj on bj.box_no = bi.box_no where {0} limit 1", where);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable SelectBoxDelivery(string where)
        {
            string query = string.Format("select * from td_box_delivery where {0} ", where);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdBoxFifo(string set, string where)
        {
            int ret = 0;
            string query = string.Format("UPDATE TD_BOX_FIFO SET {0} WHERE {1}", set, where);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public int UpdBoxInfo(string set, string where)
        {
            int ret = 0;
            string query = string.Format("UPDATE TD_BOX_INFO SET {0} WHERE {1}", set, where);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            return ret;
        }

        public int InsertBoxFifo(string boxNo, string boxNoFf, string notes, int status, string entryDate, string entryTime, int types)
        {
            string query = "INSERT INTO td_box_fifo VALUES (@boxNo, @boxNoFf, @notes, @status, @entryDate, @entryTime, @types); ";
            return ExcuteNonQuery(query, new Dictionary<string, object> {
                {"boxNo", boxNo },
                {"boxNoFf",boxNoFf },
                {"notes", notes },
                {"status", status },
                {"entryDate", entryDate },
                {"entryTime", entryTime },
                {"types", types }
            });
        }

        #region[Shipping to]
        public DataTable GetTotalBoxOS(string pallete)
        {
            string query = string.Format("SELECT TRIM(BOX_NO) AS BOX_NO FROM TD_BOX_INFO WHERE PALLETETYPE_NO = '{0}' AND STATUS = 0", pallete);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetTotalBoxOR(string pallete)
        {
            string query = string.Format("SELECT trim(box_no) as box_no FROM TD_BOX_INFO WHERE pallete_no = '{0}' and status = 1", pallete);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetPalleteOR(string boxNo)
        {
            string query = string.Format("SELECT pallete_no, item, palletetype_no FROM TD_BOX_INFO WHERE box_no = '{0}'", boxNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetCheckBoxInfoOS(string pallete, string boxNo)
        {
            string query = string.Format("SELECT * FROM TD_BOX_INFO WHERE palletetype_no = '{0}' and box_no = '{1}' and status = 0", pallete, boxNo);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetCheckPallete(string pallete)
        {
            string query = string.Format("SELECT * FROM td_palletetype_wh WHERE palletetype_no = '{0}'", pallete);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetCheckMaterSymbol(string symbol)
        {
            string query = string.Format("SELECT * FROM td_mastersymbol_wh WHERE symbol = '{0}'", symbol);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetCheckCtrlGroupSeq(int groups, string year)
        {
            string query = string.Format("SELECT * FROM td_control_group_seq WHERE groups={0} and years='{1}'", groups, year);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetRecCodeWH(string tableName, string date)
        {
            string query = string.Format("SELECT * FROM TD_REC_CODE_WH WHERE table_name='{0}' and cdate='{1}'", tableName, date);
            return ExcuteQuery(query, null, CommandType.Text);
        }
        public DataTable GetSeq(string tableNameSeq)
        {
            string query = string.Format("SELECT NEXTVAL('{0}');", tableNameSeq);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetMaxPalletePrint(string palltete)
        {
            string query = string.Format("SELECT max(seq_no) as seq_no FROM td_pallete_print WHERE pallete_no='{0}' group by pallete_no", palltete);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetShippingToPrint(string palltete, int status = 0, string location = null)
        {
            string where = "";

            switch(status)
            {
                case 0:
                    where = " and a.status > 0";
                    break;
                case 1:
                    where = " and a.status = 1";
                    break;
                case 4:
                    where = " and a.status <= 4";
                    break;
            }

            

            //string query = string.Format("SELECT a.pallete_no, ship_seq, c.seq_no  as pallete_no1, detail, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,(SELECT max(seq_no) seq_no FROM td_pallete_print WHERE pallete_no = '{0}') rev, a.etd_date, etdvn_date,count_item,count_box,d.groups ,c.symbol, e.symbol as symbol1 FROM td_box_info a left join (SELECT a.box_no,max(job_no || '-' || to_char(suffix, 'FM000')) job_order_no ,max(lot_no || ' ' || case '{2}' when 'OS1' then text(box_seq) else box_seq_or end) box_num, sum(b.qty) qty FROM td_box_info a left join td_box_jobtag b on a.box_no = b.box_no WHERE a.pallete_no = '{0}' group by a.box_no )b on a.box_no = b.box_no left join td_pallete_wh c on a.pallete_no = c.pallete_no left join td_mastersymbol_wh d on c.symbol = d.symbol and d.location_cd = '{2}' left join(SELECT  pallete_no, count(aa.item) count_item, sum(ctn) count_box FROM (SELECT pallete_no, item, count(a.item) as ctn FROM td_box_info a WHERE pallete_no = '{0}' group by pallete_no, item) aa group by pallete_no)bb on a.pallete_no = bb.pallete_no  LEFT JOIN td_palletetype_wh e on e.palletetype_no = a.palletetype_no WHERE a.pallete_no = '{0}' {1} group by a.pallete_no,c.seq_no, ship_seq, c.symbol, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,  a.etd_date, etdvn_date,count_item,count_box,d.groups ,d.detail, e.symbol order by item,box_no,box_num desc", palltete, where, location);


            string query = string.Format("SELECT a.pallete_no, ship_seq, c.seq_no  as pallete_no1, detail, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,(SELECT max(seq_no) seq_no FROM td_pallete_print WHERE pallete_no = '{0}') rev, a.etd_date, etdvn_date,count_item,count_box,d.groups ,c.symbol, e.symbol as symbol1 FROM td_box_info a left join (SELECT CC.BOX_NO, STRING_AGG(CC.JOB_ORDER_NO , '   ') AS JOB_ORDER_NO, MAX(JOB_ORDER_NO),  MAX(LOT_NO || ' ' || CASE '{2}' WHEN 'OS1' THEN TEXT(BOX_SEQ) ELSE BOX_SEQ_OR END) BOX_NUM, SUM(CC.QTY) AS QTY  FROM (SELECT A.BOX_NO, B.JOB_NO || '-' || TO_CHAR(B.SUFFIX, 'FM000') AS JOB_ORDER_NO, B.LOT_NO, B.BOX_SEQ, B.BOX_SEQ_OR, B.QTY FROM TD_BOX_INFO A LEFT JOIN TD_BOX_JOBTAG B ON A.BOX_NO = B.BOX_NO WHERE A.PALLETE_NO = '{0}' ORDER BY B.LOT_NO DESC) CC GROUP BY CC.BOX_NO) B ON A.BOX_NO = B.BOX_NO left join td_pallete_wh c on a.pallete_no = c.pallete_no left join td_mastersymbol_wh d on c.symbol = d.symbol and d.location_cd = '{2}' left join(SELECT  pallete_no, count(aa.item) count_item, sum(ctn) count_box FROM (SELECT pallete_no, item, count(a.item) as ctn FROM td_box_info a WHERE pallete_no = '{0}' group by pallete_no, item) aa group by pallete_no)bb on a.pallete_no = bb.pallete_no  LEFT JOIN td_palletetype_wh e on e.palletetype_no = a.palletetype_no WHERE a.pallete_no = '{0}' {1} group by a.pallete_no,c.seq_no, ship_seq, c.symbol, packing_user, a.item,job_order_no, a.box_no, box_num, b.qty,  a.etd_date, etdvn_date,count_item,count_box,d.groups ,d.detail, e.symbol order by item,box_no,box_num desc", palltete, where, location);
            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int SetInsertCtrlGroupSeq(int groups, string groupName, string years, int seqNo, string loc)
        {
            string query = string.Format("insert into td_control_group_seq (groups,group_name,years,seq_no,loc) values({0},'{1}','{2}',{3},'{4}')", groups, groupName, years, seqNo, loc);
            return ExcuteNonQuery(query);
        }
        public int SetUpdateCtrlGroupSeq(int groups, string years, int seqNo, string loc)
        {
            string query = string.Format("update td_control_group_seq set seq_no = {0} WHERE groups = {1} and years = '{2}' and loc = '{3}'", seqNo, groups, years, loc);
            return ExcuteNonQuery(query);
        }
        public int SetInsertRecCodeWH(string tableName, string date, int currentNo, string updDate)
        {
            string query = string.Format("insert into td_rec_code_wh (table_name, cdate, current_no, start_no, end_no, upd_date) values('{0}','{1}',{2},1,999999,'{3}')", tableName, date, currentNo, updDate);
            return ExcuteNonQuery(query);
        }
        public int SetUpdateRecCodeWH(int currentNo, string tableName, string date)
        {
            string query = string.Format("update TD_REC_CODE_WH set current_no = {0} WHERE table_name='{1}' and cdate='{2}'", currentNo, tableName, date);
            return ExcuteNonQuery(query);
        }
        public int SetResetSeq(string tableNameSeq)
        {
            //string query = string.Format("ALTER SEQUENCE {0}  RESTART WITH 100;", tableNameSeq);
            string query = string.Format("SELECT setval('{0}', 100, TRUE)", tableNameSeq);
            return ExcuteQuery(query).Rows.Count;
        }
        public int SetUpdateBoxInfoWH(string palleteNo, string date, string time, string user, int seq, string edtHCM, string edtVN, string palletetypeNo, string box)
        {
            string query = string.Format("update TD_BOX_INFO set status='1', pallete_no='{0}', packing_date='{1}', packing_time='{2}', packing_user='{3}', ship_seq={4}, etd_date='{5}', etdvn_date='{6}' WHERE palletetype_no='{7}' and box_no='{8}' and status='0'", palleteNo, date, time, user, seq, edtHCM, edtVN, palletetypeNo, box);
            return ExcuteNonQuery(query);
        }

        public int SetInsertPalleteWH(string palleteNo, string palletetypeNo, string item, int seq, string symbol, string status, string entryDate, string entryUser)
        {
            string query = string.Format(" insert into td_pallete_wh(pallete_no, parentpallete_no, item, seq_no, symbol, status, entry_date, entry_user) values('{0}','{1}','{2}',{3},'{4}','{5}','{6}','{7}')", palleteNo, palletetypeNo, item, seq, symbol, status, entryDate, entryUser);
            return ExcuteNonQuery(query);
        }

        public int SetUpdatePalleteWH(string palleteNo, string palletetypeNo, string item, string status, string entryDate)
        {
            string query = string.Format("  update td_pallete_wh set status ='{0}' WHERE pallete_no='{1}' and parentpallete_no='{2}' and entry_date='{3}'", status, palleteNo, palletetypeNo, entryDate);
            return ExcuteNonQuery(query);
        }


        public int SetInsertPalletePrint(string palleteNo, int seqNo, string date, string time)
        {
            string query = string.Format("insert into td_pallete_print(pallete_no, seq_no, entry_date, entry_time) values('{0}',{1},'{2}','{3}')", palleteNo, seqNo, date, time);
            return ExcuteNonQuery(query);
        }

        public int UpdatePackingUser(string shippingTo, string userId)
        {
            string query = string.Format("update td_box_info set packing_user = '{0}' where pallete_no = '{1}'", userId, shippingTo);
            return ExcuteNonQuery(query);
        }

        #endregion

        public DataTable GetPalleteDetailByBox(string boxno)
        {
            string query = string.Format("SELECT * FROM td_box_info a INNER JOIN td_pallete_wh b ON a.pallete_no = b.pallete_no WHERE a.box_no = '{0}'",
                boxno);
            return ExcuteQuery(query);
        }
        public bool CheckBoxNoIsExistInPallete(string boxno)
        {
            string query = string.Format("SELECT palletetype_no FROM td_box_info WHERE box_no = '{0}'", boxno);
            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count > 0)
                return true;
            return false;
        }

        public int CheckBoxNoConditionalsToStored(string boxno)
        {
            string query = string.Format("SELECT * FROM td_box_delivery WHERE box_no = '{0}'", boxno);
            DataTable dt = ExcuteQuery(query);
            if(dt.Rows.Count <= 0)
            {
                return -2; //Chua PCL
            }
            query = string.Format("SELECT * FROM td_box_delivery WHERE box_no = '{0}' AND income_status = '0'", boxno);
            dt = ExcuteQuery(query);
            if(dt.Rows.Count > 0)
            {
                return -1; //Chua nhan hang vao kho
            }

            query = string.Format("SELECT * FROM td_box_delivery WHERE box_no = '{0}' AND income_status = '1' AND store_status = '0'", boxno);
            dt = ExcuteQuery(query);
            if (dt.Rows.Count > 0)
                return 1; //OK quet SX
            return 0; //Da sap xep len pallete roi
        }

        public DataTable GetPlaceOfPallete(string palleteTypeNo)
        {
            string query = string.Format("SELECT a.*, b.* FROM td_palletetype_wh a INNER JOIN td_mastersymbol_wh b on a.symbol = b.symbol WHERE palletetype_no = '{0}'", palleteTypeNo);
            return ExcuteQuery(query);
        }

        public string GetUser(string location, string userid)
        {
            string result = string.Empty;
            string query = string.Format("SELECT first_name_en, last_name_en FROM tm_user_packing WHERE location_cd = '{0}' AND user_cd = '{1}' ", location, userid);
            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count > 0)
            {
                result = dt.Rows[0]["first_name_en"].ToString().Trim() + " " + dt.Rows[0]["last_name_en"].ToString().Trim();
            }
            return result;
        }

        public bool CheckDestination(string boxno, string symbol, string location, string itemgroup)
        {
            string nokCenter = GetSymbolOfBox(boxno).Trim();
            string sSymbol = string.Empty, sNokCenter = string.Empty;
            int flag = 0;
            if (string.IsNullOrEmpty(nokCenter))
                return false;
            if (nokCenter.Length == 4)
            {
                sSymbol = symbol.Substring(0, 1) + symbol.Substring(2, 1);
                sNokCenter = nokCenter.Substring(0, 1) + nokCenter.Substring(2, 1);
            }
            if (location.Equals("OS1"))
            {
                //20190703 doi voi ma hang BRQ0008,9 sap rieng pallete
                string itembox = GetInfoOfBoxByBoxJobtag(boxno).Rows[0]["item"].ToString().Trim();
                if (!string.IsNullOrEmpty(itemgroup))
                {
                    if (itemgroup.Contains(itembox))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                string temp = "BRQ0008-A01D0;BRQ0008-A01A0;BRQ0009-A00A0";

                if (temp.Contains(itembox))
                    return false;
               
                //end of 20190703

                if (!nokCenter.Equals(symbol))
                {
                    if (symbol.Equals("NAGOYA"))
                    {
                        if (nokCenter.Length == 4)
                        {
                            //20190628 cho DY 4 sap chung voi Nagoya
                            //if (Array.IndexOf(new string[] { "DY-1", "DY-2", "DY-3", "DY-4" }, nokCenter) > -1)
                            if (Array.IndexOf(new string[] { "DY-1", "DY-2", "DY-3"}, nokCenter) > -1)
                                flag = -1;
                            else if (nokCenter.Equals("DY-5"))
                                flag = 0;
                            else if (Array.IndexOf(new string[] { "MQND", "MQNC" }, nokCenter) > -1)
                                flag = 0;
                            else if (nokCenter.Equals("DXCO"))
                                flag = -1;
                            else if (!nokCenter.Substring(0, 1).Equals("D"))
                                flag = -1;

                        }
                        else
                        {
                            flag = -1;
                        }
                    }
                    else
                    {
                        flag = -1;
                    }
                }
                else
                {
                    flag = 0;
                }
                if (flag == 0)
                    return true;
                return false;
            }
            else //OR
            {
                if (!nokCenter.Equals(symbol))
                {
                    if (symbol.Equals("KUMAMOTO"))
                    {
                        if (!nokCenter.Substring(0, 1).Equals("D"))
                        {
                            if (!nokCenter.Substring(0, 1).Equals("M"))
                            {
                                if (nokCenter.Substring(0, 3).Equals("KUM"))
                                {
                                    flag = 0;
                                }
                                else
                                {
                                    flag = -1;
                                }
                            }
                        }
                        //if(!CheckDestinationOfSpecialItem(boxno, "KUMAMOTO"))
                        //{
                        //    flag = -1;
                        //}
                    }
                    else if (symbol.Equals("JYB"))
                    {
                        if (nokCenter.Equals("PKM"))
                            flag = 0;
                        else
                            flag = -1;
                        //if (!CheckDestinationOfSpecialItem(boxno, "JYB"))
                        //{
                        //    flag = -1;
                        //}
                    }
                    else if (symbol.Equals("MB"))
                    {
                        if (nokCenter.Length == 4)
                        {
                            if (!sNokCenter.Equals("MB"))
                                flag = -1;
                        }
                        else
                        {
                            flag = -1;
                        }

                        //if (!CheckDestinationOfSpecialItem(boxno, "MB"))
                        //{
                        //    flag = -1;
                        //}
                    }
                    else
                    {
                        flag = -1;
                    }
                }
                else
                {
                    flag = 0;
                }
                if (flag == 0)
                    return true;
                return false;
            }

        }

        public string GetSymbolOfBox(string boxno)
        {
            //string query = string.Format("SELECT C.CUST_USER_MAIN_STK_LOC FROM TD_BOX_JOBTAG A INNER JOIN TR_CUR_JOB_NBCS B ON A.JOB_NO = B.ENTRY_NO AND A.SUFFIX = B.ENTRY_LINE::INTEGER INNER JOIN TR_LBL_INFO_NBCS C ON B.JOB_ORDER_NO = C.JOB_ORDER_NO WHERE BOX_NO = '{0}' GROUP BY 1", boxno);

            //lay noi den theo starting job, neu lay dua theo packing job se co truong hop 1 thung co hai noi den.
            string query = string.Format("SELECT C.CUST_USER_MAIN_STK_LOC FROM TD_BOX_JOBTAG A INNER JOIN TR_LBL_INFO_NBCS C ON A.STARTING_JOB = C.JOB_ORDER_NO WHERE BOX_NO = '{0}' GROUP BY 1", boxno);

            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count == 1)
            {
                return dt.Rows[0]["cust_user_main_stk_loc"].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public int UpdateBoxDeliveryAfterStored(string boxno, string palleteTypeNo, string userid, DateTime date)
        {
            string query = string.Format("UPDATE td_box_delivery SET store_status = '1', store_date = '{0}', store_user = '{1}', store_time = '{2}', pallet_no = '{3}' WHERE box_no = '{4}'",
                date.ToString("yyyyMMdd"), userid, date.ToString("HHmmss"), palleteTypeNo, boxno);
            return ExcuteNonQuery(query);
        }

        public int InsertToBoxInfoAfterSotred(TdBoxInfo box)
        {
            string query = string.Format("INSERT INTO td_box_info (box_no, local_po_line, item, product_code, co_line, co_release, cust_num, cust_seq, cust_item, cust_po, qty, qty_shipped, status, box_type, qty_box, qty_pack, send_sign, palletetype_no, ship_date, shipmark_date, exp_date, stock, pallete_no, etdvn_date, etd_date, packing_user, packing_date, packing_time ) VALUES('{0}', {1}, '{2}', '{22}',{3}, {4}, '{5}', {6}, '{7}', '{8}', {9}, {10}, {11}, '{12}', {13}, {14}, {15}, '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{23}', '{24}','{25}','{26}','{27}')", box.box_no, box.local_po_line, box.item, box.co_line, box.co_release, box.cust_num, box.cust_seq, box.cust_item, box.cust_po, box.qty, box.qty_shipped, box.status, box.box_type, box.qty_box, box.qty_pack, box.send_sign, box.palletetype_no, box.ship_date, box.shipmark_date, box.exp_date, box.stock, box.pallete_no, box.product_code, box.etdvn_date, box.etd_date, box.packing_user, box.packing_date, box.packing_time);
            return ExcuteNonQuery(query);
        }

        public DataTable GetInfoBoxDelivery(string box)
        {
            string query = string.Format("SELECT * FROM td_box_delivery WHERE box_no = '{0}'", box);
            return ExcuteQuery(query);
        }

        public int InsertTdStockWH(TdStockWH item)
        {
            string query = string.Format("INSERT INTO td_stock_wh(stock, item, qty_onhand, qty_complete, qty_ship, qty_reserve) VALUES ('{0}', '{1}', {2}, {3}, {4}, {5})", item.stock, item.item, item.qty_onhand, item.qty_complete, item.qty_ship, item.qty_reserve);
            return ExcuteNonQuery(query);
        }

        public DataTable GetQtyStockByBoxJobtag(string box)
        {
            string query = string.Format("SELECT item, sum(qty) qty FROM td_box_jobtag WHERE box_no = '{0}' GROUP BY item", box);
            return ExcuteQuery(query);
        }

        public DataTable GetInfoOfBoxByBoxJobtag(string box)
        {
            string query = string.Format("SELECT * FROM td_box_jobtag WHERE box_no = '{0}'", box);
            return ExcuteQuery(query);
        }

        public int InsertTsStockResult(tsStockResult stock)
        {
            string query = string.Format("INSERT INTO ts_stock_result(update_user, trn_no, box_no, job_no,suffix, item, data_sign, qty, status) VALUES ('{0}', {1}, '{2}','{3}',{4},'{5}','{6}',{7}, '{8}')", stock.update_user, stock.trn_no, stock.box_no, stock.job_no, stock.suffix, stock.item, stock.data_sign, stock.qty, stock.status);
            return ExcuteNonQuery(query);
        }
        public long? GetTrnNoOfTsStockResult()
        {
            string query = "SELECT nextval('seq_tslocresult')";
            DataTable dt = ExcuteQuery(query);
            return (long?)Convert.ToDouble(dt.Rows[0][0].ToString());
        }

        public bool CheckItemIsExistInStockWh(string item)
        {
            string query = string.Format("SELECT item FROM td_stock_wh WHERE item = '{0}'", item);
            DataTable dt = ExcuteQuery(query);
            return dt.Rows.Count > 0;
        }

        public int UpdateTdStockWh(string item, long qtyComplete)
        {
            string query = string.Format("UPDATE td_stock_wh set qty_complete = qty_complete + {0} WHERE item = '{1}' and stock = 'STOCK'", qtyComplete, item);
            return ExcuteNonQuery(query);
        }

        /// <summary>
        /// return -1 OK
        /// return !(-1) => error code
        /// </summary>
        /// <param name="box"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public int CheckBoxCanStoreOR(string box, string item)
        {
            double qtyBalance = 0, qtyPallete = 0, qtyCheck = 0;
            int order_status;
            string pallete = string.Empty;

            string query = string.Format("SELECT pallete_no, item, order_status, sum(balance) as balance FROM  td_warehouse_plan WHERE item = '{0}' and to_date(due_date, 'YYYY-MM-DD') >= CURRENT_DATE and balance > 0 group by due_date,pallete_no,item,order_status ", item);

            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count <= 0)
            {
                return 49;
            }

            qtyBalance = Convert.ToDouble(dt.Rows[0]["balance"].ToString());
            pallete = dt.Rows[0]["pallete_no"].ToString();
            order_status = string.IsNullOrEmpty(dt.Rows[0]["order_status"].ToString()) ? 0 : Convert.ToInt32(dt.Rows[0]["order_status"].ToString());

            query = string.Format("SELECT sum(b.qty) as qty FROM td_box_info a INNER JOIN td_box_jobtag b ON b.box_no = a.box_no WHERE b.item='{1}' and a.pallete_no='{0}'", pallete, item);
            dt = ExcuteQuery(query);

            if (dt.Rows.Count > 0)
            {
                qtyPallete = Convert.ToDouble(string.IsNullOrEmpty(dt.Rows[0]["qty"].ToString()) ? "0" : dt.Rows[0]["qty"].ToString());
            }

            qtyCheck = qtyBalance - qtyPallete;

            if (qtyCheck <= 0)
            {
                if (order_status > 0)
                {
                    return 50;
                }
                else
                {
                    return 51;
                }
            }

            query = string.Format("SELECT a.box_no, a.qty FROM (SELECT a.box_no, min(lot_no||box_seq_or) AS lot, SO_THUNG, sum(a.qty) AS qty FROM td_box_jobtag a INNER JOIN td_box_delivery b ON b.box_no = a.box_no INNER JOIN td_daily_box_rec c ON c.box_no = a.box_no WHERE a.item='{0}' AND b.store_status='0' AND b.income_status='1' AND b.waiting_wh_date > (date_part('year', CURRENT_DATE) - 1) :: text||'0600' GROUP BY a.box_no, so_thung ) a ORDER BY a.lot, a.box_no, A.SO_THUNG ", item);
            dt = ExcuteQuery(query);

            foreach (DataRow dr in dt.Rows)
            {
                qtyCheck = qtyCheck - Convert.ToInt32(dr["qty"].ToString());
                if (qtyCheck >= 0 && box.Trim().Equals(dr["box_no"].ToString().Trim()))
                    return -1;

                if (qtyCheck <= 0)
                {
                    if (order_status > 0)
                    {
                        return 50;
                    }
                    else
                    {
                        return 51;
                    }
                }
            }
            return 0;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// return true: FiFo, false: OK
        /// </summary>
        /// <param name="box"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CheckFiFoScanStoringOR(string box, string item)
        {
            string MaxLotShipped = string.Empty, MinLotBox = string.Empty;
            string query = string.Format("SELECT Max(b.lot_no || b.box_seq_or) as lot_no FROM td_box_info a INNER JOIN td_box_jobtag b on a.box_no = b.box_no WHERE  a.status = 4 and a.ship_date >= To_char(CURRENT_DATE - interval '6 month', 'YYYYMMDD') and a.item = '{0}' and b.location_cd = 'OR1'", item);
            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count > 0)
            {
                MaxLotShipped = dt.Rows[0]["lot_no"].ToString().Trim();
            }
            query = string.Format("SELECT Min(lot_no || box_seq_or) as lot_no FROM td_box_jobtag WHERE box_no = '{0}'", box);
            dt = ExcuteQuery(query);
            if (dt.Rows.Count > 0)
            {
                MinLotBox = dt.Rows[0]["lot_no"].ToString().Trim();
            }
            return string.Compare(MaxLotShipped, MinLotBox) > 0 ? true : false;
        }

        public DataTable GetInfoWareHousePlanByItem(string item)
        {
            string query = string.Format("SELECT PALLETE_NO, TO_CHAR(TO_DATE(DUE_DATE, 'YYYY-MM-DD'), 'DD-MM-YYYY') DUE_DATE FROM TD_WAREHOUSE_PLAN WHERE ITEM='{0}' AND TO_DATE(DUE_DATE, 'YYYY-MM-DD') >= CURRENT_DATE  AND BALANCE > 0", item);
            return ExcuteQuery(query);
        }

        public long? GetNextNo(string typeName)
        {
            long currentNo = 0, startNo = 0, endNo = 0, newcurNo;
            string query = string.Format("SELECT * FROM td_rec_counter WHERE table_name = '{0}'", typeName);
            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count <= 0)
            {
                query = string.Format("INSERT INTO td_rec_counter (table_name, current_no, start_no, end_no, upd_date) VALUES('{0}', {1}, {2}, {3}, '{4}')", typeName, 2, 1, 9999999999, DateTime.Now.ToString("yyyyMMdd"));
                if (ExcuteNonQuery(query) <= 0)
                {
                    return -69;
                }
            }
            currentNo = (long)Convert.ToDouble(dt.Rows[0]["current_no"].ToString());
            startNo = (long)Convert.ToDouble(dt.Rows[0]["start_no"].ToString());
            endNo = (long)Convert.ToDouble(dt.Rows[0]["end_no"].ToString());
            if (currentNo < startNo)
            {
                currentNo = startNo;
            }
            else if (currentNo >= endNo)
            {
                currentNo = startNo;
            }

            newcurNo = currentNo + 1;
            if (newcurNo < startNo)
            {
                newcurNo = startNo;
            }
            else if (newcurNo >= endNo)
            {
                newcurNo = startNo;
            }

            query = string.Format("UPDATE td_rec_counter SET current_no = {0}, upd_date = '{1}' WHERE table_name = '{2}' ", newcurNo, DateTime.Now.ToString("yyyyMMdd"), typeName);
            if (ExcuteNonQuery(query) <= 0)
            {
                return -70;
            }
            return currentNo;
        }

        public DataTable GetInfoItemOfBoxIsInWaiting(string boxno)
        {
            string query = string.Format("SELECT b.* FROM td_box_jobtag a INNER JOIN td_wh_waiting_plan b ON a.item = b.item WHERE box_no = '{0}' and b.due_date >= '{1}'", boxno, DateTime.Now.ToString("yyyyMMdd"));
            return ExcuteQuery(query);    
        }
        public int GetWaitingQty(string item, string duedate)
        {
            string query = string.Format("SELECT sum(waiting_qty) qty FROM td_wh_waiting_plan WHERE item = '{0}' AND due_date = '{1}'", item, duedate);    
            return Convert.ToInt32(ExcuteQuery(query).Rows[0]["qty"].ToString());
        }
        public int GetWaitingQtyStored(string item, string duedate)
        {
            string query = string.Format("SELECT sum(box_qty) qty FROM td_waiting_plan_log WHERE item = '{0}' AND due_date = '{1}'", item, duedate);
            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count == 0)
                return 0;
            else
                return string.IsNullOrEmpty(dt.Rows[0]["qty"].ToString()) ? 0 : Convert.ToInt32(dt.Rows[0]["qty"].ToString());
        }
        public DataTable GetAllboxCanStoredByItem(string item)
        {
            string query = string.Format("SELECT a.box_no, a.qty FROM (SELECT a.box_no, min(lot_no||box_seq_or) AS lot, SO_THUNG, sum(a.qty) AS qty FROM td_box_jobtag a INNER JOIN td_box_delivery b ON b.box_no = a.box_no INNER JOIN td_daily_box_rec c ON c.box_no = a.box_no WHERE a.item='{0}' AND b.store_status='0' AND b.income_status='1' AND b.waiting_wh_date > (date_part('year', CURRENT_DATE) - 1) :: text||'0600' GROUP BY a.box_no, so_thung ) a WHERE box_no not in (select box_no from td_waiting_plan_log where due_date >= TO_CHAR(current_date, 'YYYYMMDD')) ORDER BY a.lot, a.box_no, A.SO_THUNG ", item);
            return ExcuteQuery(query);
        }
        public int InsertIntoWaitingPlanLog(string userId, string item, string duedate, string boxno, string boxQty)
        {
            string query = string.Format("INSERT INTO td_waiting_plan_log(entry_date, entry_time, entry_user, shipping_to, item, due_date, box_no, box_qty) VALUES (TO_CHAR(CURRENT_DATE, 'YYYYMMDD'), TO_CHAR(NOW(), 'HH24MISS'), '{0}', '', '{1}', '{2}', '{3}', {4})", userId, item, duedate, boxno, boxQty);
            return ExcuteNonQuery(query);
        }
        public string GetBoxQtyOfBox(string box)
        {
            string query = string.Format("SELECT sum(qty) qty FROM td_box_jobtag WHERE box_no = '{0}'", box);
            return ExcuteQuery(query).Rows[0]["qty"].ToString();
        }
        public DataTable CheckBoxInWatingLog(string box)
        {
            string query = string.Format("SELECT * FROM td_waiting_plan_log WHERE box_no = '{0}'", box);
            return ExcuteQuery(query);
        }
        public DataTable GetQtyWaitingByItem(string item)
        {
            string query = string.Format("SELECT shipping_to, item, (waiting_qty - actual_qty) qty FROM td_wh_waiting_plan WHERE item = '{0}' AND due_date >= TO_CHAR(current_date, 'YYYYMMDD') ORDER BY qty asc", item);
            return ExcuteQuery(query);
        }
        public DataTable GetInfoOfShippingTo(string shippingTo)
        {
            string query = string.Format("SELECT * FROM td_pallete_wh a INNER JOIN td_mastersymbol_wh b ON a.symbol = b.symbol WHERE pallete_no = '{0}'", shippingTo);
            return ExcuteQuery(query);
        }
        public bool CheckItemIsWaitingItem(string item)
        {
            string query = string.Format("SELECT * FROM td_wh_waiting_plan WHERE item = '{0}' AND due_date >= TO_CHAR(current_date, 'YYYYMMDD')", item);
            return ExcuteQuery(query).Rows.Count > 0;
        }
        public DataTable GetQtyWaitingByShippingTo(string shippingTo, string item)
        {
            string query = string.Format("SELECT shipping_to, item, (waiting_qty - actual_qty) qty FROM td_wh_waiting_plan WHERE shipping_to = '{0}' AND item = '{1}' AND due_date >= TO_CHAR(current_date, 'YYYYMMDD') ORDER BY qty asc", shippingTo, item);
            return ExcuteQuery(query);
        }
        public DataTable GetDestinationAndBoxInPallete(string pallete)
        {
            string query = string.Format("SELECT max(box_no) box_no, cust_user_main_stk_loc FROM (SELECT a.box_no, cust_user_main_stk_loc FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no INNER JOIN tr_lbl_info_nbcs c ON b.starting_job = c.job_order_no WHERE pallete_no = '{0}' ) x GROUP BY cust_user_main_stk_loc", pallete);
            return ExcuteQuery(query);
        }
        public int UpdatePalleteTypeInBoxInfo(string pallete_no, string palleteType_no)
        {
            string query = string.Format("UPDATE td_box_info SET palletetype_no = '{0}' WHERE pallete_no = '{1}'", palleteType_no, pallete_no);
            return ExcuteNonQuery(query);
        }
        public int UpdatePalleteTypeInPalleteWh(string pallete_no, string palleteType_no, string symbol)
        {
            string query = string.Format("UPDATE td_pallete_wh SET palletetype_no = '{0}', symbol = '{1}' WHERE pallete_no = '{2}'", palleteType_no, symbol, pallete_no);
            return ExcuteNonQuery(query);
        }  
        public bool CheckShippingToIsWaiting(string shippingTo)
        {
            string query = string.Format("SELECT * FROM td_wh_waiting_plan WHERE shipping_to = '{0}' AND due_date >= TO_CHAR(current_date, 'YYYYMMDD')", shippingTo);
            return ExcuteQuery(query).Rows.Count > 0;
        }
        //check fifo OR
        public DataTable GetMinAndMaxLotOfBoxOR(string boxno)
        {
            string query = string.Format("SELECT a.item, max(lot_no || box_seq_or) max_lot, min(lot_no || box_seq_or) min_lot FROM td_box_info a INNER JOIN td_box_jobtag b on a.box_no = b.box_no WHERE a.box_no = '{0}' GROUP BY a.item", boxno);
            return ExcuteQuery(query);
        }
        public DataTable MinLotOfItemIsNotStored(string item)
        {
            string query = string.Format("SELECT a.box_no,  min(lot_no || box_seq_or) lot_no FROM td_box_delivery a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.item = '{0}' AND income_status = '1' AND store_status = '0' GROUP bY 1 ORDER BY lot_no limit 1", item);
            return ExcuteQuery(query);
        }
        public DataTable MinLotOfItemIsStored(string item, string palleteNo)
        {
            string query = string.Format("SELECT a.box_no, min(lot_no || box_seq_or) lot_no FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.item = '{0}' AND a.status = 1 AND a.pallete_no NOT IN ('{1}') GROUP bY 1 ORDER BY lot_no limit 1", item, palleteNo);
            return ExcuteQuery(query);
        }
        public bool InsertOrUpdateBoxFiFo(string boxno, string boxff)
        {
            int ret;
            string strWhere = string.Format("box_no = '{0}' and box_no_ff = '{1}' and status = {2}", boxno, boxff, 1);
            DataTable dtCheck = SelectBoxFifo(strWhere);

            if (dtCheck.Rows.Count > 0)
            {
                //update
                string strSet = string.Format("update_date = '{0}', update_time = '{1}'", DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"));
                strWhere = string.Format("box_no= '{0}' and box_no_ff='{1}'", boxno, boxff);

                ret = UpdBoxFifo(strSet, strWhere);

                return ret < 0 ? false : true;
            }
            else
            {
                //insert
                ret = InsertBoxFifo(boxno, boxff, "", 1, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), 1);
                return ret < 0 ? false : true;

            }
        }
        public DataTable GetItemAndQtyStoredByItemPallateOR(string palleteno)
        {
            string query = string.Format("SELECT a.item, sum(b.qty) qtyActual FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.status < 4 AND a.item IN (SELECT item FROM td_box_info WHERE status = 1 AND pallete_no = '{0}' GROUP BY item) GROUP BY a.item", palleteno);
            return ExcuteQuery(query);
        }
        
        public double BalanceOfItemInWHPlan(string item)
        {
            string query = string.Format("SELECT pallete_no, item, sum(balance) as balance FROM  td_warehouse_plan WHERE item = '{0}' and to_date(due_date, 'YYYY-MM-DD') >= CURRENT_DATE and balance > 0 group by due_date,pallete_no,item ", item);
            DataTable dt = ExcuteQuery(query);
            return dt.Rows.Count <= 0 ? 0 : Convert.ToDouble(dt.Rows[0]["balance"].ToString());
        }

        public DataTable GetBalanceAndItemInWHPlan(string palleteno)
        {
            string query = string.Format("SELECT pallete_no, item, sum(balance) as balance  FROM  td_warehouse_plan  WHERE pallete_no = '{0}'  AND to_date(due_date, 'YYYY-MM-DD') >= CURRENT_DATE and balance > 0  group by due_date,pallete_no,item ", palleteno);
            return ExcuteQuery(query);
        }
        public double GetQtyActualOfItemInPallete(string palleteno, string item)
        {
            string query = string.Format("SELECT sum(b.qty) qtyActual FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no WHERE a.status = 1 AND a.item = '{0}' AND pallete_no = '{1}'", item, palleteno);
            DataTable dt = ExcuteQuery(query);
            if (string.IsNullOrEmpty(dt.Rows[0]["qtyActual"].ToString()))
                return -1;
            return Convert.ToDouble(dt.Rows[0]["qtyActual"].ToString());
        }
        public int DeleteWaitingBox(string boxno)
        {
            string query = string.Format("DELETE FROM td_waiting_plan_log WHERE box_no = '{0}'", boxno);
            return ExcuteNonQuery(query);
        }

        public bool CheckDestinationOfSpecialItem(string box, string pallete)
        {
            DataTable dt = GetInfoOfBoxByBoxJobtag(box);
            string item = dt.Rows[0]["item"].ToString().Trim();

            if (Array.IndexOf(new string[] { "CO07064-G0A00AE", "DO01034-G0B00AC", "DO1621-G02AD" }, item) < 0)
            {
                return true;
            }
            else
            {
                if (Array.IndexOf(new string[] { "MB", "JYB" }, pallete) < 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
                 

        }

        public string GetMaxLofOfItemStored(string item)
        {
            string query = string.Format("SELECT MAX(lot_no) lotno FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no and a.item = '{0}' AND a.status in ('1')", item);


            return ExcuteQuery(query).Rows[0]["lotno"].ToString().Trim();
        }

        public DataTable GetWaitingboxInPalleteType(string palleteType)
        {
            string query = string.Format("SELECT a.box_no, a.item, max(b.lot_no ||' ' || b.box_seq) lot_no FROM td_box_info a INNER JOIN td_box_jobtag b ON a.box_no = b.box_no INNER JOIN td_waiting_plan_log c ON a.box_no = c.box_no WHERE due_date >= '{0}' AND a.palletetype_no = '{1}' and a.status < 1 GROUP BY a.box_no, a.item", DateTime.Now.ToString("yyyyMMdd"), palleteType);
            return ExcuteQuery(query);
        }

    }
}
