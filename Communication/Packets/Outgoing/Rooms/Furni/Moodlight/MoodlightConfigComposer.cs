namespace WibboEmulator.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;
using WibboEmulator.Games.Rooms.Moodlight;

internal class MoodlightConfigComposer : ServerPacket
{
    public MoodlightConfigComposer(MoodlightData moodlightData)
        : base(ServerPacketHeader.ITEM_DIMMER_SETTINGS)
    {
        this.WriteInteger(moodlightData.Presets.Count);
        this.WriteInteger(moodlightData.CurrentPreset);

        var i = 0;
        foreach (var moodlightPreset in moodlightData.Presets)
        {
            i++;
            this.WriteInteger(i);
            this.WriteInteger(int.Parse(WibboEnvironment.BoolToEnum(moodlightPreset.BackgroundOnly)) + 1);
            this.WriteString(moodlightPreset.ColorCode);
            this.WriteInteger(moodlightPreset.ColorIntensity);
        }
    }
}
