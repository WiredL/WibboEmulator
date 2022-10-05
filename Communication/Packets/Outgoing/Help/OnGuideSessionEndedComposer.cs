namespace WibboEmulator.Communication.Packets.Outgoing.Help;

internal class OnGuideSessionEndedComposer : ServerPacket
{
    public OnGuideSessionEndedComposer(int type)
        : base(ServerPacketHeader.GUIDE_SESSION_ENDED) => this.WriteInteger(type);
}
