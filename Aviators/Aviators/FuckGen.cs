using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
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
            fucks.Add("Вот красавчик, которого вы искали:\n\n");
            fucks.Add("Бравый авиатор:\n\n");
            fucks.Add("Ну не красавец ли? Это у нас:\n\n");
        }
    }
}
