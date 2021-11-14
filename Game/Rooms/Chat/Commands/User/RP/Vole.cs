﻿using Butterfly.Game.GameClients;
using Butterfly.Game.Roleplay.Player;
using System;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class Vole : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (Params.Length != 3)
            {
                return;
            }

            if (!Room.IsRoleplay)
            {
                return;
            }

            RolePlayer Rp = UserRoom.Roleplayer;
            if (Rp == null)
            {
                return;
            }

            if (Rp.Dead || Rp.SendPrison)
            {
                return;
            }

            RoomUser TargetRoomUser = Room.GetRoomUserManager().GetRoomUserByName(Params[1].ToString());
            {
                return;
            }

            RolePlayer RpTwo = TargetRoomUser.Roleplayer;
            if (RpTwo == null)
            {
                return;
            }

            if (TargetRoomUser.GetClient().GetHabbo().Id == Session.GetHabbo().Id)
            {
                return;
            }

            if (RpTwo.Dead || RpTwo.SendPrison)
            {
                return;
            }

            int NumberMoney = (int)Math.Floor((double)((double)Rp.Money / 100) * 15);
            {
                return;
            }

            if (!((Math.Abs((TargetRoomUser.X - UserRoom.X)) >= 2) || (Math.Abs((TargetRoomUser.Y - UserRoom.Y)) >= 2)))
        }
    }
}