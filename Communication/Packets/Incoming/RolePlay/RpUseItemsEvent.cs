namespace WibboEmulator.Communication.Packets.Incoming.RolePlay;
using WibboEmulator.Communication.Packets.Outgoing.Notifications;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Rooms;

internal class RpUseItemsEvent : IPacketEvent
{
    public double Delay => 250;

    public void Parse(GameClient session, ClientPacket Packet)
    {
        var ItemId = Packet.PopInt();
        var UseCount = Packet.PopInt();

        if (session == null || session.GetUser() == null)
        {
            return;
        }

        var Room = session.GetUser().CurrentRoom;
        if (Room == null || !Room.IsRoleplay)
        {
            return;
        }

        var User = Room.GetRoomUserManager().GetRoomUserByUserId(session.GetUser().Id);
        if (User == null)
        {
            return;
        }

        if (User.Freeze)
        {
            return;
        }

        var Rp = User.Roleplayer;
        if (Rp == null || Rp.Dead || Rp.SendPrison || Rp.TradeId > 0)
        {
            return;
        }

        if (Rp.AggroTimer > 0)
        {
            User.SendWhisperChat(string.Format(WibboEnvironment.GetLanguageManager().TryGetValue("rp.useitem.notallowed", session.Langue), Math.Round((double)Rp.AggroTimer / 2)));
            return;
        }

        var RpItem = WibboEnvironment.GetGame().GetRoleplayManager().GetItemManager().GetItem(ItemId);
        if (RpItem == null)
        {
            return;
        }

        var RpItemInventory = Rp.GetInventoryItem(ItemId);
        if (RpItemInventory == null || RpItemInventory.Count <= 0 || RpItem.Type == "none")
        {
            return;
        }

        if (UseCount <= 0 || RpItem.UseType != 2)
        {
            UseCount = 1;
        }

        if (UseCount > RpItemInventory.Count)
        {
            UseCount = RpItemInventory.Count;
        }

        if (User.FreezeEndCounter <= 1)
        {
            User.Freeze = true;
            User.FreezeEndCounter = 1;
        }

        if (RpItem.Id == 75)
        {
            Rp.AddInventoryItem(45, UseCount);
        }

        switch (RpItem.Type)
        {
            case "openpage":
            {
                User.GetClient().SendPacket(new InClientLinkComposer("habbopages/roleplay/" + RpItem.Value));
                break;
            }
            case "openguide":
            {
                User.GetClient().SendPacket(new InClientLinkComposer("habbopages/westworld/westworld"));
                break;
            }
            case "hit":
            {
                Rp.Hit(User, RpItem.Value * UseCount, Room, false, true, false);
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);
                break;
            }
            case "enable":
            {
                User.ApplyEffect(RpItem.Value);
                break;
            }
            case "showtime":
            {
                User.SendWhisperChat("Il est " + Room.Roleplay.Hour + " heures et " + Room.Roleplay.Minute + " minutes");
                break;
            }
            case "money":
            {
                Rp.Money += RpItem.Value * UseCount;
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);
                Rp.SendUpdate();
                break;
            }
            case "munition":
            {
                Rp.AddMunition(RpItem.Value * UseCount);
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);
                Rp.SendUpdate();
                break;
            }
            case "energytired":
            {
                User.ApplyEffect(4, true);
                User.TimerResetEffect = 2;

                Rp.AddEnergy(RpItem.Value * UseCount);
                Rp.Hit(User, RpItem.Value * UseCount, Room, false, true, false);
                Rp.SendUpdate();
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);

                User.OnChat("*Consomme " + char.ToLowerInvariant(RpItem.Title[0]) + RpItem.Title[1..] + "*");
                break;
            }
            case "healthtired":
            {
                User.ApplyEffect(4, true);
                User.TimerResetEffect = 2;

                Rp.RemoveEnergy(RpItem.Value * UseCount);
                Rp.AddHealth(RpItem.Value * UseCount);
                Rp.SendUpdate();
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);

                User.OnChat("*Consomme " + char.ToLowerInvariant(RpItem.Title[0]) + RpItem.Title[1..] + "*");
                break;
            }
            case "healthenergy":
            {
                User.ApplyEffect(4, true);
                User.TimerResetEffect = 2;

                Rp.AddEnergy(RpItem.Value * UseCount);
                Rp.AddHealth(RpItem.Value * UseCount);
                Rp.SendUpdate();
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);

                User.OnChat("*Consomme " + char.ToLowerInvariant(RpItem.Title[0]) + RpItem.Title[1..] + "*");
                break;
            }
            case "energy":
            {
                User.ApplyEffect(4, true);
                User.TimerResetEffect = 2;

                Rp.AddEnergy(RpItem.Value * UseCount);
                Rp.SendUpdate();
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);

                User.OnChat("*Consomme " + char.ToLowerInvariant(RpItem.Title[0]) + RpItem.Title[1..] + "*");
                break;
            }
            case "health":
            {
                User.ApplyEffect(737, true);
                User.TimerResetEffect = 4;

                Rp.AddHealth(RpItem.Value * UseCount);
                Rp.SendUpdate();
                Rp.RemoveInventoryItem(RpItem.Id, UseCount);

                User.OnChat("*Consomme " + char.ToLowerInvariant(RpItem.Title[0]) + RpItem.Title[1..] + "*");
                break;
            }
            case "weapon_cac":
            {
                if (Rp.WeaponCac.Id == RpItem.Value)
                {
                    break;
                }

                Rp.WeaponCac = WibboEnvironment.GetGame().GetRoleplayManager().GetWeaponManager().GetWeaponCac(RpItem.Value);
                User.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("rp.changearmecac", session.Langue));
                break;
            }
            case "weapon_far":
            {
                if (Rp.WeaponGun.Id == RpItem.Value)
                {
                    break;
                }

                Rp.WeaponGun = WibboEnvironment.GetGame().GetRoleplayManager().GetWeaponManager().GetWeaponGun(RpItem.Value);
                User.SendWhisperChat(WibboEnvironment.GetLanguageManager().TryGetValue("rp.changearmefar", session.Langue));
                break;
            }
        }
    }
}
