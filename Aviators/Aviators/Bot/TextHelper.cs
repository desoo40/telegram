namespace Aviators.Bot
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

            if (inStr == "ргуфксмит" || inStr == "гцолифк" || inStr == "ргуфк")
                return "ХК \"Гладиаторы\" ГЦОЛИФК";

            if (inStr == "тверичи")
                return "МХК \"Тверичи\"";

            if (inStr == "капитан")
                return "МХК \"Капитан\"";

            if (inStr == "юургу")
                return "ХК \"Политехник\" ЮУрГУ";

            if (inStr == "мсха")
                return "ХК \"Тимирязевские зубры\" МСХА";

            if (inStr == "ранхигс")
                return "ХК \"Сенатор\" РАНХиГС";

            if (inStr == "umb")
                return "UMB (Словакия)";

            if (inStr == "ukpraha")
                return "UK PRAHA (Чехия)";

            if (inStr == "миит")
                return "ХК \"Дизель\" МИИТ";

            if (inStr == "мисис")
                return "ХК \"Стальные медведи\" МИСиС";

            if (inStr == "мифи")
                return "ХК \"Реактор\" МИФИ";

            if (inStr == "ву")
                return "ХК Военный университет";

            if (inStr == "маи3")
                return "ХК \"Ледяные волки\" МАИ";

            if (inStr == "светон")
                return "ХК \"Светон\"";

            if (inStr == "ннгу")
                return "ХК ННГУ им. Н.И. Лобачевского";

            if (inStr == "пгафксит")
                return "ХК ПГАФКСиТ";

            if (inStr == "мгмсу")
                return "ХК \"Зубастые акулы\" МГМСУ";

            if (inStr == "гуу")
                return "ХК ГУУ";

            return s;
        }
    }
}