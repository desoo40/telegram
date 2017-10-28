using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    public class Chat
    {
        public Telegram.Bot.Types.Chat BotChat { get; set; }

        public long Id => BotChat.Id;

        public bool IsAdmin { get; set; } = false;

        public List<Command> WaitingCommands { get; set; }

        public Tournament Tournament { get; set; }

        public Season Season { get; set; }

        public bool isTextOnly { get; set; } = false;


        public Chat(Telegram.Bot.Types.Chat botChat)
        {
            BotChat = botChat;
            WaitingCommands = new List<Command>();
        }

        #region Старое

        //public bool WhoMode { get; set; }  = false;
        //public bool PersonalStatMode { get; set; }  = false;

        //public bool AddMode { get; set; }  = false;
        //public bool RemoveMode { get; set; }  = false;

        //internal void ResetMode()
        //{
        //    WhoMode = false;
        //    PersonalStatMode = false;
        //    AddMode = false;
        //    RemoveMode = false;
        //}

        #endregion
    }
}
