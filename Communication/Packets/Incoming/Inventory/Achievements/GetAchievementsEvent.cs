namespace WibboEmulator.Communication.Packets.Incoming.Inventory.Achievements;
using WibboEmulator.Games.GameClients;

internal class GetAchievementsEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet) => WibboEnvironment.GetGame().GetAchievementManager().GetList(session);
}