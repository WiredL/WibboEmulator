namespace WibboEmulator.Communication.Packets.Incoming.Structure;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Games.GameClients;

internal class GetFurnitureAliasesMessageEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket Packet) => session.SendPacket(new FurnitureAliasesComposer());
}