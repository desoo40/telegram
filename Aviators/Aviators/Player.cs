using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    class Player
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhotoFile { get; set; }

        public Player(int number, string name, string surname)
        {
            Number = number;
            Name = name;
            Surname = surname;
            PhotoFile = string.Format("{0}_{1}.jpg", number, name);
        }
    }
}
