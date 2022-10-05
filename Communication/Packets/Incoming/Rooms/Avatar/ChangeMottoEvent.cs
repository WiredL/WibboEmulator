namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Database.Daos;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Quests;
using WibboEmulator.Utilities;

internal class ChangeMottoEvent : IPacketEvent
{
    public double Delay => 500;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        var newMotto = StringCharFilter.Escape(Packet.PopString());
        if (newMotto == session.GetUser().Motto)
        {
            return;
        }

        if (newMotto.Length > 38)
        {
            newMotto = newMotto[..38];
        }

        if (session.Antipub(newMotto, "<MOTTO>"))
        {
            return;
        }

        if (!session.GetUser().HasPermission("perm_word_filter_override"))
        {
            newMotto = WibboEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(newMotto);
        }

        if (session.GetUser().IgnoreAll)
        {
            return;
        }

        session.GetUser().Motto = newMotto;

        using (var dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            UserDao.UpdateMotto(dbClient, session.GetUser().Id, newMotto);
        }

        WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.PROFILE_CHANGE_MOTTO, 0);

        if (session.GetUser().InRoom)
        {
            var currentRoom = session.GetUser().CurrentRoom;
            if (currentRoom == null)
            {
                return;
            }

            var roomUserByUserId = currentRoom.GetRoomUserManager().GetRoomUserByUserId(session.GetUser().Id);
            if (roomUserByUserId == null)
            {
                return;
            }

            if (roomUserByUserId.IsTransf || roomUserByUserId.IsSpectator)
            {
                return;
            }

            currentRoom.SendPacket(new UserChangeComposer(roomUserByUserId, false));
        }

        WibboEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Motto", 1);
    }
}