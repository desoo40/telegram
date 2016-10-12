using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    public class Player
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhotoFile { get; set; }
        public string Position { get; set; }

        public List<GameAction> Actions { get; set; }

        public Player(int number, string name, string surname)
        {
            Actions = new List<GameAction>();

            Number = number;
            Name = name;
            Surname = surname;
            PhotoFile = string.Format("{0}_{1}.jpg", number, surname.ToLower());
        }

        public override string ToString()
        {
            return Number + " - " + Name + " " + Surname;
        }
    }

    public class Game
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string Team1 { get; set; }
        public string Team2 { get; set; }

        public Tuple<int, int> Score { get; set; }

        public string Place { get; set; }
        public string Tournament { get; set; }
        public string Season { get; set; }
        public string Type { get; set; }

        public List<GameAction> Actions { get; set; }

        public Game()
        {
            Actions= new List<GameAction>();
        }
    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GameAction
    {
        public int Id { get; set; }
        public Player Player { get; set; }
        public Game Game { get; set; }
        public Action Action{ get; set; }

}

    public enum Action
    {
        Игра,
        Гол,
        Пас,
        Штраф,
        Плюс,
        Минус
    }
}
