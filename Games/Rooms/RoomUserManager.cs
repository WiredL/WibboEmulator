namespace WibboEmulator.Games.Rooms;
using System.Collections.Concurrent;
using System.Drawing;
using WibboEmulator.Communication.Packets.Outgoing.GameCenter;
using WibboEmulator.Communication.Packets.Outgoing.Handshake;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Avatar;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Engine;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Permissions;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.session;
using WibboEmulator.Core;
using WibboEmulator.Database.Daos.Bot;
using WibboEmulator.Database.Daos.Room;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Quests;
using WibboEmulator.Games.Roleplay.Enemy;
using WibboEmulator.Games.Rooms.AI;
using WibboEmulator.Games.Rooms.Games.Teams;
using WibboEmulator.Games.Rooms.Map.Movement;
using WibboEmulator.Games.Rooms.PathFinding;

public delegate void UserAndItemDelegate(RoomUser user, Item item);

public class RoomUserManager
{
    private readonly Room _room;
    private readonly ConcurrentDictionary<string, RoomUser> _usersByUsername;
    private readonly ConcurrentDictionary<int, RoomUser> _usersByUserID;

    private readonly ConcurrentDictionary<int, RoomUser> _users;
    private readonly ConcurrentDictionary<int, RoomUser> _pets;
    private readonly ConcurrentDictionary<int, RoomUser> _bots;

    private readonly List<int> _usersRank;

    private int _primaryPrivateUserID;
    public int BotPetCount => this._pets.Count + this._bots.Count;

    public event RoomEventDelegate OnUserEnter;

    public RoomUserManager(Room room)
    {
        this._room = room;
        this._users = new ConcurrentDictionary<int, RoomUser>();
        this._pets = new ConcurrentDictionary<int, RoomUser>();
        this._bots = new ConcurrentDictionary<int, RoomUser>();
        this._usersByUsername = new ConcurrentDictionary<string, RoomUser>();
        this._usersByUserID = new ConcurrentDictionary<int, RoomUser>();
        this._usersRank = new List<int>();
        this._primaryPrivateUserID = 1;
    }

    public void UserEnter(RoomUser thisUser) => this.OnUserEnter?.Invoke(thisUser, null);

    public int GetRoomUserCount() => this._room.RoomData.UsersNow;

    public RoomUser DeploySuperBot(RoomBot Bot)
    {
        var key = this._primaryPrivateUserID++;
        var roomUser = new RoomUser(0, this._room.Id, key, this._room);

        Bot.Id = -key;

        _ = this._users.TryAdd(key, roomUser);

        roomUser.SetPos(Bot.X, Bot.Y, Bot.Z);
        roomUser.SetRot(Bot.Rot, false);

        roomUser.BotData = Bot;
        roomUser.BotAI = Bot.GenerateBotAI(roomUser.VirtualId);

        roomUser.BotAI.Init(Bot.Id, roomUser, this._room);

        roomUser.SetStatus("flatctrl", "4");
        this.UpdateUserStatus(roomUser, false);
        roomUser.UpdateNeeded = true;

        this._room.SendPacket(new UsersComposer(roomUser));

        roomUser.BotAI.OnSelfEnterRoom();

        if (this._bots.ContainsKey(roomUser.BotData.Id))
        {
            this._bots[roomUser.BotData.Id] = roomUser;
        }
        else
        {
            _ = this._bots.TryAdd(roomUser.BotData.Id, roomUser);
        }

        return roomUser;
    }

    public bool UpdateClientUsername(RoomUser User, string OldUsername, string NewUsername)
    {
        if (!this._usersByUsername.ContainsKey(OldUsername.ToLower()))
        {
            return false;
        }

        _ = this._usersByUsername.TryRemove(OldUsername.ToLower(), out User);
        _ = this._usersByUsername.TryAdd(NewUsername.ToLower(), User);
        return true;
    }

    public RoomUser DeployBot(RoomBot Bot, Pet PetData)
    {
        var key = this._primaryPrivateUserID++;
        var roomUser = new RoomUser(0, this._room.Id, key, this._room);

        _ = this._users.TryAdd(key, roomUser);

        roomUser.SetPos(Bot.X, Bot.Y, Bot.Z);
        roomUser.SetRot(Bot.Rot, false);

        roomUser.BotData = Bot;

        if (this._room.IsRoleplay)
        {
            RPEnemy Enemy;
            if (Bot.IsPet)
            {
                Enemy = WibboEnvironment.GetGame().GetRoleplayManager().GetEnemyManager().GetEnemyPet(Bot.Id);
            }
            else
            {
                Enemy = WibboEnvironment.GetGame().GetRoleplayManager().GetEnemyManager().GetEnemyBot(Bot.Id);
            }

            if (Enemy != null)
            {
                roomUser.BotData.RoleBot = new RoleBot(Enemy);
                if (Bot.IsPet)
                {
                    roomUser.BotData.AiType = BotAIType.RoleplayPet;
                }
                else
                {
                    roomUser.BotData.AiType = BotAIType.RoleplayBot;
                }
            }
        }

        roomUser.BotAI = Bot.GenerateBotAI(roomUser.VirtualId);

        if (roomUser.IsPet)
        {
            roomUser.BotAI.Init(Bot.Id, roomUser, this._room);
            roomUser.PetData = PetData;
            roomUser.PetData.VirtualId = roomUser.VirtualId;
        }
        else
        {
            roomUser.BotAI.Init(Bot.Id, roomUser, this._room);
        }

        roomUser.SetStatus("flatctrl", "4");

        if (Bot.Status == 1)
        {
            roomUser.SetStatus("sit", "0.5");
            roomUser.IsSit = true;
        }

        if (Bot.Status == 2)
        {
            roomUser.SetStatus("lay", "0.7");
            roomUser.IsLay = true;
        }

        this.UpdateUserStatus(roomUser, false);
        roomUser.UpdateNeeded = true;

        if (Bot.IsDancing)
        {
            roomUser.DanceId = 3;
            this._room.SendPacket(new DanceComposer(roomUser.VirtualId, 3));
        }

        if (Bot.Enable > 0)
        {
            roomUser.ApplyEffect(Bot.Enable);
        }

        if (Bot.Handitem > 0)
        {
            roomUser.CarryItem(Bot.Handitem, true);
        }

        this._room.SendPacket(new UsersComposer(roomUser));

        roomUser.BotAI.OnSelfEnterRoom();
        if (roomUser.IsPet)
        {
            if (this._pets.ContainsKey(roomUser.PetData.PetId))
            {
                this._pets[roomUser.PetData.PetId] = roomUser;
            }
            else
            {
                _ = this._pets.TryAdd(roomUser.PetData.PetId, roomUser);
            }
        }
        else if (this._bots.ContainsKey(roomUser.BotData.Id))
        {
            this._bots[roomUser.BotData.Id] = roomUser;
        }
        else
        {
            _ = this._bots.TryAdd(roomUser.BotData.Id, roomUser);
        }

        return roomUser;
    }

    public void RemoveBot(int VirtualId, bool Kicked)
    {
        var roomUserByVirtualId = this.GetRoomUserByVirtualId(VirtualId);
        if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot)
        {
            return;
        }

        if (roomUserByVirtualId.IsPet)
        {
            _ = this._pets.TryRemove(roomUserByVirtualId.PetData.PetId, out var PetRemoval);
        }
        else
        {
            _ = this._bots.TryRemove(roomUserByVirtualId.BotData.Id, out var BotRemoval);
        }

        roomUserByVirtualId.BotAI.OnSelfLeaveRoom(Kicked);

        this._room.SendPacket(new UserRemoveComposer(roomUserByVirtualId.VirtualId));

        this._room.GetGameMap().RemoveTakingSquare(roomUserByVirtualId.SetX, roomUserByVirtualId.SetY);
        this._room.GetGameMap().RemoveUserFromMap(roomUserByVirtualId, new Point(roomUserByVirtualId.X, roomUserByVirtualId.Y));

        _ = this._users.TryRemove(roomUserByVirtualId.VirtualId, out var toRemove);

    }

    private void UpdateUserEffect(RoomUser User, int x, int y)
    {
        if (User == null)
        {
            return;
        }

        if (User.IsPet)
        {
            return;
        }

        if (!this._room.GetGameMap().ValidTile(x, y))
        {
            return;
        }

        var pByte = this._room.GetGameMap().EffectMap[x, y];
        if (pByte > 0)
        {
            var itemEffectType = ByteToItemEffectType.Parse(pByte);
            if (itemEffectType == User.CurrentItemEffect)
            {
                return;
            }

            switch (itemEffectType)
            {
                case ItemEffectType.NONE:
                    User.ApplyEffect(0);
                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.SWIM:
                    User.ApplyEffect(29);
                    if (User.GetClient() != null)
                    {
                        WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(User.GetClient(), QuestType.EXPLORE_FIND_ITEM, 1948);
                    }

                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.SWIMLOW:
                    User.ApplyEffect(30);
                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.SWIMHALLOWEEN:
                    User.ApplyEffect(37);
                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.ICESKATES:
                    if (User.GetClient() != null)
                    {
                        if (User.GetClient().GetUser().Gender == "M")
                        {
                            User.ApplyEffect(38);
                        }
                        else
                        {
                            User.ApplyEffect(39);
                        }
                    }
                    else
                    {
                        User.ApplyEffect(38);
                    }

                    User.CurrentItemEffect = ItemEffectType.ICESKATES;
                    if (User.GetClient() != null)
                    {
                        WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(User.GetClient(), QuestType.EXPLORE_FIND_ITEM, 1413);
                    }

                    break;
                case ItemEffectType.NORMALSKATES:
                    if (User.GetClient() != null)
                    {
                        if (User.GetClient().GetUser().Gender == "M")
                        {
                            User.ApplyEffect(55);
                        }
                        else
                        {
                            User.ApplyEffect(56);
                        }
                    }
                    else
                    {
                        User.ApplyEffect(55);
                    }

                    User.CurrentItemEffect = itemEffectType;
                    if (User.GetClient() != null)
                    {
                        WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(User.GetClient(), QuestType.EXPLORE_FIND_ITEM, 2199);
                    }

                    break;
                case ItemEffectType.TRAMPOLINE:
                    User.ApplyEffect(193);
                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.TREADMILL:
                    User.ApplyEffect(194);
                    User.CurrentItemEffect = itemEffectType;
                    break;
                case ItemEffectType.CROSSTRAINER:
                    User.ApplyEffect(195);
                    User.CurrentItemEffect = itemEffectType;
                    break;

            }
        }
        else
        {
            if (User.CurrentItemEffect == ItemEffectType.NONE || pByte != 0)
            {
                return;
            }

            User.ApplyEffect(0);
            User.CurrentItemEffect = ItemEffectType.NONE;
        }
    }

    public List<RoomUser> GetUsersForSquare(int x, int y) => this._room.GetGameMap().GetRoomUsers(new Point(x, y)).OrderBy(u => u.IsBot).ToList();

    public RoomUser GetUserForSquare(int x, int y) => Enumerable.FirstOrDefault<RoomUser>(this._room.GetGameMap().GetRoomUsers(new Point(x, y)).OrderBy(u => u.IsBot));

    public RoomUser GetUserForSquareNotBot(int x, int y) => Enumerable.FirstOrDefault<RoomUser>(this._room.GetGameMap().GetRoomUsers(new Point(x, y)).Where(u => u.IsBot == false));

    public bool AddAvatarToRoom(GameClient session)
    {
        if (this._room == null)
        {
            return false;
        }

        if (session == null || session.GetUser() == null)
        {
            return false;
        }

        var PersonalID = this._primaryPrivateUserID++;

        var user = new RoomUser(session.GetUser().Id, this._room.Id, PersonalID, this._room)
        {
            IsSpectator = session.GetUser().SpectatorMode
        };

        if (!this._users.TryAdd(PersonalID, user))
        {
            return false;
        }

        if (session.GetUser().Rank > 5 && !this._usersRank.Contains(user.UserId))
        {
            this._usersRank.Add(user.UserId);
        }

        session.GetUser().CurrentRoomId = this._room.Id;
        session.GetUser().LoadingRoomId = 0;

        var Username = session.GetUser().Username;
        var UserId = session.GetUser().Id;

        if (this._usersByUsername.ContainsKey(Username.ToLower()))
        {
            _ = this._usersByUsername.TryRemove(Username.ToLower(), out var userRemoved);
        }

        if (this._usersByUserID.ContainsKey(UserId))
        {
            _ = this._usersByUserID.TryRemove(UserId, out var userRemoved);
        }

        _ = this._usersByUsername.TryAdd(Username.ToLower(), user);
        _ = this._usersByUserID.TryAdd(UserId, user);

        var roomModel = this._room.GetGameMap().Model;
        if (roomModel == null)
        {
            return false;
        }

        user.SetPos(roomModel.DoorX, roomModel.DoorY, roomModel.DoorZ);
        user.SetRot(roomModel.DoorOrientation, false);

        if (session.GetUser().IsTeleporting)
        {
            var roomItem = this._room.GetRoomItemHandler().GetItem(user.GetClient().GetUser().TeleporterId);
            if (roomItem != null)
            {
                roomItem.GetRoom().GetGameMap().TeleportToItem(user, roomItem);

                roomItem.InteractingUser2 = session.GetUser().Id;
                roomItem.ReqUpdate(1);
            }
        }

        if (user.GetClient() != null && user.GetClient().GetUser() != null)
        {
            user.GetClient().GetUser().IsTeleporting = false;
            user.GetClient().GetUser().TeleporterId = 0;
            user.GetClient().GetUser().TeleportingRoomID = 0;
        }

        if (!user.IsSpectator)
        {
            this._room.SendPacket(new UsersComposer(user));
        }

        if (user.IsSpectator)
        {
            var roomUserByRank = this._room.GetRoomUserManager().GetStaffRoomUser();
            if (roomUserByRank.Count > 0)
            {
                foreach (var StaffUser in roomUserByRank)
                {
                    if (StaffUser != null && StaffUser.GetClient() != null && StaffUser.GetClient().GetUser() != null && StaffUser.GetClient().GetUser().HasPermission("perm_show_invisible"))
                    {
                        StaffUser.SendWhisperChat(user.GetUsername() + " est entré dans l'appart en mode invisible !", true);
                    }
                }
            }
        }

        if (session.GetUser().HasPermission("perm_owner_all_rooms"))
        {
            user.SetStatus("flatctrl", "5");
            session.SendPacket(new YouAreOwnerComposer());
            session.SendPacket(new YouAreControllerComposer(5));
        }
        else if (this._room.CheckRights(session, true))
        {
            user.SetStatus("flatctrl", "4");
            session.SendPacket(new YouAreOwnerComposer());
            session.SendPacket(new YouAreControllerComposer(4));
        }
        else if (this._room.CheckRights(session))
        {
            user.SetStatus("flatctrl", "1");
            session.SendPacket(new YouAreControllerComposer(1));
        }
        else
        {
            user.RemoveStatus("flatctrl");
            session.SendPacket(new YouAreNotControllerComposer());
        }

        if (!user.IsBot)
        {
            if (session.GetUser().GetBadgeComponent().HasBadgeSlot("ADM")) // STAFF
            {
                user.CurrentEffect = 540;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("PRWRD1")) // PROWIRED
            {
                user.CurrentEffect = 580;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("GPHWIB")) // GRAPHISTE
            {
                user.CurrentEffect = 557;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("wibbo.helpeur")) // HELPEUR
            {
                user.CurrentEffect = 544;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("WIBARC")) // ARCHI
            {
                user.CurrentEffect = 546;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("CRPOFFI")) // CROUPIER
            {
                user.CurrentEffect = 570;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("ZEERSWS")) // WIBBOSTATIONORIGINERADIO
            {
                user.CurrentEffect = 552;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("WBASSO")) // ASSOCIER
            {
                user.CurrentEffect = 576;
            }
            else if (session.GetUser().GetBadgeComponent().HasBadgeSlot("WIBBOCOM")) // AGENT DE COMMUNICATION
            {
                user.CurrentEffect = 581;
            }

            if (user.CurrentEffect > 0)
            {
                this._room.SendPacket(new AvatarEffectComposer(user.VirtualId, user.CurrentEffect));
            }
        }

        user.UpdateNeeded = true;

        foreach (var Bot in this._bots.Values.ToList())
        {
            if (Bot == null || Bot.BotAI == null)
            {
                continue;
            }

            Bot.BotAI.OnUserEnterRoom(user);
        }

        if (!user.IsBot && this._room.RoomData.OwnerName != user.GetClient().GetUser().Username)
        {
            WibboEnvironment.GetGame().GetQuestManager().ProgressUserQuest(user.GetClient(), QuestType.SOCIAL_VISIT, 0);
        }

        if (!user.IsBot)
        {
            if (session.GetUser().RolePlayId > 0 && this._room.RoomData.OwnerId != session.GetUser().RolePlayId)
            {
                var RPManager = WibboEnvironment.GetGame().GetRoleplayManager().GetRolePlay(session.GetUser().RolePlayId);
                if (RPManager != null)
                {
                    var Rp = RPManager.GetPlayer(session.GetUser().Id);
                    if (Rp != null)
                    {
                        RPManager.RemovePlayer(session.GetUser().Id);
                    }
                }
                session.GetUser().RolePlayId = 0;
            }

            if (this._room.IsRoleplay && this._room.RoomData.OwnerId != session.GetUser().RolePlayId)
            {
                var RPManager = WibboEnvironment.GetGame().GetRoleplayManager().GetRolePlay(this._room.RoomData.OwnerId);
                if (RPManager != null)
                {
                    var Rp = RPManager.GetPlayer(session.GetUser().Id);
                    if (Rp == null)
                    {
                        RPManager.AddPlayer(session.GetUser().Id);
                    }
                }

                session.GetUser().RolePlayId = this._room.RoomData.OwnerId;
            }
        }


        user.InGame = this._room.IsRoleplay;

        return true;
    }

    public void RemoveUserFromRoom(GameClient session, bool NotifyClient, bool NotifyKick)
    {
        try
        {
            if (session == null)
            {
                return;
            }

            if (session.GetUser() == null)
            {
                return;
            }

            if (NotifyClient)
            {
                if (NotifyKick)
                {
                    session.SendPacket(new GenericErrorComposer(4008));
                }
                session.SendPacket(new CloseConnectionComposer());
            }

            var user = this.GetRoomUserByUserId(session.GetUser().Id);
            if (user == null)
            {
                return;
            }

            if (this._usersRank.Contains(user.UserId))
            {
                _ = this._usersRank.Remove(user.UserId);
            }

            if (user.Team != TeamType.NONE)
            {
                this._room.GetTeamManager().OnUserLeave(user);
                this._room.GetGameManager().UpdateGatesTeamCounts();

                session.SendPacket(new IsPlayingComposer(false));
            }

            if (this._room.GotJanken())
            {
                this._room.GetJanken().RemovePlayer(user);
            }

            if (user.RidingHorse)
            {
                user.RidingHorse = false;
                var roomUserByVirtualId = this.GetRoomUserByVirtualId(user.HorseID);
                if (roomUserByVirtualId != null)
                {
                    roomUserByVirtualId.RidingHorse = false;
                    roomUserByVirtualId.HorseID = 0;
                }
            }

            if (user.IsSit || user.IsLay)
            {
                user.IsSit = false;
                user.IsLay = false;
            }

            if (this._room.HasActiveTrade(session.GetUser().Id))
            {
                this._room.TryStopTrade(session.GetUser().Id);
            }

            if (user.Roleplayer != null)
            {
                WibboEnvironment.GetGame().GetRoleplayManager().GetTrocManager().RemoveTrade(user.Roleplayer.TradeId);
            }

            if (user.IsSpectator)
            {
                var roomUserByRank = this._room.GetRoomUserManager().GetStaffRoomUser();
                if (roomUserByRank.Count > 0)
                {
                    foreach (var StaffUser in roomUserByRank)
                    {
                        if (StaffUser != null && StaffUser.GetClient() != null && StaffUser.GetClient().GetUser() != null && StaffUser.GetClient().GetUser().HasPermission("perm_show_invisible"))
                        {
                            StaffUser.SendWhisperChat(user.GetUsername() + " était en mode invisible. Il vient de partir de l'appartement.", true);
                        }
                    }
                }
            }

            session.GetUser().CurrentRoomId = 0;
            session.GetUser().LoadingRoomId = 0;

            session.GetUser().ForceUse = -1;

            this.RemoveRoomUser(user);

            user.Freeze = true;
            user.FreezeEndCounter = 0;
            user.Dispose();

            _ = this._usersByUserID.TryRemove(user.UserId, out user);
            _ = this._usersByUsername.TryRemove(session.GetUser().Username.ToLower(), out user);
        }
        catch (Exception ex)
        {
            ExceptionLogger.LogCriticalException("Error during removing user (" + session.ConnectionID + ") from room:" + ex.ToString());
        }
    }

    private void RemoveRoomUser(RoomUser user)
    {
        this._room.GetGameMap().RemoveTakingSquare(user.SetX, user.SetY);
        this._room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));

        this._room.SendPacket(new UserRemoveComposer(user.VirtualId));

        _ = this._users.TryRemove(user.VirtualId, out var toRemove);
    }

    public void UpdateUserCount(int count)
    {
        if (this._room.RoomData.UsersNow == count)
        {
            return;
        }

        using (var dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
        {
            RoomDao.UpdateUsersNow(dbClient, this._room.Id, count);
        }

        this._room.RoomData.UsersNow = count;
    }

    public RoomUser GetRoomUserByVirtualId(int VirtualId)
    {
        if (!this._users.TryGetValue(VirtualId, out var User))
        {
            return null;
        }

        return User;
    }

    public RoomUser GetRoomUserByUserId(int pId)
    {
        if (this._usersByUserID.ContainsKey(pId))
        {
            return this._usersByUserID[pId];
        }
        else
        {
            return null;
        }
    }

    public RoomUser GetUserByTracker(string IPWeb, string MachineId)
    {
        foreach (var User in this.GetUserList())
        {
            if (User == null)
            {
                continue;
            }

            if (User.GetClient() == null)
            {
                continue;
            }

            if (User.GetClient().GetUser() == null)
            {
                continue;
            }

            if (User.GetClient().GetConnection() == null)
            {
                continue;
            }

            if (User.GetClient().MachineId != MachineId)
            {
                continue;
            }

            if (User.GetClient().GetUser().IP != IPWeb)
            {
                continue;
            }

            return User;
        }

        return null;
    }

    public List<RoomUser> GetRoomUsers()
    {
        var List = new List<RoomUser>();

        List = this.GetUserList().Where(x => !x.IsBot).ToList();

        return List;
    }

    public ICollection<RoomUser> GetUserList() => this._users.Values;

    public RoomUser GetBotByName(string name) => this._bots.Values.Where(b => b.IsBot && b.BotData.Name == name).FirstOrDefault();

    public RoomUser GetBotOrPetByName(string name) => this._bots.Values.Concat(this._pets.Values).Where(b => (b.IsBot && b.BotData.Name == name) || (b.IsPet && b.BotData.Name == name)).FirstOrDefault();

    public List<RoomUser> GetStaffRoomUser()
    {
        var list = new List<RoomUser>();
        foreach (var UserId in this._usersRank)
        {
            var roomUser = this.GetRoomUserByUserId(UserId);
            if (roomUser != null)
            {
                list.Add(roomUser);
            }
        }
        return list;
    }

    public RoomUser GetRoomUserByName(string pName)
    {
        if (this._usersByUsername.ContainsKey(pName.ToLower()))
        {
            return this._usersByUsername[pName.ToLower()];
        }
        else
        {
            return null;
        }
    }

    public void SaveBots(IQueryAdapter dbClient)
    {
        var botList = this.GetBots();
        if (botList.Count <= 0)
        {
            return;
        }

        BotUserDao.SaveBots(dbClient, botList);
    }

    public void SavePets(IQueryAdapter dbClient)
    {
        var Petlist = this.GetPets();
        if (Petlist.Count <= 0)
        {
            return;
        }

        BotPetDao.SavePet(dbClient, Petlist);
    }

    public List<RoomUser> GetBots()
    {
        var Bots = new List<RoomUser>();
        foreach (var User in this._bots.Values.ToList())
        {
            if (User == null || !User.IsBot || User.IsPet)
            {
                continue;
            }

            Bots.Add(User);
        }

        return Bots;
    }

    public List<RoomUser> GetPets()
    {
        var Pets = new List<RoomUser>();
        foreach (var User in this._pets.Values.ToList())
        {
            if (User == null || !User.IsPet)
            {
                continue;
            }

            Pets.Add(User);
        }

        return Pets;
    }

    public void SerializeStatusUpdates()
    {
        var Users = new List<RoomUser>();
        var RoomUsers = this.GetUserList();

        if (RoomUsers == null)
        {
            return;
        }

        foreach (var User in RoomUsers.ToList())
        {
            if (User == null || !User.UpdateNeeded)
            {
                continue;
            }

            User.UpdateNeeded = false;
            Users.Add(User);
        }

        if (Users.Count > 0)
        {
            this._room.SendPacket(new UserUpdateComposer(Users));
        }
    }

    public void UpdateUserStatusses()
    {
        foreach (var User in this.GetUserList().ToList())
        {
            this.UpdateUserStatus(User, false);
        }
    }

    private bool IsValid(RoomUser user) => user.IsBot || (user.GetClient() != null && user.GetClient().GetUser() != null && user.GetClient().GetUser().CurrentRoomId == this._room.Id);

    public bool TryGetPet(int petId, out RoomUser pet) => this._pets.TryGetValue(petId, out pet);

    public bool TryGetBot(int botId, out RoomUser bot) => this._bots.TryGetValue(botId, out bot);

    public void UpdateUserStatus(RoomUser user, bool cyclegameitems)
    {
        if (user == null)
        {
            return;
        }

        if (user.ContainStatus("lay") || user.ContainStatus("sit") || user.ContainStatus("sign"))
        {
            if (user.ContainStatus("lay"))
            {
                user.RemoveStatus("lay");
            }

            if (user.ContainStatus("sit"))
            {
                user.RemoveStatus("sit");
            }

            if (user.ContainStatus("sign"))
            {
                user.RemoveStatus("sign");
            }

            user.UpdateNeeded = true;
        }

        var roomItemForSquare = this._room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y)).OrderBy(p => p.Z).ToList();

        var newZ = !user.RidingHorse || user.IsPet ? this._room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, roomItemForSquare) : this._room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, roomItemForSquare) + 1.0;
        if (newZ != user.Z)
        {
            user.Z = newZ;
            user.UpdateNeeded = true;
        }

        foreach (var roomItem in roomItemForSquare)
        {
            if (user == null)
            {
                continue;
            }

            if (cyclegameitems)
            {
                if (roomItem.EffectId != 0 && !user.IsBot)
                {
                    user.ApplyEffect(roomItem.EffectId);
                }

                roomItem.UserWalksOnFurni(user, roomItem);
            }

            if (roomItem.GetBaseItem().IsSeat)
            {
                if (!user.ContainStatus("sit"))
                {
                    user.SetStatus("sit", roomItem.Height.ToString());
                    user.IsSit = true;
                }
                user.Z = roomItem.Z;
                user.RotHead = roomItem.Rotation;
                user.RotBody = roomItem.Rotation;
                user.UpdateNeeded = true;
            }

            switch (roomItem.GetBaseItem().InteractionType)
            {
                case InteractionType.BED:
                    if (!user.ContainStatus("lay"))
                    {
                        user.SetStatus("lay", roomItem.Height.ToString() + " null");
                        user.IsLay = true;
                    }
                    user.Z = roomItem.Z;
                    user.RotHead = roomItem.Rotation;
                    user.RotBody = roomItem.Rotation;
                    user.UpdateNeeded = true;
                    break;
                case InteractionType.PRESSUREPAD:
                case InteractionType.TRAMPOLINE:
                case InteractionType.TREADMILL:
                case InteractionType.CROSSTRAINER:
                    roomItem.ExtraData = "1";
                    roomItem.UpdateState(false, true);
                    break;
                case InteractionType.GUILD_GATE:
                    roomItem.ExtraData = "1;" + roomItem.GroupId;
                    roomItem.UpdateState(false, true);
                    break;
                case InteractionType.ARROW:
                    if (!cyclegameitems || user.IsBot)
                    {
                        break;
                    }

                    if (roomItem.InteractingUser != 0)
                    {
                        break;
                    }

                    user.CanWalk = true;
                    roomItem.InteractingUser = user.GetClient().GetUser().Id;
                    roomItem.ReqUpdate(2);
                    break;
                case InteractionType.BANZAIGATEBLUE:
                case InteractionType.BANZAIGATERED:
                case InteractionType.BANZAIGATEYELLOW:
                case InteractionType.BANZAIGATEGREEN:
                    if (cyclegameitems && !user.IsBot)
                    {
                        var EffectId = (int)roomItem.Team + 32;
                        var managerForBanzai = this._room.GetTeamManager();
                        if (user.Team != roomItem.Team)
                        {
                            if (user.Team != TeamType.NONE)
                            {
                                managerForBanzai.OnUserLeave(user);
                            }

                            user.Team = roomItem.Team;
                            managerForBanzai.AddUser(user);

                            this._room.GetGameManager().UpdateGatesTeamCounts();
                            if (user.CurrentEffect != EffectId)
                            {
                                user.ApplyEffect(EffectId);
                            }

                            if (user.GetClient() != null)
                            {
                                user.GetClient().SendPacket(new IsPlayingComposer(true));
                            }
                        }
                        else
                        {
                            managerForBanzai.OnUserLeave(user);
                            this._room.GetGameManager().UpdateGatesTeamCounts();
                            if (user.CurrentEffect == EffectId)
                            {
                                user.ApplyEffect(0);
                            }

                            if (user.GetClient() != null)
                            {
                                user.GetClient().SendPacket(new IsPlayingComposer(false));
                            }

                            user.Team = TeamType.NONE;
                            continue;
                        }
                    }
                    break;
                case InteractionType.BANZAIBLO:
                    if (cyclegameitems && user.Team != TeamType.NONE && !user.IsBot)
                    {
                        this._room.GetGameItemHandler().OnWalkableBanzaiBlo(user, roomItem);
                    }
                    break;
                case InteractionType.BANZAIBLOB:
                    if (cyclegameitems && user.Team != TeamType.NONE && !user.IsBot)
                    {
                        this._room.GetGameItemHandler().OnWalkableBanzaiBlob(user, roomItem);
                    }
                    break;
                case InteractionType.BANZAITELE:
                    if (cyclegameitems)
                    {
                        this._room.GetGameItemHandler().OnTeleportRoomUserEnter(user, roomItem);
                    }

                    break;
                case InteractionType.FREEZEYELLOWGATE:
                case InteractionType.FREEZEREDGATE:
                case InteractionType.FREEZEGREENGATE:
                case InteractionType.FREEZEBLUEGATE:
                    if (cyclegameitems && !user.IsBot)
                    {
                        var EffectId = (int)roomItem.Team + 39;
                        var managerForFreeze = this._room.GetTeamManager();
                        if (user.Team != roomItem.Team)
                        {
                            if (user.Team != TeamType.NONE)
                            {
                                managerForFreeze.OnUserLeave(user);
                            }

                            user.Team = roomItem.Team;
                            managerForFreeze.AddUser(user);
                            this._room.GetGameManager().UpdateGatesTeamCounts();
                            if (user.CurrentEffect != EffectId)
                            {
                                user.ApplyEffect(EffectId);
                            }

                            if (user.GetClient() != null)
                            {
                                user.GetClient().SendPacket(new IsPlayingComposer(true));
                            }
                        }
                        else
                        {
                            managerForFreeze.OnUserLeave(user);
                            this._room.GetGameManager().UpdateGatesTeamCounts();
                            if (user.CurrentEffect == EffectId)
                            {
                                user.ApplyEffect(0);
                            }

                            if (user.GetClient() != null)
                            {
                                user.GetClient().SendPacket(new IsPlayingComposer(false));
                            }

                            user.Team = TeamType.NONE;
                        }
                    }
                    break;
                case InteractionType.FBGATE:
                    if (cyclegameitems || string.IsNullOrEmpty(roomItem.ExtraData) || !roomItem.ExtraData.Contains(',') || user == null || user.IsBot || user.IsTransf || user.IsSpectator)
                    {
                        break;
                    }

                    if (user.GetClient().GetUser().LastMovFGate && user.GetClient().GetUser().BackupGender == user.GetClient().GetUser().Gender)
                    {
                        user.GetClient().GetUser().LastMovFGate = false;
                        user.GetClient().GetUser().Look = user.GetClient().GetUser().BackupLook;
                    }
                    else
                    {
                        // mini Fix
                        var _gateLook = (user.GetClient().GetUser().Gender.ToUpper() == "M") ? roomItem.ExtraData.Split(',')[0] : roomItem.ExtraData.Split(',')[1];
                        if (_gateLook == "")
                        {
                            break;
                        }

                        var gateLook = "";
                        foreach (var part in _gateLook.Split('.'))
                        {
                            if (part.StartsWith("hd"))
                            {
                                continue;
                            }

                            gateLook += part + ".";
                        }
                        gateLook = gateLook[..^1];

                        // Generating New Look.
                        var Parts = user.GetClient().GetUser().Look.Split('.');
                        var NewLook = "";
                        foreach (var Part in Parts)
                        {
                            if (/*Part.StartsWith("hd") || */Part.StartsWith("sh") || Part.StartsWith("cp") || Part.StartsWith("cc") || Part.StartsWith("ch") || Part.StartsWith("lg") || Part.StartsWith("ca") || Part.StartsWith("wa"))
                            {
                                continue;
                            }

                            NewLook += Part + ".";
                        }
                        NewLook += gateLook;

                        user.GetClient().GetUser().BackupLook = user.GetClient().GetUser().Look;
                        user.GetClient().GetUser().BackupGender = user.GetClient().GetUser().Gender;
                        user.GetClient().GetUser().Look = NewLook;
                        user.GetClient().GetUser().LastMovFGate = true;
                    }

                    user.GetClient().SendPacket(new UserChangeComposer(user, true));

                    if (user.GetClient().GetUser().InRoom)
                    {
                        this._room.SendPacket(new UserChangeComposer(user, false));
                    }
                    break;
                case InteractionType.FREEZETILEBLOCK:
                    if (!cyclegameitems)
                    {
                        break;
                    }

                    this._room.GetFreeze().OnWalkFreezeBlock(roomItem, user);
                    break;
                default:
                    break;
            }
        }
        if (cyclegameitems)
        {
            this._room.GetBanzai().HandleBanzaiTiles(user.Coordinate, user.Team, user);
        }

        if (user.IsSit || user.IsLay)
        {
            if (user.IsSit)
            {
                if (!user.ContainStatus("sit"))
                {
                    if (user.IsTransf)
                    {
                        user.SetStatus("sit", "0");
                    }
                    else
                    {
                        user.SetStatus("sit", "0.5");
                    }

                    user.UpdateNeeded = true;
                }
            }
            else if (user.IsLay)
            {
                if (!user.ContainStatus("lay"))
                {
                    if (user.IsTransf)
                    {
                        user.SetStatus("lay", "0");
                    }
                    else
                    {
                        user.SetStatus("lay", "0.7");
                    }

                    user.UpdateNeeded = true;
                }

            }
        }
    }

    public void OnCycle(ref int idleCount)
    {
        var userCounter = 0;

        var ToRemove = new List<RoomUser>();

        foreach (var User in this.GetUserList().OrderBy(a => Guid.NewGuid()).ToList())
        {
            if (!this.IsValid(User))
            {
                if (User.GetClient() != null && User.GetClient().GetUser() != null)
                {
                    this.RemoveUserFromRoom(User.GetClient(), false, false);
                }
                else
                {
                    this.RemoveRoomUser(User);
                }
            }

            if (User.IsDispose)
            {
                continue;
            }

            if (User.RidingHorse && User.IsPet)
            {
                continue;
            }

            if (this._room.IsRoleplay)
            {
                var RPManager = WibboEnvironment.GetGame().GetRoleplayManager().GetRolePlay(this._room.RoomData.OwnerId);
                if (RPManager != null)
                {
                    if (User.IsBot)
                    {
                        if (User.BotData.RoleBot != null)
                        {
                            User.BotData.RoleBot.OnCycle(User, this._room);
                        }
                    }
                    else
                    {
                        var Rp = User.Roleplayer;
                        if (Rp != null)
                        {
                            Rp.OnCycle(User, RPManager);
                        }
                    }
                }
            }

            User.IdleTime++;

            if (!User.IsAsleep && User.IdleTime >= 600 && !User.IsBot)
            {
                User.IsAsleep = true;
                this._room.SendPacket(new SleepComposer(User.VirtualId, true));
            }

            if (User.CarryItemID > 0 && User.CarryTimer > 0)
            {
                User.CarryTimer--;
                if (User.CarryTimer <= 0)
                {
                    User.CarryItem(0);
                }
            }

            if (User.UserTimer > 0)
            {
                User.UserTimer--;
            }

            if (User.FreezeEndCounter > 0)
            {
                User.FreezeEndCounter--;
                if (User.FreezeEndCounter <= 0)
                {
                    User.Freeze = false;
                }
            }

            if (User.TimerResetEffect > 0)
            {
                User.TimerResetEffect--;
                if (User.TimerResetEffect <= 0)
                {
                    User.ApplyEffect(User.CurrentEffect, true);
                }
            }

            if (this._room.GotFreeze())
            {
                this._room.GetFreeze().CycleUser(User);
            }

            if (User.SetStep)
            {
                if (this.SetStepForUser(User))
                {
                    continue;
                }

                if (User.RidingHorse && !User.IsPet)
                {
                    var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                    if (this.SetStepForUser(roomUserByVirtualId))
                    {
                        continue;
                    }
                }
            }
            else
            {
                User.AllowMoveToRoller = true;
                User.AllowBall = true;
                User.MoveWithBall = false;
            }

            if (User.IsWalking && !User.Freezed && !User.Freeze && !(this._room.FreezeRoom && User.GetClient() != null && User.GetClient().GetUser().Rank < 6))
            {
                this.CalculatePath(User);

                User.UpdateNeeded = true;
                if (User.RidingHorse && !User.IsPet)
                {
                    var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                    roomUserByVirtualId.UpdateNeeded = true;
                }
            }
            else if (User.ContainStatus("mv"))
            {
                User.RemoveStatus("mv");
                User.IsWalking = false;
                User.UpdateNeeded = true;

                if (User.RidingHorse && !User.IsPet)
                {
                    var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                    roomUserByVirtualId.RemoveStatus("mv");
                    roomUserByVirtualId.IsWalking = false;
                    roomUserByVirtualId.UpdateNeeded = true;
                }
            }

            if (User.IsBot && User.BotAI != null)
            {
                User.BotAI.OnTimerTick();
            }
            else if (!User.IsSpectator)
            {
                userCounter++;
            }
        }

        if (userCounter == 0)
        {
            idleCount++;
        }

        foreach (var user in ToRemove)
        {
            var clientByUserId = WibboEnvironment.GetGame().GetGameClientManager().GetClientByUserID(user.UserId);
            if (clientByUserId != null)
            {
                this.RemoveUserFromRoom(clientByUserId, true, false);
            }
            else
            {
                this.RemoveRoomUser(user);
            }
        }
        ToRemove.Clear();

        this.UpdateUserCount(userCounter);
    }

    private void CalculatePath(RoomUser User)
    {
        var gameMap = this._room.GetGameMap();

        var nextStep = Pathfinder.GetNextStep(User.X, User.Y, User.GoalX, User.GoalY, gameMap.GameMap, gameMap.ItemHeightMap, gameMap.UserOnMap, gameMap.SquareTaking, gameMap.Model.MapSizeX, gameMap.Model.MapSizeY, User.AllowOverride, gameMap.DiagonalEnabled, this._room.RoomData.AllowWalkthrough, gameMap.ObliqueDisable);
        if (User.WalkSpeed)
        {
            nextStep = Pathfinder.GetNextStep(nextStep.X, nextStep.Y, User.GoalX, User.GoalY, gameMap.GameMap, gameMap.ItemHeightMap, gameMap.UserOnMap, gameMap.SquareTaking, gameMap.Model.MapSizeX, gameMap.Model.MapSizeY, User.AllowOverride, gameMap.DiagonalEnabled, this._room.RoomData.AllowWalkthrough, gameMap.ObliqueDisable);
        }

        if (User.BreakWalkEnable && User.StopWalking)
        {
            User.StopWalking = false;
            this.UpdateUserStatus(User, false);
            User.RemoveStatus("mv");

            if (User.RidingHorse && !User.IsPet)
            {
                var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                roomUserByVirtualId.IsWalking = false;
                this.UpdateUserStatus(roomUserByVirtualId, false);
                roomUserByVirtualId.RemoveStatus("mv");
            }
        }
        else if ((nextStep.X == User.X && nextStep.Y == User.Y) || this._room.GetGameItemHandler().CheckGroupGate(User, new Point(nextStep.X, nextStep.Y)))
        {
            User.IsWalking = false;
            this.UpdateUserStatus(User, false);
            User.RemoveStatus("mv");

            if (User.RidingHorse && !User.IsPet)
            {
                var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                roomUserByVirtualId.IsWalking = false;
                this.UpdateUserStatus(roomUserByVirtualId, false);
                roomUserByVirtualId.RemoveStatus("mv");
            }
        }
        else
        {
            this.HandleSetMovement(nextStep, User);

            if (User.BreakWalkEnable && !User.StopWalking)
            {
                User.StopWalking = true;
            }

            if (User.RidingHorse && !User.IsPet)
            {
                var roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                this.HandleSetMovement(nextStep, roomUserByVirtualId);
                roomUserByVirtualId.UpdateNeeded = true;
            }

            if (User.IsSit || User.IsLay)
            {
                User.IsSit = false;
                User.IsLay = false;
            }

            this._room.GetSoccer().OnUserWalk(User, nextStep.X == User.GoalX && nextStep.Y == User.GoalY);
            this._room.GetBanzai().OnUserWalk(User);
        }
    }

    private void HandleSetMovement(SquarePoint nextStep, RoomUser User)
    {
        var nextX = nextStep.X;
        var nextY = nextStep.Y;

        var nextZ = this._room.GetGameMap().SqAbsoluteHeight(nextX, nextY);
        if (User.RidingHorse && !User.IsPet)
        {
            nextZ += 1;
        }

        User.RemoveStatus("mv");
        User.RemoveStatus("lay");
        User.RemoveStatus("sit");

        User.SetStatus("mv", nextX + "," + nextY + "," + nextZ);

        int newRot;
        if (User.FacewalkEnabled)
        {
            newRot = User.RotBody;
        }
        else
        {
            newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.MoonwalkEnabled);
        }

        User.RotBody = newRot;
        User.RotHead = newRot;

        User.SetStep = true;
        User.SetX = nextX;
        User.SetY = nextY;
        User.SetZ = nextZ;

        this._room.GetGameMap().AddTakingSquare(nextX, nextY);

        this.UpdateUserEffect(User, User.SetX, User.SetY);
    }

    private bool SetStepForUser(RoomUser User)
    {
        this._room.GetGameMap().UpdateUserMovement(User.Coordinate, new Point(User.SetX, User.SetY), User);

        var coordinatedItems = this._room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y)).ToList();

        User.X = User.SetX;
        User.Y = User.SetY;
        User.Z = User.SetZ;

        this._room.CollisionUser(User);

        if (User.IsBot)
        {
            var BotCollisionUser = this._room.GetGameMap().LookHasUserNearNotBot(User.X, User.Y);
            if (BotCollisionUser != null)
            {
                this._room.GetWiredHandler().TriggerBotCollision(BotCollisionUser, User.BotData.Name);
            }
        }

        if (this._room.IsRoleplay)
        {
            var Rp = User.Roleplayer;
            if (Rp != null && !Rp.Dead)
            {
                var ItemTmp = this._room.GetRoomItemHandler().GetFirstTempDrop(User.X, User.Y);
                if (ItemTmp != null && ItemTmp.InteractionType == InteractionTypeTemp.MONEY)
                {
                    Rp.Money += ItemTmp.Value;
                    Rp.SendUpdate();
                    if (User.GetClient() != null)
                    {
                        User.SendWhisperChat(string.Format(WibboEnvironment.GetLanguageManager().TryGetValue("rp.pickdollard", User.GetClient().Langue), ItemTmp.Value));
                    }

                    User.OnChat("*Récupère un objet au sol*");
                    this._room.GetRoomItemHandler().RemoveTempItem(ItemTmp.Id);
                }
                else if (ItemTmp != null && ItemTmp.InteractionType == InteractionTypeTemp.RPITEM)
                {
                    var RpItem = WibboEnvironment.GetGame().GetRoleplayManager().GetItemManager().GetItem(ItemTmp.Value);
                    if (RpItem != null)
                    {
                        if (!RpItem.AllowStack && Rp.GetInventoryItem(RpItem.Id) != null)
                        {
                            if (User.GetClient() != null)
                            {
                                User.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("rp.itemown", User.GetClient().Langue));
                            }
                        }
                        else
                        {
                            Rp.AddInventoryItem(RpItem.Id);

                            if (User.GetClient() != null)
                            {
                                User.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("rp.itempick", User.GetClient().Langue));
                            }
                        }
                    }

                    User.OnChat("*Récupère un objet au sol*");
                    this._room.GetRoomItemHandler().RemoveTempItem(ItemTmp.Id);
                }
            }
        }

        foreach (var roomItem in coordinatedItems)
        {
            roomItem.UserWalksOffFurni(User, roomItem);

            if (roomItem.GetBaseItem().InteractionType == InteractionType.GUILD_GATE)
            {
                roomItem.ExtraData = "0;" + roomItem.GroupId;
                roomItem.UpdateState(false, true);
            }
            else if (roomItem.GetBaseItem().InteractionType is InteractionType.PRESSUREPAD
                or InteractionType.TRAMPOLINE
                or InteractionType.TREADMILL
                or InteractionType.CROSSTRAINER)
            {
                roomItem.ExtraData = "0";
                roomItem.UpdateState(false, true);
            }
            else if (roomItem.GetBaseItem().InteractionType == InteractionType.FOOTBALL)
            {
                if (!User.AllowMoveToRoller || roomItem.InteractionCountHelper > 0 || this._room.OldFoot)
                {
                    continue;
                }

                switch (User.RotBody)
                {
                    case 0:
                        roomItem.MovementDir = MovementDirection.down;
                        break;
                    case 1:
                        roomItem.MovementDir = MovementDirection.downleft;
                        break;
                    case 2:
                        roomItem.MovementDir = MovementDirection.left;
                        break;
                    case 3:
                        roomItem.MovementDir = MovementDirection.upleft;
                        break;
                    case 4:
                        roomItem.MovementDir = MovementDirection.up;
                        break;
                    case 5:
                        roomItem.MovementDir = MovementDirection.upright;
                        break;
                    case 6:
                        roomItem.MovementDir = MovementDirection.right;
                        break;
                    case 7:
                        roomItem.MovementDir = MovementDirection.downright;
                        break;
                }
                roomItem.InteractionCountHelper = 6;
                roomItem.InteractingUser = User.VirtualId;
                roomItem.ReqUpdate(1);
            }
        }

        this.UpdateUserStatus(User, true);
        this._room.GetGameMap().RemoveTakingSquare(User.SetX, User.SetY);

        User.SetStep = false;
        User.AllowMoveToRoller = false;

        if (User.SetMoveWithBall)
        {
            User.SetMoveWithBall = false;
            User.MoveWithBall = false;
        }
        return false;
    }

    public void Destroy()
    {
        this._usersByUsername.Clear();
        this._usersByUserID.Clear();
        this.OnUserEnter = null;
        this._pets.Clear();
        this._bots.Clear();
        this._users.Clear();
        this._usersRank.Clear();
    }
}
