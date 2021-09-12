﻿using Butterfly.Database.Interfaces;
using Butterfly.HabboHotel.GameClients;
using System.Linq;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd
{
    internal class RoomSell : IChatCommand
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
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.1", Session.Langue));
                return;
            }

            if (!int.TryParse(Params[1], out int Prix))
            {
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.2", Session.Langue));
                return;
            }
            if (Prix < 1)
            {
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.3", Session.Langue));
                return;
            }
            if (Prix > 99999999)
            {
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.4", Session.Langue));
                return;
            }

            if (Room.RoomData.Group != null)
            {
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.5", Session.Langue));
                return;
            }

            if (Room.RoomData.SellPrice > 0)
            {
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.error.6", Session.Langue));
                return;
            }

            using (IQueryAdapter queryreactor = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor.SetQuery("UPDATE rooms SET price= @price WHERE id = @roomid LIMIT 1");
                queryreactor.AddParameter("roomid", Room.Id);
                queryreactor.AddParameter("price", Prix);
                queryreactor.RunQuery();
            }

            Room.RoomData.SellPrice = Prix;

            UserRoom.SendWhisperChat(string.Format(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.valide", Session.Langue), Prix));

            foreach (RoomUser user in Room.GetRoomUserManager().GetUserList().ToList())            {                if (user == null || user.IsBot)
                {
                    continue;
                }

                user.SendWhisperChat(string.Format(ButterflyEnvironment.GetLanguageManager().TryGetValue("roomsell.warn", Session.Langue), Prix));            }
        }
    }
}
