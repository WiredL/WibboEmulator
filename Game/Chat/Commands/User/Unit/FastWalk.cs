﻿using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;

namespace Butterfly.Game.Chat.Commands.Cmd
{
    internal class FastWalk : IChatCommand
    {
        public void Execute(Client Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            UserRoom.WalkSpeed = !UserRoom.WalkSpeed;

            if (UserRoom.WalkSpeed)
            {
                Session.SendWhisper(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.walkppeed.true", Session.Langue));
            }
            else
            {
                Session.SendWhisper(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.walkppeed.false", Session.Langue));
            }
        }
    }
}
