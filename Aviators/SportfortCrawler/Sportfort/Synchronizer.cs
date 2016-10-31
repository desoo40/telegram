using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SportfortCrawler
{
    public class Synchronizer
    {
        public static void InitializateSources()
        {
            Console.WriteLine("PageParsers.Initializate...");
            PageParsers.Initializate();

            Console.WriteLine("InitializatePlayersInfo...");
            //InitializatePlayersInfo();

            Console.WriteLine("InitializateGamesTrainingInfo...");
            //InitializateGamesTrainingInfo();

            Console.WriteLine("InitializateLastGamesInfo...");
            InitializateLastGamesInfo();
        }
        public static void InitializateLastGamesInfo()
        {
            Console.WriteLine("ParseLastGamesHomePage...");
            var games = PageParsers.ParseLastGamesHomePage(Configs.Config.SportFortHomePage);
            var gamesTxt = "";

            Console.WriteLine("Parsed " + games.Count + " games");
            foreach (var game in games)
            {
                gamesTxt += $"{game}\n";
            }

            if (gamesTxt == "") return;

            using (var stream = new StreamWriter(Configs.Config.DBGamesInfoPath))
            {
                stream.Write(gamesTxt);
            }
        }

        public static void InitializateGamesTrainingInfo()
        {
            Console.WriteLine("ParseHomePage...");
            var events = PageParsers.ParseHomePage(Configs.Config.SportFortHomePage);
            var eventsTxt = "";

            Console.WriteLine("Parsed " + events.Count + " events");
            foreach (var even in events)
            {
                //type= Игра; 
                var type = even.Type.Trim();

                //date= 30 октября;
                //time= 11:00;
                var date = even.Data.Split('\n')[0].Split(',')[0].Trim();
                var time = even.Data.Split('\n')[0].Split(',')[1].Trim();
                eventsTxt += $"type={type};date={date};time={time};";

                var other = even.Data.Split('\n');
                 //дата, время
                 //Янтарь
                 //г.Москва, ул.Маршала Катукова, д.26
                 //Сезон 2016 - 2017 дивизион КБЧ-Восток
                 //Янтарь - 2
                 //Wild Woodpeckers
                 //Будут: 10
                 //Возможно: 2
                 //Не будут: 0
                 //
                //place= Янтарь;
                //address= г.Москва, ул.Маршала Катукова, д.26; details = Сезон 2016 - 2017 дивизион КБЧ-Восток % Янтарь - 2 Wild Woodpeckers%% Будут:1 % Возможно:1 % Не будут: 1;
                for(var i=0; i<other.Length;++i)
                {
                    if (other[i] == "") continue;
                    if (i == 0) continue;
                    if (i == 1)
                    {
                        eventsTxt += $"place={other[i].Trim()};";
                        continue;
                    }
                    if (i == 2)
                    {
                        eventsTxt += $"address={other[i].Trim()};details=";
                        continue;
                    }

                    if (i == other.Length - 4)
                    {
                        eventsTxt += $"%";
                    }

                    if (i >= other.Length - 4)
                    {
                        eventsTxt += $"{other[i].Trim()}%";
                        continue;
                    }

                    eventsTxt += $"{other[i].Trim()}%";
                }
                eventsTxt += $";";
                
                //be= Игорь Смирнов;
                foreach(var be in even.MembersBe)
                {
                    eventsTxt += $"be={be.Trim()};";
                }
                //maybe= Латохин Дмитрий;
                foreach (var be in even.MembersMayBe)
                {
                    eventsTxt += $"maybe={be.Trim()};";
                }
                //notbe= Скалин Петр
                foreach (var be in even.MembersNotBe)
                {
                    eventsTxt += $"notbe={be.Trim()};";
                }

                eventsTxt += $"\n";
            }

            if (eventsTxt == "") return;


            using (var stream = new StreamWriter(Configs.Config.DBEventsInfoPath))
            {
                stream.Write(eventsTxt);
            }
        }

    public static void InitializatePlayersInfo()
        {
            Console.WriteLine("ParseTeamMembersPage...");
            var members = PageParsers.ParseTeamMembersPage(Configs.Config.SportFortTeamMembersPage);
            var players = "";

            Console.WriteLine("Parsed " + members.Count + " members");
            foreach (var member in members)
            {
                Console.WriteLine(member);

                var fields = member.Split(';');
                var keyValueFields = new List<KeyValuePair<string, string>>();
                foreach (var field in fields)
                {
                    if (!field.Contains('=')) continue;
                    var keyValue = field.Split('=');
                    keyValueFields.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                }
                var name = keyValueFields.Where(x => x.Key == "name").Select(x => x.Value).First();
                var surname = keyValueFields.Where(x => x.Key == "surname").Select(x => x.Value).First();
                var numberValues = keyValueFields.Where(x => x.Key == "number").Select(x => x.Value);
                var number = "0";
                if (numberValues.Count() != 0) number = numberValues.First();
                players += $"{number};{surname};{name}\n";

                var photoUrl = keyValueFields.Where(x => x.Key == "photo").Select(x => x.Value).First();

                using (var web = new WebClient())
                {
                    try
                    {
                        web.DownloadFile(photoUrl, Path.Combine(Configs.Config.DBPlayersPhotoDirPath, $"{number}_{surname.ToLower()}.jpg"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Can't download photo of member: " + ex.Message + "\n" + ex.InnerException);
                    }
                }
            }

            if (players == "") return;

            using (var stream = new StreamWriter(Configs.Config.DBPlayersInfoPath))
            {
                stream.Write(players);
            }
        }

        public static void UpdateSources()
        {

        }
    }
}
