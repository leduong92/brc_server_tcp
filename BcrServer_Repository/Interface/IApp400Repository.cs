using BcrServer_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Repository
{
    public interface IApp400Repository
    {
        #region [METHOD'S KHOA]
        int UpdateFinishedLotQty(string boxNo);

        int UpdateStatusIncomingBox(string boxNo);

        int InsertBoxReturn(string boxNo, string user);

        int DeleteBoxDelivery(string boxNo);
        int DeleteBoxInfo(string boxNo);
        int DeletePclPrint(string boxNo);
        int DeleteDailyBoxRec(string boxNo);

        DataTable GetBoxInfo(string boxNo);

        DataTable GetBoxInfoReturnWithCondition(string pallete = null, string shippingTo = null, int type = 0);

        int UpdateBoxInfoStatusReturn(int type = 0, string boxNo = null, string pallete = null);
        DataTable GetBoxForSwitchPallete(string pallete);

        bool IsBoxStored(string pallete, string boxNo);

        int UpdatePalleteInBoxInfo(string pallete, string boxNo);

        DataTable GetEtdBoxInfo(string shippingTo);

        DataTable GetAllBoxFromShipping(string shippingTo);

        int UpdateStatusOfBoxReturnShippingToReceived(string shipping, string box, string date, string time, string user);

        int DeleteTTShipingPrint(string boxNo);

        #endregion [METHOD'S KHOA]


        #region [METHOD'S DUONG]

        #region [OS] TRA THUNG HANG TU INSTOCK VE KHU VUC RECEIVE
        DataTable GetBoxFromPallete(string pallete);

        DataTable GetDataBoxInfo(string palleteTypeNo, string boxNo);

        int UpdBoxDelivery(string boxNo, string userId);

        int DelBoxInfo(string palleteTypeNo, string boxNo);

        int InsertTsStockResult(string values);

        int UpdatePalleteInBoxDelivery(string pallete, string boxNo);
        #endregion

        #region [OR] TRA THUNG HANG TU INSTOCK VE KHU VUC RECEIVE
        DataTable GetDataBoxInfo(string boxNo);

        int UpdBoxDelivery(string boxNo);

        int DelBoxInfo(string boxNo);

        #endregion

        #region CHINH SUA NGAY ETD VA SO LAN XUAT

        DataTable GetDataPallete(string pallete);

        DataTable GetDataPalleteTypeNo(string palleteTypeNo);

        int UpdBoxInFo(string setQuery, string palleteNo);
        #endregion

        #endregion [METHOD'S DUONG]


        #region [METHOD'S THANH]

        #endregion [METHOD'S THANH]


        #region [METHOD'S HIEU]
        DataTable GetTotalBoxOS(string palleteNo);
        DataTable GetPalleteOS(string palleteNo);

        DataTable GetLostShippingToOS(string palltete);
        DataTable GetCheckPallete(string palleteNo);
        DataTable GetSeq(string palleteNo);
        int SetUpdatePalleteWHSeq(string palleteNo, int seqNo);
        #endregion [METHOD'S HIEU]


        #region [METHOD'S MY]
        DataTable GetAllBoxInPalleteExceptBorrowedBox(string pallete);
        DataTable GetInfoOfPalleteInBoxInfo(string pallete);
        DataTable GetDestinationInfoOfPallete(string pallete);
        bool CheckBoxIsStoredInPallete(string pallete, string box_no);
        int UpdateBoxInfoWhenChangePallete(TdBoxInfo box, List<string> listBox = null);
        DataTable GetMinMaxLotOfBox(string box);
        string GetMinLotBeforeReserOrShipByItem(string item, int status);
        string GetMinLotOfReceiveByItem(string item);
        int InsertTTShippingPrintByModel(TtShippingPrint shipping);
        DataTable GetAllBoxInPalleteReser(string pallete);
        bool CheckBoxIsReseredInPallete(string pallete, string box);
        int UpdateActualQtyWaitingPlan(int boxQty, string item, string ShippingTo);
        int GetStatusBoxInfoOfBox(string boxno);
        string GetMinLotStoredByItem(string item, string exceptShippingTo);
        string GetMaxLotOfReserShipOS(string item);

        #endregion [METHOD'S MY]

        bool FindBox(string boxNo);

        int UpdateFlagOfFindBox(string boxNo);
        bool CheckIsExistInInComingBox(string boxno);
        int CheckWaitingShippingToIsOK(string shippingTo);
        int UpdateShippingToForWaitingBox(string shippingTo, string box);
        DataTable GetAllWaitingBoxOfShippingTo(string shippingTo);
        string GetNumberPallete(string shippingTo);
        int GetNumberBoxIncompleteInPallete(string shippingto);
    }
}
