namespace WibboEmulator.Communication.Packets.Outgoing.Navigator.New;
using WibboEmulator.Games.Navigator;

internal class NavigatorMetaDataParserComposer : ServerPacket
{
    public NavigatorMetaDataParserComposer(ICollection<TopLevelItem> TopLevelItems)
        : base(ServerPacketHeader.NAVIGATOR_METADATA)
    {
        this.WriteInteger(TopLevelItems.Count);//Count
        foreach (var TopLevelItem in TopLevelItems.ToList())
        {
            //TopLevelContext
            this.WriteString(TopLevelItem.SearchCode);//Search code
            this.WriteInteger(0);//Count of saved searches?
            /*{
                //SavedSearch
                base.WriteInteger(TopLevelItem.Id);//Id
               base.WriteString(TopLevelItem.SearchCode);//Search code
               base.WriteString(TopLevelItem.Filter);//Filter
               base.WriteString(TopLevelItem.Localization);//localization
            }*/
        }
    }
}
