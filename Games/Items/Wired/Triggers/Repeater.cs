namespace WibboEmulator.Games.Items.Wired.Triggers;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Wired;

public class Repeater : WiredTriggerBase, IWired, IWiredCycleable
{
    public int DelayCycle => this.GetIntParam(0);
    public bool IsTeleport => false;

    public Repeater(Item item, Room room) : base(item, room, (int)WiredTriggerType.TRIGGER_PERIODICALLY)
    {
        this.DefaultIntParams(0);

        this.Room.WiredHandler.RequestCycle(new WiredCycle(this, null, null));
    }

    public bool OnCycle(RoomUser user, Item item)
    {
        this.Room.WiredHandler.ExecutePile(this.Item.Coordinate, null, null);
        return true;
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveInDatabase(dbClient, this.Id, string.Empty, this.DelayCycle.ToString());

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay)
    {
        if (int.TryParse(wiredTriggerData, out var delay))
        {
            this.SetIntParam(0, delay);
        }
    }
}
