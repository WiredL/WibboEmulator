namespace WibboEmulator.Communication.Packets.Incoming.Moderation;
using WibboEmulator.Games.GameClients;

internal class ReleaseTicketEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!session.GetUser().HasPermission("perm_mod"))
        {
            return;
        }

        var num = packet.PopInt();
        for (var index = 0; index < num; ++index)
        {
            WibboEnvironment.GetGame().GetModerationManager().ReleaseTicket(session, packet.PopInt());
        }
    }
}
