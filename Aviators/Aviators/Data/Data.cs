using System;
using System.Collections.Generic;
using System.Linq;

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

        public int Goals
        {
            get { return Actions.Count(a => a.Action == Action.Гол); }
        }
        public int Pas
        {
            get { return Actions.Count(a => a.Action == Action.Пас); }
        }
        public int Shtraf
        {
            get { return Actions.Count(a => a.Action == Action.Штраф); }
        }
        public int Games
        {
            get { return Actions.Count(a => a.Action == Action.Игра); }
        }
        public int PlusMinus
        {
            get { return (Actions.Count(a => a.Action == Action.Плюс) - Actions.Count(a => a.Action == Action.Минус)); }
        }

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

        public GameAction(Player player, string game, Action action)
        {
            Player = player;
            Game = new Game {Id = Convert.ToInt32(game)};
            Action = action;
        }

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
