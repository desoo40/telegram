using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;
using Aviators.Configs;

namespace Aviators.Bot
{
    public static class AviaBot
    {
        private static TelegramBotClient Bot;
        private static string Username;
        private static CommandProcessor Commands;

        public static readonly List<Player> Players = new List<Player>();
        public static readonly List<Chat> Chats = new List<Chat>();

        public static bool End = true;
        public static void Start()
        {
            Bot = new TelegramBotClient(Config.BotToken.Denis);
            Commands = new CommandProcessor(Bot);

            var me = Bot.GetMeAsync().Result;
            Console.WriteLine("Hello my name is " + me.FirstName);
            Console.WriteLine("Username is " + me.Username);
            Console.WriteLine("Press ctrl+c to kill me.");

            Bot.OnMessage += Bot_OnMessage;
            AviaBot.Username = me.Username;

            Console.WriteLine("StartReceiving...");
            Bot.StartReceiving();

            while (End)
            {
                //Nothing to do, just sleep 1 sec
                //ctrl+c break cycle
                Thread.Sleep(1000);
            }

            Console.WriteLine("StopReceiving...");
            Bot.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var msg = e.Message.Text;
            var cid = e.Message.Chat.Id;
            var fromId = e.Message.From.Id;

            Console.WriteLine("Incoming request: " + msg);
            Console.WriteLine("Search known chat: " + e.Message.Chat.FirstName + "; " + cid);

            var chatFinded = Chats.FindLast(chat => chat.Id == cid);
            if (chatFinded == null)
            {
                chatFinded = new Chat(cid);
                Chats.Add(chatFinded);
            }

            if (msg == null) return;

            msg = msg.Trim('/');
            msg = msg.Replace(AviaBot.Username, "");
            msg = msg.Replace("@","");

            try
            {
                Commands.FindCommand(msg, chatFinded, fromId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown Commands.FindCommand exceprion: " + ex.Message);
            }
        }
    }
}
