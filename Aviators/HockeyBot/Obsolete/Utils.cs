using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyBot
{
    class Utils
    {
        static readonly List<Player> Players = new List<Player>();
        static void LoadPlayers(string path)
        {
            Console.WriteLine("Try parse " + path);
            using (var sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var fields = line.Split(';');
                    if (fields.Length != 3) continue;

                    Players.Add(new Player(int.Parse(fields[0]), fields[2], fields[1]));
                }
            }
        }
    }
}
