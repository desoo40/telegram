using System.IO;
using System.Configuration;

namespace SportfortCrawler.Configs
{
    public static class Config
    {
        public static readonly string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBPlayersPhotoDirPath"];
        public static readonly string DBPlayersInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBPlayersInfoPath"];
        public static readonly string DBEventsInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBEventsInfoPath"];
        public static readonly string DBGamesInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBGamesInfoPath"];
        public static readonly string DBStatsInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBStatsInfoPath"];
        public static readonly string DBFile = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBFile"];

        public static readonly string SportFortTeamMembersPage = ConfigurationManager.AppSettings["TeamMembersPage"];
        public static readonly string SportFortHomePage = ConfigurationManager.AppSettings["HomePage"];
        public static readonly string SportFortTournamentsStatsPage = ConfigurationManager.AppSettings["TournamentsStatsPage"];
        public static readonly string SportFortTournamentsTablesPage = ConfigurationManager.AppSettings["TournamentsTablesPage"];

        public static readonly string PWD = ConfigurationManager.AppSettings["PWD"];
    }
}
