namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms.PathFinding;

internal class LookToEvent : IPacketEvent
{
    public double Delay => 100;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetUser().CurrentRoomId, out var room))
        {
            return;
        }

        var User = room.GetRoomUserManager().GetRoomUserByUserId(session.GetUser().Id);
        if (User == null || User.RidingHorse)
        {
            return;
        }

        User.Unidle();
        var X2 = Packet.PopInt();
        var Y2 = Packet.PopInt();
        if (X2 == User.X && Y2 == User.Y)
        {
            if (User.SetStep)
            {
                var rotation = Rotation.RotationIverse(User.RotBody);
                User.SetRot(rotation, false, true);
            }
            return;
        }

        var Rotation2 = Rotation.Calculate(User.X, User.Y, X2, Y2);
        User.SetRot(Rotation2, false);
    }
}