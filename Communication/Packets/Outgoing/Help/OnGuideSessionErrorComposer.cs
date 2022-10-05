namespace WibboEmulator.Communication.Packets.Outgoing.Help;

internal class OnGuideSessionErrorComposer : ServerPacket
{
    public OnGuideSessionErrorComposer(int type)
        : base(ServerPacketHeader.GUIDE_SESSION_ERROR) => this.WriteInteger(type);
}
