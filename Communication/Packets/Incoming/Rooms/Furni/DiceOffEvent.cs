namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Furni;
using WibboEmulator.Games.GameClients;

internal class DiceOffEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetUser().CurrentRoomId, out var room))
        {
            return;
        }

        var roomItem = room.GetRoomItemHandler().GetItem(packet.PopInt());
        if (roomItem == null)
        {
            return;
        }

        var UserHasRights = false;
        if (room.CheckRights(session))
        {
            UserHasRights = true;
        }

        roomItem.Interactor.OnTrigger(session, roomItem, -1, UserHasRights, false);
        roomItem.OnTrigger(room.GetRoomUserManager().GetRoomUserByUserId(session.GetUser().Id));
    }
}
