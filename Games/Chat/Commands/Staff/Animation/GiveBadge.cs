namespace WibboEmulator.Games.Chat.Commands.Cmd;
using WibboEmulator.Communication.Packets.Outgoing.Users;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal class GiveBadge : IChatCommand
{
    public void Execute(GameClient session, Room Room, RoomUser UserRoom, string[] Params)
    {
        var clientByUsername = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUsername(Params[1]);
        if (clientByUsername != null)
        {
            /*if (session.Langue != clientByUsername.Langue)
            {
                session.SendWhisper(ButterflyEnvironment.GetLanguageManager().TryGetValue(string.Format("cmd.authorized.langue.user", clientByUsername.Langue), session.Langue));
                return;
            }*/

            var BadgeCode = Params[2];
            clientByUsername.GetUser().GetBadgeComponent().GiveBadge(BadgeCode, true);
            clientByUsername.SendPacket(new ReceiveBadgeComposer(BadgeCode));
        }
        else
        {
            session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("input.usernotfound", session.Langue));
        }
    }
}
