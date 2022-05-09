﻿using Butterfly.Database.Interfaces;
using Butterfly.Game.Rooms;
using Butterfly.Game.Items.Wired.Interfaces;
using System.Data;

namespace Butterfly.Game.Items.Wired.Actions
{
    public class ShowMessage : WiredActionBase, IWired, IWiredEffect
    {
        public ShowMessage(Item item, Room room) : base(item, room, (int)WiredActionType.CHAT)
        {
        }

        public override bool OnCycle(RoomUser user, Item item)
        {
            if (this.StringParam == "")
            {
                return false;
            }

            if (user != null && !user.IsBot && user.GetClient() != null && user.GetClient().GetUser() != null)
            {
                string textMessage = this.StringParam;
                textMessage = textMessage.Replace("#username#", user.GetUsername());
                textMessage = textMessage.Replace("#point#", user.WiredPoints.ToString());
                textMessage = textMessage.Replace("#roomname#", this.RoomInstance.GetWiredHandler().GetRoom().RoomData.Name.ToString());
                textMessage = textMessage.Replace("#vote_yes#", this.RoomInstance.GetWiredHandler().GetRoom().VotedYesCount.ToString());
                textMessage = textMessage.Replace("#vote_no#", this.RoomInstance.GetWiredHandler().GetRoom().VotedNoCount.ToString());
                textMessage = textMessage.Replace("#wpcount#", user.GetClient().GetUser().WibboPoints.ToString());

                if (user.Roleplayer != null)
                {
                    textMessage = textMessage.Replace("#money#", user.Roleplayer.Money.ToString());
                }

                user.SendWhisperChat(textMessage);
            }

            return false;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, this.Id, string.Empty, this.StringParam, false, null, this.Delay);
        }

        public void LoadFromDatabase(DataRow row)
        {
            int delay;
            if (int.TryParse(row["delay"].ToString(), out delay))
	            this.Delay = delay;

            if (int.TryParse(row["trigger_data_2"].ToString(), out delay))
                this.Delay = delay;

            this.StringParam = row["trigger_data"].ToString();
        }
    }
}
