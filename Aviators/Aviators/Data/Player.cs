using System.Collections.Generic;
using System.Linq;

namespace Aviators
{
    public class Player
    {
        public Player(int number, string name, string surname)
        {
            Actions = new List<GameAction>();

            Number = number;
            Name = name;
            Surname = surname;
            PhotoFile = string.Format("{0}_{1}.jpg", number, surname.ToLower());
        }

        public int Id { get; set; }
        public int Number { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Lastname { get; set; }

        public string PhotoFile { get; set; }
        public PlayerPosition Position { get; set; }
        public string VK { get; set; }
        public string INSTA { get; set; }

        public bool isK { get; set; }
        public bool isA { get; set; }

        #region Статистика

        //Временная хуйня наверное
        public int AllStatGoal { get; set; }
        public int AllStatAssist { get; set; }
        public int AllStatBomb { get; set; }

        public int AllStatMinute { get; set; }
        public int StatMinute { get; set; }


        public float StatAverragePerGame => (float) ((Goals + Pas)/(float) Games);

        public List<GameAction> Actions { get; set; }

        public int Goals => Actions.Count(a => a.Action == Action.Гол);

        public int Pas => Actions.Count(a => a.Action == Action.Пас);

        public int Shtraf => Actions.Where(a => a.Action == Action.Штраф).Sum(a => a.Param);

        public int Games => Actions.Count(a => a.Action == Action.Игра);

        public int PlusMinus
            => (Actions.Count(a => a.Action == Action.Плюс) - Actions.Count(a => a.Action == Action.Минус));

        #endregion

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
}