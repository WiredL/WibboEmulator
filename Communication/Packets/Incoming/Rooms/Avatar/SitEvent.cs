namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Games.GameClients;

internal class SitEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        var room = session.GetUser().CurrentRoom;        if (room == null)
        {
            return;
        }

        var roomUserByUserId = room.GetRoomUserManager().GetRoomUserByUserId(session.GetUser().Id);        if (roomUserByUserId == null)
        {
            return;
        }

        if (roomUserByUserId.ContainStatus("sit") || roomUserByUserId.ContainStatus("lay"))
        {
            return;
        }

        if (roomUserByUserId.RotBody % 2 == 0)        {            roomUserByUserId.SetStatus("sit", "0.5");            roomUserByUserId.IsSit = true;            roomUserByUserId.UpdateNeeded = true;        }
    }
}
