﻿using Butterfly.Communication.Packets.Outgoing.Inventory.Furni;
using Butterfly.Database.Daos;
using Butterfly.Database.Interfaces;
using Butterfly.HabboHotel.Catalog;
using Butterfly.HabboHotel.GameClients;
using Butterfly.HabboHotel.Items;
using System.Data;

namespace Butterfly.HabboHotel.Rooms.Chat.Commands.Cmd
{
    internal class RegenLTD : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, RoomUser UserRoom, string[] Params)
        {
            if (!ButterflyEnvironment.GetGame().GetCatalog().TryGetPage(984897, out CatalogPage Page))
            {
                return;
            }

            using (IQueryAdapter dbClient = ButterflyEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (CatalogItem Item in Page.Items.Values)
                {
                    int LimitedStack = Item.LimitedEditionStack;

                    for (int LimitedNumber = 1; LimitedNumber < LimitedStack + 1; LimitedNumber++)
                    {
                        DataRow Row = ItemDao.GetOneLimitedId(dbClient, LimitedNumber, LimitedStack, Item.ItemId);

                        if (Row != null)
                        {
                            continue;
                        }

                        Item NewItem = ItemFactory.CreateSingleItemNullable(Item.Data, Session.GetHabbo(), "", LimitedNumber, LimitedStack);

                        if (NewItem == null)
                        {
                            continue;
                        }

                        if (Session.GetHabbo().GetInventoryComponent().TryAddItem(NewItem))
                        {
                            Session.SendPacket(new FurniListNotificationComposer(NewItem.Id, 1));
                        }
                    }
                }
            }
        }
    }
}