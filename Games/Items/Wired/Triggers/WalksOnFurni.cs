namespace WibboEmulator.Games.Items.Wired.Triggers;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Events;

public class WalksOnFurni(Item item, Room room) : WiredTriggerBase(item, room, (int)WiredTriggerType.AVATAR_WALKS_ON_FURNI), IWired
{
    private void OnUserWalksOnFurni(object obj, ItemTriggeredEventArgs args) => this.Room.WiredHandler.ExecutePile(this.Item.Coordinate, args.User, args.Item);

    public override void LoadItems(bool inDatabase = false)
    {
        base.LoadItems();

        if (this.Items != null)
        {
            foreach (var roomItem in this.Items.ToList())
            {
                roomItem.OnUserWalksOnFurni += this.OnUserWalksOnFurni;
            }
        }
    }

    public override void Dispose()
    {
        if (this.Items != null)
        {
            foreach (var roomItem in this.Items.ToList())
            {
                roomItem.OnUserWalksOnFurni -= this.OnUserWalksOnFurni;
            }
        }

        base.Dispose();
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveInDatabase(dbClient, this.Item.Id, string.Empty, string.Empty, false, this.Items);

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay) => this.LoadStuffIds(wiredTriggersItem);
}
