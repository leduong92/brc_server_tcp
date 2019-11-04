using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcrServer_Repository
{
    public interface ICommonRepository
    {
        DataTable GetDataByUserId(string userId);

        int Insert();
        int Insert2();
    }
}
