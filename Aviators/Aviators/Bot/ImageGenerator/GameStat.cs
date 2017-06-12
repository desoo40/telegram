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
        string BlankPath { get; set; }

        public class Text 
        {
            Rectangle Position { get; set; }
            int OffsetX { get; set; }
            int OffsetY { get; set; }
            Font Font { get; set; }
            Brushes Color { get; set; }
            Format Format { get; set; }
        }

        #region Images
        Rectangle HomeLogo { get; set; }
        Rectangle EnemyLogo { get; set; }
        Rectangle TournamentLogo { get; set; }
        Rectangle BestPlayerImage { get; set; }
        #endregion
        
        #region Texts
        Text Team1 { get; set; }
        Text Team2 { get; set; }
        Text Place { get; set; }
        Text Date { get; set; }
        Text Score1 { get; set; }
        Text Score2 { get; set; }
        Text Stat { get; set; }
        Text NextGameOrLogo { get; set; }
        Text Pucks { get; set; }
        Text Best { get; set; }
        #endregion
        
        public GameStat(string path)
        {
            var lines = File.ReadAllLines(path);

            if (lines.Length == 0)
            {
                Console.WriteLine(@"ERROR(файл не содержит строк)");
                return;
            }

            while (true)
            {
                
            }
        }
    }
}
