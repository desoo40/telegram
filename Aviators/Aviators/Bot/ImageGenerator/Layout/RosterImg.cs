using System;
using System.Drawing;
using System.IO;

namespace Aviators.Bot.ImageGenerator
{
    public class RosterImg
    {
        static TextInImg tmp = new TextInImg();

        public Rectangle EnemyLogo { get; set; }
        public Rectangle TournamentLogo { get; set; }
        public TextInImg Description { get; set; }
        public TextInImg Date { get; set; }
        public Player Forward { get; set; }
        public Player Defender { get; set; }
        public Player Goalie { get; set; }

        public class Player
        {
            public Rectangle Position { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
            public TextInImg Name { get; set; }
            public TextInImg Number { get; set; }
            public TextInImg AorK { get; set; }

            public Player(string[] lines, int i)
            {

                while (lines[i] != "#")
                {
                    if (lines[i] == "Position")
                        Position = tmp.GetRectFromLine(lines[++i]);

                    if (lines[i] == "Repeatability")
                        FillOffset(lines[++i]);

                    if (lines[i] == "Name")
                        Name = new TextInImg(lines, ++i);

                    if (lines[i] == "AorK")
                        AorK = new TextInImg(lines, ++i);

                    if (lines[i] == "Number")
                        Number = new TextInImg(lines, ++i);

                    ++i;

                }
            }

            private void FillOffset(string s)
            {
                if (tmp.CheckForNothing(s))
                {
                    OffsetX = 0;
                    OffsetY = 0;
                }
                else
                {
                    var spl = s.Split(' ');

                    OffsetX = Convert.ToInt32(spl[0]);
                    OffsetY = Convert.ToInt32(spl[1]);
                }
            }
        }

        public RosterImg(string path)
        {
            var lines = File.ReadAllLines(path);

            if (lines.Length == 0)
            {
                Console.WriteLine(@"ERROR(файл не содержит строк)");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == "Images")
                    i = FillImages(lines, ++i);

                if (lines[i] == "Texts")
                    i = FillTexts(lines, ++i);

                if (lines[i] == "Players")
                    i = FillPlayers(lines, ++i);
            }
        }

        private int FillPlayers(string[] lines, int i)
        {
            while (i < lines.Length)
            {
                if (lines[i] == "Forwards")
                    Forward = new Player(lines, ++i);

                if (lines[i] == "Defenders")
                    Defender = new Player(lines, ++i);

                if (lines[i] == "Goalies")
                    Goalie = new Player(lines, ++i);

                ++i;
            }
            return i;
        }

        private int FillTexts(string[] lines, int i)
        {
            while (lines[i] != "+")
            {
                if (lines[i] == "Description")
                    Description = new TextInImg(lines, ++i);

                if (lines[i] == "Date")
                    Date = new TextInImg(lines, ++i);

                ++i;
            }
            return i;
        }

        private int FillImages(string[] lines, int i)
        {
            while (lines[i] != "+")
            {
                if (lines[i] == "Tournament")
                    TournamentLogo = tmp.GetRectFromLine(lines[++i]);

                if (lines[i] == "EnemyLogo")
                    EnemyLogo = tmp.GetRectFromLine(lines[++i]);

                ++i;
            }
            return i;
        }
    }
}