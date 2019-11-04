using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Model
{
    public class TdStockWH
    {
        [DisplayName("stock")]
        public string stock { get; set; }

        [DisplayName("item")]
        public string item { get; set; }

        [DisplayName("qty_onhand")]
        public long? qty_onhand { get; set; }

        [DisplayName("qty_complete")]
        public long? qty_complete { get; set; }

        [DisplayName("qty_ship")]
        public long? qty_ship { get; set; }

        [DisplayName("qty_reserve")]
        public long? qty_reserve { get; set; }

    }
}
