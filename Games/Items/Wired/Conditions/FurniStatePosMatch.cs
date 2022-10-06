namespace WibboEmulator.Games.Items.Wired.Conditions;
using System.Data;
using WibboEmulator.Database.Daos.Item;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.Items.Wired.Bases;
using WibboEmulator.Games.Items.Wired.Interfaces;
using WibboEmulator.Games.Rooms;

public class FurniStatePosMatch : WiredConditionBase, IWiredCondition, IWired
{
    private readonly Dictionary<int, ItemsPosReset> _itemsData;

    public FurniStatePosMatch(Item item, Room room) : base(item, room, (int)WiredConditionType.STATES_MATCH)
    {
        this._itemsData = new Dictionary<int, ItemsPosReset>();

        this.IntParams.Add(0);
        this.IntParams.Add(0);
        this.IntParams.Add(0);
    }
    public bool AllowsExecution(RoomUser user, Item item)
    {
        var state = ((this.IntParams.Count > 0) ? this.IntParams[0] : 0) == 1;
        var direction = ((this.IntParams.Count > 1) ? this.IntParams[1] : 0) == 1;
        var position = ((this.IntParams.Count > 2) ? this.IntParams[2] : 0) == 1;

        foreach (var roomItem in this.Items.ToList())
        {
            if (!this._itemsData.TryGetValue(roomItem.Id, out var itemPosReset))
            {
                continue;
            }

            if (state)
            {
                if (itemPosReset.ExtraData != "Null")
                {
                    if (!(roomItem.ExtraData == "" && itemPosReset.ExtraData == "0") && !(roomItem.ExtraData == "0" && itemPosReset.ExtraData == ""))
                    {

                        if (roomItem.ExtraData != itemPosReset.ExtraData)
                        {
                            return false;
                        }
                    }
                }
            }

            if (direction)
            {
                if (itemPosReset.Rot != roomItem.Rotation)
                {
                    return false;
                }
            }

            if (position)
            {
                if (itemPosReset.X != roomItem.X || itemPosReset.Y != roomItem.Y)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override void LoadItems(bool inDatabase = false)
    {
        base.LoadItems();

        if (inDatabase)
        {
            return;
        }

        this._itemsData.Clear();

        foreach (var roomItem in this.Items.ToList())
        {
            if (!this._itemsData.ContainsKey(roomItem.Id))
            {
                this._itemsData.Add(roomItem.Id, new ItemsPosReset(roomItem.Id, roomItem.X, roomItem.Y, roomItem.Z, roomItem.Rotation, roomItem.ExtraData));
            }
            else
            {
                _ = this._itemsData.Remove(roomItem.Id);
                this._itemsData.Add(roomItem.Id, new ItemsPosReset(roomItem.Id, roomItem.X, roomItem.Y, roomItem.Z, roomItem.Rotation, roomItem.ExtraData));
            }
        }
    }

    public void SaveToDatabase(IQueryAdapter dbClient)
    {
        var triggerItems = "";

        foreach (var roomItem in this._itemsData.Values)
        {
            triggerItems += roomItem.Id + ":" + roomItem.X + ":" + roomItem.Y + ":" + roomItem.Z + ":" + roomItem.Rot + ":" + roomItem.ExtraData + ";";
        }

        triggerItems = triggerItems.TrimEnd(';');

        var state = (this.IntParams.Count > 0) ? this.IntParams[0] : 0;
        var direction = (this.IntParams.Count > 1) ? this.IntParams[1] : 0;
        var position = (this.IntParams.Count > 2) ? this.IntParams[2] : 0;

        var triggerData2 = state + ";" + direction + ";" + position;

        ItemWiredDao.Delete(dbClient, this.ItemInstance.Id);
        ItemWiredDao.Insert(dbClient, this.ItemInstance.Id, "", triggerData2, false, triggerItems, this.Delay);
    }

    public void LoadFromDatabase(DataRow row)
    {
        this.IntParams.Clear();

        if (int.TryParse(row["trigger_data"].ToString(), out var delay))
        {
            this.Delay = delay;
        }

        var triggerData2 = row["trigger_data_2"].ToString();

        if (triggerData2 != null && triggerData2.Length == 5)
        {
            var dataSplit = triggerData2.Split(';');

            if (int.TryParse(dataSplit[0], out var state))
            {
                this.IntParams.Add(state);
            }

            if (int.TryParse(dataSplit[1], out var direction))
            {
                this.IntParams.Add(direction);
            }

            if (int.TryParse(dataSplit[2], out var position))
            {
                this.IntParams.Add(position);
            }
        }

        var triggerItems = row["triggers_item"].ToString();

        if (triggerItems is null or "")
        {
            return;
        }

        foreach (var item in triggerItems.Split(';'))
        {
            var itemData = item.Split(':');
            if (itemData.Length != 6)
            {
                continue;
            }

            if (!int.TryParse(itemData[0], out var id))
            {
                continue;
            }

            if (!this.StuffIds.Contains(id))
            {
                this.StuffIds.Add(id);
            }

            this._itemsData.Add(Convert.ToInt32(itemData[0]), new ItemsPosReset(Convert.ToInt32(itemData[0]), Convert.ToInt32(itemData[1]), Convert.ToInt32(itemData[2]), Convert.ToDouble(itemData[3]), Convert.ToInt32(itemData[4]), itemData[5]));
        }
    }
}
