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
            DirectoryInfo di = new DirectoryInfo("Incoming");
            var files = di.GetFiles();

            if (files.Length == 0)
            {
                return;
                //Console.WriteLine("Входящих файлов не обнаружено");
            }
            Console.WriteLine("Обрабатываем файлы во входящей папке");

            foreach (var fileInfo in files)
            {
                var r = ProcessOneFile(fileInfo.FullName);
                if (r == false)
                    Console.WriteLine("Файл не удалось обработать.");
            }
        }

        public static bool ProcessOneFile(string path)
        {
            Console.Write("Обрабатываем файл: " + Path.GetFileNameWithoutExtension(path) + " ... ");

            string name;

            if (Path.GetFileNameWithoutExtension(path).Contains("PlayersInfo"))
            {
                var players = ParsePlayerInfo(path);
                DB.DBCommands.DBPlayer.UpdatePlayersInfo(players);

                name = DB.DBCommands.AddParseFile(Path.GetFileNameWithoutExtension(path), -1);
            }
            else
            {
                Game game = null;
                try
                {
                    game = ParseTXTFile(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (game == null) return false;

                var findGame = DB.DBCommands.DBGame.FindGame(game);
                if (findGame == null)
                {
                    var r = DB.DBCommands.AddNewGameAndPlayers(game);
                    if (r == false) return false;

                }
                else
                {
                    if (!DB.DBCommands.UpdateGameAndPlayer(findGame, game)) return false;
                }

                name = DB.DBCommands.AddParseFile(Path.GetFileNameWithoutExtension(path), game.Id);
            }

            if (!Directory.Exists("Complete"))
                Directory.CreateDirectory("Complete");

            File.Move(path, "Complete\\" + name + ".txt");
            Console.WriteLine("OK");
            return true;
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
                    string s;
                    if (lines[i].Contains("("))
                    {
                        int si = lines[i].IndexOf("(");

                        var p = lines[i].Substring(si);
                        if (p[1] == 'К') isK = true;
                        if (p[1] == 'А') isA = true;

                        s = lines[i].Replace("\t", " ").Substring(0, si);
                    }
                    else
                        s = lines[i].Replace("\t", " ");

                    var playerInfo = s.Split();

                    var newplayer= new Player(Convert.ToInt32(playerInfo[0]), playerInfo[2], playerInfo[1]);

                    if (playerInfo.Length > 3)
                        newplayer.Patronymic = playerInfo[3];

                    newplayer.isA = isA;
                    newplayer.isK = isK;

                    //Добавим действие, что он играл
                    var action = new GameAction(newplayer, "0", Action.Игра);
                    newplayer.Actions.Add(action);

                    if (isA)
                        newplayer.Actions.Add(new GameAction(newplayer, "0", Action.Ассистент));
                    if (isK)
                        newplayer.Actions.Add(new GameAction(newplayer, "0", Action.Капитан));

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

                    while (lines[i + 1] != "Вратарь")
                    {
                        var var = lines[++i].Split(' ');

                        if (var[0].ToLower() == "к") continue;

                        var player = Roster.Find(p => p.Number == Convert.ToInt32(var[0]));

                        var action   = new GameAction(player, "0", Action.Штраф);
                        action.Param = Convert.ToInt32(var[1]);
                        player.Actions.Add(action);
                    }

                }

                if (lines[i] == "Вратарь" && lines[i + 1].Contains("-")) // что делать?
                {
                    while (lines[i+1] != "Лучший")
                    {
                        var var = lines[++i].Split('-');
                        var vara = Roster.Find(p => p.Number == Convert.ToInt32(var[0]));

                        var time = var[1].Split(':');
                        vara.Actions[0].Param = Convert.ToInt32(time[0]);
                    }
                   
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
                //Game.Actions.Add(new GameAction(player, "0", Action.Игра));
                foreach (var playerAction in player.Actions)
                {
                    Game.Actions.Add(playerAction);
                }
            }

            return Game;
        }

        private static List<Player> ParsePlayerInfo(string file)
        {
            var playersString = File.ReadAllLines(file);

            var players = new List<Player>();

            foreach (var s in playersString)
            {
                var spl = s.Split(';');

                var player = new Player(Convert.ToInt32(spl[0]), spl[2], spl[1], spl[3]);

                PlayerPosition pp;
                PlayerPosition.TryParse(spl[4],true, out pp);
                player.Position = pp;

                player.VK = spl[5];
                player.INSTA = spl[6];

                //switch (spl[2])
                //{
                //    case "Вратарь": 
                //        player.Position =  PlayerPosition.Вратарь;
                //        break;
                //    case "Нападающий":
                //        player.Position = PlayerPosition.Нападающий;
                //        break;
                //    case "Защитник":
                //        player.Position = PlayerPosition.Защитник;
                //        break;
                //    case "Тренер":
                //        player.Position = PlayerPosition.Тренер;
                //        break;

                //}
                players.Add(player);
            }

            return players;
        }
    }
}
