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
    }
}