namespace WibboEmulator.Games.Chat.Commands.User.Several;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal class GiveItem : IChatCommand
{
    public void Execute(GameClient session, Room room, RoomUser userRoom, string[] parameters)
    {
        if (parameters.Length != 2)
        {
            return;
        }

        if (userRoom.CarryItemID <= 0 || userRoom.CarryTimer <= 0)
        {
            return;
        }

        var roomUserByUserIdTarget = room.RoomUserManager.GetRoomUserByName(parameters[1]);
        if (roomUserByUserIdTarget == null)
        {
            return;
        }

        if (Math.Abs(userRoom.X - roomUserByUserIdTarget.X) >= 3 || Math.Abs(userRoom.Y - roomUserByUserIdTarget.Y) >= 3)
        {
            return;
        }

        roomUserByUserIdTarget.CarryItem(userRoom.CarryItemID);
        userRoom.CarryItem(0);
    }
}
