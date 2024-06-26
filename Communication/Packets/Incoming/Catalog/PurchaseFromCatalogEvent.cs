namespace WibboEmulator.Communication.Packets.Incoming.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Catalog;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Achievements;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Bots;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Furni;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Pets;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Purse;
using WibboEmulator.Communication.Packets.Outgoing.Rooms.Notifications;
using WibboEmulator.Database.Daos.Catalog;
using WibboEmulator.Database.Daos.Item;
using WibboEmulator.Database.Daos.Log;
using WibboEmulator.Database.Daos.User;
using System.Data;
using WibboEmulator.Games.Catalogs.Utilities;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;
using WibboEmulator.Games.Users.Premium;
using WibboEmulator.Database;
using WibboEmulator.Core.Language;
using WibboEmulator.Games.Catalogs;

internal sealed class PurchaseFromCatalogEvent : IPacketEvent
{
    public double Delay => 500;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var pageId = packet.PopInt();
        var itemId = packet.PopInt();
        var extraData = packet.PopString(512);
        var amount = packet.PopInt();

        if (!CatalogManager.TryGetPage(pageId, out var page))
        {
            return;
        }

        if (!page.Enabled || !page.HavePermission(session.User))
        {
            return;
        }

        if (!page.Items.TryGetValue(itemId, out var item))
        {
            if (page.ItemOffers.TryGetValue(itemId, out var value))
            {
                item = value;
                if (item == null)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        if (page.IsPremium && session.User.Rank < 2)
        {
            session.SendNotification("Vous devez être membre du premium club pour pouvoir acheter ce mobilier");
            session.SendPacket(new PurchaseErrorComposer());
            return;
        }

        if (amount < 1 || amount > 100 || !ItemUtility.CanSelectAmount(item))
        {
            amount = 1;
        }

        if (item.IsLimited)
        {
            var leftTimeLTD = DateTime.Now - session.User.LastLTDPurchaseTime;
            if (leftTimeLTD.TotalSeconds <= 10)
            {
                session.SendNotification($"Vous devez attendre encore {10 - (int)leftTimeLTD.TotalSeconds} secondes avant de pouvoir racheter un LTD");
                session.SendPacket(new PurchaseErrorComposer());
                return;
            }

            session.User.LastLTDPurchaseTime = DateTime.Now;
        }

        var amountPurchase = item.Amount > 1 ? item.Amount : amount;

        var totalCreditsCost = amount > 1 ? item.CostCredits * amount : item.CostCredits;
        var totalPixelCost = amount > 1 ? item.CostDuckets * amount : item.CostDuckets;
        var totalWibboPointCost = amount > 1 ? item.CostWibboPoints * amount : item.CostWibboPoints;
        var totalLimitCoinCost = amount > 1 ? item.CostLimitCoins * amount : item.CostLimitCoins;

        if (session.User.Credits < totalCreditsCost ||
            session.User.Duckets < totalPixelCost ||
            session.User.WibboPoints < totalWibboPointCost ||
            session.User.LimitCoins < totalLimitCoinCost)
        {
            session.SendPacket(new PurchaseErrorComposer());
            return;
        }

        if (!ItemUtility.TryProcessExtraData(item, session, ref extraData))
        {
            session.SendPacket(new PurchaseErrorComposer());
            return;
        }

        if (session.User.InventoryComponent.IsOverlowLimit(amountPurchase, item.Data.Type))
        {
            session.SendNotification(LanguageManager.TryGetValue("catalog.purchase.limit", session.Language));
            session.SendPacket(new PurchaseErrorComposer());
            return;
        }

        using var dbClient = DatabaseManager.Connection;

        var limitedEditionSells = 0;
        var limitedEditionStack = 0;

        if (item.IsLimited)
        {
            if (item.LimitedEditionStack <= item.LimitedEditionSells)
            {
                session.SendNotification(LanguageManager.TryGetValue("notif.buyltd.error", session.Language));
                session.SendPacket(new PurchaseErrorComposer());
                return;
            }

            item.LimitedEditionSells++;

            CatalogItemLimitedDao.Update(dbClient, item.Id, item.LimitedEditionSells);

            limitedEditionSells = item.LimitedEditionSells;
            limitedEditionStack = item.LimitedEditionStack;
        }

        if (item.CostCredits > 0)
        {
            session.User.Credits -= totalCreditsCost;
            session.SendPacket(new CreditBalanceComposer(session.User.Credits));
        }

        if (item.CostDuckets > 0)
        {
            session.User.Duckets -= totalPixelCost;
            session.SendPacket(new ActivityPointNotificationComposer(session.User.Duckets, session.User.Duckets));
        }

        if (item.CostWibboPoints > 0)
        {
            session.User.WibboPoints -= totalWibboPointCost;
            session.SendPacket(new ActivityPointNotificationComposer(session.User.WibboPoints, 0, 105));

            UserDao.UpdateRemovePoints(dbClient, session.User.Id, totalWibboPointCost);
        }

        if (item.CostLimitCoins > 0)
        {
            session.User.LimitCoins -= totalLimitCoinCost;
            session.SendPacket(new ActivityPointNotificationComposer(session.User.LimitCoins, 0, 55));

            LimitCoinsPrime(dbClient, session, totalLimitCoinCost);

            UserDao.UpdateRemoveLimitCoins(dbClient, session.User.Id, totalLimitCoinCost);
            LogShopDao.Insert(dbClient, session.User.Id, totalLimitCoinCost, $"Achat de {item.Name} (x{item.Amount})", item.Id);
        }

        switch (item.Data.Type)
        {
            default:
                var generatedGenericItems = new List<Item>();

                Item newItem;
                switch (item.Data.InteractionType)
                {
                    default:
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(dbClient, item.Data, session.User, extraData, amountPurchase);

                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                            }
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(dbClient, item.Data, session.User, extraData, limitedEditionSells, limitedEditionStack);

                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                            }
                        }
                        break;

                    case InteractionType.TELEPORT:
                    case InteractionType.TELEPORT_ARROW:
                        for (var i = 0; i < amountPurchase; i++)
                        {
                            var teleItems = ItemFactory.CreateTeleporterItems(dbClient, item.Data, session.User);

                            if (teleItems != null)
                            {
                                generatedGenericItems.AddRange(teleItems);
                            }
                        }
                        break;

                    case InteractionType.MOODLIGHT:
                    {
                        if (amountPurchase > 1)
                        {
                            var items = ItemFactory.CreateMultipleItems(dbClient, item.Data, session.User, extraData, amountPurchase);

                            if (items != null)
                            {
                                generatedGenericItems.AddRange(items);
                                foreach (var itemMoodlight in items)
                                {
                                    ItemMoodlightDao.Insert(dbClient, itemMoodlight.Id);
                                }
                            }
                        }
                        else
                        {
                            newItem = ItemFactory.CreateSingleItemNullable(dbClient, item.Data, session.User, extraData);

                            if (newItem != null)
                            {
                                generatedGenericItems.Add(newItem);
                                ItemMoodlightDao.Insert(dbClient, newItem.Id);
                            }
                        }
                    }
                    break;
                }

                foreach (var purchasedItem in generatedGenericItems)
                {
                    session.User.InventoryComponent.TryAddItem(purchasedItem);
                }

                if (item.Data.Amount >= 0)
                {
                    item.Data.Amount += generatedGenericItems.Count;
                    ItemStatDao.UpdateAdd(dbClient, item.Data.Id, generatedGenericItems.Count);
                }
                break;

            case ItemType.R:
                var bot = BotUtility.CreateBot(dbClient, item.Data, session.User.Id);
                if (bot != null)
                {
                    if (!session.User.InventoryComponent.TryAddBot(bot))
                    {
                        break;
                    }

                    session.SendPacket(new UnseenItemsComposer(bot.Id, UnseenItemsType.Bot));
                    session.SendPacket(new BotInventoryComposer(session.User.InventoryComponent.Bots));
                }
                break;

            case ItemType.P:
            {
                var bits = extraData.Split('\n');

                var petName = bits[0];
                var race = bits[1];
                var color = bits[2];

                var generatedPet = PetUtility.CreatePet(session.User.Id, petName, item.Data.SpriteId, race, color);
                if (generatedPet != null)
                {
                    if (!session.User.InventoryComponent.TryAddPet(generatedPet))
                    {
                        break;
                    }

                    session.SendPacket(new UnseenItemsComposer(generatedPet.PetId, UnseenItemsType.Pet));
                    session.SendPacket(new PetInventoryComposer(session.User.InventoryComponent.Pets));
                }
                break;
            }

            case ItemType.B:
            {
                break;
            }

            case ItemType.C:
            {
                if (item.Name == "premium_club_3") //Legend
                {
                    session.User.Premium.AddPremiumDays(dbClient, 31, PremiumClubLevel.LEGEND);
                }
                else if (item.Name == "premium_club_2") //Epic
                {
                    session.User.Premium.AddPremiumDays(dbClient, 31, PremiumClubLevel.EPIC);
                }
                else if (item.Name == "premium_club_1") //Classic
                {
                    session.User.Premium.AddPremiumDays(dbClient, 31, PremiumClubLevel.CLASSIC);
                }
                else
                {
                    break;
                }

                session.User.Premium.SendPackets();
                break;
            }
        }

        if (!string.IsNullOrEmpty(item.Badge) && !session.User.BadgeComponent.HasBadge(item.Badge))
        {
            session.User.BadgeComponent.GiveBadge(item.Badge);
        }

        session.SendPacket(new PurchaseOKComposer(item, item.Data));
    }

    private static void LimitCoinsPrime(IDbConnection dbClient, GameClient session, int totalLimitCoinCost)
    {
        var notifImage = "";
        var wibboPointCount = 0;
        var winwinCount = totalLimitCoinCost * 10;

        if (session.User.HasPermission("premium_legend"))
        {
            notifImage = "premium_legend";
            wibboPointCount = totalLimitCoinCost * 3;
            winwinCount += (int)Math.Floor(winwinCount * 1.5);
        }
        else if (session.User.HasPermission("premium_epic"))
        {
            notifImage = "premium_epic";
            wibboPointCount = totalLimitCoinCost * 2;
            winwinCount += winwinCount;
        }
        else if (session.User.HasPermission("premium_classic"))
        {
            notifImage = "premium_classic";
            wibboPointCount = totalLimitCoinCost;
            winwinCount += (int)Math.Floor(winwinCount * 0.5);
        }

        if (wibboPointCount > 0)
        {
            session.User.WibboPoints += wibboPointCount;
            session.SendPacket(new ActivityPointNotificationComposer(session.User.WibboPoints, 0, 105));

            UserDao.UpdateAddPoints(dbClient, session.User.Id, wibboPointCount);
        }

        if (winwinCount > 0)
        {
            UserStatsDao.UpdateAchievementScore(dbClient, session.User.Id, winwinCount);

            session.User.AchievementPoints += winwinCount;
            session.SendPacket(new AchievementScoreComposer(session.User.AchievementPoints));
        }

        if (winwinCount > 0 && wibboPointCount > 0)
        {
            session.SendPacket(RoomNotificationComposer.SendBubble(notifImage, $"Vous avez reçu {wibboPointCount} WibboPoints ainsi que {winwinCount} Win-wins!"));
        }
        else
        {
            session.SendPacket(RoomNotificationComposer.SendBubble(notifImage, $"Vous avez reçu {winwinCount} Win-wins!"));
        }
    }
}
