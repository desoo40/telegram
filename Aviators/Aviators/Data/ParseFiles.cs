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
                Console.Write("Обрабатываем файл: " + fileInfo.Name + " ... ");
                ParseTXTFile(fileInfo.FullName);
            }
        }

        private static void ParseTXTFile(string file)
        {
            //ростер - это состав
            List<Player> Roster = new List<Player>();
            Game Game = new Game();

            var lines = File.ReadAllLines(file);

            if (lines.Length == 0)
            {
                Console.WriteLine("ERROR(файл не содержит строк)");
                return;
            }

            Game.Date = Convert.ToDateTime(lines[0]);
            Game.Tournament = new Tournament(lines[1]);


            Game.Team1 = "Авиаторы";
            Game.Team2 = lines[2];

            int i = 5;
            while (lines[i] != "Счет")
            {
                if (lines[i] != "")
                {
                    var s = lines[i].Replace("\t", " ");
                    var playerInfo = s.Split();
                    var newplayer= new Player(Convert.ToInt32(playerInfo[0]), playerInfo[2], playerInfo[1]);
                    Roster.Add(newplayer);
                }

                ++i;
            }

            #region такое

            while (i < lines.Length - 1)
            {
                if (lines[i] == "Счет")
                {
                    var score = lines[++i].Split('-');

                    Game.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));
                }

                if (lines[i] == "Голы") // Боря допишет
                {
                    var goals = lines[++i].Split(';');
                    foreach (var goal in goals)
                    {
                        var g = new Goal();
                        //TODO по тупому, переделать на регекс
                        if (goal.Contains("("))
                        {
                            var a = goal.Split('(');
                            int a_num = Convert.ToInt32(a[0]);
                            var author = Roster.Find(p => p.Number == a_num);
                            if (author == null) continue;

                            g.Author = author;

                            if (goal.Contains(","))
                            {
                                var ass = a[1].Split(',');
                                int a1_num = Convert.ToInt32(ass[0]);
                                var as1 = Roster.Find(p => p.Number == a1_num);
                                if (as1 == null) continue;
                                g.Assistant1 = as1;

                                int a2_num = Convert.ToInt32(ass[1].Replace(')',' '));
                                var as2 = Roster.Find(p => p.Number == a2_num);
                                if (as2 == null) continue;
                                g.Assistant2 = as2;

                            }
                            else
                            {
                                int a1_num = Convert.ToInt32(a[1].Replace(')', ' '));
                                var as1 = Roster.Find(p => p.Number == a1_num);
                                if (as1 == null) continue;
                                g.Assistant1 = as1;
                            }
                        }
                        else
                        {
                            var author = Roster.Find(p => p.Number == Convert.ToInt32(goal));
                            if (author == null) continue;
                            g.Author = author;
                        }
                        Game.Goal.Add(g);
                    }
                }

                if (lines[i] == "Броски" && lines[i + 1].Contains("-"))
                {
                    var shots = lines[++i].Split('-');

                    Game.Stat1.Shots = Convert.ToInt32(shots[0]);
                    Game.Stat2.Shots = Convert.ToInt32(shots[1]);

                }

                if (lines[i] == "В створ" && lines[i + 1].Contains("-"))
                {
                    var shotsIn = lines[++i].Split('-');

                    Game.Stat1.ShotsIn = Convert.ToInt32(shotsIn[0]);
                    Game.Stat2.ShotsIn = Convert.ToInt32(shotsIn[1]);

                }

                if (lines[i] == "Заблокированные" && lines[i + 1].Contains("-"))
                {
                    var block = lines[++i].Split('-');

                    Game.Stat1.BlockShots = Convert.ToInt32(block[0]);
                    Game.Stat2.BlockShots = Convert.ToInt32(block[1]);

                }

                if (lines[i] == "Вбрасывания" && lines[i + 1].Contains("-"))
                {
                    var faceoff = lines[++i].Split('-');

                    Game.Stat1.Faceoff = Convert.ToInt32(faceoff[0]);
                    Game.Stat2.Faceoff = Convert.ToInt32(faceoff[1]);

                }

                if (lines[i] == "Силовые" && lines[i + 1].Contains("-"))
                {
                    var hits = lines[++i].Split('-');

                    Game.Stat1.Hits = Convert.ToInt32(hits[0]);
                    Game.Stat2.Hits = Convert.ToInt32(hits[1]);

                }

                if (lines[i] == "Штрафы" && lines[i + 1].Contains("-"))
                {
                    var pen = lines[++i].Split('-');

                    Game.Stat1.Penalty = Convert.ToInt32(pen[0]);
                    Game.Stat2.Penalty = Convert.ToInt32(pen[1]);

                }

                if (lines[i] == "Вратарь" && lines[i + 1].Contains("-")) // что делать?
                {


                }

                if (lines[i] == "Лучший" && lines[i + 1].Contains("-")) // MVP ++
                {

                }

                if (lines[i] == "Зрители" && lines[i + 1].Contains("-"))
                {
                    Game.Viewers = Convert.ToInt32(lines[++i]);
                }

                i++;
            }

            #endregion

            DB.DBCommands.AddNewGameAndPlayers(Game, Roster);


            Console.WriteLine("OK");

        }
    }
}
