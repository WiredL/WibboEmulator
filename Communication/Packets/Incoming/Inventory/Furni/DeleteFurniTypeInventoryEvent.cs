namespace WibboEmulator.Communication.Packets.Incoming.Inventory.Furni;

using WibboEmulator.Database;
using WibboEmulator.Games.GameClients;

internal sealed class DeleteFurniTypeInventoryEvent : IPacketEvent
{
    public double Delay => 1000;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.User == null)
        {
            return;
        }

        if (session.User.InventoryComponent == null)
        {
            return;
        }

        var itemId = packet.PopInt();

        var item = session.User.InventoryComponent.GetItem(itemId);

        if (item == null)
        {
            return;
        }

        if (item.ItemData.IsRare && !session.User.HasPermission("empty_items_all"))
        {
            return;
        }

        var items = session.User.InventoryComponent.GetItemsByType(item.BaseItemId);

        using var dbClient = DatabaseManager.Connection;
        session.User.InventoryComponent.DeleteItems(dbClient, items, item.BaseItemId);
    }
}
