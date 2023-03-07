namespace WibboEmulator.Games.Items.Wired.Actions;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;
using WibboEmulator.Games.Rooms.Games.Teams;

public class GiveScore : WiredActionBase, IWiredEffect, IWired
{
    private int _currentGameCount;

    public GiveScore(Item item, Room room) : base(item, room, (int)WiredActionType.GIVE_SCORE)
    {
        this._currentGameCount = 0;
        this.RoomInstance.GameManager.OnGameStart += this.OnGameStart;

        this.IntParams.Add(1);
        this.IntParams.Add(1);
    }

    private void OnGameStart(object sender, EventArgs e) => this._currentGameCount = 0;

    public override bool OnCycle(RoomUser user, Item item)
    {
        var scoreToGive = (this.IntParams.Count > 0) ? this.IntParams[0] : 0;
        var maxCountPerGame = (this.IntParams.Count > 1) ? this.IntParams[1] : 0;

        if (user == null || user.Team == TeamType.None || maxCountPerGame <= this._currentGameCount)
        {
            return false;
        }

        this._currentGameCount++;
        this.RoomInstance.GameManager.AddPointToTeam(user.Team, scoreToGive, user);

        return false;
    }

    public override void Dispose()
    {
        base.Dispose();

        this.RoomInstance.
        GameManager.OnGameStart -= this.OnGameStart;
    }

    public void SaveToDatabase(IQueryAdapter dbClient)
    {
        var scoreToGive = (this.IntParams.Count > 0) ? this.IntParams[0] : 0;
        var maxCountPerGame = (this.IntParams.Count > 1) ? this.IntParams[1] : 0;

        WiredUtillity.SaveTriggerItem(dbClient, this.ItemInstance.Id, scoreToGive.ToString(), maxCountPerGame.ToString(), false, null, this.Delay);
    }

    public void LoadFromDatabase(string wiredTriggerData, string wiredTriggerData2, string wiredTriggersItem, bool wiredAllUserTriggerable, int wiredDelay)
    {
        this.IntParams.Clear();

        this.Delay = wiredDelay;

        if (int.TryParse(wiredTriggerData, out var maxCount))
        {
            this.IntParams.Add(maxCount);
        }

        if (int.TryParse(wiredTriggerData2, out var score))
        {
            this.IntParams.Add(score);
        }
    }
}
