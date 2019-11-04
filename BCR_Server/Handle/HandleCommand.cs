using BcrServer_Helper;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace BcrServer
{
    public class HandleCommand
    {
        static object syncObj = new object();
        public string DataToSent { get; set; }
        public HandleCommand()
        {
        }

        public void SendBackToHT(ref TcpClient client, ref NetworkStream nwStream, string dataSend = null)
        {
            try
            {
                lock (syncObj)
                {
                    dataSend = DataToSent;
                    string textToSend = dataSend.Length.ToString("D5") + dataSend;

                    Console.WriteLine(">> Send Back To: {0}", client.Client.RemoteEndPoint.ToString());
                    Console.WriteLine(">> Sending Back: " + textToSend);
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
                    //if (nwStream.CanWrite)
                    //{
                    //    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                    //}

                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                    Console.WriteLine(">> Send OK ({0} bytes)\n", dataSend.Length.ToString("D5"));

                    if (nwStream != null)
                    {
                        nwStream.Flush();
                        nwStream.Dispose();
                        nwStream.Close();
                        nwStream = null;
                    }

                    if (client != null)
                        client.Close();
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = File.AppendText(Properties.Settings.Default.PATH_EXECUTE + "\\Log.txt"))
                {
                    WriteLog.Instance.Write(ex.Message, "SendBackToHT", sw);
                }

                //Console.WriteLine(">> SendBackToHT \n" + ex.Message);

                if (nwStream != null)
                {
                    nwStream.Flush();
                    nwStream.Dispose();
                    nwStream.Close();
                    nwStream = null;
                }

                if (client != null)
                    client.Close();
            }
        }

        public void CommandDistribution(ref TcpClient client, ref NetworkStream nwStream, string dataReceived)
        {
            if (string.IsNullOrEmpty(dataReceived))
                return;

            string msg = string.Empty;
            string command = string.Empty;
            string subCommand = string.Empty;

            lock (syncObj)
            {
                HandleReceiveData hdlRcvData = new HandleReceiveData();
                hdlRcvData.SplitDataByModel(dataReceived);

                command = hdlRcvData.RecvList[0].MenuId.Remove(0, 1);
                subCommand = hdlRcvData.RecvList[0].SubMenu;

                DataToSent = string.Empty;

                switch (command.Substring(0, 1))
                {
                    case "1":
                        DataToSent = App1Commands(hdlRcvData, command, subCommand);
                        break;
                    case "2":
                        DataToSent = App2Commands(hdlRcvData, command, subCommand);
                        break;
                    case "3":
                        DataToSent = App3Commands(hdlRcvData, command, subCommand);
                        break;
                    case "4":
                        DataToSent = App4Commands(hdlRcvData, command, subCommand);
                        break;
                    case "5":
                        DataToSent = App5Commands(hdlRcvData, command, subCommand);
                        break;
                }
            }
            SendBackToHT(ref client, ref nwStream, DataToSent);
        }

        private string App1Commands(HandleReceiveData hdlRcvData, string command, string subCommand)
        {
            lock (syncObj)
            {
                DataToSent = string.Empty;

                switch (command)
                {
                    case "100": //User Checking
                        DataToSent = App100.Instance.GetUserProfileByUserId(hdlRcvData.RecvList[0].UserId);
                        break;
                }

                return DataToSent;
            }
        }

        private string App2Commands(HandleReceiveData hdlRcvData, string command, string subCommand)
        {
            lock (syncObj)
            {
                DataToSent = string.Empty;
                switch (command)
                {
                    case "200": //Thay doi pallete su dung cho OR
                        DataToSent = App200.Instance.ChangePalleteByBoxNo(hdlRcvData.RecvList[0].Data);
                        break;
                    case "201":
                        DataToSent = App200.Instance.GetBoxAndReceiveByPclNo(hdlRcvData.RecvList[0].Data);
                        break;
                    case "202":
                        DataToSent = AppCommon.ShowOkMsg(0);
                        break;
                    case "203":
                        switch (subCommand)
                        {
                            case "S01": //Tim kiem thung hang bi thieu tren pallete
                                DataToSent = App200.Instance.PrintBoxFoundInPallete(hdlRcvData.RecvList[0].Data);
                                break;
                            default: //No SubCommand // Lay tat ca cac thung co tren pallete dua vao 1 thung bat ky.
                                DataToSent = App200.Instance.GetAllBoxInPalleteByBoxNo(hdlRcvData.RecvList[0].Data[0]);
                                break;
                        }
                        break;
                    case "204": // Dua hang len container
                        DataToSent = App200.Instance.LoadPalleteToContainer(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                        //DataToSent = AppCommon.ShowOkMsg(0);
                        break;
                    case "205": // Chot container
                        DataToSent = App200.Instance.SetLockSignForPallete(hdlRcvData.RecvList[0].Data);
                        break;
                }
                return DataToSent;
            }
        }

        private string App3Commands(HandleReceiveData hdlRcvData, string command, string subCommand)
        {
            lock (syncObj)
            {
                DataToSent = string.Empty;

                switch (command)
                {
                    //20191015 - Khong su dung chuc nang in phieu pcl or
                    //case "300": //IN PHIEU PCL NHAP KHO CHO ORING
                    //    DataToSent = App300.Instance.PrintPCLOR();
                    //    break;
                    case "301": //Nhan Hag thanh pham
                        //switch (subCommand)
                        //{
                        //    //Cap nhat trang thai da nhap kho cho cac thung hang 
                        //    //Insert du lieu cho Incoming Protection.
                        //    case "S01":
                        //        DataToSent = App300.Instance.ReceiveBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                        //        break;
                        //    //Kiem tra trang thai PCL va tra ve tat ca cac thung thuoc ds PCL gui len
                        //    default: //No SubCommand
                        //        DataToSent = App300.Instance.GetAllBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                        //        break;
                        //}

                        switch (hdlRcvData.RecvList[0].Location)
                        {
                            case "OS1":
                                switch (subCommand)
                                {
                                    //Cap nhat trang thai da nhap kho cho cac thung hang 
                                    //Insert du lieu cho Incoming Protection.
                                    case "S01":
                                        DataToSent = App300.Instance.ReceiveBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                        break;
                                    //Kiem tra trang thai PCL va tra ve tat ca cac thung thuoc ds PCL gui len
                                    default: //No SubCommand
                                        DataToSent = App300.Instance.GetAllBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                        break;
                                }
                                break;
                            case "OR1":
                                switch (subCommand)
                                {
                                    case "S01":
                                        DataToSent = App300.Instance.GetAllBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                        break;
                                    case "S02": //Instock
                                        DataToSent = App300.Instance.ReceiveBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                        break;
                                    case "S03": //Check exist MITSUBA ITEM.
                                        DataToSent = App300.Instance.GetBoxOfMitsuba(hdlRcvData.RecvList[0].Data);
                                        break;
                                    case "S04": //Check box thieu
                                        DataToSent = App300.Instance.GetMissingBox(hdlRcvData.RecvList[0].Data[0]);
                                        break;
                                    default: //Get all pcl not receive yet for OR
                                        DataToSent = App300.Instance.GetAllPCLNotReceiveYetForOR();
                                        break;
                                }
                                break;
                        }
                        break;
                    case "302": //SAP XEP HANG LEN PALLETE
                        switch (subCommand)
                        {
                            case "S01": //Kiem tra tra ve noi den cua pallete 
                                DataToSent = App300.Instance.CheckPlaceInPallete(hdlRcvData.RecvList[0].Data[0]);
                                break;
                            case "S02":
                                DataToSent = App300.Instance.StoringOS(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                            case "S03":
                                DataToSent = App300.Instance.StoringOR(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data[0]);
                                break;
                            case "S04": //Kiem tra hang cho OS
                                DataToSent = App300.Instance.CheckWaitingData(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                        }
                        break;
                    case "303"://In shipping to / shipping mark
                        switch (hdlRcvData.RecvList[0].Location)
                        {
                            case "OS1":
                                switch (subCommand)
                                {
                                    case "S01": //Lay tat ca cac thung tren pallete
                                        DataToSent = App300.Instance.GetTotalBoxOS(hdlRcvData.RecvList[0].Data[0]);
                                        break;
                                    default: //No SubCommand // In shipping to / Shipping mark.
                                        DataToSent = App300.Instance.SetShippingToOS(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                        break;
                                }
                                break;
                            case "OR1":
                                switch (subCommand)
                                {
                                    case "S01": //Lay tat ca cac thung dua vao thung dau tien
                                        DataToSent = App300.Instance.GetTotalBoxOR(hdlRcvData.RecvList[0].Data[0], hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId);
                                        break;
                                    default: //No SubCommand //  In shipping to / Shipping mark.
                                        DataToSent = App300.Instance.SetShippingToOR(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data[0]);
                                        break;
                                }
                                break;
                        }
                        break;
                    //20191007 - PLN / WH y/c khoa chuc nang Reser.
                    //case "304": //Reservation
                    //    DataToSent = App300.Instance.Reservation(hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                    //    break;
                    case "305": //SHIP
                        switch (hdlRcvData.RecvList[0].Location)
                        {
                            case "OS1":
                                //ShipOS
                                DataToSent = App300.Instance.ShipOS(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                            case "OR1":
                                //ShipOR
                                DataToSent = App300.Instance.ShipOR(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "306": //Kiem ton kho
                        if (hdlRcvData.RecvList[0].Location.ToUpper().Equals("OS1"))
                        {
                            switch (subCommand)
                            {
                                case "S01":
                                    DataToSent = App300.Instance.HandleDataForInventoryChecking(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                    break;
                                //Lay thong tin cua nhung pallete da in shipping to nhungw chua ship trong tuan nay => cung can kiem ton kho.
                                case "S02":
                                    DataToSent = App300.Instance.GetAllPalletePrintedShippingTo(hdlRcvData.RecvList[0].Data[0]);
                                    break;
                                case "S03":
                                    DataToSent = App300.Instance.HandlePalleteForInventoryChecking(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                    break;
                                default:
                                    DataToSent = App300.Instance.GetAllBoxByPalleteNo(hdlRcvData.RecvList[0].Data[0]);
                                    break;
                            }
                        }
                        else if (hdlRcvData.RecvList[0].Location.ToUpper().Equals("OR1"))
                        {
                            switch (subCommand)
                            {
                                case "R01":
                                    DataToSent = App300.Instance.HandleDataForInventoryCheckingForOR(hdlRcvData.RecvList[0].Data[0], hdlRcvData.RecvList[0].UserId);
                                    break;
                                case "R02":
                                    DataToSent = App300.Instance.GetAllPalletePrintedShippingToForOR();
                                    break;
                                case "R03":
                                    DataToSent = App300.Instance.HandlePalleteForInventoryCheckingForOR(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                    break;
                                default:
                                    DataToSent = "";
                                    break;
                            }
                        }

                        break;
                    
                }

                return DataToSent;
            }
        }

        private string App4Commands(HandleReceiveData hdlRcvData, string command, string subCommand)
        {
            lock (syncObj)
            {
                DataToSent = string.Empty;
                switch (command)
                {
                    case "400": //TRA HANG VE SAN XUAT
                        if (hdlRcvData.RecvList[0].Location.ToUpper().Equals("OS1"))
                        {
                            switch (subCommand)
                            {
                                case "S01":  //TRA HANG VE SAN XUAT
                                    DataToSent = App400.Instance.ReturnBoxToProduction(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                    break;
                                case "S02": //CHO SAN XUAT MUON LAI THUNG HANG
                                    DataToSent = App400.Instance.BorrowBoxForOS(hdlRcvData.RecvList[0].Data);
                                    break;
                                case "S03": //NHAN LA THUNG HANG TU SAN XUAT
                                    DataToSent = App400.Instance.GetBoxFromPalleteOrShippingTo(hdlRcvData.RecvList[0].Data[0]);
                                    break;
                                case "S04": //NHAN LA THUNG HANG TU SAN XUAT
                                    DataToSent = App400.Instance.TakeBoxBackForOS(hdlRcvData.RecvList[0].Data);
                                    break;
                            }
                        }
                        else if (hdlRcvData.RecvList[0].Location.ToUpper().Equals("OR1"))
                        {
                            switch (subCommand)
                            {
                                case "S01":  //TRA HANG VE SAN XUAT
                                    DataToSent = App400.Instance.ReturnBoxToProduction(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                    break;
                            }
                        }
                        break;
                    case "401": //TRA THUNG HANG TU INSTOCK VE RECEIVE
                        switch (hdlRcvData.RecvList[0].Location)
                        {
                            case "OS1":
                                switch (subCommand)
                                {
                                    case "S01": // get all box from pallete
                                        DataToSent = App400.Instance.GetBoxByPallete(hdlRcvData.RecvList[0].Data[0]);
                                        break;
                                    default:
                                        DataToSent = App400.Instance.OSReturnBoxFromInstockToReceive(hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].Data);
                                        break;
                                }
                                break;
                            case "OR1":
                                DataToSent = App400.Instance.ORReturnBoxFromInstockToReceive(hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].Data);
                                break;
                        }
                        break;
                    case "402": //HOAN DOI THUNG GIUA 2 SHIPPING TO, TRA HANG TU SHIPPING TO RA SAP XEP
                        switch (subCommand)
                        {
                            case "S01": //TRA VE TAT CA CAC BOX TRONG SHIPPING TO 
                                DataToSent = App400.Instance.GetAllBoxInPalleteExceptBorrowedBox(hdlRcvData.RecvList[0].Data[0]);
                                break;
                            case "S02": //HOAN DOI SHIPPING OS/OR
                                DataToSent = App400.Instance.TranferPalleteTemp(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                        }
                        break;
                    case "403"://In mat shipping to
                        DataToSent = App400.Instance.SetLostShippingToOR(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data[0]);
                        break;
                    case "404": //NHAN HANG THNAH PHAM NGAY KIEM KE
                        switch (subCommand)
                        {
                            //Cap nhat trang thai da nhap kho cho cac thung hang 
                            //Insert du lieu cho Incoming Protection.
                            case "S01":
                                DataToSent = App400.Instance.InstockWhenCheckInventory(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                break;
                            //Kiem tra trang thai PCL va tra ve tat ca cac thung thuoc ds PCL gui len
                            default: //No SubCommand
                                DataToSent = App300.Instance.GetAllBoxByPCL(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                break;
                        }
                        break;
                    case "405"://In shipping to chi dinh
                        DataToSent = App400.Instance.PrintShippingToSpecify(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                        break;
                    case "406": //HOAN DOI THUNG - PALLETE
                        switch (subCommand)
                        {
                            case "S01":
                                DataToSent = App400.Instance.SwitchBoxInPallete(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                break;
                            default:
                                DataToSent = App400.Instance.CheckPalleteForSwitchBox(hdlRcvData.RecvList[0].Data[0]);
                                break;
                        }
                        break;
                    case "407"://Reprint shipping to / shipping mark

                        switch (subCommand)
                        {
                            case "S01": //Lay tat ca cac thung tren pallete
                                DataToSent = App400.Instance.GetTotalBoxOS(hdlRcvData.RecvList[0].Data[0]);
                                break;
                            default: //No SubCommand // Reprint shipping to / Shipping mark.
                                DataToSent = App400.Instance.PrintReprintShippingTo(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data[0]);
                                break;
                        }
                        break;
                    case "408"://Doi vi tri thu tu phieu shipping to
                        DataToSent = App400.Instance.SetTranferNumPLT(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                        break;
                    case "409":
                        DataToSent = App400.Instance.ChangeEtdAndNOT(hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].Data);
                        break;
                    case "411"://Reprint shipping to / shipping mark (The goods has ship)
                        DataToSent = App400.Instance.SetLostShippingToOS(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data[0]);
                        break;
                    case "412": //INSTOCK TO RESER - SHIP
                        switch (subCommand)
                        {
                            case "S01": //KIEM TRA PALLETE TRA VE NOI DEN
                                DataToSent = App300.Instance.CheckPlaceInPallete(hdlRcvData.RecvList[0].Data[0]);
                                break;
                            case "S02": //TRANSFER INSTOCK TO RESER - SHIP
                                DataToSent = App400.Instance.TransferInstockToReserShip(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                                break;
                            default:
                                break;
                        }
                        break;
                    case "414": //THEM THUNG TU KV NGOAI VAO SHIPPING TO
                        switch (subCommand)
                        {
                            case "S01":
                                DataToSent = App400.Instance.AddBoxFromPalleteIntoShippingTo(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Location);
                                break;
                            default:
                                DataToSent = App400.Instance.GetTotalBoxInPallete(hdlRcvData.RecvList[0].Data[0]);
                                break;
                        }
                        break;
                    case "415": // TRA THUNG TU SHIP VE INSTOCK
                        switch (subCommand)
                        {
                            case "S01":
                                DataToSent = App400.Instance.GetBoxFromShipping(hdlRcvData.RecvList[0].Data[0]);
                                break;
                            case "S02":
                                DataToSent = App400.Instance.ReturnBoxFromShippingToReceived(hdlRcvData.RecvList[0].Data, hdlRcvData.RecvList[0].UserId);
                                break;
                        }
                        break;
                    case "416": //KIEM TRA BOX CO THE DUA LEN SHIPPING TO NAO
                        DataToSent = App400.Instance.CheckBoxCanInsertToShippingTo(hdlRcvData.RecvList[0].Data[0]);
                        break;
                    //case "417": //TRANSFER AFTER RESER
                    //    switch (subCommand)
                    //    {
                    //        case "S01": //CHECK PALLETE AND RETURN ALL BOX IN PALLETE
                    //            DataToSent = App400.Instance.GetAllBoxInPalleteResered(hdlRcvData.RecvList[0].Data[0]);
                    //            break;
                    //        case "S02":
                    //            DataToSent = App400.Instance.TranferAfterResered(hdlRcvData.RecvList[0].Location, hdlRcvData.RecvList[0].UserId, hdlRcvData.RecvList[0].Data);
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //    break;
                    case "418":
                        DataToSent = App400.Instance.FindBox(hdlRcvData.RecvList[0].Data[0]);
                        break;
                    case "419": //KIEM TRA TINH TRANg SHIPPING TO HANG CHO
                        DataToSent = App400.Instance.CheckStatusShippingTo(hdlRcvData.RecvList[0].Data[0]);
                        break;

                }

                return DataToSent;
            }
        }

        private string App5Commands(HandleReceiveData hdlRcvData, string command, string subCommand)
        {
            lock (syncObj)
            {
                DataToSent = string.Empty;
                switch (command)
                {
                    case "500":
                        DataToSent = "";
                        break;
                }

                return DataToSent;
            }
        }
    }
}
