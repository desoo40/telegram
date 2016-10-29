using System;
using System.IO;
using System.Text.RegularExpressions;
using Telegram.Bot;
using HockeyBot.Configs;
using Telegram.Bot.Types;
using File = System.IO.File;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

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

                //do command
                if (command == "помощь")
                {
                    Help(chatFinded);
                    continue;
                }
                if (command == "игры")
                {
                    Game(chatFinded);
                    continue;
                }
                if (command == "трени")
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

        public async void ContinueWaitingPlayerStatistic(Chat chatFinded, int msgid)
        {
            var stat = chatFinded.WaitingStatistics.FindLast(x => x.Msg.MessageId == msgid);
            if (stat== null) return;

            var statistic = "Статистика:\n\nПривет! Я Статистика. :)";

            await Bot.EditMessageCaptionAsync(chatFinded.Id, msgid, stat.Msg.Caption);
            await Bot.SendTextMessageAsync(chatFinded.Id, statistic, parseMode: ParseMode.Markdown);
            chatFinded.WaitingStatistics.Remove(stat);
        }

        public async void ContinueWaitingEvent(Chat chatFinded, int msgid)
        {
            var even = chatFinded.WaitingEvents.FindLast(x => x.Msg.MessageId == msgid);
            if (even == null) return;

            var more = $"\n\n{even.Even.Address}\n\n{even.Even.Details}";
            var who = $"{even.Even.Members}";

            await Bot.EditMessageTextAsync(chatFinded.Id, msgid, even.Msg.Text + more);
            if (who != "")
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, who, parseMode: ParseMode.Markdown);
            }
            chatFinded.WaitingEvents.Remove(even);
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

                        var button = new InlineKeyboardButton("Cтатистика");
                        var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

                        var msg = await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription, replyMarkup: keyboard);
                        chatFinded.WaitingStatistics.Add(new WaitingStatistic() { Msg = msg, Plr = player });
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

                            var button = new InlineKeyboardButton("Cтатистика");
                            var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

                            var msg = await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription, replyMarkup: keyboard);
                            chatFinded.WaitingStatistics.Add(new WaitingStatistic() { Msg = msg, Plr = player });
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
            try
            {
                var games = DB.GetEventsByType("Игра");
                if (games.Count == 0)
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, "Ближайших игр не найдено.");
                    return;
                }
                else
                {
                    if (games.Count > 1)
                    {
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Ура! По вашему запросу найдено несколько игр, сейчас их все покажу.");
                    }

                    foreach (var game in games)
                    {
                        var button = new InlineKeyboardButton("Подробнее");
                        var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });
                                                
                        var msg = await Bot.SendTextMessageAsync(chatFinded.Id, $"{game.Date} {game.Time}\n{game.Place}", replyMarkup: keyboard);
                        chatFinded.WaitingEvents.Add(new WaitingEvent() { Msg = msg, Even = game });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(chatFinded.Id, "Ваш запрос не удалось обработать.");
            }
        }

        private async void Training(Chat chatFinded)
        {
            try
            {
                var games = DB.GetEventsByType("Треня");
                if (games.Count == 0)
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, "Ближайших трень не найдено.");
                    return;
                }
                else
                {                   
                    foreach (var game in games)
                    {
                        var button = new InlineKeyboardButton("Подробнее");
                        var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

                        var msg = await Bot.SendTextMessageAsync(chatFinded.Id, $"{game.Date} {game.Time}\n{game.Place}", replyMarkup: keyboard);
                        chatFinded.WaitingEvents.Add(new WaitingEvent() { Msg = msg, Even = game });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(chatFinded.Id, "Ваш запрос не удалось обработать.");
            }
        }

        private async void StatisticByNumber(Chat chatFinded, int number)
        {
            chatFinded.PersonalStatMode = false;
            var result = "Игрок не найден";
            var player = DB.GetPlayerStatisticByNumber(number);
            if (player != null)
            {
                result = "Пока не закодили :(";
            }

            await Bot.SendTextMessageAsync(chatFinded.Id, result);
        }

        private async void Help(Chat chatFinded)
        {
            var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup();
            keys.Keyboard = new Telegram.Bot.Types.KeyboardButton[3][];
            keys.OneTimeKeyboard = true;

            var p = DB.GetAllPlayerWitoutStatistic();
            var num = p[(new Random()).Next(p.Count - 1)].Number;
            var name = p[(new Random()).Next(p.Count - 1)].Number;
            var surname = p[(new Random()).Next(p.Count - 1)].Surname;

            keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + num),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + surname) };
            keys.Keyboard[1] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "трени"),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "игры") };
            keys.Keyboard[2] = new Telegram.Bot.Types.KeyboardButton[2] {
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "кричалки"),
                new Telegram.Bot.Types.KeyboardButton(chatFinded.Id > 0 ? "" : "/" + "помощь") };

            var help =
@"Бот умеет:

Поискать игрока по
№|'имени'|'фамилии'
'%номер%'|'%имя%'|'%фамилия%'

Показать
/игры
/трени
/кричалки

/помощь";

            help = help.Replace("'%номер%'", $"{num}");
            help = help.Replace("'%имя%'", $"{name}");
            help = help.Replace("'%фамилия%'", $"{surname}");

            await Bot.SendTextMessageAsync(chatFinded.Id, help, false, false, 0, keys);
        }
    }
}
