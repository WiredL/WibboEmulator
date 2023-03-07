namespace WibboEmulator.Games.Items.Wired.Conditions;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class LessThanTimer : WiredConditionBase, IWiredCondition, IWired
{
    public LessThanTimer(Item item, Room room) : base(item, room, (int)WiredConditionType.TIME_ELAPSED_LESS) => this.IntParams.Add(0);

    public bool AllowsExecution(RoomUser user, Item item)
    {
        var timeout = (this.IntParams.Count > 0) ? this.IntParams[0] : 0;

        var dateTime = this.RoomInstance.LastTimerReset;
        return (DateTime.Now - dateTime).TotalSeconds < timeout / 2;
    }

    public void SaveToDatabase(IQueryAdapter dbClient)
    {
        var timeout = (this.IntParams.Count > 0) ? this.IntParams[0] : 0;

        WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, timeout.ToString(), false, null);
    }

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay)
    {
        this.IntParams.Clear();

        if (int.TryParse(wiredTriggerData, out var timeout))
        {
            this.IntParams.Add(timeout);
        }
    }
}
