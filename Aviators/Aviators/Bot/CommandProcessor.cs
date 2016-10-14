using System;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Aviators.Configs;

namespace Aviators
{
    /// <summary>
    /// Тут обрабатываются команды для бота
    /// </summary
    public class CommandProcessor
    {
        private TelegramBotClient Bot;
        readonly Randomiser Gen;
        DBCore DB;

        public CommandProcessor(TelegramBotClient bot)
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

                    var photopath = Path.Combine(Config.DBPlayersPhotoDirPath, player.PhotoFile);

                    Console.WriteLine($"Send player:{player.Surname}");
                    if (File.Exists(photopath))
                    {
                        var photo = new Telegram.Bot.Types.FileToSend(player.Number + ".jpg",
                            (new StreamReader(photopath)).BaseStream);
                        await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription);
                    }
                    else
                    {
                        Console.WriteLine($"Photo file {photopath} not found.");
                        await Bot.SendTextMessageAsync(chatFinded.Id, playerDescription);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(chatFinded.Id, "Больше так не делай, мне больно(");

            }
        }

        public async void FindCommand(string msg, Chat chatFinded, int fromId)
        {
            if (chatFinded.AddMode)
            {
                chatFinded.AddMode = false;
                DB.AddPlayerFromMsg(msg);
                await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали добавить {msg}.");
                return;
            }

            Regex rxNums = new Regex(@"^\d+$"); // делаем проверку на число
            if (rxNums.IsMatch(msg))
            {
                if (chatFinded.StatMode)
                {
                    chatFinded.StatMode = false;
                    var request = new string[] { "статистика", msg };
                    Statistic(chatFinded, request);
                    return;
                }
                if (chatFinded.RemoveMode)
                {
                    chatFinded.RemoveMode = false;
                    DB.RemovePlayer(int.Parse(msg));
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали удалить {int.Parse(msg)}, проверим успешность поиском.");
                    ShowPlayerByNubmer(msg, chatFinded);                    
                    return;
                }

                ShowPlayerByNubmer(msg, chatFinded);
            }
            else
            {
                msg = msg.ToLower();
                var fields = msg.Split(' ');
                var flenght = fields.Length;
                
                if (fields[0] == "помощь")
                {
                    Help(chatFinded);
                    chatFinded.ResetMode();
                    return;
                }
                if (fields[0] == "статистика")
                {
                    if(flenght == 2)
                    {
                        Statistic(chatFinded, fields);
                        chatFinded.ResetMode();
                        return;
                    }
                    if (flenght == 1 && !chatFinded.StatMode)
                    {
                        chatFinded.StatMode = true;
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Введите номер игрока, например, /10");
                        return;
                    }
                }
                if (flenght == 2 && fields[0] == "расписание")
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
                    chatFinded.ResetMode();
                    return;
                }
                if (fields[0] == "add")
                {
                    if(!Config.BotAdmin.isAdmin(fromId))
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться этой командой.");
                        chatFinded.ResetMode();
                        return;
                    }
                    if (flenght == 1 && !chatFinded.StatMode)
                    {
                        chatFinded.AddMode = true;
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Добавить игрока в формате '99;Залупа;Чистая'");
                        return;
                    }                    
                }
                if (fields[0] == "remove")
                {
                    if (!Config.BotAdmin.isAdmin(fromId))
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться этой командой.");
                        chatFinded.ResetMode();
                        return;
                    }
                    if (flenght == 1 && !chatFinded.StatMode)
                    {
                        chatFinded.RemoveMode = true;
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Удалить игрока по номеру");
                        return;
                    }
                }

                chatFinded.ResetMode();
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
