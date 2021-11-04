using System.Data;
using Butterfly.Database.Interfaces;

namespace Butterfly.Database.Daos
{
    class CatalogVoucherDao
    {
        internal static void Update(IQueryAdapter dbClient, string code)
        {
            dbClient.SetQuery("UPDATE catalog_vouchers SET current_uses = current_uses + '1' WHERE voucher = @code LIMIT 1");
            dbClient.AddParameter("code", code);
            dbClient.RunQuery();
        }

        internal static DataTable GetAll(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT voucher,type,value,current_uses,max_uses FROM catalog_vouchers WHERE enabled = '1'");
            return dbClient.GetTable();
        }
    }
}