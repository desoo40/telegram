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

        public bool AddMode { get; set; }  = false;
        public bool RemoveMode { get; set; }  = false;
        public List<Command> WaitingCommands { get; set; }

        public Tournament Tournament { get; set; }


        public Chat(long id)
        {
            Id = id;
            WaitingCommands = new List<Command>();


        }
        internal void ResetMode()
        {
            WhoMode = false;
            PersonalStatMode = false;
            AddMode = false;
            RemoveMode = false;
        }
    }
}
