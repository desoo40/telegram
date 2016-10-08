using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstBot
{
    class Class1
    {
        static void Main(string[] args)
        {
            testApi();
        }

        static void testApi()
        {
            var Bot = new Telegram.Bot.Api("297610365:AAG3yzYtC0XgLrQC0ong1qdJ5odMkqiGHno");
            var me = Bot.GetMeAsync().Result;
            System.Console.WriteLine("Hello my name is " + me.FirstName);
        }
    }
}
