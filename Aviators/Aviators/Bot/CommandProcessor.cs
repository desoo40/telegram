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

        public async void ShowPlayerByNubmer(int playerNumber, Chat chatFinded)
        {
            if (playerNumber < 0 || playerNumber > 100)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат, введите корректный номер игрока.");
                return;
            }

            try
            {
                var player = DB.GetPlayerByNumber(playerNumber);
                if (player == null)
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Игрок под номером {playerNumber} не найден.");
                }
                else
                {
                    var playerDescription = Gen.GetPlayerDescr();
                    playerDescription += $"#{player.Number} {player.Name} {player.Surname}";

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
                await Bot.SendTextMessageAsync(chatFinded.Id, "Ваш запрос не удалось обработать.");

            }
        }

        public async void FindCommand(string msg, Chat chatFinded, int fromId)
        {
            if (chatFinded.AddMode)
            {
                chatFinded.AddMode = false;
                var playerinfo = msg.Split(';');
                if (playerinfo.Length == 3)
                {
                    var player = new Player(int.Parse(playerinfo[0]), playerinfo[1].Trim(), playerinfo[2].Trim());
                    DB.AddPlayer(player);
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали добавить {player.Number}.");
                    return;
                }
                else
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Неверный формат запроса: {msg}");
                    return;
                }
            }

            Regex rxNums = new Regex(@"^\d+$"); // делаем проверку на число
            if (rxNums.IsMatch(msg))
            {
                var number = int.Parse(msg);
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
                    DB.RemovePlayerByNumber(number);
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали удалить {number}, проверим успешность поиском.");
                    ShowPlayerByNubmer(number, chatFinded);                    
                    return;
                }

                ShowPlayerByNubmer(number, chatFinded);
            }
            else
            {
                msg = msg.ToLower();
                var commands = msg.Split(' ');

                foreach (var command in commands)
                {
                    chatFinded.CommandsQueue.Enqueue(command);
                }

                var first = chatFinded.CommandsQueue.Dequeue();

                if (first == "помощь")
                {
                    Help(chatFinded);
                    chatFinded.ResetMode();
                    return;
                }
                if (first == "статистика")
                {
                    chatFinded.StatMode = true;

                    if (chatFinded.CommandsQueue.Count == 0)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id,
                            "Введите:\n\n" +
                            "/топ - топ игроков\n" +
                            "фамилию или номер игрока для просмотра его статистики");
                    }
                }

                if (chatFinded.StatMode)
                {
                    


                    var currСommand = chatFinded.CommandsQueue.Dequeue();

                }

                if (first == "расписание")
                {
                    //TimeTable(chatFinded, fields);
                    return;
                }
                if (first == "следующая")
                {
                    NextGame(chatFinded);
                    return;
                }
                if (first == "соперник")
                {
                    //EnemyTeam(chatFinded, fields);
                    return;
                }
                if (first == "кричалки")
                {
                    Slogans(chatFinded);
                    chatFinded.ResetMode();
                    return;
                }
                if (first == "add")
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
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Добавить игрока в формате '99;Имя;Фамилия'");
                        return;
                    }                    
                }
                if (first == "remove")
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
                var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
                keys.Keyboard = new Telegram.Bot.Types.KeyboardButton[1][];
                keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[1] { new Telegram.Bot.Types.KeyboardButton("/помощь") };
                keys.ResizeKeyboard = true;
                keys.OneTimeKeyboard = true;
                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверная команда, воспользуйтесь /помощь",false,false,0,keys);
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
            string outpStr ="";

            if (fields[1] == "топчик")
            {
                var count = 5;
                if (fields.Length > 2)
                {
                    try
                    {
                        count = Convert.ToInt32(fields[2]);
                    }
                    catch (Exception)
                    {
                        outpStr = "Неверно ввели количество\r\n";
                    }
                }

                var players = DB.GetTopPlayers(count);
                foreach (var player in players)
                {
                    if (player != null)
                        outpStr+= $"{player.Surname} забросил {player.Goals} шайб\r\n";
                }
            }
            else
            {
                var player = DB.GetPlayerStatistic(fields[1]);
                if (player == null) outpStr = "Игрок не найден";
                else outpStr = $"{player.Surname} забросил {player.Goals} шайб";
            }
           

            await Bot.SendTextMessageAsync(chatFinded.Id, outpStr);
        }

        private async void Help(Chat chatFinded)
        {
            var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
            keys.Keyboard = new Telegram.Bot.Types.KeyboardButton[4][];
            keys.OneTimeKeyboard = true;

            var p = DB.GetAllPlayerWitoutStatistic();
            var n = p[(new Random()).Next(p.Count - 1)].Number;

            if (chatFinded.Id > 0)
            {
                keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("" + n), new Telegram.Bot.Types.KeyboardButton("Статистика") };
                keys.Keyboard[1] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("Расписание"), new Telegram.Bot.Types.KeyboardButton("Следующая") };
                keys.Keyboard[2] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("Соперник"), new Telegram.Bot.Types.KeyboardButton("Кричалки") };
                keys.Keyboard[3] = new Telegram.Bot.Types.KeyboardButton[1] { new Telegram.Bot.Types.KeyboardButton("Помощь") };
            }
            else
            {
                keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("/" + n), new Telegram.Bot.Types.KeyboardButton("/статистика") };
                keys.Keyboard[1] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("/расписание"), new Telegram.Bot.Types.KeyboardButton("/следующая") };
                keys.Keyboard[2] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("/соперник"), new Telegram.Bot.Types.KeyboardButton("/кричалки") };
                keys.Keyboard[3] = new Telegram.Bot.Types.KeyboardButton[1] { new Telegram.Bot.Types.KeyboardButton("/помощь") };
            }

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
                "/помощь - помощь по управлению\n\n", false, false, 0, keys);
        }
    }
}
