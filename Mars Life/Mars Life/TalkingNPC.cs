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
    public struct CommunicationData
    {
        public string type;
        public string dialogue;
        public string option1;
        public string option2;
        public int movementInt;

        public CommunicationData (string message)
        {
            type = "Message";
            dialogue = message;
            option1 = "";
            option2 = "";
            movementInt = 0;
        }
        public CommunicationData(string message, string op1, string op2)
        {
            type = "Question";
            dialogue = message;
            option1 = op1;
            option2 = op2;
            movementInt = 0;
        }

        public CommunicationData(string message, int _movementInt)
        {
            type = "Message";
            dialogue = message;
            option1 = "";
            option2 = "";
            movementInt = _movementInt;
        }
        public CommunicationData(string message, string op1, string op2, int _movementInt)
        {
            type = "Question";
            dialogue = message;
            option1 = op1;
            option2 = op2;
            movementInt = _movementInt;
        }
    }

    public struct NPCStats
    {
        public string Name;
        
        public NPCStats(string name)
        {
            Name = name;
        }
    }

    public class TalkingNPC:NPC
    {
        private List<CommunicationData> dialogue;
        public NPCStats stats;

        public int ChatIndex {get; private set;}
        public int ChatSectionResetIndex { get; protected set; }

        public bool IncrimentChatIndex() 
        {
            if (ChatIndex >= dialogue.Count - 1) { ChatIndex = ChatSectionResetIndex; return false; }
            else { ++ChatIndex; return true; }
        }
        public CommunicationData GetCurrentCommunication()
        {
            return dialogue[ChatIndex];
        }
        public CommunicationData GetNextCommunication()
        {
            ++ChatIndex;
            if (ChatIndex >= dialogue.Count - 1) { ChatIndex = ChatSectionResetIndex; return GetCurrentCommunication(); }
            else { return dialogue[ChatIndex]; }
        }
        public CommunicationData GetPreviousCommunication()
        {
            --ChatIndex;
            if (ChatIndex < 0) { ChatIndex = 0; return new CommunicationData(); }
            else { return dialogue[ChatIndex]; }
        }
        public CommunicationData GetCommunicationAtIndex(int index)
        {
            if (index <= (dialogue.Count - 1)) { return dialogue[index]; }
            else { return new CommunicationData(); }
        }

        public TalkingNPC(NPCStats npcStats, Texture2D texture, Vector2 locationOnMap, List<MovementData> movement, List<CommunicationData> commData)
            :base(texture, locationOnMap, movement)
        {
            stats = npcStats;
            dialogue = commData;
            currentMovementIndex = 0;
            ChatIndex = 0;
            ChatSectionResetIndex = 0;
        }
        public TalkingNPC(NPCStats npcStats, Texture2D texture, Vector2 locationOnMap, List<CommunicationData> commData)
            : base(texture, locationOnMap, new List<MovementData>{ new MovementData("Face Down", 1) })
        {
            stats = npcStats;
            dialogue = commData;
            currentMovementIndex = 0;
            ChatIndex = 0;
            ChatSectionResetIndex = 0;
        }

    }
}
