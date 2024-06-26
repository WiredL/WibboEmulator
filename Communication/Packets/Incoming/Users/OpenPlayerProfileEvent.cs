namespace WibboEmulator.Communication.Packets.Incoming.Users;
using WibboEmulator.Communication.Packets.Outgoing.Users;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Groups;
using WibboEmulator.Games.Users;

internal sealed class OpenPlayerProfileEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var userId = packet.PopInt();

        _ = packet.PopBoolean();

        var targetData = UserManager.GetUserById(userId);
        if (targetData == null)
        {
            return;
        }

        var groups = GroupManager.GetGroupsForUser(targetData.MyGroups);

        var friendCount = 0;

        if (targetData.Messenger != null)
        {
            friendCount = targetData.Messenger.Count;
        }

        session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
    }
}
