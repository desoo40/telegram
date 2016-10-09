using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;

namespace MyFirstBot
{
    class Program
    {
        static readonly TelegramBotClient Bot = new TelegramBotClient("297610365:AAG3yzYtC0XgLrQC0ong1qdJ5odMkqiGHno");
        static readonly FuckGen Fuck = new FuckGen();
        static bool End = true;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            StartBot();
        }

        static void StartBot()
        {
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine("Hello my name is " + me.FirstName);

            Bot.OnMessage += Bot_OnMessage;
            Bot.StartReceiving();

            while (End)
            {
                //Nothing to do, just sleep 1 sec
                //ctrl+c break cycle
                Thread.Sleep(1000);
            }

            Bot.StopReceiving();      
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            Console.WriteLine(e.Message.Text);
            
            await Bot.SendTextMessage(e.Message.Chat.Id, Fuck.GetFuck());
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("It's the end! Bye.");
            End = false;
        }
    }
}
