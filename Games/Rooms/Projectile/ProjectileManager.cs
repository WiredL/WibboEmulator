namespace WibboEmulator.Games.Rooms.Projectile;
using System.Collections.Concurrent;
using System.Drawing;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Rooms.Map.Movement;
using WibboEmulator.Utilities;

public class ProjectileManager
{
    private readonly List<ItemTemp> _projectile;
    private readonly ConcurrentQueue<ItemTemp> _queueProjectile;
    private readonly Room _room;

    private readonly ServerPacketList _messages;

    public ProjectileManager(Room room)
    {
        this._projectile = new List<ItemTemp>();
        this._queueProjectile = new ConcurrentQueue<ItemTemp>();
        this._room = room;
        this._messages = new ServerPacketList();
    }

    public void OnCycle()
    {
        if (this._projectile.Count == 0 && this._queueProjectile.IsEmpty)
        {
            return;
        }

        foreach (var item in this._projectile.ToArray())
        {
            if (item == null)
            {
                continue;
            }

            var endProjectile = false;
            var usersTouch = new List<RoomUser>();
            var newPoint = new Point(item.X, item.Y);
            var newX = item.X;
            var newY = item.Y;
            var newZ = item.Z;

            if (item.InteractionType == InteractionTypeTemp.GRENADE)
            {
                newPoint = MovementUtility.GetMoveCoord(item.X, item.Y, 1, item.Movement);
                newX = newPoint.X;
                newY = newPoint.Y;

                if (item.Distance > 2)
                {
                    newZ += 1;
                }
                else
                {
                    newZ -= 1;
                }

                if (item.Distance <= 0)
                {
                    usersTouch = this._room.GetGameMap().GetNearUsers(new Point(newPoint.X, newPoint.Y), 2);

                    endProjectile = true;
                }

                item.Distance--;
            }
            else
            {
                for (var i = 1; i <= 3; i++)
                {
                    newPoint = MovementUtility.GetMoveCoord(item.X, item.Y, i, item.Movement);

                    usersTouch = this._room.GetGameMap().GetRoomUsers(newPoint);

                    foreach (var userTouch in usersTouch)
                    {
                        if (this.CheckUserTouch(userTouch, item))
                        {
                            endProjectile = true;
                        }
                    }

                    if (this._room.GetGameMap().CanStackItem(newPoint.X, newPoint.Y, true) && (this._room.GetGameMap().SqAbsoluteHeight(newPoint.X, newPoint.Y) <= item.Z + 0.5))
                    {
                        newX = newPoint.X;
                        newY = newPoint.Y;
                    }
                    else
                    {
                        endProjectile = true;
                    }

                    if (endProjectile)
                    {
                        break;
                    }

                    item.Distance--;
                    if (item.Distance <= 0)
                    {
                        endProjectile = true;
                        break;
                    }
                }
            }

            this._messages.Add(new SlideObjectBundleComposer(item.X, item.Y, item.Z, newX, newY, newZ, item.Id));

            item.X = newX;
            item.Y = newY;
            item.Z = newZ;

            if (endProjectile)
            {
                foreach (var userTouch in usersTouch)
                {
                    this.CheckUserHit(userTouch, item);
                }

                this.RemoveProjectile(item);
            }
        }

        var bulletUser = new Dictionary<int, int>();

        if (!this._queueProjectile.IsEmpty)
        {
            var toAdd = new List<ItemTemp>();
            while (!this._queueProjectile.IsEmpty)
            {
                if (this._queueProjectile.TryDequeue(out var item))
                {
                    if (!bulletUser.ContainsKey(item.VirtualUserId))
                    {
                        bulletUser.Add(item.VirtualUserId, 1);
                        this._projectile.Add(item);
                    }
                    else
                    {
                        bulletUser[item.VirtualUserId]++;

                        toAdd.Add(item);
                    }
                }

            }
            foreach (var Item in toAdd)
            {
                this._queueProjectile.Enqueue(Item);
            }

            toAdd.Clear();
        }

        bulletUser.Clear();

        this._room.SendMessage(this._messages);
        this._messages.Clear();
    }

    private void CheckUserHit(RoomUser userTouch, ItemTemp item)
    {
        if (userTouch == null)
        {
            return;
        }

        if (this._room.IsRoleplay)
        {
            if (userTouch.VirtualId == item.VirtualUserId)
            {
                return;
            }

            if (userTouch.IsBot)
            {
                if (userTouch.BotData.RoleBot == null)
                {
                    return;
                }

                userTouch.BotData.RoleBot.Hit(userTouch, item.Value, this._room, item.VirtualUserId, item.TeamId);
            }
            else
            {
                var Rp = userTouch.Roleplayer;

                if (Rp == null)
                {
                    return;
                }

                if (!Rp.PvpEnable && Rp.AggroTimer == 0)
                {
                    return;
                }

                Rp.Hit(userTouch, item.Value, this._room, true, item.InteractionType == InteractionTypeTemp.PROJECTILE_BOT);
            }
        }
        else
        {
            this._room.GetWiredHandler().TriggerCollision(userTouch, null);
        }
    }

    private bool CheckUserTouch(RoomUser userTouch, ItemTemp item)
    {
        if (userTouch == null)
        {
            return false;
        }

        if (!this._room.IsRoleplay)
        {
            return true;
        }

        if (userTouch.VirtualId == item.VirtualUserId)
        {
            return false;
        }

        if (userTouch.IsBot)
        {
            if (userTouch.BotData.RoleBot == null)
            {
                return false;
            }

            if (userTouch.BotData.RoleBot.Dead)
            {
                return false;
            }

            return true;
        }
        else
        {
            var Rp = userTouch.Roleplayer;

            if (Rp == null)
            {
                return false;
            }

            if ((!Rp.PvpEnable && Rp.AggroTimer == 0) || Rp.Dead || Rp.SendPrison)
            {
                return false;
            }

            return true;
        }
    }


    private void RemoveProjectile(ItemTemp item)
    {
        if (!this._projectile.Contains(item))
        {
            return;
        }

        this._projectile.Remove(item);

        this._room.GetRoomItemHandler().RemoveTempItem(item.Id);
    }

    public void AddProjectile(int id, int x, int y, double z, MovementDirection movement, int dmg = 0, int distance = 10, int teamId = -1, bool isBot = false)
    {
        var item = this._room.GetRoomItemHandler().AddTempItem(id, 77151726, x, y, z, "1", dmg, isBot ? InteractionTypeTemp.PROJECTILE_BOT : InteractionTypeTemp.PROJECTILE, movement, distance, teamId);
        this._queueProjectile.Enqueue(item);
    }
}
