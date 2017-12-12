using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace Aviators
{
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
                Name = ""; //õç
            }
        }

    }
}