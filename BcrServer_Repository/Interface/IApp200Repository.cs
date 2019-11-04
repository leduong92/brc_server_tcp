using BcrServer_Model;
using System.Data;

namespace BcrServer_Repository
{
    public interface IApp200Repository
    {
        DataTable GetBoxInfomationByBoxNo(string boxNo);
        DataTable GetChangePalleteInfoByBoxNo(string boxNo);
        DataTable GetPalleteInfoByPalleteNo(string palleteNo);
        DataTable CheckBoxAlreadyArrangedtoPallete(string boxNo);   
        DataTable GetBoxInfoByBoxNo(string boxNo);
        DataTable GetBoxAndReceiveByPclNo(string pclNo);
        DataTable GetAllBoxByShippingTo(string shippingTo);
        DataTable GetAllBoxInPalleteByBoxNo(string boxNo);
        DataTable GetBoxInfoByBoxJob(string boxNo);
        bool IsBoxExistsOnNewPallete(string boxNo, string palleteNo);
        int UpdatePalleteNoByBoxNo(string boxNo, string palleteTypeNo, string palleteNo);
        DataTable GetAllBoxNotInPallete(string notIn, string oneBox);
        int DeleteBoxNotInPallete(string oneBox);
        int InsertToBoxNotInPallete(BoxNotExist box);
        bool IsPalleteExists(string palleteNo);

        int UpdateContainerByPallete(string palleteNo, string container, string exportSeq, string containerDate, string location, string user, string containerTime);
        string GetSymbolOfPalleteNo(string palleteNo);
        bool IsPalleteCanExport(string palleteNo);

        int SetContainerLockSignForPallete(string container);

        DataTable GetBoxInfoByContainerNoAndDate(string container, string containerDate);

        int UpdateContainerLockSign(string container, string containerDate);
    }
}
