namespace WibboEmulator.Communication.Packets.Incoming.Rooms.Furni;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Achievements;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Purse;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Database;
using WibboEmulator.Database.Daos.Item;
using WibboEmulator.Database.Daos.User;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Rooms;

internal sealed class CreditFurniRedeemEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!session.User.InRoom)
        {
            return;
        }

        if (!RoomManager.TryGetRoom(session.User.RoomId, out var room))
        {
            return;
        }

        if (!room.CheckRights(session, true))
        {
            return;
        }

        var itemId = packet.PopInt();

        var exchange = room.RoomItemHandling.GetItem(itemId);
        if (exchange == null)
        {
            return;
        }

        if (exchange.Data.InteractionType != InteractionType.EXCHANGE)
        {
            return;
        }

        using var dbClient = DatabaseManager.Connection;
        ItemDao.DeleteById(dbClient, exchange.Id);

        room.RoomItemHandling.RemoveFurniture(null, exchange.Id);

        if (!int.TryParse(exchange.ItemData.ItemName.Split('_')[1], out var value))
        {
            return;
        }

        if (value > 0)
        {
            if (exchange.ItemData.ItemName.StartsWith("CF_") || exchange.ItemData.ItemName.StartsWith("CFC_"))
            {
                session.User.Credits += value;
                session.SendPacket(new CreditBalanceComposer(session.User.Credits));
            }
            else if (exchange.ItemData.ItemName.StartsWith("PntEx_"))
            {
                session.User.WibboPoints += value;
                session.SendPacket(new ActivityPointNotificationComposer(session.User.WibboPoints, 0, 105));

                UserDao.UpdateAddPoints(dbClient, session.User.Id, value);
            }
            else if (exchange.ItemData.ItemName.StartsWith("WwnEx_"))
            {
                UserStatsDao.UpdateAchievementScore(dbClient, session.User.Id, value);

                session.User.AchievementPoints += value;
                session.SendPacket(new AchievementScoreComposer(session.User.AchievementPoints));

                var roomUserByUserId = room.RoomUserManager.GetRoomUserByUserId(session.User.Id);
                if (roomUserByUserId != null)
                {
                    session.SendPacket(new UserChangeComposer(roomUserByUserId, true));
                    room.SendPacket(new UserChangeComposer(roomUserByUserId, false));
                }
            }
        }
    }
}
