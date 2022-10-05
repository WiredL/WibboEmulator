﻿namespace WibboEmulator.Communication.RCON.Commands.User;
using WibboEmulator.Communication.Packets.Outgoing.Navigator;

internal class FollowCommand : IRCONCommand
{
    public bool TryExecute(string[] parameters)
    {
        if (parameters.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parameters[1], out var Userid))
        {
            return false;
        }

        if (Userid == 0)
        {
            return false;
        }

        var Client = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUserID(Userid);
        if (Client == null)
        {
            return false;
        }


        if (!int.TryParse(parameters[2], out var Userid2))
        {
            return false;
        }

        if (Userid2 == 0)
        {
            return false;
        }

        var Client2 = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUserID(Userid2);
        if (Client2 == null)
        {
            return false;
        }

        if (Client2.GetUser() == null || Client2.GetUser().CurrentRoom == null)
        {
            return false;
        }

        var room = Client2.GetUser().CurrentRoom;
        if (room == null)
        {
            return false;
        }

        Client.SendPacket(new GetGuestRoomResultComposer(Client, room.RoomData, false, true));
        return true;
    }
}
