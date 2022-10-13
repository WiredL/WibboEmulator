namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Avatar;
using WibboEmulator.Games.GameClients;

internal class DropHandItemEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetUser().CurrentRoomId, out var room))
        {
            return;
        }

        var roomUserByUserId = room.RoomUserManager.GetRoomUserByUserId(session.GetUser().Id);
        if (roomUserByUserId == null || roomUserByUserId.CarryItemID <= 0 || roomUserByUserId.CarryTimer <= 0)
        {
            return;
        }

        roomUserByUserId.CarryItem(0);
    }
}
