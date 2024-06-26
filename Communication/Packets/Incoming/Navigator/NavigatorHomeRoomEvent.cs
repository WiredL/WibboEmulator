namespace WibboEmulator.Communication.Packets.Incoming.Navigator;
using WibboEmulator.Communication.Packets.Outgoing.Navigator;
using WibboEmulator.Database;
using WibboEmulator.Database.Daos.User;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class NavigatorHomeRoomEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var roomId = packet.PopInt();

        var roomData = RoomManager.GenerateRoomData(roomId);
        if (roomId != 0 && (roomData == null || !roomData.OwnerName.ToLower().Equals(session.User.Username, StringComparison.CurrentCultureIgnoreCase)))
        {
            return;
        }

        session.User.HomeRoom = roomId;
        using (var dbClient = DatabaseManager.Connection)
        {
            UserDao.UpdateHomeRoom(dbClient, session.User.Id, roomId);
        }

        session.SendPacket(new NavigatorHomeRoomComposer(roomId, 0));
    }
}
