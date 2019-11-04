using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BcrServer_Model;
using System.Collections.Generic;

namespace BcrServer.Reports
{
    public partial class rptBoxNotInPallete : DevExpress.XtraReports.UI.XtraReport
    {
        public rptBoxNotInPallete()
        {
            InitializeComponent();
        }
        public void LoadData(List<BoxNotExist> printlist)
        {
            objectDataSource1.DataSource = printlist;
        }
        public void LoadDataAndTitle(List<BoxNotExist> printlist)
        {
            xrLabel11.Text = "HÀNG CHỜ CHƯA ĐƯA VÀO SHIPPING TO";
            objectDataSource1.DataSource = printlist;
        }
    }
}
