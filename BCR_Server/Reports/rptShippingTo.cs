using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BcrServer_Model;
using System.Collections.Generic;

namespace BcrServer
{
    public partial class rptShippingTo : DevExpress.XtraReports.UI.XtraReport
    {
        public rptShippingTo()
        {
            InitializeComponent();
           
        }

        public void LoadData(List<ShippingTo> printlist)
        {
            objectDataSource.DataSource = printlist;
        }
    }
}
