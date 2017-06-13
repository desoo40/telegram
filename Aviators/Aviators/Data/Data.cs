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
        public string Lastname { get; set; }

        public bool isK { get; set; }
        public bool isA { get; set; }


        //Временная хуйня наверное
        public int StatGoal { get; set; }
        public int StatAssist { get; set; }
        public int StatBomb { get; set; }

        public float StatAverragePerGame => (float) ((Goals + Pas) / (float) Games);


        public string PhotoFile { get; set; }
        public PlayerPosition Position { get; set; }
        public string VK { get; set; }
        public string INSTA { get; set; }

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

    public enum PlayerPosition
    {
       Вратарь, 
       Нападающий,
       Защитник,
       Тренер
    }

    public class PlayerStat
    {
        public int PlusMinus { get; set; }
        public int Goals { get; set; }
        public int Assist { get; set; }
        public int Shots { get; set; }
        public int ShotsIn { get; set; }
        public int WinFaceoff { get; set; }
        public int LoseFaceoff { get; set; }
        public int BlockShots { get; set; }
        public int Hits { get; set; }
        public int Penalty { get; set; }
        public int MVP { get; set; }

    }
    public class Game
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public TeamStat Stat1 { get; set; }
        public TeamStat Stat2 { get; set; }

        public int Viewers { get; set; }
        public Tuple<int, int> Score { get; set; }

        public Place Place { get; set; }
        public Tournament Tournament { get; set; }
        public string Season { get; set; }

        public List<GameAction> Actions { get; set; }
        public List<Goal> Goal { get; set; }
        public int MVP { get; set; }

        public bool PenaltyGame { get; set; }
        public bool OvertimeGame { get; set; }

        public Player BestPlayer { get; set; }

        public List<Player> Roster
        {
            get
            {
                return Actions.Where(a => a.Action == Action.Игра).Select(p => p.Player).ToList();
            }
        }

        public string Description { get; set; }

        public Game()
        {
            Actions= new List<GameAction>();
            Goal = new List<Goal>();

            Score = new Tuple<int, int>(0,0);

            Stat1 = new TeamStat();
            Stat2 = new TeamStat();
        }
    }

    public class TeamStat
    {
        public int Shots { get; set; }
        public int ShotsIn { get; set; }
        public int Faceoff { get; set; }
        public int BlockShots { get; set; }
        public int Hits { get; set; }
        public int Penalty { get; set; }
    }

    public class Tournament
    {
        public Tournament(string s)
        {
            Name = s;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public Season Season { get; set; }
    }

    public class Season
    {
        public Season(string s)
        {
            Name = s;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Place
    {
        public Place(string s)
        {
            Name = s;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string FullAdress { get; set; }
        public string GeoPos { get; set; }

    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GameAction
    {

        public GameAction(Player player, string game_id, Action action)
        {
            Player = player;
            Game = new Game {Id = Convert.ToInt32(game_id)};
            Action = action;
        }

        public GameAction(Player player, Game game, Action action)
        {
            Player = player;
            Game = game;
            Action = action;
        }

        public int Id { get; set; }
        public Player Player { get; set; }
        public Game Game { get; set; }
        public Action Action{ get; set; }

}

    public class Goal
    {
        public int Id { get; set; }
        public Player Author { get; set; }
        public Player Assistant1 { get; set; }
        public Player Assistant2 { get; set; }

        public bool isPenalty { get; set; }
        public bool PowerPlay { get; set; }
        public bool ShortHand { get; set; }


    }

    public enum Action
    {
        Игра,
        Гол,
        Пас,
        Штраф,
        Плюс,
        Минус,

        Лучший,
        Капитан,
        Ассистент
    }
}
