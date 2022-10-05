namespace WibboEmulator.Games.Rooms.Wired;
using System.Collections.Concurrent;
using System.Drawing;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Items.Wired;
using WibboEmulator.Games.Items.Wired.Interfaces;

public class WiredHandler
{
    private readonly ConcurrentDictionary<Point, List<Item>> _actionStacks;
    private readonly ConcurrentDictionary<Point, List<Item>> _conditionStacks;

    private readonly ConcurrentDictionary<Point, List<RoomUser>> _wiredUsed;

    private readonly List<Point> _specialRandom;
    private readonly Dictionary<Point, int> _specialUnseen;

    private ConcurrentQueue<WiredCycle> _requestingUpdates;

    private readonly Room _room;
    private bool _doCleanup = false;

    public event BotCollisionDelegate TrgBotCollision;
    public event UserAndItemDelegate TrgCollision;
    public event RoomEventDelegate TrgTimer;

    public WiredHandler(Room room)
    {
        this._actionStacks = new ConcurrentDictionary<Point, List<Item>>();
        this._conditionStacks = new ConcurrentDictionary<Point, List<Item>>();
        this._requestingUpdates = new ConcurrentQueue<WiredCycle>();
        this._wiredUsed = new ConcurrentDictionary<Point, List<RoomUser>>();


        this._specialRandom = new List<Point>();
        this._specialUnseen = new Dictionary<Point, int>();

        this._room = room;
    }

    public void AddFurniture(Item item)
    {
        var itemCoord = item.Coordinate;
        if (WiredUtillity.TypeIsWiredAction(item.GetBaseItem().InteractionType))
        {
            if (this._actionStacks.ContainsKey(itemCoord))
            {
                this._actionStacks[itemCoord].Add(item);
            }
            else
            {
                this._actionStacks.TryAdd(itemCoord, new List<Item>() { item });
            }
        }
        else if (WiredUtillity.TypeIsWiredCondition(item.GetBaseItem().InteractionType))
        {
            if (this._conditionStacks.ContainsKey(itemCoord))
            {
                this._conditionStacks[itemCoord].Add(item);
            }
            else
            {
                this._conditionStacks.TryAdd(itemCoord, new List<Item>() { item });
            }
        }
        else if (item.GetBaseItem().InteractionType == InteractionType.SPECIALRANDOM)
        {
            if (!this._specialRandom.Contains(itemCoord))
            {
                this._specialRandom.Add(itemCoord);
            }
        }
        else if (item.GetBaseItem().InteractionType == InteractionType.SPECIALUNSEEN)
        {
            if (!this._specialUnseen.ContainsKey(itemCoord))
            {
                this._specialUnseen.Add(itemCoord, 0);
            }
        }
    }

    public void RemoveFurniture(Item item)
    {
        var itemCoord = item.Coordinate;
        if (WiredUtillity.TypeIsWiredAction(item.GetBaseItem().InteractionType))
        {
            var coordinate = item.Coordinate;
            if (!this._actionStacks.ContainsKey(coordinate))
            {
                return;
            }
            this._actionStacks[coordinate].Remove(item);
            if (this._actionStacks[coordinate].Count == 0)
            {
                this._actionStacks.TryRemove(coordinate, out var newList);
            }
        }
        else if (WiredUtillity.TypeIsWiredCondition(item.GetBaseItem().InteractionType))
        {
            if (!this._conditionStacks.ContainsKey(itemCoord))
            {
                return;
            }
            this._conditionStacks[itemCoord].Remove(item);
            if (this._conditionStacks[itemCoord].Count == 0)
            {
                this._conditionStacks.TryRemove(itemCoord, out var newList);
            }
        }
        else if (item.GetBaseItem().InteractionType == InteractionType.SPECIALRANDOM)
        {
            if (this._specialRandom.Contains(itemCoord))
            {
                this._specialRandom.Remove(itemCoord);
            }
        }
        else if (item.GetBaseItem().InteractionType == InteractionType.SPECIALUNSEEN)
        {
            if (this._specialUnseen.ContainsKey(itemCoord))
            {
                this._specialUnseen.Remove(itemCoord);
            }
        }
    }

    public void OnCycle()
    {
        if (this._doCleanup)
        {
            this.ClearWired();
        }
        else
        {
            if (!this._requestingUpdates.IsEmpty)
            {
                var toAdd = new List<WiredCycle>();
                while (!this._requestingUpdates.IsEmpty)
                {
                    if (!this._requestingUpdates.TryDequeue(out var handler))
                    {
                        continue;
                    }

                    if (handler.WiredCycleable.Disposed())
                    {
                        continue;
                    }

                    if (handler.OnCycle())
                    {
                        toAdd.Add(handler);
                    }
                }

                foreach (var cycle in toAdd)
                {
                    this._requestingUpdates.Enqueue(cycle);
                }
            }

            this._wiredUsed.Clear();
        }
    }

    private void ClearWired()
    {
        foreach (var list in this._actionStacks.Values)
        {
            foreach (var roomItem in list)
            {
                if (roomItem.WiredHandler != null)
                {
                    roomItem.WiredHandler.Dispose();
                    roomItem.WiredHandler = null;
                }
            }
        }

        foreach (var list in this._conditionStacks.Values)
        {
            foreach (var roomItem in list)
            {
                if (roomItem.WiredHandler != null)
                {
                    roomItem.WiredHandler.Dispose();
                    roomItem.WiredHandler = null;
                }
            }
        }

        this._conditionStacks.Clear();
        this._actionStacks.Clear();
        this._wiredUsed.Clear();
        this._doCleanup = false;
    }

    public void OnPickall() => this._doCleanup = true;

    public void ExecutePile(Point coordinate, RoomUser user, Item item)
    {
        if (this._doCleanup)
        {
            return;
        }

        if (!this._actionStacks.ContainsKey(coordinate))
        {
            return;
        }

        if (user != null && user.IsSpectator)
        {
            return;
        }

        if (user != null)
        {
            if (this._wiredUsed.ContainsKey(coordinate))
            {
                if (this._wiredUsed[coordinate].Contains(user))
                {
                    return;
                }
                else
                {
                    this._wiredUsed[coordinate].Add(user);
                }
            }
            else
            {
                this._wiredUsed.TryAdd(coordinate, new List<RoomUser>() { user });
            }
        }

        if (this._conditionStacks.ContainsKey(coordinate))
        {
            var conditionStack = this._conditionStacks[coordinate];
            var cycleCountCdt = 0;
            foreach (var roomItem in conditionStack.ToArray())
            {
                cycleCountCdt++;
                if (cycleCountCdt > 20)
                {
                    break;
                }

                if (roomItem == null || roomItem.WiredHandler == null)
                {
                    continue;
                }

                if (!((IWiredCondition)roomItem.WiredHandler).AllowsExecution(user, item))
                {
                    return;
                }
            }
        }

        var actionStack = this._actionStacks[coordinate].OrderBy(p => p.Z).ToList();

        if (this._specialRandom.Contains(coordinate))
        {
            var countAct = actionStack.Count - 1;

            var rdnWired = WibboEnvironment.GetRandomNumber(0, countAct);
            var actRand = actionStack[rdnWired];
            ((IWiredEffect)actRand.WiredHandler).Handle(user, item);
        }
        else if (this._specialUnseen.ContainsKey(coordinate))
        {
            var countAct = actionStack.Count - 1;

            var nextWired = this._specialUnseen[coordinate];
            if (nextWired > countAct)
            {
                nextWired = 0;
                this._specialUnseen[coordinate] = 0;
            }

            this._specialUnseen[coordinate]++;

            var actNext = actionStack[nextWired];
            if (actNext != null && actNext.WiredHandler != null)
            {
                ((IWiredEffect)actNext.WiredHandler).Handle(user, item);
            }
        }
        else
        {
            var cycleCount = 0;
            foreach (var roomItem in actionStack.ToArray())
            {
                cycleCount++;

                if (cycleCount > 20)
                {
                    break;
                }

                if (roomItem != null && roomItem.WiredHandler != null)
                {
                    ((IWiredEffect)roomItem.WiredHandler).Handle(user, item);
                }
            }
        }
    }

    public void RequestCycle(WiredCycle handler) => this._requestingUpdates.Enqueue(handler);

    public Room GetRoom() => this._room;

    public void Destroy()
    {
        if (this._actionStacks != null)
        {
            this._actionStacks.Clear();
        }

        if (this._conditionStacks != null)
        {
            this._conditionStacks.Clear();
        }

        if (this._requestingUpdates != null)
        {
            this._requestingUpdates = null;
        }

        this.TrgCollision = null;
        this.TrgBotCollision = null;
        this.TrgTimer = null;
        this._wiredUsed.Clear();
    }

    public void TriggerCollision(RoomUser roomUser, Item item) => this.TrgCollision?.Invoke(roomUser, item);

    public void TriggerBotCollision(RoomUser roomUser, string botName) => this.TrgBotCollision?.Invoke(roomUser, botName);

    public void TriggerTimer() => this.TrgTimer?.Invoke(null, null);
}
