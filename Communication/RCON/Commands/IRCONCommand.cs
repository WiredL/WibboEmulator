﻿namespace WibboEmulator.Communication.RCON.Commands;

public interface IRCONCommand
{
    bool TryExecute(string[] parameters);
}
