using BcrServer_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Repository
{
    public interface IApp300Repository
    {
   
        DataTable GetAllBoxByPCL(string pclNo);

        DataTable GetAllBoxORWithoutPCL();

        DataTable GetAllBoxMitsubaByPCL(string pclNo, string items);

        DataTable FindBoxNotScannedYet(string boxes);

        string CheckingPclReceived(string pclNo);

        int UpdateIncomeStatusOfBox(string pclList, string userId, string instockDate);

        int InsertUnpostedJobBySelect(string pclList, string userId);

        DataTable GetAllBoxByPallete(string pallete);

        /// <summary>
        /// Lay group theo pallete
        /// </summary>
        /// <param name="pallete">type = 0 => 1 pallete  - type = 1 => 1 hoac nhieu pallete</param>
        /// <param name="type">0: td_palltetype_wh - 1: td_pallte_wh</param>
        /// <returns></returns>
        DataTable GetGroupNumberByPallete(string pallete, int type = 0);

        DataTable GetBoxInfoByManyBox(string boxes, int groups);

        int DeleteTdCheckStockByBoxNo(string boxNo);

        int InsertTdCheckStockBySelect(string pallete, string symbol, string box, string userId);

        DataTable GetAllPalletebyGroup(int groups);

        DataTable GetBoxInfoNotShipByPallete(string pallete);

        DataTable CheckBoxStatusByBoxNo(string boxNo);

        DataTable GetPalletePrintedShippingToForOR();
 
        DataTable GetPlcRecorded();
        DataTable GetAllBoxByPclNoOR(string pclNo);
        DataTable GetCountBoxInTdBoxDelivery(string pclNo);
        DataTable GetCountBoxInTtPclPrint(string pclNo);
        bool IsBoxNoRecoredFI(string boxNo);
        bool IsBoxNoReceived(string boxNo);
        long? GetTrnNoOfTsStockResult(string tableName);

        DataTable OS1GetALLDataReservedPallete(string shippingTos);

        DataTable OS1GetAllDataReceivedInStockNotInPallete(string item);

        int InsertOrUpdateFifo(string dataCurrent, string dataToCheck, string lotPrevious, string LotCurrent, string item, int status, int types);

        DataTable OS1GetItemNotInPalleteAndNotShipByDate(string item, string shippingTos, string date);

        DataTable OS1GetDataPallete(string pallete);

        int GetSeqForAll(int mode, string seqName);

        DataTable OS1GetShippingToInTdBoxInfo(string palleteNo);

        DataTable GetBoxJobTagByBoxInShippingTo(string boxNo);

        DataTable GetSeq(string columnsName, string entryDate);

        int InsertTtShippingPrint(string value);

        int UpdTdCheckStock(string query);

        int UpdTdBoxInfo(string numberShipSeq, string entryDate, string entryTime, string userId, string pallete);

        int InsertBoxTraceBySelect(string pallete);

        DataTable ORGetDataPallete(string shippingTos);

        DataTable ORGetDataPalleteInBoxInfo(string shippingTos);

        DataTable ORGetDataByPallete(string shippingTo);

        int UpdTdBoxInfoOR(string numberShipSeq, string entryDate, string entryTime, string userId, string pallete);

        DataTable SelectBoxInfo(string where);
        DataTable SelectGroupItem_BJBI(string palleteNo);
        DataTable SelectWithoutFrom(string item);
        DataTable SelectLotInfo(string item, string palleteNo, string date, int count);
        DataTable SelectBoxTop1(string where);
        DataTable SelectBoxDelivery(string where);
        int UpdBoxInfo(string set, string where);
        int InsertBoxFifo(string boxNo, string boxNoFf, string notes, int status, string entryDate, string entryTime, int types);
        DataTable SelectBoxFifo(string where);

        int UpdBoxFifo(string set, string where);

        /*Add new function check fifo for reser*/
        DataTable GetMaxLotByItemOnReceive(string item);
   
        DataTable GetTotalBoxOS(string pallete);//
        DataTable GetTotalBoxOR(string pallete);//
        DataTable GetPalleteOR(string boxNo);//
        DataTable GetCheckBoxInfoOS(string pallete, string boxNo);//    
        DataTable GetCheckPallete(string pallete);//
        DataTable GetCheckMaterSymbol(string symbol);//
        DataTable GetCheckCtrlGroupSeq(int groups, string year);//     
        DataTable GetRecCodeWH(string tableName, string date);//
        DataTable GetSeq(string tableNameSeq);//
        DataTable GetMaxPalletePrint(string palltete);//
        DataTable GetShippingToPrint(string palltete, int status = 0, string location = null);
        
        int SetInsertCtrlGroupSeq(int groups, string groupName, string years, int seqNo, string loc);//
        int SetUpdateCtrlGroupSeq(int groups, string years, int seqNo, string loc);//
        int SetResetSeq(string tableNameSeq);//
        int SetInsertRecCodeWH(string tableName, string date, int currentNo, string updDate);//
        int SetUpdateRecCodeWH(int currentNo, string tableName, string date);//
        int SetUpdateBoxInfoWH(string palleteNo, string date, string time, string user, int seq, string edtHCM, string edtVN, string palletetypeNo, string box);//
        int SetInsertPalleteWH(string palleteNo, string palletetypeNo, string item, int seq, string symbol, string status, string entryDate, string entryUser);//
        int SetInsertPalletePrint(string palleteNo, int seqNo, string date, string time);//

        int UpdatePackingUser(string shippingTo, string userId);
 
        DataTable GetPalleteDetailByBox(string boxno);
        DataTable GetPlaceOfPallete(string palleteTypeNo);
        string GetUser(string location, string userid);
        bool CheckBoxNoIsExistInPallete(string boxno);
        int CheckBoxNoConditionalsToStored(string boxno);
        bool CheckDestination(string boxno, string symbol, string location, string itemGroup);
        string GetSymbolOfBox(string boxno);
        int UpdateBoxDeliveryAfterStored(string boxno, string palleteTypeNo, string userid, DateTime date);
        int InsertToBoxInfoAfterSotred(TdBoxInfo box);
        DataTable GetInfoBoxDelivery(string box);
        int InsertTdStockWH(TdStockWH item);
        DataTable GetQtyStockByBoxJobtag(string box);
        int InsertTsStockResult(tsStockResult stock);
        DataTable GetInfoOfBoxByBoxJobtag(string box);
        long? GetTrnNoOfTsStockResult();
        bool CheckItemIsExistInStockWh(string item);
        int UpdateTdStockWh(string item, long qtyComplete);
        int CheckBoxCanStoreOR(string box, string item);
        bool CheckFiFoScanStoringOR(string box, string item);
        DataTable GetInfoWareHousePlanByItem(string item);
        long? GetNextNo(string typeName);
        DataTable GetInfoItemOfBoxIsInWaiting(string boxno);
        int GetWaitingQty(string item, string duedate);
        int GetWaitingQtyStored(string item, string duedate);
        DataTable GetAllboxCanStoredByItem(string item);
        int InsertIntoWaitingPlanLog(string userId, string item, string duedate, string boxno, string boxQty);
        string GetBoxQtyOfBox(string box);
        DataTable CheckBoxInWatingLog(string box);
        DataTable GetQtyWaitingByItem(string item);
        DataTable GetInfoOfShippingTo(string shippingTo);
        bool CheckItemIsWaitingItem(string item);
        DataTable GetQtyWaitingByShippingTo(string shippingTo, string item);
        DataTable GetDestinationAndBoxInPallete(string pallete);
        int UpdatePalleteTypeInBoxInfo(string pallete_no, string palleteType_no);
        int UpdatePalleteTypeInPalleteWh(string pallete_no, string palleteType_no, string symbol);
        bool CheckShippingToIsWaiting(string shippingTo);
        //check fifo ship OR
        DataTable GetMinAndMaxLotOfBoxOR(string boxno);
        DataTable MinLotOfItemIsNotStored(string item);
        DataTable MinLotOfItemIsStored(string item, string palleteNo);
        bool InsertOrUpdateBoxFiFo(string boxno, string boxff);
        DataTable GetItemAndQtyStoredByItemPallateOR(string palleteno);
        double BalanceOfItemInWHPlan(string item);
        DataTable GetBalanceAndItemInWHPlan(string palleteno);
        double GetQtyActualOfItemInPallete(string palleteno, string item);
        int DeleteWaitingBox(string boxno);

        bool CheckDestinationOfSpecialItem(string boxno, string pallete);

        string GetMaxLofOfItemStored(string item);
        DataTable GetWaitingboxInPalleteType(string palleteType);

    
    }
}
