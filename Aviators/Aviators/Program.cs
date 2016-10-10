using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Aviators
{
    class Program
    {
        static readonly TelegramBotClient Bot = new TelegramBotClient("272766435:AAH9_EKKEHS9KOMhc1bdXQgHD8BMNY8YNN4");
        static readonly List<Chat> Chats = new List<Chat>();
        static readonly List<Player> Players = new List<Player>();
        static readonly FuckGen Fuck = new FuckGen();
        static readonly string DBPlayersInfoPath = Directory.GetCurrentDirectory() + @"\data_base\PlayersInfo.txt";
        static readonly string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + @"\data_base\PlayersPhoto\";
        static bool End = true;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            LoadPlayers(DBPlayersInfoPath);

            StartBot();
        }

        static void LoadPlayers(string path)
        {
            Console.WriteLine("Try parse " + path);
            using (var sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var fields = line.Split(';');
                    if (fields.Length != 3) continue;

                    Players.Add(new Player(int.Parse(fields[0]),fields[2],fields[1]));
                }
            }
        }

        static void StartBot()
        {
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine("Hello my name is " + me.FirstName);
            Console.WriteLine("Press ctrl+c to kill me.");

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
            var msg = e.Message.Text;
            Console.WriteLine("Incoming request: " + msg);

            if (msg == null) return;

            var cid = e.Message.Chat.Id;

            Console.WriteLine("Search known chat: " + cid);
            var chatFinded = Chats.FindLast(chat => chat.Id == cid);
            if(chatFinded == null)
            {
                chatFinded = new Chat(cid);
                Chats.Add(chatFinded);
            }

            if(chatFinded.WhoMode)
            {
                if (msg.Contains("кто"))
                {
                    chatFinded.WhoMode = false;
                    await Bot.SendTextMessage(chatFinded.Id, "Вы вышли из режима поиска игрока.");
                    return;
                }

                try
                {
                    msg = msg.Trim('/',' ');
                    int playerNumber = int.Parse(msg);
                    //var randomPlayer = Players[(new Random()).Next(Players.Count)];
                    var playerByNumber = Players.FindLast(p => p.Number == playerNumber);

                    if (playerByNumber == null)
                    {
                        await Bot.SendTextMessage(chatFinded.Id, string.Format("Игрок под номером {0} не найден.", playerNumber));
                    }
                    else
                    {
                        var playerDescription = string.Format("Игрок под номером {0}: {1} {2}", playerByNumber.Number, playerByNumber.Name, playerByNumber.Surname);
                        var photo = new Telegram.Bot.Types.FileToSend(playerByNumber.Number + ".jpg", (new StreamReader(Path.Combine(DBPlayersPhotoDirPath, playerByNumber.PhotoFile))).BaseStream);

                        await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Bot.SendTextMessage(chatFinded.Id, "Что-то пошло не так... Сорьки");
                }
            }
            else
            {
                if (msg.Contains("кто"))
                {
                    chatFinded.WhoMode = true;
                    await Bot.SendTextMessage(chatFinded.Id, "Вы вошли в режим поиска игрока.");
                    return;
                }

                await Bot.SendTextMessage(chatFinded.Id, "Вы можете войти в режим поиска по номеру игрока просто напечатав 'кто', выход из режима таким же образом.");
            }

        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("It's the end! Bye.");
            End = false;
            e.Cancel = true;
        }
    }
}
