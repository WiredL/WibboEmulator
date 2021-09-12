﻿using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Roleplay.Player;
using System;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd
{
    internal class Cac : IChatCommand
    {
        public string PermissionRequired
        {
            get { return ""; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return ""; }
        }
        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                return;
            }

            if (!Room.IsRoleplay || !Room.Pvp)
            {
                return;
            }

            RolePlayer Rp = UserRoom.Roleplayer;
            if (Rp == null)
            {
                return;
            }

            if (Rp.Dead || !Rp.PvpEnable || Rp.SendPrison || UserRoom.Freeze)
            {
                return;
            }

            int WeaponEanble = Rp.WeaponCac.Enable;

            UserRoom.ApplyEffect(WeaponEanble, true);            UserRoom.TimerResetEffect = Rp.WeaponCac.FreezeTime + 1;

            if (UserRoom.FreezeEndCounter <= Rp.WeaponCac.FreezeTime)
            {
                UserRoom.Freeze = true;
                UserRoom.FreezeEndCounter = Rp.WeaponCac.FreezeTime;
            }            RoomUser TargetRoomUser = Room.GetRoomUserManager().GetRoomUserByName(Params[1].ToString());            if (TargetRoomUser == null)
            {
                RoomUser BotOrPet = Room.GetRoomUserManager().GetBotOrPetByName(Params[1].ToString());
                if (BotOrPet == null || BotOrPet.BotData == null || BotOrPet.BotData.RoleBot == null)
                {
                    return;
                }

                if (BotOrPet.BotData.RoleBot.Dead)
                {
                    return;
                }

                if (Math.Abs(BotOrPet.X - UserRoom.X) >= 2 || Math.Abs(BotOrPet.Y - UserRoom.Y) >= 2)
                {
                    return;
                }

                int Dmg = ButterflyEnvironment.GetRandomNumber(Rp.WeaponCac.DmgMin, Rp.WeaponCac.DmgMax);
                BotOrPet.BotData.RoleBot.Hit(BotOrPet, Dmg, Room, UserRoom.VirtualId, -1);

            }
            else
            {
                RolePlayer RpTwo = TargetRoomUser.Roleplayer;
                if (RpTwo == null || (!RpTwo.PvpEnable && RpTwo.AggroTimer <= 0))
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

                if (Math.Abs(TargetRoomUser.X - UserRoom.X) >= 2 || Math.Abs(TargetRoomUser.Y - UserRoom.Y) >= 2)
                {
                    return;
                }

                int Dmg = ButterflyEnvironment.GetRandomNumber(Rp.WeaponCac.DmgMin, Rp.WeaponCac.DmgMax);

                Rp.AggroTimer = 30;
                RpTwo.Hit(TargetRoomUser, Dmg, Room);
            }

        }
    }
}
