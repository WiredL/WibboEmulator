namespace WibboEmulator.Communication.Packets.Incoming.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Catalog;
using WibboEmulator.Games.Catalogs;
using WibboEmulator.Games.GameClients;

internal sealed class GetClubOffersEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var offerId = packet.PopInt();

        var pagePremium = CatalogManager.Pages.FirstOrDefault(x => x.Template == "vip_buy");

        if (pagePremium == null)
        {
            return;
        }

        if (!pagePremium.Enabled || !pagePremium.HavePermission(session.User))
        {
            return;
        }

        session.SendPacket(new HabboClubOffersComposer(pagePremium.Items));
    }
}
