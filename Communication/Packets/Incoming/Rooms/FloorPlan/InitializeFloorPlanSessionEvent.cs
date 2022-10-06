namespace WibboEmulator.Communication.Packets.Incoming.Rooms.FloorPlan;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.FloorPlan;
using WibboEmulator.Games.GameClients;

internal class InitializeFloorPlanSessionEvent : IPacketEvent
{
    public double Delay => 0;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (!WibboEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetUser().CurrentRoomId, out var room))
        {
            return;
        }

        session.SendPacket(new FloorPlanSendDoorComposer(room.GetGameMap().Model.DoorX, room.GetGameMap().Model.DoorY, room.GetGameMap().Model.DoorOrientation));
    }
}
