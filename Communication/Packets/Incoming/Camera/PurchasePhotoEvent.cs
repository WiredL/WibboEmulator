namespace WibboEmulator.Communication.Packets.Incoming.Camera;
using WibboEmulator.Communication.Packets.Outgoing.Camera;
using WibboEmulator.Games.GameClients;
using WibboEmulator.Games.Items;

internal class PurchasePhotoEvent : IPacketEvent
{
    public double Delay => 200;

    public void Parse(GameClient session, ClientPacket packet)
    {
        var photoId = packet.PopString();

        if (string.IsNullOrEmpty(photoId))
        {
            photoId = session.GetUser().LastPhotoId;
        }
        else if (!WibboEnvironment.IsValidAlphaNumeric(photoId) || photoId.Length != 32)
        {
            session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("notif.buyphoto.error", session.Langue) + " ( " + photoId + " ) ");
            return;
        }

        if (string.IsNullOrEmpty(photoId))
        {
            session.SendNotification(WibboEnvironment.GetLanguageManager().TryGetValue("notif.buyphoto.error", session.Langue) + " ( " + photoId + " ) ");
            return;
        }

        var photoItemId = WibboEnvironment.GetSettings().GetData<int>("photo.item.id");
        if (!WibboEnvironment.GetGame().GetItemManager().GetItem(photoItemId, out var itemData))
        {
            return;
        }

        var photoSmallItemId = WibboEnvironment.GetSettings().GetData<int>("photo.small.item.id");
        if (!WibboEnvironment.GetGame().GetItemManager().GetItem(photoSmallItemId, out var itemDataSmall))
        {
            return;
        }

        var time = WibboEnvironment.GetUnixTimestamp();
        var extraData = "{\"w\":\"" + "/photos/" + photoId + ".png" + "\", \"n\":\"" + session.GetUser().Username + "\", \"s\":\"" + session.GetUser().Id + "\", \"u\":\"" + "0" + "\", \"t\":\"" + time + "000" + "\"}";

        var itemSmall = ItemFactory.CreateSingleItemNullable(itemDataSmall, session.GetUser(), extraData);
        _ = session.GetUser().GetInventoryComponent().TryAddItem(itemSmall);

        var item = ItemFactory.CreateSingleItemNullable(itemData, session.GetUser(), extraData);
        _ = session.GetUser().GetInventoryComponent().TryAddItem(item);

        session.SendPacket(new CameraPurchaseSuccesfullComposer());
    }
}
