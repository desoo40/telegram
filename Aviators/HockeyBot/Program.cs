﻿using System;
using System.Net;
using HockeyBot.Bot;
using HockeyBot.Sportfort;

namespace HockeyBot
{
    class Program
    {
        /// <summary>
        /// Флаг для создания базы и табличек, что бы из кода.
        /// Можно так же аргумент в свойствах проекта прописывать, но неудобно
        /// </summary>
        private static bool InitFromCode = true;

        static void Main(string[] args)
        {
            //to ignore untrusted SSL certificates, linux and mono love it ;)
            ServicePointManager.ServerCertificateValidationCallback = Network.SSL.Validator;

            Console.CancelKeyPress += Console_CancelKeyPress;

            if (InitFromCode || args.Length > 0 && args[0] == "init")
            {
                try
                {
                    Synchronizer.InitializateSources();
                    DBCore.Initialization();
                }
                    catch(Exception e)
                {
                        Console.WriteLine("Unknown DBCore exception: " + e.Message);
                }
            }

            Console.WriteLine("Starting Bot...");
            try
            {
                Bot.HockeyBot.Start();
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
            Bot.HockeyBot.End = false;
            if (!Bot.HockeyBot.End) e.Cancel = true;
        }
    }
}

