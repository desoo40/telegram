using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstBot
{
    public class FuckGen
    {
        private readonly List<string> fucks = new List<string>();
        private readonly Random random = new Random();

        public FuckGen()
        {
            InitializateFucks();
        }

        public string GetFuck()
        {
            var index = random.Next(fucks.Count);
            return fucks[index];
        }

        private void InitializateFucks()
        {
            fucks.Add("Пошёл нахуй!");
            fucks.Add("Соси");
            fucks.Add("Fuck you!");
            fucks.Add("Ты залупа!");
            fucks.Add("А может ты залупоед?");
            fucks.Add("Знаешь что я делал с твоей мамкой?");
            fucks.Add("Подрочи, полегчает.");
        }
    }
}
