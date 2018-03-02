using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Aviators.Configs;
using Telegram.Bot.Types.Enums;

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
                //if (Program.LoadIncome)
                //    Parse.ProcessFiles();

                //Nothing to do, just sleep 1 sec
                //ctrl+c break cycle
                Thread.Sleep(10000);
            }

            Console.WriteLine("StopReceiving...");
            Bot.StopReceiving();
        }

        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var msg = e.Message.Text;
            var cid = e.Message.Chat.Id;
            var fromId = e.Message.From.Id; //для проверки на права доступа(типа добавления и т.д.)

            if (e.Message.Type == MessageType.DocumentMessage)
            {
                Console.WriteLine("Принимаем файл: " + e.Message.Document.FileName);

                try
                {
                    var file = await Bot.GetFileAsync(e.Message.Document.FileId);
                    //var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                    using (var saveStream = new FileStream("Incoming" + "/" + e.Message.Document.FileName, FileMode.Create))
                    {
                        await file.FileStream.CopyToAsync(saveStream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось принять файл");
                }

                await Bot.SendTextMessageAsync(cid, "Файл принят");

                bool res = Parse.ProcessOneFile("Incoming" + "/" + e.Message.Document.FileName);

                if (res) await Bot.SendTextMessageAsync(cid, "Файл обработан");
                else await Bot.SendTextMessageAsync(cid, "Файл не удалось обработать");

                return;
            }

            Console.WriteLine("Incoming request: " + msg);
            Console.WriteLine("Search known chat: " + e.Message.Chat.FirstName + "; " + cid);

            var chatFinded = Chats.FindLast(chat => chat.Id == cid);
            if (chatFinded == null)
            {
                chatFinded = DB.DBCommands.FindOrInsertChat(e.Message.Chat);
                Chats.Add(chatFinded);

                Commands.SendKeyboardButtons(chatFinded);
            }

            if (msg == null) return;
            msg = msg.ToLower();
            msg = msg.Trim('/');
            msg = msg.Replace(AviaBot.Username.ToLower(), "");
            msg = msg.Replace("@","");

            DB.DBCommands.AddChatIncomeMsg(chatFinded, msg);

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
                Console.WriteLine("Cannot find chat for this command: " + e.CallbackQuery.Message);
            }
            else
            {
                Commands.ContinueCommand(chatFinded, e.CallbackQuery);
            }
        }

    }
}
