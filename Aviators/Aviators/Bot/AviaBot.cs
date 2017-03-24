﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public static readonly List<Chat> Chats = new List<Chat>();

        public static bool End = true;
        public static void Start()
        {
            Bot = new TelegramBotClient(Config.BotToken.Boris);
            Commands = new CommandProcessor(Bot);

            var me = Bot.GetMeAsync().Result;
            Console.WriteLine("Hello my name is " + me.FirstName);
            Console.WriteLine("Username is " + me.Username);
            Console.WriteLine("Press ctrl+c to kill me.");

            Bot.OnMessage += Bot_OnMessage;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;
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
            var fromId = e.Message.From.Id; //для проверки на права доступа(типа добавления и т.д.)

            Console.WriteLine("Incoming request: " + msg);
            Console.WriteLine("Search known chat: " + e.Message.Chat.FirstName + "; " + cid);

            var chatFinded = Chats.FindLast(chat => chat.Id == cid);
            if (chatFinded == null)
            {
                chatFinded = new Chat(cid);
                Chats.Add(chatFinded);
            }

            if (msg == null) return;
            msg = msg.ToLower();
            msg = msg.Trim('/');
            msg = msg.Replace(AviaBot.Username.ToLower(), "");
            msg = msg.Replace("@","");

            try
            {
                Commands.FindCommands(msg, chatFinded, fromId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown Commands.FindCommand exceprion: " + ex.Message);
            }
        }

        private static void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            Console.WriteLine("Incoming callback from: " + e.CallbackQuery.From);

            int msgid = Convert.ToInt32(e.CallbackQuery.InlineMessageId);

            var chatFinded = Chats.FindLast(chat => chat.WaitingCommands.Any(c=>c.Message.MessageId == e.CallbackQuery.Message.MessageId));
            if (chatFinded == null)
            {
                Console.WriteLine("Cannot find chst for this command: " + e.CallbackQuery.Message);

            }
            else
            {
                Commands.ContinueCommand(chatFinded, e.CallbackQuery.Message.MessageId);
            }
        }

    }
}