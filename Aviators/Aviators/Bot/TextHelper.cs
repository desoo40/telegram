﻿namespace Aviators.Bot
{
    public class TextHelper
    {
        public string SimpleNameFinder(Game game, Player player)
        {
            foreach (var man in game.Roster)
            {
                if (man.Surname == player.Surname && man.Number != player.Number)
                {
                    if (player.Surname == "Зайцев")
                        return player.Number == 71
                            ? player.Surname + " " + player.Name[0] + ".И."
                            : player.Surname + " " + player.Name[0] + ".A.";

                    return player.Surname + " " + player.Name[0] + ".";

                }
            }
            return player.Surname;
        }

        public string FullNameFinder(string s)
        {
            var inStr = s.ToLower();

            if (inStr == "рэу" || inStr == "плешка")
                return "ХК РЭУ им. Плеханова";

            if (inStr == "миэт" || inStr == "зеленоград")
                return "ХК \"Электроник\" МИЭТ";

            if (inStr == "мгту" || inStr == "бауманка")
                return "ХК МГТУ им. Баумана";

            if (inStr == "тгу" || inStr == "держава")
                return "ХК \"Держава\" ТГУ";

            if (inStr == "ргуфксит" || inStr == "гцолифк" || inStr == "ргуфк")
                return "ХК \"Гладиаторы\" ГЦОЛИФК";

            if (inStr == "тверичи")
                return "МХК \"Тверичи\"";

            return s;
        }
    }
}