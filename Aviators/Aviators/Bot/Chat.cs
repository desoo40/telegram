using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    public class Chat
    {
        public long Id { get; set; }

        public bool WhoMode { get; set; }  = false;
        public bool PersonalStatMode { get; set; }  = false;
        public struct Statistic
        {
            public bool Bomb;
            public bool Snip;
            public bool Asist;
            public bool BadBoy;
            public bool Usefull;
        }

        public Statistic Stat;
        public bool AddMode { get; set; }  = false;
        public bool RemoveMode { get; set; }  = false;

        public Queue<string> CommandsQueue { get; set; } = new Queue<string>();

        public Chat(long id)
        {
            Id = id;
            Stat = DefaultStat();
        }

        private Statistic DefaultStat()
        {
            return new Statistic()
            {
                Asist = false,
                BadBoy = false,
                Bomb = false,
                Snip = false,
                Usefull = false
            };
        }

        internal void ResetMode()
        {
            WhoMode = false;
            PersonalStatMode = false;
            AddMode = false;
            RemoveMode = false;
            Stat = DefaultStat();
            CommandsQueue.Clear();
        }
    }
}
