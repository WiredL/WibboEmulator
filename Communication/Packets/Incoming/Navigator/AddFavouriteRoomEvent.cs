namespace WibboEmulator.Communication.Packets.Incoming.Navigator;
using WibboEmulator.Communication.Packets.Outgoing.Navigator;
using WibboEmulator.Database;
using WibboEmulator.Database.Daos.User;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal sealed class AddFavouriteRoomEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.User == null)
        {
            return;
        }

        var roomId = packet.PopInt();

        var roomData = RoomManager.GenerateRoomData(roomId);
        if (roomData == null || session.User.FavoriteRooms.Count >= 30 || session.User.FavoriteRooms.Contains(roomId))
        {
            return;
        }
        else
        {
            session.SendPacket(new UpdateFavouriteRoomComposer(roomId, true));

            session.User.FavoriteRooms.Add(roomId);

            using var dbClient = DatabaseManager.Connection;
            UserFavoriteDao.Insert(dbClient, session.User.Id, roomId);
        }
    }
}
