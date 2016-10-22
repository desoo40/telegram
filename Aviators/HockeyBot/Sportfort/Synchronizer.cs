using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HockeyBot.Configs;

namespace HockeyBot.Sportfort
{
    public class Synchronizer
    {
        public static void InitializateSources()
        {
            var players = TeamMembersPageParser.ParseTeamMembersPage(new Uri(Config.SportFortTeamMembersPage));
            foreach(var player in players)
            {
                Console.WriteLine(player.Name);
            }

        }
        public static void UpdateSources()
        {

        }
    }
}
