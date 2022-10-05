﻿namespace WibboEmulator.Games.Navigator;
using System.Data;
using WibboEmulator.Core;
using WibboEmulator.Database.Daos;
using WibboEmulator.Database.Interfaces;

public sealed class NavigatorManager
{
    private readonly Dictionary<int, FeaturedRoom> _featuredRooms;

    private readonly Dictionary<int, TopLevelItem> _topLevelItems;
    private readonly Dictionary<int, SearchResultList> _searchResultLists;

    public NavigatorManager()
    {
        this._topLevelItems = new Dictionary<int, TopLevelItem>();
        this._searchResultLists = new Dictionary<int, SearchResultList>();

        this._topLevelItems.Add(1, new TopLevelItem(1, "official_view", "", ""));
        this._topLevelItems.Add(2, new TopLevelItem(2, "hotel_view", "", ""));
        this._topLevelItems.Add(3, new TopLevelItem(3, "rooms_game", "", ""));
        this._topLevelItems.Add(4, new TopLevelItem(4, "myworld_view", "", ""));

        this._featuredRooms = new Dictionary<int, FeaturedRoom>();
    }

    public void Init(IQueryAdapter dbClient)
    {
        if (this._searchResultLists.Count > 0)
        {
            this._searchResultLists.Clear();
        }

        if (this._featuredRooms.Count > 0)
        {
            this._featuredRooms.Clear();
        }

        var Table = NavigatorCategoryDao.GetAll(dbClient);

        if (Table != null)
        {
            foreach (DataRow Row in Table.Rows)
            {
                if (Convert.ToInt32(Row["enabled"]) == 1)
                {
                    if (!this._searchResultLists.ContainsKey(Convert.ToInt32(Row["id"])))
                    {
                        this._searchResultLists.Add(Convert.ToInt32(Row["id"]), new SearchResultList(Convert.ToInt32(Row["id"]), Convert.ToString(Row["category"]), Convert.ToString(Row["category_identifier"]), Convert.ToString(Row["public_name"]), true, -1, Convert.ToInt32(Row["required_rank"]), Convert.ToInt32(Row["minimized"]) == 1, NavigatorViewModeUtility.GetViewModeByString(Convert.ToString(Row["view_mode"])), Convert.ToString(Row["category_type"]), Convert.ToString(Row["search_allowance"]), Convert.ToInt32(Row["order_id"])));
                    }
                }
            }
        }

        var GetPublics = NavigatorPublicDao.GetAll(dbClient);

        if (GetPublics != null)
        {
            foreach (DataRow Row in GetPublics.Rows)
            {
                if (Convert.ToInt32(Row["enabled"]) == 1)
                {
                    if (!this._featuredRooms.ContainsKey(Convert.ToInt32(Row["room_id"])))
                    {
                        this._featuredRooms.Add(Convert.ToInt32(Row["room_id"]), new FeaturedRoom(Convert.ToInt32(Row["room_id"]), Convert.ToString(Row["image_url"]), LanguageManager.ParseLanguage(Convert.ToString(Row["langue"])), (string)Row["category_type"]));
                    }
                }
            }
        }
    }

    public List<SearchResultList> GetCategorysForSearch(string Category)
    {
        var Categorys =
            from Cat in this._searchResultLists
            where Cat.Value.Category == Category
            orderby Cat.Value.OrderId ascending
            select Cat.Value;
        return Categorys.ToList();
    }

    public ICollection<SearchResultList> GetResultByIdentifier(string Category)
    {
        var Categorys =
            from Cat in this._searchResultLists
            where Cat.Value.CategoryIdentifier == Category
            orderby Cat.Value.OrderId ascending
            select Cat.Value;
        return Categorys.ToList();
    }

    public ICollection<SearchResultList> GetFlatCategories()
    {
        var Categorys =
            from Cat in this._searchResultLists
            where Cat.Value.CategoryType == NavigatorCategoryType.CATEGORY
            orderby Cat.Value.OrderId ascending
            select Cat.Value;
        return Categorys.ToList();
    }

    public ICollection<SearchResultList> GetEventCategories()
    {
        var Categorys =
            from Cat in this._searchResultLists
            where Cat.Value.CategoryType == NavigatorCategoryType.PROMOTION_CATEGORY
            orderby Cat.Value.OrderId ascending
            select Cat.Value;
        return Categorys.ToList();
    }

    public ICollection<TopLevelItem> GetTopLevelItems() => this._topLevelItems.Values;

    public ICollection<SearchResultList> GetSearchResultLists() => this._searchResultLists.Values;

    public bool TryGetTopLevelItem(int Id, out TopLevelItem TopLevelItem) => this._topLevelItems.TryGetValue(Id, out TopLevelItem);

    public bool TryGetSearchResultList(int Id, out SearchResultList SearchResultList) => this._searchResultLists.TryGetValue(Id, out SearchResultList);

    public bool TryGetFeaturedRoom(int RoomId, out FeaturedRoom PublicRoom) => this._featuredRooms.TryGetValue(RoomId, out PublicRoom);

    public ICollection<FeaturedRoom> GetFeaturedRooms(Language Langue) => this._featuredRooms.Values.Where(F => F.Langue == Langue).ToList();
}
