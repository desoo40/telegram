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
        public static readonly string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.110 Safari/537.36";
        public static void InitializateSources()
        {
            PageParsers.Initializate();
            //InitializatePlayersInfo();
            InitializateGamesTrainingInfo();
        }
        public static void InitializateGamesTrainingInfo()
        {
            var events = PageParsers.ParseHomePage(Configs.Config.SportFortHomePage);
        }

        public static void InitializatePlayersInfo()
        {
            var members = PageParsers.ParseTeamMembersPage(Configs.Config.SportFortTeamMembersPage);
            var players = "";
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
