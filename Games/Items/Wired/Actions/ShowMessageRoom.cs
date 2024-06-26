namespace WibboEmulator.Games.Items.Wired.Actions;
using System.Data;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class ShowMessageRoom(Item item, Room room) : WiredActionBase(item, room, (int)WiredActionType.CHAT), IWired, IWiredEffect
{
    public override bool OnCycle(RoomUser user, Item item)
    {
        if (this.StringParam == "")
        {
            return false;
        }

        var textMessage = this.StringParam;
        WiredUtillity.ParseMessage(user, this.Room, ref textMessage);

        foreach (var userTarget in this.Room.RoomUserManager.UserList.ToList())
        {
            userTarget?.SendWhisperChat(textMessage);
        }

        return false;
    }

    public void SaveToDatabase(IDbConnection dbClient) => WiredUtillity.SaveInDatabase(dbClient, this.Id, string.Empty, this.StringParam, false, null, this.Delay);

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay)
    {
        this.Delay = wiredDelay;

        this.StringParam = wiredTriggerData;
    }
}
