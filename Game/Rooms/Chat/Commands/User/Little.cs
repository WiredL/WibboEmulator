using Butterfly.Communication.Packets.Outgoing.Rooms.Engine;
using Butterfly.Game.GameClients;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
            if (Params.Length != 2)
            {
                return;
            }

            if (UserRoom.Team != Team.none || UserRoom.InGame)
            {
                return;
            }

            if (Session.GetHabbo().SpectatorMode || UserRoom.InGame)
            {
                return;
            }

            if (!UserRoom.SetPetTransformation("little" + Params[1], 0))
            {
                Session.SendHugeNotif(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.littleorbig.help", Session.Langue));
                return;
            }

            UserRoom.transformation = true;

            Room.SendPacket(new UserRemoveComposer(UserRoom.VirtualId));
            Room.SendPacket(new UsersComposer(UserRoom));