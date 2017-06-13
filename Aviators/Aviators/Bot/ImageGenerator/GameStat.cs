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
        public TextInImg Team1 { get; set; }
        public TextInImg Team2 { get; set; }
        public TextInImg Place { get; set; }
        public TextInImg Date { get; set; }
        public TextInImg Score { get; set; }
        public TextInImg Stat { get; set; }
        public TextInImg NextGameOrLogo { get; set; }
        public TextInImg Pucks { get; set; }
        public TextInImg Best { get; set; }
        public TextInImg Viewers { get; set; }
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
                    Team1 = new TextInImg(lines, ++i);

                if (lines[i] == "Team2")
                    Team2 = new TextInImg(lines, ++i);

                if (lines[i] == "Place")
                    Place = new TextInImg(lines, ++i);

                if (lines[i] == "Date")
                    Date = new TextInImg(lines, ++i);

                if (lines[i] == "Score")
                    Score = new TextInImg(lines, ++i);

                if (lines[i] == "Stat")
                    Stat = new TextInImg(lines, ++i);

                if (lines[i] == "NextGame")
                    NextGameOrLogo = new TextInImg(lines, ++i);

                if (lines[i] == "Pucks")
                    Pucks = new TextInImg(lines, ++i);

                if (lines[i] == "Best")
                    Best = new TextInImg(lines, ++i);

                if (lines[i] == "Viewers")
                    Viewers = new TextInImg(lines, ++i);

                ++i;

            }
            return i;
        }

        private int FillImages(string[] lines, int i)
        {
            TextInImg tmp = new TextInImg();
            while (lines[i] != "*")
            {
                if (lines[i] == "Logo1")
                    HomeLogo = tmp.GetRectFromLine(lines[++i]);


                if (lines[i] == "Logo2")
                    EnemyLogo = tmp.GetRectFromLine(lines[++i]);


                if (lines[i] == "Tournament")
                    TournamentLogo = tmp.GetRectFromLine(lines[++i]);


                if (lines[i] == "Best")
                    BestPlayerImage = tmp.GetRectFromLine(lines[++i]);

                ++i;
            }
            return i;
        }
    }
}
