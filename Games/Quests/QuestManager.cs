﻿using WibboEmulator.Communication.Packets.Incoming;
using WibboEmulator.Communication.Packets.Outgoing.Inventory.Purse;
using WibboEmulator.Communication.Packets.Outgoing.Quests;
using WibboEmulator.Database.Daos;
using WibboEmulator.Database.Interfaces;
using WibboEmulator.Games.GameClients;
using System.Data;

namespace WibboEmulator.Games.Quests
{
    public class QuestManager
    {
        private Dictionary<int, Quest> _quests;
        private Dictionary<string, int> _questCount;

        public void Init(IQueryAdapter dbClient)
        {
            this._quests = new Dictionary<int, Quest>();
            this._questCount = new Dictionary<string, int>();

            this.ReloadQuests(dbClient);
        }

        public void ReloadQuests(IQueryAdapter dbClient)
        {
            this._quests.Clear();

            DataTable table = EmulatorQuestDao.GetAll(dbClient);
            foreach (DataRow dataRow in table.Rows)
            {
                int id = Convert.ToInt32(dataRow["id"]);
                string category = (string)dataRow["category"];
                int seriesNumber = Convert.ToInt32(dataRow["series_number"]);
                int goalType = Convert.ToInt32(dataRow["goal_type"]);
                int goalData = Convert.ToInt32(dataRow["goal_data"]);
                string name = (string)dataRow["name"];
                int reward = Convert.ToInt32(dataRow["reward"]);
                string dataBit = (string)dataRow["data_bit"];

                this._quests.Add(id, new Quest(id, category, seriesNumber, (QuestType)goalType, goalData, name, reward, dataBit));

                this.AddToCounter(category);
            }
        }

        private void AddToCounter(string category)
        {
            if (this._questCount.TryGetValue(category, out int num))
            {
                this._questCount[category] = num + 1;
            }
            else
            {
                this._questCount.Add(category, 1);
            }
        }

        public Quest GetQuest(int Id)
        {
            this._quests.TryGetValue(Id, out Quest quest);

            return quest;
        }

        public int GetAmountOfQuestsInCategory(string Category)
        {
            this._questCount.TryGetValue(Category, out int num);

            return num;
        }

        public void ProgressUserQuest(GameClient Session, QuestType QuestType, int EventData = 0)
        {
            if (Session == null || Session.GetUser() == null || Session.GetUser().CurrentQuestId <= 0)
            {
                return;
            }

            Quest quest = this.GetQuest(Session.GetUser().CurrentQuestId);
            if (quest == null || quest.GoalType != QuestType)
            {
                return;
            }

            int questProgress = Session.GetUser().GetQuestProgress(quest.Id);
            bool flag = false;
            int progress;
            if (QuestType != QuestType.EXPLORE_FIND_ITEM)
            {
                progress = questProgress + 1;
                if (progress >= (long)quest.GoalData)
                {
                    flag = true;
                }
            }
            else
            {
                if (EventData != quest.GoalData)
                {
                    return;
                }

                progress = quest.GoalData;
                flag = true;
            }
            using (IQueryAdapter dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                UserQuestDao.Update(dbClient, Session.GetUser().Id, quest.Id, progress);
            }

            Session.GetUser().Quests[Session.GetUser().CurrentQuestId] = progress;
            Session.SendPacket(new QuestStartedComposer(Session, quest));

            if (!flag)
            {
                return;
            }

            Session.GetUser().CurrentQuestId = 0;
            Session.GetUser().LastCompleted = quest.Id;
            Session.SendPacket(new QuestCompletedComposer(Session, quest));
            Session.GetUser().Duckets += quest.Reward;
            Session.SendPacket(new ActivityPointNotificationComposer(Session.GetUser().Duckets, 1));
            this.SendQuestList(Session);
        }

        public Quest GetNextQuestInSeries(string Category, int Number)
        {
            foreach (Quest quest in this._quests.Values)
            {
                if (quest.Category == Category && quest.Number == Number)
                {
                    return quest;
                }
            }

            return null;
        }

        public void SendQuestList(GameClient Session, bool send = true)
        {
            Dictionary<string, int> dictionary1 = new Dictionary<string, int>();
            Dictionary<string, Quest> dictionary2 = new Dictionary<string, Quest>();

            foreach (Quest quest in this._quests.Values)
            {
                if (!dictionary1.ContainsKey(quest.Category))
                {
                    dictionary1.Add(quest.Category, 1);
                    dictionary2.Add(quest.Category, null);
                }
                if (quest.Number >= dictionary1[quest.Category])
                {
                    int questProgress = Session.GetUser().GetQuestProgress(quest.Id);
                    if (Session.GetUser().CurrentQuestId != quest.Id && questProgress >= (long)quest.GoalData)
                    {
                        dictionary1[quest.Category] = quest.Number + 1;
                    }
                }
            }

            foreach (Quest quest in this._quests.Values)
            {
                foreach (KeyValuePair<string, int> keyValuePair in dictionary1)
                {
                    if (quest.Category == keyValuePair.Key && quest.Number == keyValuePair.Value)
                    {
                        dictionary2[keyValuePair.Key] = quest;
                        break;
                    }
                }
            }

            Session.SendPacket(new QuestListComposer(dictionary2, Session, send));
        }

        public void ActivateQuest(GameClient Session, ClientPacket Message)
        {
            Quest quest = this.GetQuest(Message.PopInt());
            if (quest == null)
            {
                return;
            }

            using (IQueryAdapter dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                UserQuestDao.Replace(dbClient, Session.GetUser().Id, quest.Id);
            }

            Session.GetUser().CurrentQuestId = quest.Id;
            this.SendQuestList(Session);
            Session.SendPacket(new QuestStartedComposer(Session, quest));
        }

        public void GetCurrentQuest(GameClient Session)
        {
            if (!Session.GetUser().InRoom)
            {
                return;
            }

            Quest quest = this.GetQuest(Session.GetUser().LastCompleted);
            Quest nextQuestInSeries = this.GetNextQuestInSeries(quest.Category, quest.Number + 1);
            if (nextQuestInSeries == null)
            {
                return;
            }

            using (IQueryAdapter dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                UserQuestDao.Replace(dbClient, Session.GetUser().Id, nextQuestInSeries.Id);
            }

            Session.GetUser().CurrentQuestId = nextQuestInSeries.Id;
            this.SendQuestList(Session);
            Session.SendPacket(new QuestStartedComposer(Session, nextQuestInSeries));
        }

        public void CancelQuest(GameClient Session)
        {
            Quest quest = this.GetQuest(Session.GetUser().CurrentQuestId);
            if (quest == null)
            {
                return;
            }

            Session.GetUser().CurrentQuestId = 0;
            using (IQueryAdapter dbClient = WibboEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                UserQuestDao.Delete(dbClient, Session.GetUser().Id, quest.Id);
            }

            Session.SendPacket(new QuestAbortedComposer());
            this.SendQuestList(Session);
        }
    }
}