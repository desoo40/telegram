using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Aviators.Bot
{
    public static class ButtonsGenerator
    {
        #region Создание кнопок

        public static InlineKeyboardMarkup MakeKeyboardTeams(List<Team> teams)
        {
            var massiv = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < teams.Count; i++)
            {
                if (i == teams.Count - 1) massiv.Add(new[] { new InlineKeyboardButton(teams[i].Name) });
                else if (teams[i].Name.Length > 20) massiv.Add(new[] { new InlineKeyboardButton(teams[i].Name) });
                else
                {
                    massiv.Add(new[] { new InlineKeyboardButton(teams[i].Name), new InlineKeyboardButton(teams[i + 1].Name) });
                    i++;
                }
            }


            var keyboard = new InlineKeyboardMarkup(massiv.ToArray());
            return keyboard;
        }

        public static InlineKeyboardMarkup MakeKeyboardTournaments(List<Tournament> tour)
        {
            var massiv = new List<InlineKeyboardButton[]>();

            //надо как то выбирать все , для этого сделаем сверху кнопу
            massiv.Add(new[] { new InlineKeyboardButton("Все", 0.ToString()) });

            for (int i = 0; i < tour.Count; i++)
            {
                if (i == tour.Count - 1)
                    massiv.Add(new[] { new InlineKeyboardButton(tour[i].Name, tour[i].Id.ToString()) });
                else
                    if (tour[i].Name.Length > 20) massiv.Add(new[] { new InlineKeyboardButton(tour[i].Name, tour[i].Id.ToString()) });
                else
                {
                    massiv.Add(new[] { new InlineKeyboardButton(tour[i].Name, tour[i].Id.ToString()), new InlineKeyboardButton(tour[i + 1].Name, tour[i + 1].Id.ToString()) });
                    i++;
                }
            }


            var keyboard = new InlineKeyboardMarkup(massiv.ToArray());
            return keyboard;
        }

        public static InlineKeyboardMarkup MakeKeyboardPlayers(List<Player> players)
        {
            var massiv = new List<InlineKeyboardButton[]>();
            foreach (Player p in players)
            {
                var str = $"{p.Number} - {p.Surname} {p.Name} {p.Patronymic}";
                massiv.Add(new[] { new InlineKeyboardButton(str, p.Id.ToString()) });
            }

            var keyboard = new InlineKeyboardMarkup(massiv.ToArray());
            return keyboard;
        }

        public static InlineKeyboardMarkup MakeKeyboardGames(List<Game> games)
        {
            var massiv = new List<InlineKeyboardButton[]>();
            foreach (Game g in games)
            {
                var str = $"{g.Date.ToShortDateString()} {g.Tournament.Name} {g.Team1} {g.Score.Item1}:{g.Score.Item2} {g.Team2}";
                massiv.Add(new[] { new InlineKeyboardButton(str, g.Id.ToString()) });
            }

            var keyboard = new InlineKeyboardMarkup(massiv.ToArray());
            return keyboard;
        }

        public static InlineKeyboardMarkup MakeKeyboardSeason(List<Season> seasons)
        {
            var massiv = new List<InlineKeyboardButton[]>();

            //надо как то выбирать все , для этого сделаем сверху кнопу
            massiv.Add(new[] { new InlineKeyboardButton("Все", 0.ToString()) });

            foreach (Season s in seasons)
            {
                massiv.Add(new[] { new InlineKeyboardButton(s.Name, s.Id.ToString()) });
            }

            var keyboard = new InlineKeyboardMarkup(massiv.ToArray());
            return keyboard;
        }

        #endregion
    }
}
