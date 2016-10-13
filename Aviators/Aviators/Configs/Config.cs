using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators.Configs
{
    public static class Config
    {
        public static readonly string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + @"/DB/PlayersPhoto/";
        public static readonly string DBPlayersInfoPath = Directory.GetCurrentDirectory() + @"/DB/PlayersInfo.txt";
        public static readonly string DBFile = Directory.GetCurrentDirectory() + @"/DB/database.db";
    }
}
