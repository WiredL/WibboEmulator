namespace WibboEmulator.Communication.Packets.Outgoing.Navigator.New;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Navigator;

internal class NavigatorSearchResultSetComposer : ServerPacket
{
    public NavigatorSearchResultSetComposer(string Category, string Data, ICollection<SearchResultList> SearchResultLists, GameClient session, int GoBack = 1, int FetchLimit = 12)
        : base(ServerPacketHeader.NAVIGATOR_SEARCH)
    {
        this.WriteString(Category);//Search code.
        this.WriteString(Data);//Text?

        this.WriteInteger(SearchResultLists.Count);//Count
        foreach (var SearchResult in SearchResultLists.ToList())
        {
            this.WriteString(SearchResult.CategoryIdentifier);
            this.WriteString(SearchResult.PublicName);
            this.WriteInteger(NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult.SearchAllowance) != 0 ? GoBack : NavigatorSearchAllowanceUtility.GetIntegerValue(SearchResult.SearchAllowance));//0 = nothing, 1 = show more, 2 = back Action allowed.
            this.WriteBoolean(SearchResult.Minimized);//True = minimized, false = open.
            this.WriteInteger(SearchResult.ViewMode == NavigatorViewMode.REGULAR ? 0 : SearchResult.ViewMode == NavigatorViewMode.THUMBNAIL ? 1 : 0);//View mode, 0 = tiny/regular, 1 = thumbnail

            NavigatorHandler.Search(this, SearchResult, Data, session, FetchLimit);
        }
    }
}
