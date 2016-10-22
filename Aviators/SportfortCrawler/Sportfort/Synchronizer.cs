using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SportfortCrawler
{
    public class Synchronizer
    {
        public static void InitializateSources()
        {
            var players = TeamMembersPageParser.ParseTeamMembersPage("http://sportfort.ru/WildWoodpeckers/TeamMembersPage");
            foreach(var player in players)
            {
                Console.WriteLine(player);
            }
        }
        public static void UpdateSources()
        {

        }
    }
}
