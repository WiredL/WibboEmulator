using Butterfly.Game.GameClients;
            {
                return;
            }

            GameClient clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            {
                return;
            }

            Room room = ButterflyEnvironment.GetGame().GetRoomManager().GetRoom(clientByUsername.GetHabbo().CurrentRoomId);
            {
                return;
            }

            RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabboId(clientByUsername.GetHabbo().Id);
            {
                return;
            }

            RoomUser roomUserByHabbo2 = room.GetRoomUserManager().GetRoomUserByHabboId(Session.GetHabbo().Id);
            {
                return;
            }

            room.SendPacket(room.GetRoomItemHandler().TeleportUser(roomUserByHabbo, roomUserByHabbo2.Coordinate, 0, room.GetGameMap().SqAbsoluteHeight(roomUserByHabbo2.X, roomUserByHabbo2.Y)));
            //room.GetRoomUserManager().UpdateUserStatus(roomUserByHabbo, false);

        }