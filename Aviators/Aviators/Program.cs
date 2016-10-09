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
        static readonly TelegramBotClient Bot = new TelegramBotClient("297610365:AAG3yzYtC0XgLrQC0ong1qdJ5odMkqiGHno");
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
            Console.WriteLine("Incoming request: " + e.Message.Text);

            var cid = e.Message.Chat.Id;
            var randomPlayer = Players[(new Random()).Next(Players.Count)];
            var playerDescription = string.Format("Игрок под номером {0}: {1} {2}", randomPlayer.Number, randomPlayer.Name, randomPlayer.Surname);
            var photo = new Telegram.Bot.Types.FileToSend(randomPlayer.Number + ".jpg", (new StreamReader(Path.Combine(DBPlayersPhotoDirPath, randomPlayer.PhotoFile))).BaseStream);

            //await Bot.SendTextMessage(cid, playerDescription);
            await Bot.SendPhoto(cid, photo, playerDescription);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("It's the end! Bye.");
            End = false;
            e.Cancel = true;
        }
    }
}
