namespace WibboEmulator.Communication.Packets.Incoming.Quests;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Quests;

internal sealed class GetCurrentQuestEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet) => QuestManager.GetCurrentQuest(session);
}
