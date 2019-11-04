using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{
    public class tsStockResult
    {
        [DisplayName("file_sign")]
        public string file_sign { get; set; }

        [DisplayName("active_sign")]
        public string active_sign { get; set; }

        [DisplayName("entry_date")]
        public string entry_date { get; set; }

        [DisplayName("update_date")]
        public string update_date { get; set; }

        [DisplayName("update_time")]
        public string update_time { get; set; }

        [DisplayName("update_user")]
        public string update_user { get; set; }

        [DisplayName("update_pgm")]
        public string update_pgm { get; set; }

        [DisplayName("update_sign")]
        public string update_sign { get; set; }

        [DisplayName("trn_no")]
        public long? trn_no { get; set; }

        [DisplayName("box_no")]
        public string box_no { get; set; }

        [DisplayName("job_no")]
        public string job_no { get; set; }

        [DisplayName("suffix")]
        public int suffix { get; set; }

        [DisplayName("item")]
        public string item { get; set; }

        [DisplayName("data_sign")]
        public string data_sign { get; set; }

        [DisplayName("qty")]
        public int qty { get; set; }

        [DisplayName("status")]
        public string status { get; set; }

        [DisplayName("result_date")]
        public string result_date { get; set; }

        [DisplayName("memo")]
        public string memo { get; set; }

    }
}
