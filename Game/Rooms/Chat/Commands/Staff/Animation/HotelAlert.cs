using Butterfly.Communication.Packets.Outgoing;
using Butterfly.Game.GameClients;

namespace Butterfly.Game.Rooms.Chat.Commands.Cmd
{
    internal class HotelAlert : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            string Message = CommandManager.MergeParams(Params, 1);
            if (Session.Antipub(Message, "<CMD>", Room.Id))
            {
                return;
            }

            /*ServerPacket alert = new ServerPacket(ServerPacketHeader.GENERIC_ALERT);
            alert.WriteString(ButterflyEnvironment.GetLanguageManager().TryGetValue("hotelallert.notice", Session.Langue) + "\r\n" + Message + "\r\n- " + Session.GetHabbo().Username);
            ButterflyEnvironment.GetGame().GetClientManager().SendMessage(alert);*/

            string Message = CommandManager.MergeParams(Params, 1);
            ButterflyEnvironment.GetGame().GetClientManager().SendPacket(new BroadcastMessageAlertComposer(Message + "\r\n" + "- " + Session.GetHabbo().Username));
                            

        }
    }
}
