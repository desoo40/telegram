using System;
using System.Collections.Generic;
using System.Linq;

namespace Aviators
{
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

        public int Param { get; set; }


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
