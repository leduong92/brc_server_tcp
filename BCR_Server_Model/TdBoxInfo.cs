using System.ComponentModel;

namespace BcrServer_Model
{
    public class TdBoxInfo
    {
        [DisplayName("box_no")]
        public string box_no { get; set; }

        [DisplayName("vender_code")]
        public string vender_code { get; set; }

        [DisplayName("local_po")]
        public string local_po { get; set; }

        [DisplayName("local_po_line")]
        public int local_po_line { get; set; }

        [DisplayName("item")]
        public string item { get; set; }

        [DisplayName("product_code")]
        public string product_code { get; set; }

        [DisplayName("co_num")]
        public string co_num { get; set; }

        [DisplayName("co_line")]
        public int co_line { get; set; }

        [DisplayName("co_release")]
        public int co_release { get; set; }

        [DisplayName("co_duedate")]
        public string co_duedate { get; set; }

        [DisplayName("cust_num")]
        public string cust_num { get; set; }

        [DisplayName("cust_seq")]
        public int cust_seq { get; set; }

        [DisplayName("cust_item")]
        public string cust_item { get; set; }

        [DisplayName("cust_po")]
        public string cust_po { get; set; }

        [DisplayName("object_data_sign")]
        public string object_data_sign { get; set; }

        [DisplayName("qty")]
        public int qty { get; set; }

        [DisplayName("qty_shipped")]
        public int qty_shipped { get; set; }

        [DisplayName("status")]
        public int status { get; set; }

        [DisplayName("parent_box_no")]
        public string parent_box_no { get; set; }

        [DisplayName("parent_box_no2")]
        public string parent_box_no2 { get; set; }

        [DisplayName("picking_no")]
        public string picking_no { get; set; }

        [DisplayName("tag_no")]
        public string tag_no { get; set; }

        [DisplayName("box_type")]
        public string box_type { get; set; }

        [DisplayName("qty_box")]
        public int qty_box { get; set; }

        [DisplayName("qty_pack")]
        public int qty_pack { get; set; }

        [DisplayName("send_sign")]
        public int send_sign { get; set; }

        [DisplayName("palletetype_no")]
        public string palletetype_no { get; set; }

        [DisplayName("invoice_no")]
        public string invoice_no { get; set; }

        [DisplayName("invoice_date")]
        public string invoice_date { get; set; }

        [DisplayName("time_stamp")]
        public string time_stamp { get; set; }

        [DisplayName("picking_line")]
        public int picking_line { get; set; }

        [DisplayName("container_no")]
        public string container_no { get; set; }

        [DisplayName("ship_date")]
        public string ship_date { get; set; }

        [DisplayName("ship_user")]
        public string ship_user { get; set; }

        [DisplayName("reserved_date")]
        public string reserved_date { get; set; }

        [DisplayName("reserved_user")]
        public string reserved_user { get; set; }

        [DisplayName("shipmark_date")]
        public string shipmark_date { get; set; }

        [DisplayName("shipmark_user")]
        public string shipmark_user { get; set; }

        [DisplayName("exp_date")]
        public string exp_date { get; set; }

        [DisplayName("exp_user")]
        public string exp_user { get; set; }

        [DisplayName("ship_time")]
        public string ship_time { get; set; }

        [DisplayName("reserved_time")]
        public string reserved_time { get; set; }

        [DisplayName("shipmark_time")]
        public string shipmark_time { get; set; }

        [DisplayName("exp_time")]
        public string exp_time { get; set; }

        [DisplayName("stock")]
        public string stock { get; set; }

        [DisplayName("ship_no")]
        public string ship_no { get; set; }

        [DisplayName("shipmark_no")]
        public string shipmark_no { get; set; }

        [DisplayName("pallete_no")]
        public string pallete_no { get; set; }

        [DisplayName("packing_date")]
        public string packing_date { get; set; }

        [DisplayName("packing_time")]
        public string packing_time { get; set; }

        [DisplayName("packing_user")]
        public string packing_user { get; set; }

        [DisplayName("move_stock_date")]
        public string move_stock_date { get; set; }

        [DisplayName("move_stock_time")]
        public string move_stock_time { get; set; }

        [DisplayName("move_stock_user")]
        public string move_stock_user { get; set; }

        [DisplayName("move_pltemp_date")]
        public string move_pltemp_date { get; set; }

        [DisplayName("move_pltemp_time")]
        public string move_pltemp_time { get; set; }

        [DisplayName("move_pltemp_user")]
        public string move_pltemp_user { get; set; }

        [DisplayName("move_plreser_date")]
        public string move_plreser_date { get; set; }

        [DisplayName("move_plreser_time")]
        public string move_plreser_time { get; set; }

        [DisplayName("move_plreser_user")]
        public string move_plreser_user { get; set; }

        [DisplayName("move_status")]
        public short? move_status { get; set; }

        [DisplayName("ship_seq")]
        public long? ship_seq { get; set; }

        [DisplayName("etd_date")]
        public string etd_date { get; set; }

        [DisplayName("etdvn_date")]
        public string etdvn_date { get; set; }

        [DisplayName("status_return")]
        public int status_return { get; set; }

        [DisplayName("borrowed_date")]
        public string borrowed_date { get; set; }

        [DisplayName("borrowed_time")]
        public string borrowed_time { get; set; }

        [DisplayName("returnwh_date")]
        public string returnwh_date { get; set; }

        [DisplayName("returnwh_time")]
        public string returnwh_time { get; set; }

        [DisplayName("shipping_sequence")]
        public string shipping_sequence { get; set; }

        [DisplayName("transfer_sign")]
        public string transfer_sign { get; set; }

        [DisplayName("container_date")]
        public string container_date { get; set; }
    }
}
