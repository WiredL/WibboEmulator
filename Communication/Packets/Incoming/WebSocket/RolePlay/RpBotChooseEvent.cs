﻿using Butterfly.Game.Clients;
using Butterfly.Game.Rooms;
using Butterfly.Game.WebClients;

namespace Butterfly.Communication.Packets.Incoming.WebSocket
{
    internal class RpBotChooseEvent : IPacketWebEvent
    {
        public double Delay => 250;

        public void Parse(WebClient Session, ClientPacket Packet)
        {
            string Message = Packet.PopString();

            Client Client = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.UserId);
            if (Client == null || Client.GetHabbo() == null)
            {
                return;
            }

            Room Room = Client.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabboId(Client.GetHabbo().Id);
            if (User == null)
            {
                return;
            }

            Room.AllowsShous(User, Message);
        }
    }
}