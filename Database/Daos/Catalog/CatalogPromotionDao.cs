﻿namespace WibboEmulator.Database.Daos;
using System.Data;
using WibboEmulator.Database.Interfaces;

internal class CatalogPromotionDao
{
    internal static DataTable GetAll(IQueryAdapter dbClient)
    {
        dbClient.SetQuery("SELECT `id`, `title`, `title_en`, `title_br`, `image`, `unknown`, `page_link`, `parent_id` FROM `catalog_promotion`");
        return dbClient.GetTable();
    }
}
