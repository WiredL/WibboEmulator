namespace WibboEmulator.Communication.Packets.Incoming.Inventory.Pets;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Pets;
using WibboEmulator.Games.GameClients;

internal class GetPetInventoryEvent : IPacketEvent
{
    public double Delay => 5000;

    public void Parse(GameClient session, ClientPacket packet)
    {
        if (session.GetUser() == null)
        {
            return;
        }

        if (session.GetUser().GetInventoryComponent() == null)
        {
            return;
        }

        session.GetUser().GetInventoryComponent().LoadInventory();

        session.SendPacket(new PetInventoryComposer(session.GetUser().GetInventoryComponent().GetPets()));
    }
}