using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Mars_Life
{
    public struct Quest
    {
        public string Name; //{get; private set;}
        public string Description; //{ get; private set; }
        public string Type; //{ get; private set; }
        public int ChatIndex;
        public bool Complete;// { get; set; }

        public Quest(string questName, string questDescription, int chatIndex, string questType)
        {
            Name = questName;
            Description = questDescription;
            ChatIndex = chatIndex;
            Type = questType;
            Complete = false;
        }
    }

    public class QuestNPC:TalkingNPC
    {
        private List<Quest> quests;

        public int QuestIndex { get; private set; }

        public void AddQuest(Quest q)
        {
            quests.Add(q);
        }
        public void AddQuests(List<Quest> q)
        {
            quests.AddRange(q);
        }

        public Quest GetCurrentQuest()
        {
            return quests[QuestIndex];
        }
        public Quest GetNextQuest()
        {
            if (quests.Count - 1 == QuestIndex){ return new Quest();  }
            else { return quests[QuestIndex + 1]; }
        }
        public Quest GetPreviousQuest()
        {
            if (QuestIndex < 0) { return new Quest(); }
            else { return quests[QuestIndex - 1]; }
        }
        public Quest GetQuestAtIndex(int index)
        {
            if (index <= (quests.Count - 1)) { return quests[index]; }
            else { return new Quest(); }
        }

        public void CompleteQuest()
        {
            if (QuestIndex <= quests.Count-1)
            {
                Quest temp = quests[QuestIndex];
                temp.Complete = true;

                quests[QuestIndex] = temp;
                ++QuestIndex;
            }
        }

        public QuestNPC(NPCStats stats, Texture2D texture, Vector2 locationOnMap, List<Quest> questList, List<CommunicationData> commData)
            : base(stats, texture, locationOnMap, new List<MovementData> { new MovementData("Face Down", 1) }, commData)
        {
            quests = questList;
            QuestIndex = 0;
        }

        public QuestNPC(NPCStats stats, Texture2D texture, Vector2 locationOnMap, List<Quest> questList, List<MovementData> movements, List<CommunicationData> commData)
            : base(stats, texture, locationOnMap, movements, commData)
        {
            quests = questList;
            QuestIndex = 0;
        }
    }
}
