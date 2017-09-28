using System;
using System.Collections.Generic;
using System.Net;
using Aviators.Bot;
using Aviators.Bot.ImageGenerator;

namespace Aviators
{
    class Program
    {
        /// <summary>
        /// Флаг для создания базы и табличек, что бы из кода.
        /// Можно так же аргумент в свойствах проекта прописывать, но неудобно
        /// </summary>
        private static bool InitFromCode = false;
        public static bool LoadIncome = true;

        static void Main(string[] args)
        {
            //to ignore untrusted SSL certificates, linux and mono love it ;)
            ServicePointManager.ServerCertificateValidationCallback = Network.SSL.Validator;

            Console.CancelKeyPress += Console_CancelKeyPress;

            #region Тест для генератора статы


            //var ig = new ImageGenerator2();
            //Game g = new Game();
            //g.BestPlayer = new Player(71, "Кирилл", "Зайцев");
            //g.Team2 = "РЭУ";
            //g.Date = DateTime.Now;
            //g.Tournament = new Tournament("МСХЛ");
            ////ig.Roster(g);
            ///////////////////////////////////////////////////////
            //g.Viewers = 551;
            //g.Team2 = "Тампа-Бэй Лайтнинг";
            //g.Tournament = new Tournament("НХЛ");
            //g.Score = new Tuple<int, int>(7, 6);

            //var kek = new Random();

            //g.Stat1.Shots = kek.Next(0, 100);
            //g.Stat2.Shots = kek.Next(0, 100);
            //g.Stat1.ShotsIn = kek.Next(0, 100);
            //g.Stat2.ShotsIn = kek.Next(0, 100);
            //g.Stat1.Faceoff = kek.Next(0, 100);
            //g.Stat2.Faceoff = kek.Next(0, 100);
            //g.Stat1.Hits = kek.Next(0, 100);
            //g.Stat2.Shots = kek.Next(0, 100);
            //g.Stat1.Penalty = kek.Next(0, 100);
            //g.Stat2.Penalty = kek.Next(0, 100);
            //g.Stat1.BlockShots = kek.Next(0, 100);
            //g.Stat2.BlockShots = kek.Next(0, 100);




            //g.Place = new Place(@"Малая Арена ВТБ, г. Москва");

            //ig.GameStatistic(g);

            //return;
            //#endregion
            //#region тест для подгрузки разметки статы

            //GameStat stat1 = new GameStat("Images\\gameStat.txt");

            #endregion

            if (InitFromCode || args.Length > 0 && args[0] == "init")
            {
                try
                {
                    DBCore.Initialization();
                }
                    catch(Exception e)
                {
                        Console.WriteLine("Unknown DBCore exception: " + e.Message);
                }
            }

            if (LoadIncome || args.Length > 0 && args[0] == "load")
                Parse.ProcessFiles();

            Console.WriteLine("Starting Bot...");
            try
            {
                AviaBot.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown AviaBot exception: " + e.Message);
                Console.WriteLine("Bot will be terminated.");
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("It's the end! Bye.");
            AviaBot.End = false;
            if (!AviaBot.End) e.Cancel = true;
        }
    }
}


