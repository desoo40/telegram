using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyBot
{
    class Utils
    {
        static readonly List<Player> Players = new List<Player>();
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

                    Players.Add(new Player(int.Parse(fields[0]), fields[2], fields[1]));
                }
            }
        }
        static void Commands()
        {
            if (command == "бомбардиры")
            {
                chatFinded.Stat.Bomb = true;
                if (isLastCommand)
                {
                    TourAnswer(chatFinded);
                }
                continue;
            }

            if (command == "снайперы")
            {
                chatFinded.Stat.Snip = true;
                if (isLastCommand)
                {
                    TourAnswer(chatFinded);
                }
                continue;
            }

            if (command == "асистенты")
            {
                chatFinded.Stat.Asist = true;
                if (isLastCommand)
                {
                    TourAnswer(chatFinded);
                }
                continue;
            }

            if (command == "штрафники")
            {
                chatFinded.Stat.BadBoy = true;
                if (isLastCommand)
                {
                    TourAnswer(chatFinded);
                }
                continue;
            }

            if (command == "полезность")
            {
                chatFinded.Stat.Usefull = true;
                if (isLastCommand)
                {
                    TourAnswer(chatFinded);
                }
                continue;
            }
        }

        private async void TourAnswer(Chat chatFinded)
        {
            var tours = DB.GetTournaments();

            var rowCount = tours.Length % 2 == 0 ? tours.Length / 2 : tours.Length / 2 + 1;
            ++rowCount; // ибо "официальные" и "все"

            var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            {
                Keyboard = new Telegram.Bot.Types.KeyboardButton[rowCount][],
                OneTimeKeyboard = true
            };


            keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("Все"),
                                                                          new Telegram.Bot.Types.KeyboardButton("Официальные") }; // помнить о слешах
            for (var i = 0; i < tours.Length; ++i)
            {
                var row = i / 2 + 1;
                var column = i % 2;

                if (keys.Keyboard[row] == null)
                {
                    var isLast = (tours.Length - i - 1 == 0);
                    var c = isLast ? 1 : 2;

                    keys.Keyboard[row] = new KeyboardButton[c];
                }
                keys.Keyboard[row][column] = new KeyboardButton(chatFinded.Id > 0 ? tours[i] : "/" + tours[i]);
            }

            await Bot.SendTextMessageAsync(chatFinded.Id, "Выберете турнир:", false, false, 0, keys);
        }

    }
}
