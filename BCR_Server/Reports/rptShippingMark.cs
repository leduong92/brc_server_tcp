using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BcrServer_Model;
using System.Collections.Generic;

namespace BcrServer
{
    public partial class rptShippingMark : DevExpress.XtraReports.UI.XtraReport
    {
        public rptShippingMark()
        {
            InitializeComponent();
        }


        public void LoadDataToReport(List<ShippingMark> printlist)
        {
            objectDataSource.DataSource = printlist;           

        }
    }
}
