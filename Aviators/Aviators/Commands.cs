using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Aviators
{
    public class Commands
    {
        private TelegramBotClient Bot;
        readonly FuckGen Fuck;
        DBCore DB;

        string DBPlayersPhotoDirPath = Directory.GetCurrentDirectory() + @"/data_base/PlayersPhoto/";

        public Commands(TelegramBotClient bot)
        {
            Bot = bot;
            Fuck = new FuckGen();
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
                    var playerDescription = Fuck.GetFuck();
                    playerDescription += string.Format("#{0} {1} {2}", player.Number, player.Name,
                        player.Surname);
                    var photo = new Telegram.Bot.Types.FileToSend(player.Number + ".jpg",
                        (new StreamReader(Path.Combine(DBPlayersPhotoDirPath, player.PhotoFile))).BaseStream);

                    Console.WriteLine($"Send player:{player.Surname}");
                    ;
                ;
                    await Bot.SendPhotoAsync(chatFinded.Id, photo, playerDescription);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(chatFinded.Id, "Больше так не делай, мне больно(");

            }
        }
    }
}
