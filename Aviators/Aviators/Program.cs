using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Aviators
{
    class Program
    {
        //АВИАТОРЫ
        static readonly TelegramBotClient Bot = new TelegramBotClient("272766435:AAH9_EKKEHS9KOMhc1bdXQgHD8BMNY8YNN4");
        //БОРИН БОТ
        //static readonly TelegramBotClient Bot = new TelegramBotClient("124248191:AAGDONDKlfyU1R0bv3MqWRYbvZQJiSJycm8");
        //ДЕНИСА БОТ
        //static readonly TelegramBotClient Bot = new TelegramBotClient("297610365:AAEflHFUSK87OiCmjjS4H05D_FDtN57ijLY");

        static readonly List<Player> Players = new List<Player>();
        static readonly List<Chat> Chats = new List<Chat>();
        static readonly string DBPlayersInfoPath = Directory.GetCurrentDirectory() + @"/data_base/PlayersInfo.txt";
        static bool End = true;

        //Флаг для создания базы и табличек, что бы из кода.
        //Можно так же аргумент в свойствах проекта прописывать, но неудобно
        private static bool InitFromCode = true;

        /// <summary>
        /// Тут будут храниться команды для бота
        /// </summary>
        private static Commands Commands;

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static void Main(string[] args)
        {
            //to ignore untrusted SSL certificates, linux and mono love it ;)
            ServicePointManager.ServerCertificateValidationCallback = Validator;
            Console.CancelKeyPress += Console_CancelKeyPress;

            if (InitFromCode || args.Length > 0 && args[0] == "init")
            {
                Console.WriteLine("Start Initializate");
                DBCore db = new DBCore();

                Console.WriteLine("CreateDB");
                db.CreateDefaultDB();

                Console.WriteLine("FillPlayersFromFile");
                db.LoadPlayersFromFile();
                //Console.WriteLine("FillPlayersFromFile");
                //db.LoadTeamsFromFile();

                db.Disconnect();
                Console.WriteLine("Finish Initializate");
            }

            //Console.WriteLine("Loading players...");
            //LoadPlayers(DBPlayersInfoPath);
            Commands = new Commands(Bot);

            Console.WriteLine("Starting Bot...");
            StartBot();
        }

        //static void LoadPlayers(string path)
        //{
        //    Console.WriteLine("Try parse " + path);
        //    using (var sr = new StreamReader(path))
        //    {
        //        while (!sr.EndOfStream)
        //        {
        //            var line = sr.ReadLine();
        //            var fields = line.Split(';');
        //            if (fields.Length != 3) continue;

        //            Players.Add(new Player(int.Parse(fields[0]), fields[2], fields[1]));
        //        }
        //    }
        //}

        static void StartBot()
        {
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
            //Console.WriteLine("Bot_OnMessage...");

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

            modeCode();
        }


        

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("It's the end! Bye.");
            End = false;
            if (!End) e.Cancel = true;
        }

        private static void modeCode()
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


