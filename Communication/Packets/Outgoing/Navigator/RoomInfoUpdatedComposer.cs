namespace WibboEmulator.Communication.Packets.Outgoing.Navigator;

internal class RoomInfoUpdatedComposer : ServerPacket
{
    public RoomInfoUpdatedComposer(int roomId)
        : base(ServerPacketHeader.ROOM_SETTINGS_UPDATED) => this.WriteInteger(roomId);
}
