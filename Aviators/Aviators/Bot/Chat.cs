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
        public bool StatMode { get; set; }  = false;
        public bool AddMode { get; set; }  = false;
        public bool RemoveMode { get; set; }  = false;

        public Queue<string> CommandsQueue { get; set; }

        public Chat(long id)
        {
            Id = id;
        }

        internal void ResetMode()
        {
            WhoMode = false;
            StatMode = false;
            AddMode = false;
            RemoveMode = false;
            CommandsQueue.Clear();
        }
    }
}
