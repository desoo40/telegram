using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.Echo
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("297610365:AAG3yzYtC0XgLrQC0ong1qdJ5odMkqiGHno");

        static void Main(string[] args)
        {
            testApi();
        }

      
        static void testApi()
        {
            var me = Bot.GetMeAsync().Result;
            System.Console.WriteLine("Hello my name is " + me.FirstName);
        }
    }
}