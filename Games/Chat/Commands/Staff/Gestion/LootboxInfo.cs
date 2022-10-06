﻿namespace WibboEmulator.Games.Chat.Commands.Staff.Gestion;
using System.Text;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal class LootboxInfo : IChatCommand
{
    public void Execute(GameClient session, Room Room, RoomUser UserRoom, string[] parameters)
    {
        var basicCount = WibboEnvironment.GetGame().GetLootManager().GetRarityCounter(1);
        var communCount = WibboEnvironment.GetGame().GetLootManager().GetRarityCounter(2);
        var epicCount = WibboEnvironment.GetGame().GetLootManager().GetRarityCounter(3);
        var legendaryCount = WibboEnvironment.GetGame().GetLootManager().GetRarityCounter(4);

        var stringBuilder = new StringBuilder();

        _ = stringBuilder.Append("- Information sur le nombre de rare distribuer ce mois-ci -\r");
        _ = stringBuilder.Append("Basique: " + basicCount + "\r");
        _ = stringBuilder.Append("Commun: " + communCount + "\r");
        _ = stringBuilder.Append("Epique: " + epicCount + "\r");
        _ = stringBuilder.Append("Legendaire: " + legendaryCount + "\r");

        session.SendNotification(stringBuilder.ToString());
    }
}
