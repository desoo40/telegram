using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators.Bot.ImageGenerator
{
    public class GameStat
    {
        #region Images

        public Rectangle HomeLogo { get; set; }
        public Rectangle EnemyLogo { get; set; }
        public Rectangle TournamentLogo { get; set; }
        public Rectangle BestPlayerImage { get; set; }

        #endregion

        #region Texts

        public class Text
        {
            public Rectangle Position { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
            public Font Font { get; set; }
            public Brush Color { get; set; }
            public StringFormat StrFormatting { get; set; }


            public Text(string[] lines, int i)
            {
                Format format = new Format();

                if (CheckForNothing(lines[i]))
                    return;

                var tmpFont = "Times New Roman";
                var tmpFontSize = 14;
                FontStyle tmpFontStyle = FontStyle.Regular;

                while (lines[i] != "*")
                {
                    if (lines[i] == "Position")
                        Position = GetRectFromLine(lines[++i]);


                    if (lines[i] == "Repeatability")
                    {
                        if (CheckForNothing(lines[++i]))
                        {
                            OffsetX = 0;
                            OffsetY = 0;
                            continue;
                        }

                        var spl = lines[i].Split(' ');

                        OffsetX = Convert.ToInt32(spl[0]);
                        OffsetY = Convert.ToInt32(spl[1]);
                    }

                    if (lines[i] == "Font")
                    {
                        if (!CheckForNothing(lines[++i]))
                            tmpFont = lines[i];
                    }

                    if (lines[i] == "Font size")
                    {
                        if (!CheckForNothing(lines[++i]))
                            tmpFontSize = Convert.ToInt32(lines[i]);
                    }

                    if (lines[i] == "Font Style")
                    {
                        if (!CheckForNothing(lines[++i]))
                        {
                            if (lines[i].ToLower() == "regular")
                                tmpFontStyle = FontStyle.Regular;

                            if (lines[i].ToLower() == "bold")
                                tmpFontStyle = FontStyle.Bold;
                        }
                    }

                    if (lines[i] == "Color")
                    {
                        if (!CheckForNothing(lines[++i]))
                        {
                            if (lines[i].ToLower() == "white")
                                Color = Brushes.White;

                            if (lines[i].ToLower() == "black")
                                Color = Brushes.Black;

                            if (lines[i].ToLower() == "blue")
                                Color = Brushes.Blue;
                        }
                    }

                    if (lines[i] == "Formatting")
                    {
                        if (!CheckForNothing(lines[++i]))
                        {
                            if (lines[i].ToLower() == "center")
                                StrFormatting = format.centerFormat;

                            if (lines[i].ToLower() == "left")
                                StrFormatting = format.leftFormat;

                            if (lines[i].ToLower() == "right")
                                StrFormatting = format.rightFormat;
                        }
                    }

                    ++i;
                }

                Font = new Font(tmpFont, tmpFontSize, tmpFontStyle);
            }
        }

        public Text Team1 { get; set; }
        public Text Team2 { get; set; }
        public Text Place { get; set; }
        public Text Date { get; set; }
        public Text Score { get; set; }
        public Text Stat { get; set; }
        public Text NextGameOrLogo { get; set; }
        public Text Pucks { get; set; }
        public Text Best { get; set; }
        public Text Viewers { get; set; }


        #endregion

        public GameStat(string path)
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
            }
        }

        private int FillTexts(string[] lines, int i)
        {
            while (i < lines.Length)
            {
                if (lines[i] == "Team1")
                    Team1 = new Text(lines, ++i);

                if (lines[i] == "Team2")
                    Team2 = new Text(lines, ++i);

                if (lines[i] == "Place")
                    Place = new Text(lines, ++i);

                if (lines[i] == "Date")
                    Date = new Text(lines, ++i);

                if (lines[i] == "Score")
                    Score = new Text(lines, ++i);

                if (lines[i] == "Stat")
                    Stat = new Text(lines, ++i);

                if (lines[i] == "NextGame")
                    NextGameOrLogo = new Text(lines, ++i);

                if (lines[i] == "Pucks")
                    Pucks = new Text(lines, ++i);

                if (lines[i] == "Best")
                    Best = new Text(lines, ++i);

                if (lines[i] == "Viewers")
                    Viewers = new Text(lines, ++i);

                ++i;

            }
            return i;
        }

        private int FillImages(string[] lines, int i)
        {
            while (lines[i] != "*")
            {
                if (lines[i] == "Logo1")
                    HomeLogo = GetRectFromLine(lines[++i]);


                if (lines[i] == "Logo2")
                    EnemyLogo = GetRectFromLine(lines[++i]);


                if (lines[i] == "Tournament")
                    TournamentLogo = GetRectFromLine(lines[++i]);


                if (lines[i] == "Best")
                    BestPlayerImage = GetRectFromLine(lines[++i]);

                ++i;
            }
            return i;
        }

        private static Rectangle GetRectFromLine(string s)
        {
            if (CheckForNothing(s))
                return new Rectangle();

            var spl = s.Split(',');
            return new Rectangle(Convert.ToInt32(spl[0]),
                Convert.ToInt32(spl[1]),
                Convert.ToInt32(spl[2]),
                Convert.ToInt32(spl[3]));
        }

        private static bool CheckForNothing(string s)
        {
            return s == "-";
        }
    }
}
