using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    public class Randomiser
    {
        private readonly List<string> playersDescr = new List<string>();
        private readonly List<string> slogans = new List<string>();
        private readonly Random random = new Random();

        public Randomiser()
        {
            InitializateDescr();
            InitializateSlogans();
        }

        public string GetPlayerDescr()
        {
            var index = random.Next(playersDescr.Count);
            return playersDescr[index];
        }

        public string GetSlogan()
        {
            var index = random.Next(slogans.Count);
            return slogans[index];
        }

        private void InitializateDescr()
        {
            playersDescr.Add("Вот красавчик, которого вы искали:\n\n");
            playersDescr.Add("Бравый авиатор:\n\n");
            playersDescr.Add("Ну не красавец ли? Это у нас:\n\n");
        }

        private void InitializateSlogans()
        {
            slogans.Add("КТО МЫ - МЫ МАИ, ДИКОЕ МАИ!");
            slogans.Add("МАИ - ЭТО Я!\n" +
                        "МАИ - ЭТО МЫ!\n" +
                        "МАИ - ЭТО ЛУЧШИЕ ЛЮДИ СТРАНЫ!");
            slogans.Add("ЗА ВУЗ РОДНОЙ ОДНОЙ СТЕНОЙ!");
        }
    }
}
