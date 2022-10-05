namespace WibboEmulator.Games.Items.Wired.Triggers;
using System.Data;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Wired;

public class TimerTrigger : WiredTriggerBase, IWired, IWiredCycleable
{
    public int DelayCycle => (this.IntParams.Count > 0) ? this.IntParams[0] : 0;
    private readonly RoomEventDelegate _delegateFunction;

    public TimerTrigger(Item item, Room room) : base(item, room, (int)WiredTriggerType.TRIGGER_ONCE)
    {
        this._delegateFunction = new RoomEventDelegate(this.ResetTimer);
        this.RoomInstance.GetWiredHandler().TrgTimer += this._delegateFunction;

        this.IntParams.Add(0);
    }

    public void ResetTimer(object sender, EventArgs e) => this.RoomInstance.GetWiredHandler().RequestCycle(new WiredCycle(this, null, null));

    public bool OnCycle(RoomUser user, Item item)
    {
        this.RoomInstance.GetWiredHandler().ExecutePile(this.ItemInstance.Coordinate, null, null);
        return false;
    }

    public override void Dispose()
    {
        this.RoomInstance.GetWiredHandler().TrgTimer -= this._delegateFunction;

        base.Dispose();
    }

    public void SaveToDatabase(IQueryAdapter dbClient) => WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, this.DelayCycle.ToString(), false, null);

    public void LoadFromDatabase(DataRow row)
    {
        this.IntParams.Clear();

        if (int.TryParse(row["trigger_data"].ToString(), out var delay))
        {
            this.IntParams.Add(delay);
        }
    }
}
