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

namespace SportfortCrawler
{
    class PageParsers
    {
        private static bool finishedLoading = false;
        private static WebConfig config = new Awesomium.Core.WebConfig();

        internal static void Initializate()
        {
            try
            {
                Awesomium.Core.WebCore.Initialize(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        internal static List<string> ParseTeamMembersPage(string page)
        {
            var members = new List<string>();

            using (WebView view = WebCore.CreateWebView(800, 600))
            {
                view.Source = new Uri(page);

                finishedLoading = false;
                view.LoadingFrameComplete += (s, e) =>
                {
                    if (e.IsMainFrame)
                        finishedLoading = true;
                };

                while (!finishedLoading)
                {
                    Thread.Sleep(100);
                    WebCore.Update();
                }

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

                using (var stream = new StreamWriter("members"))
                {
                    var result = "";
                    foreach (var member in members)
                    {
                        result += member + "\n";
                    }
                    stream.Write(result + "\n");
                }
            }

            return members;
        }

        internal static List<Event> ParseHomePage(string sportFortHomePage)
        {
            var events = new List<Event>();

            using (WebView view = WebCore.CreateWebView(800, 600))
            {
                view.Source = new Uri(sportFortHomePage);

                finishedLoading = false;
                view.LoadingFrameComplete += (s, e) =>
                {
                    if (e.IsMainFrame)
                        finishedLoading = true;
                };

                while (!finishedLoading)
                {
                    Thread.Sleep(100);
                    WebCore.Update();
                }

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
                    for (int i=0; i< nodes.Count; ++i)
                    {
                        if (nodes[i].Name != "div") continue;

                        var attr = nodes[i].Attributes["class"];
                        if(attr != null && attr.Value == "sf_game sf_text_center" && (i+4) < nodes.Count)
                        {
                            var even = new Event();
                            even.Type = "Игра";

                            var gameData = "";
                            var gameDataSource = (nodes[i].InnerText + nodes[i+4].InnerText).Split('\n');
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
                            var gameDataSource = (nodes[i].InnerText + "\n" + nodes[i+2].InnerText).Split('\n');
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

                using (var stream = new StreamWriter("parsedEvents"))
                {
                    var result = "";
                    foreach (var even in events)
                    {
                        result += "----------------\n";
                        result += even.Type + ":\n";
                        result += even.Data + "\n";
                        result += "Будут:\n";
                        foreach(var member in even.MembersBe) result += member + "\n";
                        result += "Возможно:\n";
                        foreach (var member in even.MembersMayBe) result += member + "\n";
                        result += "Не будут:\n";
                        foreach (var member in even.MembersNotBe) result += member + "\n";
                    }

                    stream.Write(result);
                }
            }

            return events;
        }
    }
}
