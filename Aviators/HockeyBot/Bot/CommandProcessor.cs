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

        public async void FindCommands(string msg, Chat chatFinded, int fromId)
        {
            var commands = msg.Split(' ');

            if(commands.Length > 10)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Сорри, но мне лень обрабатывать столько команд.");
                return;
            }

            if(chatFinded.CommandsQueue.Count > 20)
            {
                Console.WriteLine("Too bug queue. Reset it.");
                chatFinded.CommandsQueue.Clear();
                return;
            }

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
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Введите номер игрока");
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
                }

                //do command
                if (command == "помощь")
                {
                    Help(chatFinded);
                    continue;
                }
                if (command == "игра")
                {
                    Game(chatFinded);
                    continue;
                }
                if (command == "треня")
                {
                    Training(chatFinded);
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

                if (isLastCommand)
                {
                    //в случае букв ищем по имени или фамилии 
                    try
                    {
                        ShowPlayersByNameOrSurname(chatFinded, command);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        ExceptionOnCmd(chatFinded, ex);
                        continue;
                    }
                }                
            }
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

        private async void ShowPlayersByNameOrSurname(Chat chatFinded, string nameOrSurname)
        {
            try
            {
                var players = DB.GetPlayersByNameOrSurname(nameOrSurname);
                if (players.Count == 0)
                {
                    //иначе пользователь ввёл хуйню
                    WrongCmd(chatFinded);
                    return;
                }
                else
                {
                    if(players.Count > 1)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "По вашему запросу найдено несколько игроков, сейчас их покажу.");
                    }

                    foreach (var player in players)
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

        private async void Game(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я ближайшая игра");
        }

        private async void Training(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, мы тренировки");
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

        private async void Help(Chat chatFinded)
        {
            var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
            keys.Keyboard = new Telegram.Bot.Types.KeyboardButton[3][];
            keys.OneTimeKeyboard = true;

            var p = DB.GetAllPlayerWitoutStatistic();
            var n = p[(new Random()).Next(p.Count - 1)].Number;

            keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + n),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "статистика") };
            keys.Keyboard[1] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "треня"),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "игра") };
            keys.Keyboard[2] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "кричалки"),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "помощь") };

            var help =
@"Управляй мною:

'%номер%' 
'имя'
'фамилия' - поиск игрока

/треня - встречи на неделе
/игра

/статистика '№'|'фамилия'

/кричалки для бодрости духа!

/помощь";

            help = help.Replace("'%номер%'", $"{n}");

            await Bot.SendTextMessageAsync(chatFinded.Id, help, false, false, 0, keys);
        }
    }
}
