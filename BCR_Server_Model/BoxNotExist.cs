using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{
    public class BoxNotExist
    {
        public string BoxNo { get; set; }
        public string Item { get; set; }
        public string LotNo { get; set; }
        public int Qty { get; set; }
        public string PalleteNo { get; set; }
        public string EntryDate { get; set; }
        public string EntryTime { get; set; }
        public string Destination { get; set; }
    }
}
