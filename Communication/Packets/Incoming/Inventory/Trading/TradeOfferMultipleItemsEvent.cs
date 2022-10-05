namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Games.GameClients;

internal class TradeOfferMultipleItemsEvent : IPacketEvent
{
    public double Delay => 500;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetUser().CurrentRoomId, out var room))
        {
            return;
        }

        var userTrade = room.GetUserTrade(session.GetUser().Id);
        if (userTrade == null)
        {
            return;
        }

        var ItemCount = Packet.PopInt();
        for (var i = 0; i < ItemCount; i++)
        {
            var ItemId = Packet.PopInt();
            var userItem = session.GetUser().GetInventoryComponent().GetItem(ItemId);
            if (userItem == null)
            {
                continue;
            }

            userTrade.OfferItem(session.GetUser().Id, userItem, false);
        }

        userTrade.UpdateTradeWindow();
    }
}
