using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{
    public class ShippingTo
    {
        public string PalleteNo { get; set; }
        public string ShipSeq { get; set; }
        public string PalleteNo1 { get; set; }
        public string Detail { get; set; }
        public string User { get; set; }
        public string Item { get; set; }
        public string Jobtag { get; set; }
        public string BoxNum { get; set; }
        public int Qty { get; set; }
        public int Rev { get; set; }
    }
}
