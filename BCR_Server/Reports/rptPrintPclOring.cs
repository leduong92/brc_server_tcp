using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BcrServer_Model;
using System.Collections.Generic;
using System.Data;

namespace BcrServer.Reports
{
    public partial class rptPrintPclOring : DevExpress.XtraReports.UI.XtraReport
    {
        public rptPrintPclOring()
        {
            InitializeComponent();
        }

        public void LoadData(List<PclOring> printlist)
        {
            objectDataSource1.DataSource = printlist;
        }

     
        private void Detail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            
        }

    }
}
