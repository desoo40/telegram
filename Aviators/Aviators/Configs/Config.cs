using System.IO;
using System.Configuration;

namespace Aviators.Configs
{
    public static class Config
    {
        public static readonly string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBPlayersPhotoDirPath"];
        public static readonly string DBPlayersInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBPlayersInfoPath"];
        public static readonly string DBTeamsInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBTeamsInfoPath"];
        public static readonly string DBGamesInfoPath = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBGamesInfoPath"];
        public static readonly string DBFile = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["DBFile"];
        
        public static readonly string Slogans = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["Slogans"];
        public static readonly string Descr = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["Descr"];

        public static class BotToken
        {
            public static readonly string Aviators = "272766435:AAH9_EKKEHS9KOMhc1bdXQgHD8BMNY8YNN4";
            public static readonly string Boris = "124248191:AAGDONDKlfyU1R0bv3MqWRYbvZQJiSJycm8";
            public static readonly string Denis = "297610365:AAEflHFUSK87OiCmjjS4H05D_FDtN57ijLY";
        }
        public static class BotAdmin
        {
            private static readonly int DimaId = 85914401;
            private static readonly int BorisId = 0;
            private static readonly int DenisId = 0;

            public static bool isAdmin(int id)
            {
                if (id == DimaId) return true;
                if (id == BorisId) return true;
                if (id == DenisId) return true;

                return false;
            }
        }
    }
}
