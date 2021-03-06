using System.Collections.Generic;
using System.IO;
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
            PhotoFile = GetPhotoFile();
        }

        public Player(int number, string name, string surname, string patronymic)
        {
            Actions = new List<GameAction>();

            Number = number;
            Name = name;
            Surname = surname;

            Patronymic = patronymic == "-" || patronymic == "" ? null : patronymic;

            PhotoFile = GetPhotoFile();
        }

        private string GetPhotoFile()
        {
            var files = Directory.GetFiles("DB\\PlayersPhoto");

            string str = "DB\\PlayersPhoto\\" + Surname.ToLower();

            bool findPhoto = files.Contains(str + ".jpg");

            if (!findPhoto)
            {
                str += " " + Name.ToLower();
                findPhoto = files.Contains(str + ".jpg");

                if (!findPhoto)
                {
                    if (Patronymic == null)
                    {
                        str = "DB\\PlayersPhoto\\no_photo.jpg";
                        return str;
                    }

                    str += " " + Patronymic.ToLower();
                    findPhoto = files.Contains(str + ".jpg");

                    if (!findPhoto)
                    {
                        str = "DB\\PlayersPhoto\\no_photo.jpg";
                        return str;
                    }
                }
            }
            str += ".jpg";
            return str;
        }


        public int Id { get; set; }
        public int Number { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }

        public string PhotoFile { get; set; }
        public PlayerPosition Position { get; set; }
        public string VK { get; set; }
        public string INSTA { get; set; }

        public bool isK { get; set; }
        public bool isA { get; set; }

        #region ����������

        //��������� ����� ��������
        public int AllStatGoal { get; set; }
        public int AllStatAssist { get; set; }
        public int AllStatBomb { get; set; }

        public int AllStatMinute { get; set; }
        public int StatMinute { get; set; }


        public float StatAverragePerGame => (float) ((Goals + Pas)/(float) Games);

        public List<GameAction> Actions { get; set; }

        public int Goals => Actions.Count(a => a.Action == Action.���);

        public int Pas => Actions.Count(a => a.Action == Action.���);

        public int Shtraf => Actions.Where(a => a.Action == Action.�����).Sum(a => a.Param);

        public int Games
        {
            get { return Actions.Count(a => a.Action == Action.����); }
            set { throw new System.NotImplementedException(); }
        }

        public int PlusMinus
            => (Actions.Count(a => a.Action == Action.����) - Actions.Count(a => a.Action == Action.�����));

        #endregion

        public override string ToString()
        {
            return Number + " - " + Name + " " + Surname;
        }
    }

    public enum PlayerPosition
    {
        �������,
        ����������,
        ��������,
        ������
    }
}