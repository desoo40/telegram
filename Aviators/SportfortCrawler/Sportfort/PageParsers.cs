using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core;
using System.Threading;
using System.IO;
using HtmlAgilityPack;
using SportfortCrawler.Configs;

namespace SportfortCrawler
{
    class PageParsers
    {
        private static bool finishedLoading = false;
        private static bool documentReady = false;
        private static WebConfig config = new Awesomium.Core.WebConfig();
        public static readonly string UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
        private static WebView view;

        internal static void Initializate()
        {
            try
            {
                config.UserAgent = UserAgent;
                Awesomium.Core.WebCore.Initialize(config);
                var proxyConfig = String.Format("{0}:{1}", "hqproxy.avp.ru", 3128);
                var webPreferences = new WebPreferences { ProxyConfig = proxyConfig };
                var webSession = WebCore.CreateWebSession(webPreferences);
                view = WebCore.CreateWebView(800, 600, webSession);

                view.LoginRequest += (s, e) =>
                {
                    e.Username = @"kl\latokhin";
                    e.Password = Config.PWD;
                    e.Handled = EventHandling.Modal;
                    e.Cancel = false;
                };

                Console.WriteLine("Start testing ya.ru downloading...");
                view.SynchronousMessageTimeout = 0;
                view.Source = new Uri("https://ya.ru/");

                view.LoadingFrameComplete += (s, e) =>
                {
                    if (e.IsMainFrame)
                        finishedLoading = true;
                };

                while (!finishedLoading)
                {
                    Thread.Sleep(1000);
                    WebCore.Update();
                }

                finishedLoading = false;
                Console.WriteLine("Finished: " + view.HTML.Length + " symbols loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        internal static List<string> ParseTeamMembersPage(string page)
        {
            var members = new List<string>();

            Console.WriteLine("Start downloading...");
            view.SynchronousMessageTimeout = 0;
            view.Source = new Uri(page);

            view.LoginRequest += (s, e) =>
            {
                e.Username = @"kl\latokhin";
                e.Password = Config.PWD;
                e.Handled = EventHandling.Modal;
                e.Cancel = false;
            };

            view.LoadingFrameComplete += (s, e) =>
            {
                if (e.IsMainFrame)
                    finishedLoading = true;
            };

            while (!finishedLoading)
            {
                Thread.Sleep(1000);
                WebCore.Update();
            }

            finishedLoading = false;
            Console.WriteLine("Finished");
            //</ div >< div class="sf_roster_wrapper" data-groups="[&quot;all&quot;]">
            //<a href = "javascript:void(0)" onclick="showProfileCard(66110, event);" class="sf_roster_item" id="tm_roster_66110">
            //<div class="sf_img_cont">
            //<img class="sf_img" src="https://sportfort.blob.core.windows.net/images/edb81fbe9236454b9d0ed7c382277ba2_Medium.jpg" width="96">
            //</div>
            //
            //<div class="sf_roster_item_title sf_clearfix">
            //
            //<strong id = "tm_jersey_66110" > 69 </ strong >
            //Н
            //</ div >
            //
            //
            //< div class="sf_roster_item_title_name">
            //<div class="sf_roster_full_name">
            //Дмитрий<br>
            //Латохин
            //</div>
            //</div>
            //</a>

            var doc = new HtmlDocument();
            doc.LoadHtml(view.HTML);
            var elem = doc.DocumentNode.SelectNodes("//div[@class='sf_roster_wrapper']");
            if (elem != null)
            {
                var h = new HtmlDocument();
                foreach (HtmlNode HN in elem)
                {
                    h.LoadHtml(HN.InnerHtml);

                    var member = "";
                    var nameSurname = h.DocumentNode.Descendants("div").Select(x => x.Attributes["class"].Value == "sf_roster_full_name" ? x.InnerText.Trim('\n').Trim() : null)?.ToArray().First(x => x != null).Split('\n');
                    member += $"name={nameSurname[0].Trim()};surname={nameSurname[1].Trim()};";

                    var nodes = h.DocumentNode.Descendants("div").Select(x => x.Attributes["class"].Value == "sf_roster_item_title sf_clearfix" ? x.InnerText.Trim('\n').Trim() : null);
                    if (nodes != null && nodes.Count() != 0)
                    {
                        var numbers = nodes.ToArray();
                        foreach(var number in numbers)
                        {
                            if (number != null)
                            {
                                var temp = number.Replace('\n', ' ').Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                if (temp.Count() == 1)
                                {
                                    member += $"amplua={temp[0]};";
                                }
                                if (temp.Count() == 2)
                                {
                                    member += $"number={temp[0]};amplua={temp[1]};";
                                }
                                if (temp.Count() == 3)
                                {
                                    member += $"work={temp[0]};number={temp[1]};amplua={temp[2]};";
                                }
                                break;
                            }
                        }
                    }

                    var foto = h.DocumentNode.Descendants("img").Select(x => x.Attributes["src"].Value)?.ToArray().First(x => x != null) + "";
                    member += $"photo={foto};";
                    members.Add(member);
                }
            }

            view.Stop();

            using (var stream = new StreamWriter("members"))
            {
                var result = "";
                foreach (var member in members)
                {
                    result += member + "\n";
                }
                stream.Write(result + "\n");
            }

            return members;
        }
        internal static List<Event> ParseHomePage(string sportFortHomePage)
        {
            var events = new List<Event>();

            Console.WriteLine("Start downloading...");
            view.SynchronousMessageTimeout = 0;
            view.Source = new Uri(sportFortHomePage);

            view.LoginRequest += (s, e) =>
            {
                e.Username = @"kl\latokhin";
                e.Password = Config.PWD;
                e.Handled = EventHandling.Modal;
                e.Cancel = false;
            };

            view.LoadingFrameComplete += (s, e) =>
            {
                if (e.IsMainFrame)
                    finishedLoading = true;
            };

            view.DocumentReady += (s, e) =>
            {
                Console.WriteLine("Finished");

                using (var stream = new StreamWriter("events"))
                {
                    stream.Write(view.HTML);
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(view.HTML);

                var games = doc.DocumentNode.SelectNodes("//div[@id='nextGamesBlock']")?.First();
                if (games != null)
                {
                    var hgames = new HtmlDocument();
                    hgames.LoadHtml(games.InnerHtml);

                    var nodes = hgames.DocumentNode.ChildNodes;
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        if (nodes[i].Name != "div") continue;

                        var attr = nodes[i].Attributes["class"];
                        if (attr != null && attr.Value == "sf_game sf_text_center" && (i + 4) < nodes.Count)
                        {
                            var even = new Event();
                            even.Type = "Игра";

                            var gameData = "";
                            var gameDataSource = (nodes[i].InnerText + nodes[i + 4].InnerText).Split('\n');
                            foreach (var data in gameDataSource)
                            {
                                if (data == null || data == "" || data == " " || data == "?" || data == "&nbsp;" || data.Contains("Распечатать"))
                                {
                                    continue;
                                }
                                gameData += data + "\n";
                            }
                            even.Data = gameData;

                            var membersGoBe = nodes[i + 4].Descendants("a").
                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Будут:")).
                                Select(x => x.Attributes["title"].Value).ToArray();
                            var membersGoMayBe = nodes[i + 4].Descendants("a").
                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Возможно:")).
                                Select(x => x.Attributes["title"].Value).ToArray();
                            var membersGoNotBe = nodes[i + 4].Descendants("a").
                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Не будут:")).
                                Select(x => x.Attributes["title"].Value).ToArray();

                            even.MembersBe = membersGoBe.ToList();
                            even.MembersMayBe = membersGoMayBe.ToList();
                            even.MembersNotBe = membersGoNotBe.ToList();

                            events.Add(even);
                            i += 4;
                            continue;
                        }
                    }
                }

                var trains = doc.DocumentNode.SelectNodes("//div[@id='trainingsBlock']")?.First();
                if (trains != null)
                {
                    var hgames = new HtmlDocument();
                    hgames.LoadHtml(trains.InnerHtml);

                    var nodes = hgames.DocumentNode.ChildNodes;
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        if (nodes[i].Name != "div") continue;

                        var attr = nodes[i].Attributes["class"];
                        if (attr != null && attr.Value == "sf_game sf_text_center" && (i + 2) < nodes.Count)
                        {
                            var even = new Event();
                            even.Type = "Треня";

                            var gameData = "";
                            var gameDataSource = (nodes[i].InnerText + "\n" + nodes[i + 2].InnerText).Split('\n');
                            foreach (var data in gameDataSource)
                            {
                                if (data == null
                                    || data == ""
                                    || data == " "
                                    || data == "?"
                                    || data == "&nbsp;"
                                    || (data.Contains("Буду") && !data.Contains(":"))
                                    || (data.Contains("Не буду") && !data.Contains(":"))
                                    || (data.Contains("Возможно") && !data.Contains(":"))
                                    || data.Contains("Распечатать"))
                                {
                                    continue;
                                }
                                gameData += data + "\n";
                            }
                            even.Data = gameData;

                            var membersGoBe = nodes[i + 2].Descendants("a").
                                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Будут:")).
                                                Select(x => x.Attributes["title"].Value).ToArray();
                            var membersGoMayBe = nodes[i + 2].Descendants("a").
                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Возможно:")).
                                Select(x => x.Attributes["title"].Value).ToArray();
                            var membersGoNotBe = nodes[i + 2].Descendants("a").
                                Where(x => x.Attributes.Count > 0 && x.Attributes.Contains("class") && x.Attributes.Contains("title") && x.ParentNode.InnerText.Contains("Не будут:")).
                                Select(x => x.Attributes["title"].Value).ToArray();

                            even.MembersBe = membersGoBe.ToList();
                            even.MembersMayBe = membersGoMayBe.ToList();
                            even.MembersNotBe = membersGoNotBe.ToList();

                            events.Add(even);
                            i += 5;
                            continue;
                        }
                    }
                }
                view.Stop();

                using (var stream = new StreamWriter("parsedEvents"))
                {
                    var result = "";
                    foreach (var even in events)
                    {
                        result += "----------------\n";
                        result += even.Type + ":\n";
                        result += even.Data + "\n";
                        result += "Будут:\n";
                        foreach (var member in even.MembersBe) result += member + "\n";
                        result += "Возможно:\n";
                        foreach (var member in even.MembersMayBe) result += member + "\n";
                        result += "Не будут:\n";
                        foreach (var member in even.MembersNotBe) result += member + "\n";
                    }

                    stream.Write(result);
                }


                documentReady = true;
            };

            while (!finishedLoading||!documentReady)
            {
                Thread.Sleep(1000);
                WebCore.Update();
            }

            finishedLoading = false;
            documentReady = false;

            return events;

        }
        internal static List<string> ParseLastGamesHomePage(string sportFortHomePage)
        {
            var events = new List<string>();

            Console.WriteLine("Start downloading...");
            view.SynchronousMessageTimeout = 0;
            view.Source = new Uri(sportFortHomePage);

            view.LoginRequest += (s, e) =>
            {
                e.Username = @"kl\latokhin";
                e.Password = Config.PWD;
                e.Handled = EventHandling.Modal;
                e.Cancel = false;
            };

            view.LoadingFrameComplete += (s, e) =>
            {
                if (e.IsMainFrame)
                    finishedLoading = true;
            };

            while (!finishedLoading)
            {
                Console.WriteLine("Currectly Document Ready is " + view.IsDocumentReady);
                Thread.Sleep(1000);
                WebCore.Update();
            }

            finishedLoading = false;
            Console.WriteLine("Finished");
            Console.WriteLine("Check view Error: " + view.GetLastError().ToString());

            using (var stream = new StreamWriter("games"))
            {
                stream.Write(view.HTML);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(view.HTML);

            var games = doc.DocumentNode.SelectNodes("//*[@id='floatColumn']/section[3]/div")?.First();
                
            if (games != null)
            {
                var hgames = new HtmlDocument();
                hgames.LoadHtml(games.InnerHtml);

                var nodes = hgames.DocumentNode.ChildNodes;
                for (int i=0; i< nodes.Count; ++i)
                {
                    if (nodes[i].Name != "div") continue;

                    var attr = nodes[i].Attributes["class"];
                    if (attr != null && attr.Value == "sf_game sf_text_center")
                    {

                        var gameData = "";
                        var gameDataSource = (nodes[i].InnerText).Split('\n');
                        foreach (var data in gameDataSource)
                        {
                            if (data == null || data == "" || data == " " || data == "&nbsp;" || data.Contains("Распечатать") || data.Contains("Протокол") || data.Contains("протокола") || data.Contains("Сводная"))
                            {
                                continue;
                            }
                            gameData += data + "\n";
                        }

                        var result1 = "";
                        var result2 = "";
                        var fields = gameData.Split('\n').Where(x => x != "").ToList();
                        if (fields.Count == 8)
                        {
                            for (int y = 0; y < fields.Count; ++y)
                            {
                                if (y < 2)
                                {
                                    result2 += $"{fields[1].Trim()}, {fields[0].Trim()}";
                                    y += 2;
                                }

                                if (y > 3)
                                {
                                    result1 += $"{fields[y].Trim()} ";
                                    continue;
                                }

                                //result += $"{fields[y]}\n";
                            }
                        }
                        else
                        {
                            result1 = gameData;
                        }

                        events.Add($"*{result1}*%{result2}");
                        continue;
                    }
                }
            }

            view.Stop();

            using (var stream = new StreamWriter("parsedGames"))
            {
                var result = "";
                foreach (var even in events)
                {
                    result += even + "\n";
                }

                stream.Write(result);
            }

            return events;
        }

        internal static  List<string> ParseStats(string sportFortStat)
        {
            var stats = new List<string>();

            Console.WriteLine("Start downloading...");
            view.SynchronousMessageTimeout = 0;
            view.Source = new Uri(sportFortStat);

            view.LoginRequest += (s, e) =>
            {
                e.Username = @"kl\latokhin";
                e.Password = Config.PWD;
                e.Handled = EventHandling.Modal;
                e.Cancel = false;
            };

            view.LoadingFrameComplete += (s, e) =>
            {
                if (e.IsMainFrame)
                    finishedLoading = true;
            };

            while (!finishedLoading)
            {
                Console.WriteLine("Currectly Document Ready is " + view.IsDocumentReady);
                Thread.Sleep(1000);
                WebCore.Update();
            }

            finishedLoading = false;
            Console.WriteLine("Finished");
            Console.WriteLine("Check view Error: " + view.GetLastError().ToString());

            using (var stream = new StreamWriter("stats"))
            {
                stream.Write(view.HTML);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(view.HTML);

            var statTable = doc.DocumentNode.SelectNodes("//*[@id='tournamentContent']/div/table")?.First();

            if (statTable != null)
            {
                var hstat = new HtmlDocument();
                hstat.LoadHtml(statTable.InnerHtml);

                var nodes = hstat.DocumentNode.ChildNodes;
                for (int i = 0; i < nodes.Count; ++i)
                {
                    foreach(var ch in nodes[i].ChildNodes)
                    {
                        if(ch.Name == "tr")
                        {
                            var txt = ch.InnerText;
                            if (txt.Contains("Wild Woodpeckers"))
                                stats.Add(txt);
                        }
                    }
                }
            }

            return stats;
        }

    }
}
