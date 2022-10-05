namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Communication.Packets.Outgoing.LandingView;
using WibboEmulator.Games.GameClients;

internal class GetPromoArticlesEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        var currentView = WibboEnvironment.GetGame().GetHotelView();        if (session == null || session.GetUser() == null)
        {
            return;
        }

        if (!(currentView.Count() > 0))
        {
            return;
        }
        session.SendPacket(new PromoArticlesComposer(currentView.HotelViewPromosIndexers));
    }
}
