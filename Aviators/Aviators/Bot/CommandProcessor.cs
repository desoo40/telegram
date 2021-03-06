﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aviators.Bot;
using Telegram.Bot;
using Aviators.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using Message = Telegram.Bot.Types.Message;

namespace Aviators
{
    /// <summary>
    /// Тут обрабатываются команды для бота
    /// </summary
    public class CommandProcessor
    {
        private TelegramBotClient Bot;
        private readonly Randomiser Gen;
        private ImageGenerator ImageGen;
        Regex rxNums = new Regex(@"^\d+$"); // проверка на число


        public CommandProcessor(TelegramBotClient bot)
        {
            Bot = bot;
            Gen = new Randomiser();
            ImageGen = new ImageGenerator();
        }

        public async void FindCommands(string msg, Chat chatFinded, int fromId)
        {
            var inputCommands = msg.Split(' ');

            var command = new Command(inputCommands);//сама команда молодости нашей

            if (rxNums.IsMatch(command.Name))
            {
                //в случае числа показываем игрока
                //var number = int.Parse(command.Name);
                ShowPlayerByNubmer(chatFinded, command);
                return;
            }

            switch (command.Name)
            {
                case "помощь":
                    Help(chatFinded);
                    return;

                case "start":
                    Help(chatFinded);
                    return;

                case "статистика":
                    if (string.IsNullOrEmpty(command.Argument))
                        break;
                    if (command.Argument == "команда")
                        GamesStatistic(chatFinded);
                    else
                        PlayerStatistic(chatFinded, command.Argument);
                    return;
                    
                case "расписание":
                    TimeTable(chatFinded, 0);
                    return;

                case "следующая":
                    NextGame(chatFinded);
                    return;

                case "последняя":
                    LastGame(chatFinded);
                    return;

                case "всеигры":
                    GetAllGames(chatFinded);
                    return;

                case "турнир":
                    TournametList(chatFinded, command);
                    return;
                case "сезон":
                    SeasonList(chatFinded, command);
                    return;

                case "соперник":
                    if (string.IsNullOrEmpty(command.Argument)) TeamList(chatFinded, command);
                    else EnemyTeam(chatFinded, command);
                    return;

                case "кричалки":
                    Slogans(chatFinded);
                    return;

                case "бомбардиры":
                    Top(chatFinded, Aviators.Top.Points);
                    return;
                case "снайперы":
                    Top(chatFinded, Aviators.Top.Goals);
                    return;
                case "ассистенты":
                    Top(chatFinded, Aviators.Top.Assist);
                    return;
                case "штрафники":
                    Top(chatFinded, Aviators.Top.Penalty);
                    return;
                case "полезность":
                    Top(chatFinded, Aviators.Top.PlusMinus);
                    return;
                case "среднее":
                    Top(chatFinded, Aviators.Top.APG);
                    return;

            }

            //ProcessCommands(chatFinded, fromId);            
        }

        public async void ContinueCommand(Chat chatFinded, CallbackQuery cQuery)
        {
            var msgid = cQuery.Message.MessageId;

            var command = chatFinded.WaitingCommands.LastOrDefault(m => m.Message.MessageId == msgid);
            if(command == null) return;

            if (command.Name == "соперник")
            {
                if (string.IsNullOrEmpty(command.Argument))
                {
                    command.ListArguments.Add(cQuery.Data);
                    EnemyTeam(chatFinded, command);
                }
                else
                {
                    SendGameStat(chatFinded, DB.DBCommands.DBGame.GetGame(Convert.ToInt32(cQuery.Data)));
                }
            }

            if (command.Name == "статистика")
            {
                var statistic = GetPlayerStatistic(chatFinded, command.Argument).Replace("*", "");

                await Bot.EditMessageCaptionAsync(chatFinded.Id, msgid, statistic);
            }

            if (command.Name == "игрок")
            {
                var player = DB.DBCommands.DBPlayer.GetPlayerById(Convert.ToInt32(cQuery.Data));
                SendPlayer(chatFinded, command, player);
            }

            if (command.Name == "состав")
            {
                SendGameSostav(chatFinded, Convert.ToInt32(command.Argument));
            }

            if (command.Name == "топ")
            {
                var count = Convert.ToInt32(command.Argument) + 5;

                string result = 
                    GetTop(chatFinded, (Top)(Convert.ToInt32(cQuery.Data)), count);

                if (result == "")
                {
                    await Bot.EditMessageTextAsync(chatFinded.Id, msgid, command.Message.Text, parseMode: ParseMode.Markdown);
                    return;
                }
                
                var button = new InlineKeyboardButton("Еще");
                button.CallbackData = cQuery.Data;
                var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

                Message mes;

                //TODO переделать по нормальному
                try
                {
                    mes = await Bot.EditMessageTextAsync(chatFinded.Id, msgid, result, parseMode: ParseMode.Markdown, replyMarkup: keyboard);

                }
                catch (Exception)
                {
                    
                    Console.WriteLine("Ошибка отправки сообщения");
                    return;
                }

                var newCom = new Command(new[] { "топ", count.ToString() });
                newCom.Message = mes;
                chatFinded.WaitingCommands.Add(newCom);
            }

            if (command.Name == "турнир")
            {
                var id = Convert.ToInt32(cQuery.Data);
                if (id == 0)
                {
                    chatFinded.Tournament = null;
                    await Bot.SendTextMessageAsync(chatFinded.Id, "Вы выбрали все турниры");
                }
                else
                {
                    chatFinded.Tournament = DB.DBCommands.GetTournament(Convert.ToInt32(cQuery.Data));

                    if (chatFinded.Tournament == null)
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Не удалось установить турнир");
                    else
                        await Bot.SendTextMessageAsync(chatFinded.Id,
                            $"Для вас установлен турнир *{chatFinded.Tournament.Name}*. Вся статистика будет для этого турнира. Для сброса наберите /сброс",
                            parseMode: ParseMode.Markdown);
                }

                DB.DBCommands.UpdateChatParams(chatFinded);
            }

            if (command.Name == "сезон")
            {

                var id = Convert.ToInt32(cQuery.Data);
                if (id == 0)
                {
                    chatFinded.Season = null;
                    await Bot.SendTextMessageAsync(chatFinded.Id, "Вы выбрали все сезоны");
                }
                else
                {
                    chatFinded.Season = DB.DBCommands.GetSeason(Convert.ToInt32(cQuery.Data));

                    if (chatFinded.Season == null)
                        await Bot.SendTextMessageAsync(chatFinded.Id, "Не удалось установить сезон");
                    else
                        await Bot.SendTextMessageAsync(chatFinded.Id,
                            $"Для вас установлен сезон *{chatFinded.Season.Name}*. Вся статистика будет для этого сезона. Для сброса наберите /сброс",
                            parseMode: ParseMode.Markdown);
                }

                DB.DBCommands.UpdateChatParams(chatFinded);
            }

            //chatFinded.WaitingCommands.Remove(command);
        }

        #region неиспользумое

        //private async void ProcessCommands(Chat chatFinded, int fromId)
        //{
        //    //var commands = chatFinded.CommandsQueue;

        //    //while (commands.Count > 0)
        //    //{
        //    //    var command = commands.Dequeue();
        //    //    var isLastCommand = (commands.Count == 0);                

        //    //    //set modes
        //    //    if (command == "add")
        //    //    {
        //    //        if (!Config.BotAdmin.isAdmin(fromId))
        //    //        {
        //    //            await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой add. Запрос отменён.");
        //    //            chatFinded.ResetMode();
        //    //            continue;
        //    //        }

        //    //        chatFinded.AddMode = true;
        //    //        if (isLastCommand)
        //    //        {
        //    //            await Bot.SendTextMessageAsync(chatFinded.Id, "Добавьте игрока в формате '99;Имя;Фамилия'");
        //    //        }
        //    //        continue;
        //    //    }

        //    //    if (command == "remove")
        //    //    {
        //    //        if (!Config.BotAdmin.isAdmin(fromId))
        //    //        {
        //    //            await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой remove. Запрос отменён.");
        //    //            chatFinded.ResetMode();
        //    //            continue;
        //    //        }

        //    //        chatFinded.RemoveMode = true;
        //    //        if (isLastCommand)
        //    //        {
        //    //            await Bot.SendTextMessageAsync(chatFinded.Id, "Удалите игрока по 'номеру'");
        //    //        }
        //    //        continue;
        //    //    }






        //    //    //check modes
        //    //    if (chatFinded.AddMode)
        //    //    {
        //    //        AddPlayer(chatFinded, command);
        //    //        continue;
        //    //    }

        //    //    if (chatFinded.RemoveMode)
        //    //    {
        //    //        try
        //    //        {
        //    //            var number = int.Parse(command);
        //    //            RemovePlayer(chatFinded, number);
        //    //            continue;
        //    //        }
        //    //        catch (Exception ex)
        //    //        {
        //    //            ExceptionOnCmd(chatFinded, ex);
        //    //            continue;
        //    //        }
        //    //    }

        //    //    if (chatFinded.PersonalStatMode)
        //    //    {

        //    //    }

        //    //    //do command



        //    //    //если не в режиме, не установили режим, не выполнили команду сразу, может пользователь ввёл число для поиска игрока
        //    //    if (rxNums.IsMatch(command))
        //    //    {
        //    //        //в случае числа показываем игрока
        //    //        try
        //    //        {
        //    //            var number = int.Parse(command);
        //    //            ShowPlayerByNubmer(chatFinded, number);
        //    //            continue;
        //    //        }
        //    //        catch (Exception ex)
        //    //        {
        //    //            ExceptionOnCmd(chatFinded, ex);
        //    //            continue;
        //    //        }
        //    //    }

        //    //    //иначе пользователь ввёл хуйню
        //    //    WrongCmd(chatFinded);
        //    //}
        //}

        //private async void TourAnswer(Chat chatFinded)
        //{
        //    var tours = DB.DBCommands.GetTournaments();

        //    var rowCount = tours.Count%2 == 0 ? tours.Count/2 : tours.Count/2 + 1;
        //    ++rowCount; // ибо "официальные" и "все"

        //    var keys2 = new InlineKeyboardMarkup();
        //    keys2.InlineKeyboard = new InlineKeyboardButton[1][];
        //    keys2.InlineKeyboard[0] = new InlineKeyboardButton[1];
        //    keys2.InlineKeyboard[0][0] = new InlineKeyboardButton("ГОГОУ!");


        //    //var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
        //    //    {
        //    //        Keyboard = new Telegram.Bot.Types.KeyboardButton[rowCount][],
        //    //        OneTimeKeyboard = true
        //    //    };


        //    //    keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("Все"),
        //    //                                                                  new Telegram.Bot.Types.KeyboardButton("Официальные") }; // помнить о слешах
        //    //    for (var i = 0; i < tours.Count; ++i)
        //    //    {
        //    //        var row = i/2 + 1;
        //    //        var column = i % 2;

        //    //        if (keys.Keyboard[row] == null)
        //    //        {
        //    //            var isLast = (tours.Count - i - 1 == 0);
        //    //            var c = isLast ? 1 : 2;

        //    //            keys.Keyboard[row] = new KeyboardButton[c]; 
        //    //        }
        //    //        keys.Keyboard[row][column] = new KeyboardButton(chatFinded.Id > 0 ? tours[i].Name : "/" + tours[i].Name);
        //    //    }

        //    await Bot.SendTextMessageAsync(chatFinded.Id, "Выберете турнир:", false, false, 0, keys2);
        //}

        //private async void WrongCmd(Chat chatFinded)
        //{
        //    chatFinded.ResetMode();
        //    var keys = new ReplyKeyboardMarkup();
        //    keys.Keyboard = new KeyboardButton[1][];
        //    keys.Keyboard[0] = new KeyboardButton[1]
        //    {new KeyboardButton("/помощь")};
        //    keys.ResizeKeyboard = true;
        //    keys.OneTimeKeyboard = true;
        //    await
        //        Bot.SendTextMessageAsync(chatFinded.Id, "Неверный запрос, воспользуйтесь /помощь", false, false, 0, keys);
        //}

        //private async void ExceptionOnCmd(Chat chatFinded, Exception ex)
        //{
        //    chatFinded.ResetMode();
        //    Console.WriteLine(ex.Message);
        //    await Bot.SendTextMessageAsync(chatFinded.Id, "Ваш запрос не удалось обработать. Запрос отменён.");
        //}

        //private async void AddPlayer(Chat chatFinded, string argv)
        //{
        //    //argv format is number;name;surname
        //    chatFinded.AddMode = false;
        //    var playerinfo = argv.Split(';');
        //    if (playerinfo.Length == 3)
        //    {
        //        var player = new Player(int.Parse(playerinfo[0]), playerinfo[1].Trim(), playerinfo[2].Trim());
        //        DB.DBCommands.DBPlayer.AddPlayer(player);
        //        await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали добавить {player.Number}.");
        //    }
        //    else
        //    {
        //        await Bot.SendTextMessageAsync(chatFinded.Id, $"Неверный формат запроса: {argv}");
        //    }
        //}

        //private async void RemovePlayer(Chat chatFinded, int number)
        //{
        //    chatFinded.RemoveMode = false;
        //    DB.DBCommands.DBPlayer.RemovePlayerByNumber(number);
        //    await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали удалить {number}, проверим успешность поиском.");
        //    //ShowPlayerByNubmer(chatFinded, number);
        //}

        #endregion

        #region Исполнение команд

        /// <summary>
        /// Выводит помощь по командам
        /// </summary>
        private async void Help(Chat chatFinded)
        {
            var n = 0;
            var p = DB.DBCommands.DBPlayer.GetAllPlayerWithoutStatistic();
            if (p.Count != 0)
                n = p[(new Random()).Next(p.Count - 1)].Number;

            var help =
@"Команды для управления:

Топ игроков команды:
--------------------
бомбардиры
снайперы
асистенты
--------------------

статистика 1 - выводит статистику игрока (вместо 1 любой номер) или можно написать 'статистика команда' и Вы увидите командную статистику
соперник рэу - история встреч с соперником(вместо рэу любую команду)
кричалки - выводит одну из кричалок команды

💥Также попробуйте ввести номер любимого игрока💥";

            help = help.Replace("'%номер%'", $"{n}");

            await Bot.SendTextMessageAsync(chatFinded.Id, help);
        }

        private async void ShowPlayerByNubmer(Chat chatFinded, Command command)
        {
            var playerNumber = int.Parse(command.Name);

            if (playerNumber < 0 || playerNumber > 100)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат, введите корректный номер игрока от 0 до 100.");
                return;
            }
        
            var players = DB.DBCommands.DBPlayer.GetPlayersByNumber(playerNumber);
            if (players == null || players.Count== 0)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, $"Игрок под номером {playerNumber} не найден.");
                return;
            }

            if (players.Count==1)
                SendPlayer(chatFinded, command, players[0]);

            if (players.Count > 1)
            {
                var keyboard = ButtonsGenerator.MakeKeyboardPlayers(players);

                Message mes = await Bot.SendTextMessageAsync(chatFinded.Id, "Найдено более одного игрока. Выберите:",
                    parseMode: ParseMode.Markdown, replyMarkup: keyboard);
                command.Message = mes;
                command.Name = "игрок";
                chatFinded.WaitingCommands.Add(command);
            }
        }

        private async void SendPlayer(Chat chat, Command command, Player player)
        {
            var playerDescription = Gen.GetPlayerDescr();
            playerDescription += $"#{player.Number} {player.Name} {player.Surname}\n\n" +
                                 $"{player.Position}\n\n";

            if (!string.IsNullOrEmpty(player.VK)) playerDescription += $"VK: {player.VK}\n";
            if (!string.IsNullOrEmpty(player.INSTA)) playerDescription += $"Inst: {player.INSTA}\n";

            var photopath = player.PhotoFile;

            var button = new InlineKeyboardButton("Статистика");
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

            Console.WriteLine($"Send player:{player.Surname}");
            if (File.Exists(photopath))
            {
                var photo = new Telegram.Bot.Types.FileToSend(player.Number + ".jpg",
                    (new StreamReader(photopath)).BaseStream);


                Message mes = await Bot.SendPhotoAsync(chat.Id, photo, playerDescription, replyMarkup: keyboard);
                var newcom = new Command(new[] { "статистика", command.Name });
                newcom.Message = mes;
                chat.WaitingCommands.Add(newcom);
            }
            else
            {
                Console.WriteLine($"Photo file {photopath} not found.");
                await Bot.SendTextMessageAsync(chat.Id, playerDescription);
            }
        }

        private string GetPlayerStatistic(Chat chat, string arg)
        {
            string result = "Игрок не найден";
            Player player = null;
            var players = new List<Player>();

            if (rxNums.IsMatch(arg))
            {
                var number = int.Parse(arg);
                players = DB.DBCommands.DBPlayer.GetPlayersByNumber(number);
            }
            else
            {
                players = DB.DBCommands.DBPlayer.GetPlayersBySurname(arg);
            }

            //в случае числа показываем стату
            if (players.Count == 1)
            {
                player = players[0];
                DB.DBCommands.DBPlayer.GetPlayerStatistic(chat, player);
            }
            else
            {
                //TODO дописать выбор игрока
            }
            

            if (player != null)
            {
                result = String.Format("Статистика игрока\n" +
                                       "*#{0} {1} {2}*\n\n", player.Number, player.Name, player.Surname);

                result += String.Format("{0, 0} {1, 21}\n", "Игр:", player.Games);
                result += String.Format("{0, 0} {1, 18}\n", "Голы:", player.Goals);
                result += String.Format("{0, 0} {1, 18}\n", "Пасы:", player.Pas);
                result += String.Format("{0, 0} {1, 12}\n", "Гол+Пас:", player.Goals + player.Pas);
                //result += String.Format("{0, 0} {1, 15}\n", "Штраф:", player.Shtraf);
                //result += String.Format("{0, 0} {1, 23}\n", "+/-:", player.PlusMinus);
                result += String.Format("{0, 0} {1, 9:F}\n", "Ср. за игру:", player.StatAverragePerGame);
            }
            return result;
        }
        private async void PlayerStatistic(Chat chatFinded, string arg)
        {
            var result = GetPlayerStatistic(chatFinded, arg);
            await Bot.SendTextMessageAsync(chatFinded.Id, result, parseMode: ParseMode.Markdown);
        }

        private async void Top(Chat chatFinded, Top type, int count = 5) // говнокодище Дениса, update говнокод затерт, Денис молодец
        {
            string result = GetTop(chatFinded, type, count);

            var button = new InlineKeyboardButton("Еще");
            button.CallbackData = ((int)type).ToString();
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });


            var mes = await Bot.SendTextMessageAsync(chatFinded.Id, result, parseMode: ParseMode.Markdown, replyMarkup: keyboard);

            var newCom = new Command(new[] { "топ", count.ToString() });
            newCom.Message = mes;
            chatFinded.WaitingCommands.Add(newCom);
        }

        private string GetTop(Chat chatFinded, Top type, int count)
        {
            string result = "";
            List<Player> topPlayers = DB.DBCommands.DBPlayer.GetTopPlayers(chatFinded, type, count);

            topPlayers = SortTopPlayer(type, topPlayers);

            if (topPlayers.Count < count - 5) return "";

            result += TourSeasonStringUpdate(chatFinded);

            result += $"Топ {topPlayers.Count} *{GetTypeDescription(type)}* ХК \"Авиаторы\"\n\n";

            

            if (type == Aviators.Top.Goals)
                result += string.Format("`{0,-3} {1,-20}`*{2} {3}*", "№", "Имя Фамилия", "Г", "И");

            if (type == Aviators.Top.Assist)
                result += string.Format("`{0,-3} {1,-20}`*{2} {3}*", "№", "Имя Фамилия", "А", "И");

            if (type == Aviators.Top.Points)
                result += string.Format("`{0,-3} {1,-20}`*{2} ({3}+{4}) {5}*", "№", "Имя Фамилия", "О", "Г", "А", "И");


            foreach (var topPlayer in topPlayers)
            {
                result += "\n";

                result += string.Format("`#{0,-3}{1,-20}`*{2} {3}*", topPlayer.Number,
                    topPlayer.Name + " " + topPlayer.Surname,
                    GetStringOfStatType(type, topPlayer), topPlayer.Games);

            }

            return result;
        }


        /// <summary>
        /// Денис захотел сортировку
        /// </summary>
        private List<Player> SortTopPlayer(Top type, List<Player> topPlayers)
        {
            if (type == Aviators.Top.APG)
                return topPlayers.OrderByDescending(g => g.StatAverragePerGame).ThenByDescending(g => g.Games).ToList();
            if (type == Aviators.Top.Goals)
                return topPlayers.OrderByDescending(g => g.AllStatGoal).ThenByDescending(g => g.Games).ToList();
            if (type == Aviators.Top.Assist)
                return topPlayers.OrderByDescending(g => g.AllStatAssist).ThenByDescending(g => g.Games).ToList();
            if (type == Aviators.Top.Penalty)
                return topPlayers.OrderByDescending(g => g.AllStatMinute).ThenByDescending(g => g.Games).ToList();
            if (type == Aviators.Top.PlusMinus)
                return topPlayers.OrderByDescending(g => g.PlusMinus).ThenByDescending(g => g.Games).ToList();
            if (type == Aviators.Top.Points)
                return topPlayers.OrderByDescending(g => g.AllStatBomb).ThenByDescending(g => g.Games).ThenByDescending(g => g.AllStatGoal).ToList();


            return topPlayers;
        }


        private string TourSeasonStringUpdate(Chat chatFinded)
        {
            var s = "";
            if (chatFinded.Tournament != null)
                s += $"*{chatFinded.Tournament}*";

            if (chatFinded.Season != null)
                s += $"\n*{chatFinded.Season}*";

            s += "\n";

            return s;
        }

        private async void Slogans(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, Gen.GetSlogan());
        }
        private async void LastGame(Chat chatFinded)
        {
            var game = DB.DBCommands.DBGame.GetLastGame(chatFinded);

            if (game == null)
                await Bot.SendTextMessageAsync(chatFinded.Id, "Не удалось найти игру");
            else
                SendGameStat(chatFinded, game);
        }

        private async void TournametList(Chat chatFinded, Command command)
        {
            var tours = DB.DBCommands.GetTournaments();

            var keyboard = ButtonsGenerator.MakeKeyboardTournaments(tours);
            Message mes = await Bot.SendTextMessageAsync(chatFinded.Id, "Выберите турнир", replyMarkup: keyboard);
            command.Message = mes;
            chatFinded.WaitingCommands.Add(command);
        }

        private async void SeasonList(Chat chatFinded, Command command)
        {
            List<Season> seasons = DB.DBCommands.GetSeasons();

            var keyboard = ButtonsGenerator.MakeKeyboardSeason(seasons);
            Message mes = await Bot.SendTextMessageAsync(chatFinded.Id, "Выберите сезон", replyMarkup: keyboard);
            command.Message = mes;
            chatFinded.WaitingCommands.Add(command);
        }



        private async void TeamList(Chat chatFinded, Command command)
        {
            List<Team> teams = DB.DBCommands.GetAllTeams();

            teams.RemoveAt(0);

            var keyboard = ButtonsGenerator.MakeKeyboardTeams(teams);
            Message mes = await Bot.SendTextMessageAsync(chatFinded.Id, TourSeasonStringUpdate(chatFinded) + "Выберите соперника", replyMarkup: keyboard);
            command.Message = mes;
            chatFinded.WaitingCommands.Add(command);
        }

        private async void GamesStatistic(Chat chatFinded)
        {
            var result = new List<string>();

            List<Game> allGames = DB.DBCommands.DBGame.GetAllGames(chatFinded);

            const int otstup = -20;

            var goals = allGames.Sum(g => g.Score.Item1);
            var shotsIn = allGames.Sum(g => g.Stat1.ShotsIn);
            var shots = allGames.Sum(g => g.Stat1.Shots);
            var opGoals = allGames.Sum(g => g.Score.Item2);
            var allGamesCount = allGames.Count;

            result.Add($"`{"Всего игр",otstup}` {allGamesCount}");
            result.Add($"`{"Победы",otstup}` {allGames.Count(g => g.Score.Item1 > g.Score.Item2)}");
            result.Add($"`{"Поражения",otstup}` {allGames.Count(g => g.Score.Item1 < g.Score.Item2)}");
            result.Add($"`{"Ничьи",otstup}` {allGames.Count(g => g.Score.Item1 == g.Score.Item2)}");
            result.Add($"`{"Сухие победы",otstup}` {allGames.Count(g => g.Score.Item2 == 0 && g.Score.Item1 != g.Score.Item2)}");
            result.Add($"`{"Забитые",otstup}` {goals + " (" + (goals / (float)shotsIn * 100).ToString("F") + "%)"}");
            result.Add($"`{"Пропущеные",otstup}` {opGoals}");
            //result.Add($"`{"КДРАТИО",otstup}` {goals / (float)opGoals:F}");
            result.Add($"`{"Броски",otstup}` {shots}");
            result.Add($"`{"Броски в створ",otstup}` {shotsIn + " (" + (shotsIn / (float)shots * 100).ToString("F") + "%)"}");
            result.Add($"`{"ср.голов",otstup}` {goals / (float)allGamesCount:F}");
            result.Add($"`{"ср.бросоков",otstup}` {shots / (float)allGamesCount:F}");
            result.Add($"`{"ср.бросков в створ",otstup}` {shotsIn / (float)allGamesCount:F}");
            result.Add($"`{"ср.пропущенных",otstup}` {opGoals / (float)allGamesCount:F}");
            result.Add($"`{"Силовые",otstup}` {allGames.Sum(g => g.Stat1.Hits)}");
            result.Add($"`{"Заблокированные",otstup}` {allGames.Sum(g => g.Stat1.BlockShots)}");
            result.Add($"`{"Штрафы",otstup}` {allGames.Sum(g => g.Stat1.Penalty)}");
            result.Add($"`{"% выигр.вбрасываний",otstup}` {(allGames.Sum(g => g.Stat1.Faceoff) / (float)(allGames.Sum(g => g.Stat1.Faceoff) + allGames.Sum(g => g.Stat2.Faceoff)) * 100).ToString("F") + "%"}");

            await Bot.SendTextMessageAsync(chatFinded.Id, string.Join("\n", result), parseMode: ParseMode.Markdown);
        }
        private async void SendGameStat(Chat chatFinded, Game game)
        {
            if (game == null)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Не удалось найти игру");
                return;
            }

            //var game = DB.DBCommands.DBGame.GetGame(gameId);
            var file = ImageGen.GameStatistic(game);

            var photo = new Telegram.Bot.Types.FileToSend("gamestat",
                (new StreamReader(file)).BaseStream);

            var button = new InlineKeyboardButton("Состав");
            button.CallbackData = game.Id.ToString();
            var keyboard = new InlineKeyboardMarkup(new[] { new[] { button } });

            Message mes = await Bot.SendPhotoAsync(chatFinded.Id, photo, replyMarkup: keyboard);
            var newCom = new Command(new [] {"состав", game.Id.ToString()});
            newCom.Message = mes;
            chatFinded.WaitingCommands.Add(newCom);
        }
        private async void SendGameSostav(Chat chatFinded, int gameId)
        {
            var game = DB.DBCommands.DBGame.GetGame(gameId);
            var file = ImageGen.Roster(game);

            var photo = new Telegram.Bot.Types.FileToSend("gamestat",
               (new StreamReader(file)).BaseStream);
            await Bot.SendPhotoAsync(chatFinded.Id, photo);
        }

        private async void EnemyTeam(Chat chatFinded, Command command)
        {
            var result = new List<string>();
            var team = DB.DBCommands.GetTeam(command.Argument);
            if (team.Id < 1) result.Add("Соперник не найден");

            var games = DB.DBCommands.DBGame.GetGamesTeam(chatFinded, team);
            if (games.Count < 1) result.Add("Игр не найдено");

            const int otstup = -15;

            var goals = games.Sum(g => g.Score.Item1);
            var shotsIn = games.Sum(g => g.Stat1.ShotsIn);
            var opGoals = games.Sum(g => g.Score.Item2);

            result.Add(TourSeasonStringUpdate(chatFinded));
            result.Add("Статистика встреч с соперником *" + team.Name +"*");

            result.Add($"`{"Всего игр",otstup}` {games.Count}");
            result.Add($"`{"Победы",otstup}` {games.Count(g => g.Score.Item1 > g.Score.Item2)}");
            result.Add($"`{"Поражения",otstup}` {games.Count(g => g.Score.Item1 < g.Score.Item2)}");
            result.Add($"`{"Ничьи",otstup}` {games.Count(g => g.Score.Item1 == g.Score.Item2)}");
            result.Add($"`{"Забитые",otstup}` {goals + " (" + (goals / (float)shotsIn * 100).ToString("F") + "%)"}");
            result.Add($"`{"Пропущеные",otstup}` {opGoals}");

            Player player = DB.DBCommands.DBPlayer.GetPlayerTopForTeam(team);
            if (player != null)
                result.Add($"`{"Любимчик",otstup}` {player.Surname +" (" + player.AllStatGoal+"+" + player.AllStatAssist + ")"}");

            var keyboard = ButtonsGenerator.MakeKeyboardGames(games);

            Message mes = await Bot.SendTextMessageAsync(chatFinded.Id, string.Join("\n", result),
                parseMode: ParseMode.Markdown, replyMarkup: keyboard);
            command.Message = mes;
            chatFinded.WaitingCommands.Add(command);


           
        }
        private async void NextGame(Chat chatFinded)
        {
            var file = ImageGen.Roster(DB.DBCommands.DBGame.GetLastGame(chatFinded));

            var photo = new Telegram.Bot.Types.FileToSend("gamestat",
                (new StreamReader(file)).BaseStream);

            await Bot.SendPhotoAsync(chatFinded.Id, photo);
            //await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я следующая игра");
        }

        private async void TimeTable(Chat chatFinded, int n)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я расписание",parseMode: ParseMode.Markdown);
        }

        private void GetAllGames(Chat chatFinded)
        {
            List<Game> allGames = DB.DBCommands.DBGame.GetAllGames(chatFinded);


            //TODO Переделать на кнопки с играми
            foreach (var allGame in allGames)
            {
                SendGameStat(chatFinded, allGame);
            }
        }
        #endregion


        private string GetTypeDescription(Top type)
        {
            switch (type)
            {
                case Aviators.Top.APG:
                    return "среднее очков за игру";

                case Aviators.Top.Goals:
                    return "снайперов";

                case Aviators.Top.Assist:
                    return "ассистентов";

                case Aviators.Top.Points:
                    return "бомбардиров";

                case Aviators.Top.Penalty:
                    return "штрафников";

                case Aviators.Top.PlusMinus:
                    return "полезных игроков";
            }
            return "";
        }

        private string GetStringOfStatType(Top type, Player topPlayer)
        {
            switch (type)
            {
                case Aviators.Top.APG:
                    return topPlayer.StatAverragePerGame.ToString("F");

                case Aviators.Top.Goals:
                    return topPlayer.AllStatGoal.ToString();

                case Aviators.Top.Assist:
                    return topPlayer.AllStatAssist.ToString();

                case Aviators.Top.Points:
                    return $"{topPlayer.AllStatBomb} ({topPlayer.Goals}+{topPlayer.Pas})";

                case Aviators.Top.Penalty:
                    return topPlayer.Shtraf.ToString();

                case Aviators.Top.PlusMinus:
                    return topPlayer.PlusMinus.ToString();

            }
            return "";
        }

       

        /// <summary>
        /// Отправляет кнопки при подключении к боту, которые всегда будут на месте клавиатуры.
        /// </summary>
        public async void SendKeyboardButtons(Chat chatFinded)
        {
            var keyboard = new ReplyKeyboardMarkup();
            keyboard.ResizeKeyboard = true;

            var row1 = new KeyboardButton[] // First Row
            {
                "Помощь"
            };
            keyboard.Keyboard = new[]
            {row1,
                new KeyboardButton[] // Second Row
                {
                    "Турнир",
                    "Сезон",
                },
            };
            await Bot.SendTextMessageAsync(chatFinded.Id, "Добро пожаловать!", replyMarkup: keyboard);
        }
    }


    public enum Top
    {
        Assist,
        Penalty,
        Points,
        Goals,
        PlusMinus,
        APG
    }



}
