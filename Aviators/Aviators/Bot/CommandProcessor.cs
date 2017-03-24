﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Aviators.Configs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace Aviators
{
    /// <summary>
    /// Тут обрабатываются команды для бота
    /// </summary
    public class CommandProcessor
    {
        private TelegramBotClient Bot;
        private readonly Randomiser Gen;
        //private DBCore DB;
        Regex rxNums = new Regex(@"^\d+$"); // проверка на число


        public CommandProcessor(TelegramBotClient bot)
        {
            Bot = bot;
            Gen = new Randomiser();
            //DB = new DBCore();
        }

        public void FindCommands(string msg, Chat chatFinded, int fromId)
        {
            var inputCommands = msg.Split(' ');

            var command = new Command(inputCommands);//сама команда молодости нашей

            if (rxNums.IsMatch(command.Name))
            {
                //в случае числа показываем игрока
                var number = int.Parse(command.Name);
                ShowPlayerByNubmer(chatFinded, command);
                return;
            }

            switch (command.Name)
            {
                case "помощь":
                    Help(chatFinded);
                    return;

                case "статистика":
                    if (command.Argument != null)
                    {
                        PlayerStatistic(chatFinded, command.Argument);
                        return;
                    }
                    else
                    {
                        TourAnswer(chatFinded);
                        //chatFinded.PersonalStatMode = true;
                        //if (isLastCommand)
                        // {
                        // await Bot.SendTextMessageAsync(chatFinded.Id, "Введите номер или фамилию игрока"); // рассмотреть возможность однофамильцев
                        //}
                        return;
                    }
                case "расписание":
                    TimeTable(chatFinded, 0);
                    return;

                case "следующая":
                    NextGame(chatFinded);
                    return;

                case "соперник":
                    EnemyTeam(chatFinded, "соперник");
                    return;

                case "кричалки":
                    Slogans(chatFinded);
                    return;

                case "бомбардиры":
                    Top(chatFinded, Aviators.Top.Bomb);
                    return;
                case "снайперы":
                    Top(chatFinded, Aviators.Top.Snip);
                    return;
                case "асистенты":
                    Top(chatFinded, Aviators.Top.Asist);
                    return;
                case "штрафники":
                    Top(chatFinded, Aviators.Top.BadBoy);
                    return;
                case "полезность":
                    Top(chatFinded, Aviators.Top.Usefull);
                    return;

            }

            //ProcessCommands(chatFinded, fromId);            
        }

        public async void ContinueCommand(Chat chatFinded, int msgid)
        {
            var command = chatFinded.WaitingCommands.FirstOrDefault(m => m.Message.MessageId == msgid);
            if(command == null) return;

            var statistic = GetPlayerStatistic(command.Name).Replace("*","");

            await Bot.EditMessageCaptionAsync(chatFinded.Id, msgid, statistic);
        }

        private async void ProcessCommands(Chat chatFinded, int fromId)
        {
            //var commands = chatFinded.CommandsQueue;

            //while (commands.Count > 0)
            //{
            //    var command = commands.Dequeue();
            //    var isLastCommand = (commands.Count == 0);                

            //    //set modes
            //    if (command == "add")
            //    {
            //        if (!Config.BotAdmin.isAdmin(fromId))
            //        {
            //            await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой add. Запрос отменён.");
            //            chatFinded.ResetMode();
            //            continue;
            //        }

            //        chatFinded.AddMode = true;
            //        if (isLastCommand)
            //        {
            //            await Bot.SendTextMessageAsync(chatFinded.Id, "Добавьте игрока в формате '99;Имя;Фамилия'");
            //        }
            //        continue;
            //    }

            //    if (command == "remove")
            //    {
            //        if (!Config.BotAdmin.isAdmin(fromId))
            //        {
            //            await Bot.SendTextMessageAsync(chatFinded.Id, "Вам не разрешено пользоваться командой remove. Запрос отменён.");
            //            chatFinded.ResetMode();
            //            continue;
            //        }

            //        chatFinded.RemoveMode = true;
            //        if (isLastCommand)
            //        {
            //            await Bot.SendTextMessageAsync(chatFinded.Id, "Удалите игрока по 'номеру'");
            //        }
            //        continue;
            //    }






            //    //check modes
            //    if (chatFinded.AddMode)
            //    {
            //        AddPlayer(chatFinded, command);
            //        continue;
            //    }

            //    if (chatFinded.RemoveMode)
            //    {
            //        try
            //        {
            //            var number = int.Parse(command);
            //            RemovePlayer(chatFinded, number);
            //            continue;
            //        }
            //        catch (Exception ex)
            //        {
            //            ExceptionOnCmd(chatFinded, ex);
            //            continue;
            //        }
            //    }

            //    if (chatFinded.PersonalStatMode)
            //    {

            //    }

            //    //do command
                
                

            //    //если не в режиме, не установили режим, не выполнили команду сразу, может пользователь ввёл число для поиска игрока
            //    if (rxNums.IsMatch(command))
            //    {
            //        //в случае числа показываем игрока
            //        try
            //        {
            //            var number = int.Parse(command);
            //            ShowPlayerByNubmer(chatFinded, number);
            //            continue;
            //        }
            //        catch (Exception ex)
            //        {
            //            ExceptionOnCmd(chatFinded, ex);
            //            continue;
            //        }
            //    }

            //    //иначе пользователь ввёл хуйню
            //    WrongCmd(chatFinded);
            //}
        }

        private async void TourAnswer(Chat chatFinded)
        {
            var tours = DB.DBCommands.GetTournaments();

            var rowCount = tours.Count%2 == 0 ? tours.Count/2 : tours.Count/2 + 1;
            ++rowCount; // ибо "официальные" и "все"

            var keys2 = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup();
            keys2.InlineKeyboard = new InlineKeyboardButton[1][];
            keys2.InlineKeyboard[0] = new InlineKeyboardButton[1];
            keys2.InlineKeyboard[0][0] = new InlineKeyboardButton("ГОГОУ!");


            //var keys = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            //    {
            //        Keyboard = new Telegram.Bot.Types.KeyboardButton[rowCount][],
            //        OneTimeKeyboard = true
            //    };


            //    keys.Keyboard[0] = new Telegram.Bot.Types.KeyboardButton[2] { new Telegram.Bot.Types.KeyboardButton("Все"),
            //                                                                  new Telegram.Bot.Types.KeyboardButton("Официальные") }; // помнить о слешах
            //    for (var i = 0; i < tours.Count; ++i)
            //    {
            //        var row = i/2 + 1;
            //        var column = i % 2;

            //        if (keys.Keyboard[row] == null)
            //        {
            //            var isLast = (tours.Count - i - 1 == 0);
            //            var c = isLast ? 1 : 2;

            //            keys.Keyboard[row] = new KeyboardButton[c]; 
            //        }
            //        keys.Keyboard[row][column] = new KeyboardButton(chatFinded.Id > 0 ? tours[i].Name : "/" + tours[i].Name);
            //    }

            await Bot.SendTextMessageAsync(chatFinded.Id, "Выберете турнир:", false, false, 0, keys2);
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
                DB.DBCommands.AddPlayer(player);
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
            DB.DBCommands.RemovePlayerByNumber(number);
            await Bot.SendTextMessageAsync(chatFinded.Id, $"Попробовали удалить {number}, проверим успешность поиском.");
            //ShowPlayerByNubmer(chatFinded, number);
        }

        private async void ShowPlayerByNubmer(Chat chatFinded, Command command)
        {
            var playerNumber = int.Parse(command.Name);


            if (playerNumber < 0 || playerNumber > 100)
            {
                await Bot.SendTextMessageAsync(chatFinded.Id, "Неверный формат, введите корректный номер игрока от 0 до 100.");
                return;
            }

            try
            {
                var player = DB.DBCommands.GetPlayerByNumber(playerNumber);
                if (player == null)
                {
                    await Bot.SendTextMessageAsync(chatFinded.Id, $"Игрок под номером {playerNumber} не найден.");
                }
                else
                {
                    var playerDescription = Gen.GetPlayerDescr();
                    playerDescription += $"#{player.Number} {player.Name} {player.Surname}\n\n" +
                                         $"{player.Position}\n\n" +
                                         $"VK: {player.VK}\n" +
                                         $"Inst: {player.INSTA}";


                    var photopath = Path.Combine(Config.DBPlayersPhotoDirPath, player.PhotoFile);

                    Console.WriteLine($"Send player:{player.Surname}");
                    if (File.Exists(photopath))
                    {

                        var str = (new StreamReader(photopath)).BaseStream;
                        Bitmap bitmap = new Bitmap(str);

                        var ssss = @"Говно, залупа, пенис, хер, давалка, хуй, блядина,
Головка, шлюха, жопа, член, еблан, петух, мудила,
Рукоблуд, ссанина, очко, блядун, вагина,
Сука, ебланище, влагалище, пердун, дрочила,
Пидор, пизда, туз, малафья, гомик, мудила, пилотка, манда,
Анус, вагина, путана, педрила, шалава, хуила, мошонка, елда";

                        var spl = ssss.Split(',');

                        Random r = new Random();
                        int i = r.Next(spl.Length);

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.DrawString(spl[i].Trim().ToUpper(), new Font(FontFamily.GenericSansSerif, 30), Brushes.Chartreuse, 50, 600);
                        }

                        bitmap.Save("temp.jpg", ImageFormat.Jpeg);
                        var photo = new Telegram.Bot.Types.FileToSend(player.Number + ".jpg",
                            (new StreamReader("temp.jpg")).BaseStream);

                        var button  = new InlineKeyboardButton("Статистика");
                        var keyboard = new InlineKeyboardMarkup(new[]{new[] { button }});

                        Message mes = await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription, replyMarkup:keyboard);
                        command.Message = mes;
                        chatFinded.WaitingCommands.Add(command);
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



      


        #region Исполнение команд

        /// <summary>
        /// Выводит помощь по командам
        /// </summary>
        private async void Help(Chat chatFinded)
        {
            var p = DB.DBCommands.GetAllPlayerWitoutStatistic();
            var n = p[(new Random()).Next(p.Count - 1)].Number;

            var help =
@"Управление:

Топ игроков команды:
--------------------
/бомбардиры
/снайперы
/асистенты
/штрафники
/полезность (+/-)
--------------------

/статистика игрока
/расписание - ближайшие 3 игры
/следующая игра: дата, время, соперник и место
/соперник - история встреч
/кричалки - выводит одну из кричалок команды

💥Также попробуйте ввести номер любимого игрока💥";

            help = help.Replace("'%номер%'", $"{n}");

            await Bot.SendTextMessageAsync(chatFinded.Id, help);
        }

        private string GetPlayerStatistic(string arg)
        {
            string result = "Игрок не найден";
            Player player;

            if (rxNums.IsMatch(arg))
            {
                //в случае числа показываем стату
                var number = int.Parse(arg);
                player = DB.DBCommands.GetPlayerStatisticByNumber(number);
            }
            else
            {
                //в случае букв ищем по имени или фамилии
                player = DB.DBCommands.GetPlayerStatisticByNameOrSurname(arg);
            }

            if (player != null)
            {
                result = String.Format("Статистика игрока\n" +
                                       "*#{0} {1} {2}*\n\n", player.Number, player.Name, player.Surname);

                result += String.Format("{0, 0} {1, 21}\n", "Игр:", player.Games);
                result += String.Format("{0, 0} {1, 18}\n", "Голы:", player.Goals);
                result += String.Format("{0, 0} {1, 18}\n", "Пасы:", player.Pas);
                result += String.Format("{0, 0} {1, 12}\n", "Гол+Пас:", player.Goals + player.Pas);
                result += String.Format("{0, 0} {1, 15}\n", "Штраф:", player.Shtraf);
                result += String.Format("{0, 0} {1, 23}\n", "+/-:", player.PlusMinus);
            }
            return result;
        }
        private async void PlayerStatistic(Chat chatFinded, string arg)
        {
            var result = GetPlayerStatistic(arg);
            await Bot.SendTextMessageAsync(chatFinded.Id, result, parseMode: ParseMode.Markdown);
        }

        private async void Top(Chat chatFinded, Top type) // говнокодище Дениса, update говнокод затерт, Денис молодец
        {
            string result = "";
            List<Player> topPlayers = DB.DBCommands.GetTopPlayers(5);
            //TODO сделать Денису тут все

            
            if (type == Aviators.Top.Bomb)
            {
                result = "Топ 5 *бомбардиров* ХК \"Авиаторы\":\n";


                foreach (var topPlayer in topPlayers)
                {
                    result += $"\n`#{topPlayer.Number}` ";
                    if (topPlayer.Number < 10)
                        result += "  ";  
                    result += $"{topPlayer.Name} {topPlayer.Surname}     *{topPlayer.Pas + topPlayer.Goals}*";
                }
            }

            if (type == Aviators.Top.Asist)
            {
                result = "Топ 5 *асистентов* ХК \"Авиаторы\":\n";


                foreach (var topPlayer in topPlayers)
                {
                    result += $"\n`#{topPlayer.Number}` ";
                    if (topPlayer.Number < 10)
                        result += "  "; 
                    result += $"{topPlayer.Name} {topPlayer.Surname}     *{topPlayer.Pas}*";
                }
            }

            if (type == Aviators.Top.Snip)
            {
                result = "Топ 5 *снайперов* ХК \"Авиаторы\":\n";


                foreach (var topPlayer in topPlayers)
                {
                    result += $"\n`#{topPlayer.Number}` ";
                    if (topPlayer.Number < 10)
                        result += "  ";  
                    result += $"{topPlayer.Name} {topPlayer.Surname}     *{topPlayer.Goals}*";
                }
            }

            if (type == Aviators.Top.BadBoy)
            {
                result = "Топ 5 *штрафников* ХК \"Авиаторы\":\n";


                foreach (var topPlayer in topPlayers)
                {
                    result += $"\n`#{topPlayer.Number}` ";
                    if (topPlayer.Number < 10)
                        result += "  ";  
                    result += $"{topPlayer.Name} {topPlayer.Surname}     *{topPlayer.Shtraf}*";
                }
            }

            if (type == Aviators.Top.Usefull)
            {
                result = "Топ 5 *полезных игроков* ХК \"Авиаторы\":\n";


                foreach (var topPlayer in topPlayers)
                {
                    result += $"\n`#{topPlayer.Number}` ";
                    if (topPlayer.Number < 10)
                        result += "  ";  
                    result += $"{topPlayer.Name} {topPlayer.Surname}     *{topPlayer.PlusMinus}*";
                }
            }

            await Bot.SendTextMessageAsync(chatFinded.Id, result, parseMode: ParseMode.Markdown);
        }

        private async void Slogans(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, Gen.GetSlogan());
        }

        private async void EnemyTeam(Chat chatFinded, string team)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "*Привет, я соперник*", parseMode: ParseMode.Markdown);
        }

        private async void NextGame(Chat chatFinded)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я следующая игра");
        }

        private async void TimeTable(Chat chatFinded, int n)
        {
            await Bot.SendTextMessageAsync(chatFinded.Id, "Привет, я расписание",parseMode: ParseMode.Markdown);
        }
        #endregion

       
    }

    public class Command
    {
        public string Name { get; set; }
        public string Argument => ListArguments.FirstOrDefault();
        public List<string> ListArguments { get; set; }

        public Message Message { get; set; }

        public Command(string[] input)
        {
            ListArguments = new List<string>();
            if (input.Length > 0)
            {
                Name = input[0];
                if (input.Length > 1)  ListArguments.AddRange(input.Skip(1));
            }
            else
            {
                Name = ""; //хз
            }
        }

    }

    public enum Top
    {
        All,
        Asist,
        BadBoy,
        Bomb,
        Snip,
        Usefull
    }
}