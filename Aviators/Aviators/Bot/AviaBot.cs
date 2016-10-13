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
            Console.WriteLine("Press ctrl+c to kill me.");

            Bot.OnMessage += Bot_OnMessage;

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
            Commands.FindCommand(msg, chatFinded);

            ModeCode();
        }

        private static void ModeCode()
        {
            //if(chatFinded.WhoMode)
            //{
            //    if (msg.Contains("кто"))
            //    {
            //        chatFinded.WhoMode = false;
            //        await Bot.SendTextMessageAsync(chatFinded.Id, "Вы вышли из режима поиска игрока.");
            //        return;
            //    }

            //    try
            //    {
            //        msg = msg.Trim('/',' ');
            //        int playerNumber = int.Parse(msg);
            //        //var randomPlayer = Players[(new Random()).Next(Players.Count)];
            //        var playerByNumber = Players.FindLast(p => p.Number == playerNumber);

            //        if (playerByNumber == null)
            //        {
            //            await Bot.SendTextMessageAsync(chatFinded.Id, string.Format("Игрок под номером {0} не найден.", playerNumber));
            //        }
            //        else
            //        {
            //            var playerDescription = Fuck.GetFuck();
            //            playerDescription += string.Format("#{0} {1} {2}", playerByNumber.Number, playerByNumber.Name, playerByNumber.Surname);
            //            var photo = new Telegram.Bot.Types.FileToSend(playerByNumber.Number + ".jpg", (new StreamReader(Path.Combine(DBPlayersPhotoDirPath, playerByNumber.PhotoFile))).BaseStream);

            //            await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription);
            //        }
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //        await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат ввода, введите номер");
            //    }
            //}
            //else
            //{
            //    if (msg.Contains("кто"))
            //    {
            //        chatFinded.WhoMode = true;
            //        await Bot.SendTextMessageAsync(chatFinded.Id, "Вы вошли в режим поиска игрока.");
            //        return;
            //    }

            //    await Bot.SendTextMessageAsync(chatFinded.Id, "Вы можете войти в режим поиска по номеру игрока просто напечатав 'кто', выход из режима таким же образом.");
            //}
        }
    }
}
