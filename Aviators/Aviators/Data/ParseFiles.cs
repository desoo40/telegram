using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    static class Parse
    {
        public static void ProcessFiles()
        {
            Console.WriteLine("Обрабатываем файлы во входящей папке");

            DirectoryInfo di = new DirectoryInfo("Incoming");
            var files = di.GetFiles();

            if (files.Length == 0)
                Console.WriteLine("Входящих файлов не обнаружено");

            foreach (var fileInfo in files)
            {
                Console.Write("Обрабатываем файл: " + fileInfo.Name + " ...");
                ParseTXTFile(fileInfo.FullName);
            }
        }

        private static void ParseTXTFile(string file)
        {
            //ростер - это состав
            List<Player> Roster = new List<Player>();
            Game game = new Game();

            var lines = File.ReadAllLines(file);

            game.Date = Convert.ToDateTime(lines[0]);
            game.Tournament = new Tournament(lines[1]);
            game.Stat1 = new TeamStat();
            game.Stat2 = new TeamStat();
            game.Team2 = lines[3];
            int i = 5;
            while (lines[i] != "Счет")
            {
                var s = lines[i].Replace("\t", " ");
                var playerInfo = s.Split();
                var newplayer= new Player(Convert.ToInt32(playerInfo[0]), playerInfo[1], playerInfo[2]);
                Roster.Add(newplayer);

                ++i;
            }

            while (i < lines.Length-1)
            {
                if (lines[i] == "Счет")
                {
                    var score = lines[++i].Split('-');

                    game.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));
                }

                if (lines[i] == "Голы") // Боря допишет
                {

                }

                if (lines[i] == "Броски")
                {
                    var shots = lines[++i].Split('-');

                    game.Stat1.Shots = Convert.ToInt32(shots[0]);
                    game.Stat2.Shots = Convert.ToInt32(shots[1]);

                }

                if (lines[i] == "В створ")
                {
                    var shotsIn = lines[++i].Split('-');

                    game.Stat1.ShotsIn = Convert.ToInt32(shotsIn[0]);
                    game.Stat2.ShotsIn = Convert.ToInt32(shotsIn[1]);

                }

                if (lines[i] == "Заблокированные")
                {
                    var block = lines[++i].Split('-');

                    game.Stat1.BlockShots = Convert.ToInt32(block[0]);
                    game.Stat2.BlockShots = Convert.ToInt32(block[1]);

                }

                if (lines[i] == "Вбрасывания")
                {
                    var faceoff = lines[++i].Split('-');

                    game.Stat1.Faceoff = Convert.ToInt32(faceoff[0]);
                    game.Stat2.Faceoff = Convert.ToInt32(faceoff[1]);

                }

                if (lines[i] == "Силовые")
                {
                    var hits = lines[++i].Split('-');

                    game.Stat1.Hits = Convert.ToInt32(hits[0]);
                    game.Stat2.Hits = Convert.ToInt32(hits[1]);

                }

                if (lines[i] == "Штрафы")
                {
                    var pen = lines[++i].Split('-');

                    game.Stat1.Penalty = Convert.ToInt32(pen[0]);
                    game.Stat2.Penalty = Convert.ToInt32(pen[1]);

                }

                if (lines[i] == "Вратарь") // что делать?
                {


                }

                if (lines[i] == "Лучший") // MVP ++
                {

                }

                if (lines[i] == "Зрители")
                {
                    game.Viewers = Convert.ToInt32(lines[++i]);
                }

                i++;
            }

        }
    }
}
