using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    class Chat
    {
        public long Id { get; set; }

        public bool WhoMode { get; set; } = false;

        public Chat(long id)
        {
            Id = id;
        }
    }
}
