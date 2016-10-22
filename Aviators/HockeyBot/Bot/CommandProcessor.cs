using System;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.Bot;
using HockeyBot.Configs;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace HockeyBot
{
    /// <summary>
    /// Тут обрабатываются команды для бота
    /// </summary
    public class CommandProcessor
    {
        private TelegramBotClient Bot;
        private readonly Randomiser Gen;
        private DBCore DB;

        public CommandProcessor(TelegramBotClient bot)
        {
            Bot = bot;
            Gen = new Randomiser();
            DB = new DBCore();
        }

        public void FindCommands(string msg, Chat chatFinded, int fromId)
        {
            var commands = msg.Split(' ');
            foreach (var command in commands)
            {
                chatFinded.CommandsQueue.Enqueue(command);
            }

            ProcessCommands(chatFinded, fromId);            
        }

        private async void ProcessCommands(Chat chatFinded, int fromId)
        {
            var commands = chatFinded.CommandsQueue;
            var rxNums = new Regex(@"^\d+$"); // проверка на число

            while (commands.Count > 0)
            {
                var command = commands.Dequeue();
                var isLastCommand = (commands.Count == 0);                

                //set modes
                if (command == "add")
                {
                    if (!Config.BotAdmin.isAdmin(fromId))
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой add. Запрос отменён.");
                        chatFinded.ResetMode();
                        continue;
                    }

                    chatFinded.AddMode = true;
                    if (isLastCommand)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Добавьте игрока в формате '99;Имя;Фамилия'");
                    }
                    continue;
                }

                if (command == "remove")
                {
                    if (!Config.BotAdmin.isAdmin(fromId))
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой remove. Запрос отменён.");
                        chatFinded.ResetMode();
                        continue;
                    }

                    chatFinded.RemoveMode = true;
                    if (isLastCommand)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Удалите игрока по 'номеру'");
                    }
                    continue;
                }

                if (command == "статистика")
                {
                    chatFinded.PersonalStatMode = true;
                    if (isLastCommand)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Введите номер или фамилию игрока"); // рассмотреть возможность однофамильцев
                    }
                    continue;
                }

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

                //check modes
                if (chatFinded.AddMode)
                {
                    AddPlayer(chatFinded, command);
                    continue;
                }

                if (chatFinded.RemoveMode)
                {
                    try
                    {
                        var number = int.Parse(command);
                        RemovePlayer(chatFinded, number);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        ExceptionOnCmd(chatFinded, ex);
                        continue;
                    }
                }

                if (chatFinded.PersonalStatMode)
                {
                    if (rxNums.IsMatch(command))
                    {
                        //в случае числа показываем стату
                        try
                        {
                            var number = int.Parse(command);
                            StatisticByNumber(chatFinded, number);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            ExceptionOnCmd(chatFinded, ex);
                            continue;
                        }
                    }
                    else
                    {
                        //в случае букв ищем по имени или фамилии
                        try
                        {
                            StatisticByNameOrSurname(chatFinded, command);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            ExceptionOnCmd(chatFinded, ex);
                            continue;
                        }
                    }
                }

                //do command
                if (command == "помощь")
                {
                    Help(chatFinded);
                    continue;
                }
                if (command == "расписание")
                {
                    TimeTable(chatFinded, 0);
                    continue;
                }
                if (command == "следующая")
                {
                    NextGame(chatFinded);
                    continue;
                }
                if (command == "соперник")
                {
                    EnemyTeam(chatFinded, "соперник");
                    continue;
                }
                if (command == "кричалки")
                {
                    Slogans(chatFinded);
                    continue;
                }

                //если не в режиме, не установили режим, не выполнили команду сразу, может пользователь ввёл число для поиска игрока
                if (rxNums.IsMatch(command))
                {
                    //в случае числа показываем игрока
                    try
                    {
                        var number = int.Parse(command);
                        ShowPlayerByNubmer(chatFinded, number);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        ExceptionOnCmd(chatFinded, ex);
                        continue;
                    }
                }

                //иначе пользователь ввёл хуйню
                WrongCmd(chatFinded);
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
                var row = i/2 + 1;
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

        private async void WrongCmd(Chat chatFinded)
        {
            chatFinded.ResetMode();
            var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
            keys.Keyboard = new Telegram.Bot.Types.KeyboardButton[1][];
            keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[1] { new Telegram.Bot.Types.KeyboardButton("/помощь") };
            keys.ResizeKeyboard = true;
            keys.OneTimeKeyboard = true;
            await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный запрос, воспользуйтесь /помощь", false, false, 0, keys);
        }

        private async void ExceptionOnCmd(Chat chatFinded, Exception ex)
        {
            chatFinded.ResetMode();
            Console.WriteLine(ex.Message);
            await Bot.SendTextMessageAsync(chatFinded.Id, "Ваш запрос не удалось обработать. Запрос отменён.");
        }

        private async void AddPlayer(Chat chatFinded, string argv)
        {
            //argv format is number;name;surname
            chatFinded.AddMode = false;
            var playerinfo = argv.Split(';');
            if (playerinfo.Length == 3)
            {
                var player = new Player(int.Parse(playerinfo[0]), playerinfo[1].Trim(), playerinfo[2].Trim());
                DB.AddPlayer(player);
                await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали добавить {player.Number}.");
            }
            else
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, $"Неверный формат запроса: {argv}");
            }
        }

        private async void RemovePlayer(Chat chatFinded, int number)
        {
            chatFinded.RemoveMode = false;
            DB.RemovePlayerByNumber(number);
            await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали удалить {number}, проверим успешность поиском.");
            ShowPlayerByNubmer(chatFinded, number);
        }

        private async void ShowPlayerByNubmer(Chat chatFinded, int playerNumber)
        {
            if (playerNumber < 0 || playerNumber > 100)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат, введите корректный номер игрока от 0 до 100.");
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

        private async void Slogans(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, Gen.GetSlogan());
        }

        private async void EnemyTeam(Chat chatFinded, string team)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я соперник");
        }

        private async void NextGame(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я следующая игра");
        }

        private async void TimeTable(Chat chatFinded, int n)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я расписание");
        }

        private async void StatisticByNumber(Chat chatFinded, int number)
        {
            chatFinded.PersonalStatMode = false;
            var result = "Игрок не найден";
            var player = DB.GetPlayerStatisticByNumber(number);
            if (player != null)
            {
                result = $"{player.Surname} забросил {player.Goals} шайб";
            }

            await Bot.SendTextMessageAsync(chatFinded.Id, result);
        }

        private async void StatisticByNameOrSurname(Chat chatFinded, string nameOrSurname)
        {
            chatFinded.PersonalStatMode = false;
            var result = "Игрок не найден";
            var player = DB.GetPlayerStatisticByNameOrSurname(nameOrSurname);
            if (player != null)
            {
                result = $"{player.Surname} забросил {player.Goals} шайб";
            }

            await Bot.SendTextMessageAsync(chatFinded.Id, result);
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

            var help = 
@"Управляй мною:
(в личке можно без /)

'%номер%' - поиск игрока по номеру

/статистика '№'|'фамилия' игрока

/бомбардиры
/снайперы
/асистенты
/штрафники
/полезность (+/- показатель)

- топ игроков команды по параметрам


/расписание 'n' ближайших n игр

/следующая игра: дата, время, соперник и место

/соперник 'команда' - история встреч

/кричалки - выводит одну из кричалок команды

/помощь - помощь по управлению";

            help = help.Replace("'%номер%'", $"{n}");

            await Bot.SendTextMessageAsync(chatFinded.Id, help, false, false, 0, keys);
        }
    }
}
