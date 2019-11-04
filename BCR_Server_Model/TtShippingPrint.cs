using System.ComponentModel;

namespace BcrServer_Model
{
    public class TtShippingPrint
    {
        [DisplayName("entry_date")]
        public string entry_date { get; set; }

        [DisplayName("entry_time")]
        public string entry_time { get; set; }

        [DisplayName("entry_user")]
        public string entry_user { get; set; }

        [DisplayName("box_no")]
        public string box_no { get; set; }

        [DisplayName("item")]
        public string item { get; set; }

        [DisplayName("qty_box")]
        public int qty_box { get; set; }

        [DisplayName("qty")]
        public int qty { get; set; }

        [DisplayName("lot_no")]
        public string lot_no { get; set; }

        [DisplayName("job_no")]
        public string job_no { get; set; }

        [DisplayName("suffix")]
        public int suffix { get; set; }

        [DisplayName("parentpallete")]
        public string parentpallete { get; set; }

        [DisplayName("delivery_place")]
        public string delivery_place { get; set; }

        [DisplayName("box_seq")]
        public int box_seq { get; set; }

        [DisplayName("shipping_no")]
        public string shipping_no { get; set; }

        [DisplayName("pallete_no")]
        public string pallete_no { get; set; }

        [DisplayName("ship_seq")]
        public long? ship_seq { get; set; }

        [DisplayName("loc")]
        public string loc { get; set; }
    }
}
