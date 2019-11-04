using System.Collections.Generic;
using System.Data;
using Npgsql;
using BcrServer_Model;

namespace BcrServer_Repository
{
    public class App200Repository : DataProvider, IApp200Repository
    {
        NpgsqlTransaction transaction;
        public App200Repository(NpgsqlTransaction _transaction) : base(_transaction)
        {
            this.transaction = _transaction;
        }

        public DataTable GetBoxInfomationByBoxNo(string boxNo)
        {
            string query = string.Format("select box_no, item from td_box_jobtag where box_no = '{0}' group by box_no, item", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetChangePalleteInfoByBoxNo(string boxNo)
        {
            string query = string.Format("SELECT ITEM, CHANGE_PALLETE_SIGN, PALLETE_NO FROM TD_WAREHOUSE_PLAN WHERE ITEM = (SELECT ITEM FROM TD_BOX_JOBTAG WHERE BOX_NO = '{0}' GROUP BY ITEM) AND BALANCE > 0 AND TO_DATE(DUE_DATE, 'YYYY-MM-DD') >= CURRENT_DATE AND CHANGE_PALLETE_SIGN = '*'", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetPalleteInfoByPalleteNo(string palleteNo)
        {
            string query = string.Format("SELECT SEQ_NO, SYMBOL, PARENTPALLETE_NO, PALLETE_NO FROM TD_PALLETE_WH WHERE PALLETE_NO = '{0}'", palleteNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable CheckBoxAlreadyArrangedtoPallete(string boxNo)
        {
            string query = string.Format("SELECT B.BOX_NO, B.PALLETE_NO FROM TD_BOX_DELIVERY A INNER JOIN TD_BOX_INFO B ON B.BOX_NO = A.BOX_NO WHERE A.INCOME_STATUS = '1' AND A.STORE_STATUS = '1' AND B.STATUS < 3 AND B.BOX_NO = '{0}'", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public int UpdatePalleteNoByBoxNo(string boxNo, string palleteTypeNo, string palleteNo)
        {
            int ret = 0;
            string query = string.Format("UPDATE TD_BOX_DELIVERY SET PALLET_NO = '{0}' WHERE BOX_NO = '{1}'", palleteTypeNo, boxNo);

            ret = ExcuteNonQuery(query, null, CommandType.Text);

            if(ret > 0)
            {
                query = string.Format("UPDATE TD_BOX_INFO SET PALLETETYPE_NO = '{0}', PALLETE_NO = '{1}' WHERE BOX_NO = '{2}'", palleteTypeNo, palleteNo, boxNo);

                ret = ExcuteNonQuery(query, null, CommandType.Text);
            }

            return ret;
        }

        public DataTable GetBoxAndReceiveByPclNo(string pclNo)
        {
            //string query = string.Format("SELECT BOX_NO FROM TD_BOX_DELIVERY WHERE WAITING_WH_STATUS = '1' AND INCOME_STATUS = '1' AND PCL_NO = '{0}'", pclNo);

            //For test multi HT connection
            string query = string.Format("SELECT BOX_NO FROM TD_BOX_DELIVERY WHERE PCL_NO = '{0}'", pclNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public DataTable GetBoxInfoByBoxNo(string boxNo)
        {
            string query = string.Format("SELECT A.BOX_NO, A.STATUS, A.PALLETE_NO FROM TD_BOX_INFO A INNER JOIN TD_BOX_DELIVERY B ON B.BOX_NO = A.BOX_NO AND B.INCOME_STATUS = '1' WHERE B.BOX_NO = '{0}'", boxNo);

            return ExcuteQuery(query, null, CommandType.Text);
        }

        public bool IsBoxExistsOnNewPallete(string boxNo, string palleteNo)
        {
            string query = string.Format("SELECT B.BOX_NO, B.PALLETE_NO FROM TD_BOX_DELIVERY A INNER JOIN TD_BOX_INFO B ON B.BOX_NO = A.BOX_NO WHERE A.INCOME_STATUS = '1' AND A.STORE_STATUS = '1' AND B.STATUS < 3 AND B.BOX_NO = '{0}' AND B.PALLETE_NO = '{1}'", boxNo, palleteNo);

            return ExcuteQuery(query, null, CommandType.Text).Rows.Count > 0;
        }

        public DataTable GetAllBoxByShippingTo(string shippingTo)
        {
            string query = string.Format("SELECT BOX_NO FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS < 3 ", shippingTo);

            return ExcuteQuery(query);
        }

        public DataTable GetAllBoxInPalleteByBoxNo(string boxNo)
        {
            string query = string.Format("SELECT TRIM(A.BOX_NO) AS BOX_NO FROM TD_BOX_INFO A WHERE A.PALLETE_NO = (SELECT PALLETE_NO FROM TD_BOX_INFO WHERE BOX_NO = '{0}' AND STATUS = '1') AND A.STATUS = '1'", boxNo);

            //string query = string.Format("SELECT A.BOX_NO FROM TD_BOX_INFO A WHERE A.PALLETE_NO = (SELECT PALLETE_NO FROM TD_BOX_INFO WHERE BOX_NO = '{0}') ", boxNo);

            //string query = string.Format("SELECT A.BOX_NO FROM TD_BOX_INFO A WHERE status = '4'  order by box_no limit 1000");
            return ExcuteQuery(query);
        }
        public DataTable GetBoxInfoByBoxJob(string boxNo)
        {
            //string query = string.Format("SELECT a.box_no, a.item, pallete_no, max(lot_no || box_seq_or) as lot, sum(a.qty) as qty FROM TD_BOX_JOBTAG a INNER JOIN TD_BOX_INFO b ON a.box_no = b.box_no WHERE a.BOX_NO = '{0}' GROUP BY 1,2,3 ORDER BY 4 DESC", boxNo);
            string query = string.Format("SELECT a.box_no, a.item, pallete_no, c.cust_user_main_stk_loc, max(a.lot_no || ' ' || CASE WHEN (a.box_seq_or IS NULL OR a.box_seq_or ='') THEN a.box_seq::text ELSE a.box_seq_or END ) as lot, sum(a.qty) as qty FROM TD_BOX_JOBTAG a INNER JOIN TD_BOX_INFO b ON a.box_no = b.box_no INNER JOIN TR_LBL_INFO_NBCS c ON a.starting_job = c.job_order_no  WHERE a.BOX_NO = '{0}' GROUP BY 1,2,3,4 ORDER BY 5 DESC", boxNo);
            return ExcuteQuery(query);
        }
        public DataTable GetAllBoxNotInPallete(string notIn, string oneBox)
        {                        
            string query = string.Format("SELECT box_no FROM td_box_info WHERE box_no NOT IN ({0}) AND PALLETE_NO = (SELECT PALLETE_NO FROM td_box_info WHERE box_no = '{1}')", notIn, oneBox);
            return ExcuteQuery(query);
        }
        public int InsertToBoxNotInPallete(BoxNotExist box)
        {
            string query = "INSERT INTO td_box_not_in_pallete(box_no, item, lot_no, qty, pallete_no, entry_date, entry_time, destination) VALUES (@box_no, @item, @lot, @qty, @pallete, @date, @time, @destination); ";
            return ExcuteNonQuery(query, new Dictionary<string, object> {
                {"box_no", box.BoxNo },
                {"item",box.Item },
                {"lot", box.LotNo },
                {"qty", box.Qty },
                {"pallete", box.PalleteNo },
                {"date", box.EntryDate },
                {"time", box.EntryTime },
                {"destination", box.Destination }
            });
        }
        public int DeleteBoxNotInPallete(string oneBox)
        {
            string query = string.Format("DELETE FROM td_box_not_in_pallete WHERE PALLETE_NO =  (SELECT PALLETE_NO FROM td_box_info WHERE box_no = '{0}')", oneBox);
            return ExcuteNonQuery(query);
        }

        public bool IsPalleteExists(string palleteNo)
        {
            string query = string.Format("SELECT BOX_NO FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS = '4'", palleteNo);

            return ExcuteQuery(query).Rows.Count > 0;
        }
        public bool IsPalleteCanExport(string palleteNo)
        {
            string query = string.Format("SELECT BOX_NO FROM TD_BOX_INFO WHERE PALLETE_NO = '{0}' AND STATUS = '4' AND (CONTAINER_NO = '' OR CONTAINER_NO IS NULL) AND (TRANSFER_SIGN  IS NULL OR TRANSFER_SIGN = '') ", palleteNo);

            return ExcuteQuery(query).Rows.Count > 0;
        }
        public int UpdateContainerByPallete(string palleteNo, string container, string exportSeq, string containerDate, string location, string user, string containerTime)
        {
            string query = string.Empty;

            //if (location.Trim().Equals("OS1"))
            //{
            //    query = string.Format("UPDATE TD_BOX_INFO SET container_no = '{0}', shipping_sequence = ship_seq, container_date = '{2}', container_user = '{3}' WHERE PALLETE_NO = '{1}' AND STATUS = '4'", container, palleteNo, containerDate, user);
            //}
            //else if(location.Trim().Equals("OR1"))
            //{
            //    query = string.Format("UPDATE TD_BOX_INFO SET container_no = '{0}', shipping_sequence = '{1}', container_date = '{3}', container_user = '{4}' WHERE PALLETE_NO = '{2}' AND STATUS = '4'", container, exportSeq, palleteNo, containerDate, user);
            //}

            //query = string.Format("UPDATE TD_BOX_INFO SET container_no = '{0}', shipping_sequence = '{1}', container_date = '{3}', container_user = '{4}' WHERE PALLETE_NO = '{2}' AND STATUS = '4'", container, exportSeq, palleteNo, containerDate, user);

            query = string.Format("UPDATE TD_BOX_INFO SET container_no = '{0}', shipping_sequence = '{1}', container_date = '{3}', container_user = '{4}', container_time = '{5}' WHERE PALLETE_NO = '{2}' AND STATUS = '4'", container, exportSeq, palleteNo, containerDate, user, containerTime);

            return ExcuteNonQuery(query);
        }
        public string GetSymbolOfPalleteNo(string palleteNo)
        {
            //string query = string.Format("SELECT * FROM td_palletetype_wh WHERE palletetype_no in (SELECT palletetype_no FROM td_box_info WHERE pallete_no = '{0}' GROUP BY 1)", palleteNo);

            string query = string.Format("select * from td_pallete_wh WHERE pallete_no = '{0}'", palleteNo);

            DataTable dt = ExcuteQuery(query);
            if (dt.Rows.Count <= 0)
                return string.Empty;
            return dt.Rows[0]["symbol"].ToString();
        }

        public int SetContainerLockSignForPallete(string container)
        {
            string query = string.Empty;

            query = string.Format("UPDATE TD_BOX_INFO SET LOCK_SIGN = '*' WHERE CONTAINER_NO = '{0}' AND (TRANSFER_SIGN <> '*' OR TRANSFER_SIGN IS NULL)",  container);

            return ExcuteNonQuery(query);
        }

        public DataTable GetBoxInfoByContainerNoAndDate(string container, string containerDate)
        {
            string query = string.Format("SELECT CONTAINER_NO, CONTAINER_LOCK_SIGN FROM TD_BOX_INFO WHERE CONTAINER_DATE = '{0}' AND CONTAINER_NO = '{1}' GROUP BY CONTAINER_NO, CONTAINER_LOCK_SIGN", containerDate, container);

            return ExcuteQuery(query);
        }

        public int UpdateContainerLockSign(string container, string containerDate)
        {
            string query = string.Format("UPDATE TD_BOX_INFO SET CONTAINER_LOCK_SIGN = '*' WHERE CONTAINER_DATE = '{0}' AND CONTAINER_NO = '{1}' AND (CONTAINER_LOCK_SIGN IS NULL OR CONTAINER_LOCK_SIGN = '') ", containerDate, container);

            return ExcuteNonQuery(query);
        }
    }
}
