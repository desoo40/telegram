using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SportfortCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            if(!Directory.Exists(Configs.Config.DBPlayersPhotoDirPath))
            {
                Directory.CreateDirectory(Configs.Config.DBPlayersPhotoDirPath);
            }
            
            Synchronizer.InitializateSources();
        }
    }
}
