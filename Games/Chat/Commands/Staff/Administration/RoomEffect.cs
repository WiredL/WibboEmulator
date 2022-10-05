﻿namespace WibboEmulator.Games.Chat.Commands.Cmd;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal class RoomEffect : IChatCommand
{
    public void Execute(GameClient session, Room Room, RoomUser UserRoom, string[] Params)
    {
        if (Params.Length != 2)
        {
            return;
        }

        int.TryParse(Params[1], out var number);

        if (number > 3)
        {
            number = 3;
        }
        else if (number < 0)
        {
            number = 0;
        }

        Room.SendPacket(new RoomEffectComposer(number));
    }
}