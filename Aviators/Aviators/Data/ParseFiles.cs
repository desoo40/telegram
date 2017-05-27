using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
                var game = ParseTXTFile(fileInfo.FullName);
                if (game== null) continue;

                var findGame = DB.DBCommands.DBGame.FindGame(game);
                if(findGame == null)
                    DB.DBCommands.AddNewGameAndPlayers(game);
                else
                    DB.DBCommands.UpdateGameAndPlayer(findGame, game);

                var name  = DB.DBCommands.AddParseFile(Path.GetFileNameWithoutExtension(fileInfo.Name), game.Id);

                File.Move(fileInfo.FullName, "Complete\\" + name + ".txt");

                Console.WriteLine("OK");
            }
        }

        private static Game ParseTXTFile(string file)
        {
            Regex rxNums = new Regex(@"^\d+$"); // проверка на число

            //ростер - это состав
            List<Player> Roster = new List<Player>();
            Game Game = new Game();

            var lines = File.ReadAllLines(file);

            if (lines.Length == 0)
            {
                Console.WriteLine("ERROR(файл не содержит строк)");
                return null;
            }

            Game.Date = Convert.ToDateTime(lines[0]);
            Game.Tournament = new Tournament(lines[1]);


            Game.Team1 = "Авиаторы";
            Game.Team2 = lines[2];

            int i = 1;
            while (lines.Length > i - 1 && lines[i - 1] != "Описание")
            {
                i++;
            }

            if (lines.Length > i && lines[i] != "Состав")
                Game.Description = lines[i];

            i = 1;
            while (lines[i-1] != "Состав")
            {
                i++;
            }
            while (lines[i] != "Счет")
            {
                if (lines[i] != "")
                {
                    bool isK = false, isA = false;
                    if (lines[i].Contains("("))
                    {
                        var p = lines[i].Substring(lines[i].IndexOf("("));
                        if (p[1] == 'К') isK = true;
                        if (p[1] == 'А') isA = true;
                    }

                    var s = lines[i].Replace("\t", " ");
                    var playerInfo = s.Split();
                    var newplayer= new Player(Convert.ToInt32(playerInfo[0]), playerInfo[2], playerInfo[1]);
                    newplayer.isA = isA;
                    newplayer.isK = isK;
                    Roster.Add(newplayer);
                }

                ++i;
            }

            #region такое

            while (i < lines.Length - 1)
            {
                if (lines[i] == "Счет" && lines[i + 1].Contains("-"))
                {
                    var s = lines[++i];

                    if (s.Contains("Б"))
                    {
                        Game.PenaltyGame = true;
                        s = s.Replace("(Б)", "");
                    }
                    
                    var score = s.Split('-');

                    Game.Score = new Tuple<int, int>(Convert.ToInt32(score[0]), Convert.ToInt32(score[1]));
                }

                if (lines[i] == "Голы") // Боря допишет
                {

                    //проверяем первый символ на цифру
                    if (rxNums.IsMatch(lines[i + 1][0].ToString())) //TODO как то может получше проверить

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

                                    int a2_num = Convert.ToInt32(ass[1].Replace(')', ' '));
                                    var as2 = Roster.Find(p => p.Number == a2_num);
                                    if (as2 == null) continue;
                                    g.Assistant2 = as2;

                                }
                                else
                                {
                                    var ass = a[1].Replace(')', ' ').Trim();
                                    if (rxNums.IsMatch(ass))
                                    {
                                        int a1_num = Convert.ToInt32(ass);
                                        var as1 = Roster.Find(p => p.Number == a1_num);
                                        if (as1 == null) continue;
                                        g.Assistant1 = as1;
                                    }
                                    else if (ass == "Б")
                                        g.isPenalty = true;
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

                if (lines[i] == "Лучший") // MVP ++
                {
                    if(rxNums.IsMatch(lines[i + 1]))
                        Game.BestPlayer = Roster.Find(p => p.Number == Convert.ToInt32(lines[i+1]));
                }

                if (lines[i] == "Зрители" )
                {
                    if (lines.Length > i + 1)
                        if (rxNums.IsMatch(lines[i + 1]))
                        Game.Viewers = Convert.ToInt32(lines[++i]);
                }

                if (lines[i] == "Место")
                {
                    if (lines.Length > i + 1)
                        Game.Place = new Place(lines[i + 1]);
                }

                i++;
            }

            #endregion

            //Добавляем игроков в игру(пока сделаем через Action)
            foreach (var player in Roster)
            {
                //var a = new GameAction(player, game.Id.ToString(), Action.Игра);
                Game.Actions.Add(new GameAction(player, "0", Action.Игра));
            }

            return Game;
        }
    }
}
