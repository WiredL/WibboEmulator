namespace WibboEmulator.Communication.Packets.Outgoing.RolePlay.Troc;

internal class RpTrocConfirmeComposer : ServerPacket
{
    public RpTrocConfirmeComposer(int userId)
      : base(ServerPacketHeader.RP_TROC_CONFIRME) => this.WriteInteger(userId);
}
