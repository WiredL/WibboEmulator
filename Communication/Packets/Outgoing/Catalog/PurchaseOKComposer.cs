namespace WibboEmulator.Communication.Packets.Outgoing.Catalog;
using WibboEmulator.Games.Catalog;
using WibboEmulator.Games.Items;

internal class PurchaseOKComposer : ServerPacket
{
    public PurchaseOKComposer(CatalogItem Item, ItemData BaseItem)
        : base(ServerPacketHeader.CATALOG_PURCHASE_OK)
    {
        this.WriteInteger(BaseItem.Id);
        this.WriteString(BaseItem.ItemName);
        this.WriteBoolean(false);
        this.WriteInteger(Item.CostCredits);
        this.WriteInteger(Item.CostDuckets);
        this.WriteInteger(0);
        this.WriteBoolean(true);
        this.WriteInteger(1);
        this.WriteString(BaseItem.Type.ToString().ToLower());
        this.WriteInteger(BaseItem.SpriteId);
        this.WriteString("");
        this.WriteInteger(1);
        this.WriteInteger(0);
        this.WriteString("");
        this.WriteInteger(1);
    }

    public PurchaseOKComposer()
        : base(ServerPacketHeader.CATALOG_PURCHASE_OK)
    {
        this.WriteInteger(0);
        this.WriteString("");
        this.WriteBoolean(false);
        this.WriteInteger(0);
        this.WriteInteger(0);
        this.WriteInteger(0);
        this.WriteBoolean(true);
        this.WriteInteger(1);
        this.WriteString("s");
        this.WriteInteger(0);
        this.WriteString("");
        this.WriteInteger(1);
        this.WriteInteger(0);
        this.WriteString("");
        this.WriteInteger(1);
    }
}
