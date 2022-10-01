﻿using WibboEmulator.Database.Daos;
using WibboEmulator.Database.Interfaces;
using System.Data;

namespace WibboEmulator.Games.Catalog.Vouchers
{
    public class VoucherManager
    {
        private readonly Dictionary<string, Voucher> _vouchers;

        public VoucherManager()
        {
            this._vouchers = new Dictionary<string, Voucher>();
        }

        public void Init(IQueryAdapter dbClient)
        {
            if (this._vouchers.Count > 0)
            {
                this._vouchers.Clear();
            }

            DataTable GetVouchers = CatalogVoucherDao.GetAll(dbClient);

            if (GetVouchers != null)
            {
                foreach (DataRow Row in GetVouchers.Rows)
                {
                    this._vouchers.Add(Convert.ToString(Row["voucher"]), new Voucher(Convert.ToString(Row["voucher"]), Convert.ToString(Row["type"]), Convert.ToInt32(Row["value"]), Convert.ToInt32(Row["current_uses"]), Convert.ToInt32(Row["max_uses"])));
                }
            }
        }

        public bool TryGetVoucher(string Code, out Voucher Voucher)
        {
            if (this._vouchers.TryGetValue(Code, out Voucher))
            {
                return true;
            }

            return false;
        }
    }
}
