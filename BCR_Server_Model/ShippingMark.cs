using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{
    public class ShippingMark
    {
        public string PalleteNo { get; set; }       
        public string PalleteNo1 { get; set; }
        public string Detail { get; set; }  
        public int Rev { get; set; }
        public string TotalBox { get; set; }
        public string ETDVNNDate { get; set; }
        public string ETDHCMDate { get; set; }
        public string TotalItem { get; set; }
        public int TotalQty { get; set; }
    }
}
