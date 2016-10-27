using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Awesomium.Core;

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

            //var web = new Thread(new ThreadStart( () => { WebCore.Run(); }));
            //web.Start();
            
            Synchronizer.InitializateSources();
        }
    }
}
