﻿namespace WibboEmulator.Communication.Packets.Outgoing;
using WibboEmulator.Games.Moderation;

internal class CfhTopicsInitComposer : ServerPacket
{
    public CfhTopicsInitComposer(Dictionary<string, List<ModerationPresetActions>> UserActionPresets)
        : base(ServerPacketHeader.CFH_TOPICS)
    {

        this.WriteInteger(UserActionPresets.Count);
        foreach (var Cat in UserActionPresets.ToList())
        {
            this.WriteString(Cat.Key);
            this.WriteInteger(Cat.Value.Count);
            foreach (var Preset in Cat.Value.ToList())
            {
                this.WriteString(Preset.Caption);
                this.WriteInteger(Preset.Id);
                this.WriteString(Preset.Type);
            }
        }
    }
}
