using Butterfly.Communication.Packets.Outgoing.Navigator;

using Butterfly.Game.GameClients;
            {
                return;
            }

            GameClient clientByUsername = ButterflyEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                UserRoom.SendWhisperChat(ButterflyEnvironment.GetLanguageManager().TryGetValue("cmd.follow.notallowed", Session.Langue));