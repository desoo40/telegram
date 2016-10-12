using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Aviators
{
    public class Commands
    {
        private TelegramBotClient Bot;
        readonly Randomiser Gen;
        DBCore DB;

        string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + @"/data_base/PlayersPhoto/";

        public Commands(TelegramBotClient bot)
        {
            Bot = bot;
            Gen = new Randomiser();
            DB = new DBCore();
        }

        public async void ShowPlayerByNubmer(string msg, Chat chatFinded)
        {
            try
            {
                int playerNumber = int.Parse(msg);

                if (playerNumber < 0 || playerNumber > 100)
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат, введите корректный номер игрока.");
                    return;
                }

                //var playerByNumber = Players.FindLast(p => p.Number == playerNumber);
                var player = DB.GetPlayerByNumber(playerNumber);


                if (player == null)
                {
                    await
                        Bot.SendTextMessageAsync(chatFinded.Id,
                            string.Format("Игрок под номером {0} не найден.", playerNumber));
                }
                else
                {
                    var playerDescription = Gen.GetPlayerDescr();
                    playerDescription += string.Format("#{0} {1} {2}", player.Number, player.Name,
                        player.Surname);
                    var photo = new Telegram.Bot.Types.FileToSend(player.Number + ".jpg",
                        (new StreamReader(Path.Combine(DBPlayersPhotoDirPath, player.PhotoFile))).BaseStream);

                    Console.WriteLine($"Send player:{player.Surname}");
                    
                    await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(chatFinded.Id, "Больше так не делай, мне больно(");

            }
        }

        public async void FindCommand(string msg, Chat chatFinded)
        {
            Regex rxNums = new Regex(@"^\d+$"); // делаем проверку на число
            if (rxNums.IsMatch(msg))
            {
                ShowPlayerByNubmer(msg, chatFinded);
            }
            else
            {
                msg = msg.ToLower();
                var fields = msg.Split(' ');

                if (fields[0] == "помощь")
                {
                    Help(chatFinded);
                    return;
                }
                if (fields[0] == "статистика")
                {
                    Statistic(chatFinded, fields);
                    return;
                }
                if (fields[0] == "расписание")
                {
                    TimeTable(chatFinded, fields);
                    return;
                }
                if (fields[0] == "следующая")
                {
                    NextGame(chatFinded);
                    return;
                }
                if (fields[0] == "соперник")
                {
                    EnemyTeam(chatFinded, fields);
                    return;
                }
                if (fields[0] == "кричалки")
                {
                    Slogans(chatFinded);
                    return;
                }

                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверная команда, воспользуйтесь /помощь");
            }
        }

        private async void Slogans(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, Gen.GetSlogan());
        }

        private async void EnemyTeam(Chat chatFinded, string[] fields)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я соперник");
        }

        private async void NextGame(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я следующая игра");
        }

        private async void TimeTable(Chat chatFinded, string[] fields)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я расписание");
        }

        private async void Statistic(Chat chatFinded, string[] fields)
        {
            var player = DB.GetPlayerStatistic(fields[1]);
            string outpStr;
            if (player == null) outpStr = "Игрок не найден";
            else outpStr = $"{player.Surname} забросил {player.Goals} шайб";

            await Bot.SendTextMessageAsync(chatFinded.Id, outpStr);
        }

        public async void Help(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id,
                "Мною можно управлять с помощью команд:\n" +
                "(можно вводить без '/') \n" +
                "\n" +
                "'номер' - поиск игрока по номеру\n\n" +
                "/статистика 'номер' - статистика игрока\n\n" +
                "/статистика 'фамилия' - можно также через фамилию\n\n" +
                "/расписание 'n' - расписание ближайших n игр\n\n" +
                "/следующая - дата, время, соперник и место следующей игры\n\n" +
                "/соперник 'название команды' - история встреч\n\n" +
                "/кричалки - выводит одну из кричалок команды\n\n" +
                "/помощь - помощь по управлению\n\n");
        }
    }
}
