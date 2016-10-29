using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HockeyBot
{
    public class Chat
    {
        public long Id { get; set; }

        public bool WhoMode { get; set; } = false;
        public bool PersonalStatMode { get; set; } = false;
        public bool AddMode { get; set; } = false;
        public bool RemoveMode { get; set; } = false;

        public Queue<string> CommandsQueue { get; set; } = new Queue<string>();
        public List<WaitingStatistic> WaitingStatistics { get; set; }
        public List<WaitingEvent> WaitingEvents { get; set; }

        public Chat(long id)
        {
            Id = id;
            WaitingStatistics = new List<WaitingStatistic>();
            WaitingEvents = new List<WaitingEvent>();
        }

        internal void ResetMode()
        {
            WhoMode = false;
            PersonalStatMode = false;
            AddMode = false;
            RemoveMode = false;
            CommandsQueue.Clear();
        }
    }

    public class WaitingStatistic
    {
        public Message Msg { get; set; }
        public Player Plr { get; set; }
    }

    public class WaitingEvent
    {
        public Message Msg { get; set; }
        public Event Even { get; set; }
    }
}
