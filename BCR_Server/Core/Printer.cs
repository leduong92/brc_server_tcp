using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing;
using System.Data;
using DevExpress.XtraReports.UI;
using BcrServer.Reports;
using BcrServer_Model;

namespace BcrServer
{
    public static class Printer<T> where T: new()
    {
        public static void PrintData(string reportName, List<T> printList = null) 
        {
            ReportPrintTool rptPrint = null;

            switch (reportName.ToUpper())
            {
                case "SHIPPINGMARK":
                    rptShippingMark rpt = new rptShippingMark();
                    rpt.LoadDataToReport(printList as List<ShippingMark>);
                    rptPrint = new ReportPrintTool(rpt);
                    break;
                case "SHIPPINGTO":
                    rptShippingTo rptTo = new rptShippingTo();
                    rptTo.LoadData(printList as List<ShippingTo>);
                    rptPrint = new ReportPrintTool(rptTo);
                    break;
                case "BOXNOTINPALLETE":
                    rptBoxNotInPallete rptBoxFound = new rptBoxNotInPallete();
                    rptBoxFound.LoadData(printList as List<BoxNotExist>);
                    rptPrint = new ReportPrintTool(rptBoxFound);
                    break;
                case "PCLORING": // M300OR1: In phieu PCL
                    rptPrintPclOring prtPrintPclOR = new rptPrintPclOring();
                    prtPrintPclOR.LoadData(printList as List<PclOring>);
                    rptPrint = new ReportPrintTool(prtPrintPclOR);
                    break;
                case "BOXNOTINPALLETEANDTITLE":
                    rptBoxNotInPallete rptBoxFoundWithtitle = new rptBoxNotInPallete();
                    rptBoxFoundWithtitle.LoadDataAndTitle(printList as List<BoxNotExist>);
                    rptPrint = new ReportPrintTool(rptBoxFoundWithtitle);
                    break;
            }

            //rptPrint.PrintingSystem.StartPrint += PrintingSystem_StartPrint; 
            //rptPrint.ShowPreviewDialog();
            if(string.IsNullOrEmpty(Properties.Settings.Default.PRINTER_NAME))
                rptPrint.Print();
            else
                rptPrint.Print(Properties.Settings.Default.PRINTER_NAME);
        }

        private static void PrintingSystem_StartPrint(object sender, DevExpress.XtraPrinting.PrintDocumentEventArgs e)
        {
            for (int i = 0; i < e.PrintDocument.PrinterSettings.PaperSources.Count; i++)
            {
                //Su dung khay giay so 1 cua may in
                if (e.PrintDocument.PrinterSettings.PaperSources[i].SourceName.StartsWith("Tray 1"))
                {
                    e.PrintDocument.DefaultPageSettings.PaperSource = e.PrintDocument.PrinterSettings.PaperSources[i];
                    break;
                }
            }
        }
    }
}
