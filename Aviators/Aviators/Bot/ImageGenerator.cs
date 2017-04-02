using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aviators
{
    public class ImageGenerator
    {
        PrivateFontCollection statFonts;

        public ImageGenerator ()
        {
            //Добавляем шрифт из указанного файла в em.Drawing.Text.PrivateFontCollection
            statFonts = new PrivateFontCollection();
            statFonts.AddFontFile("Fonts/MyriadPro-Cond.otf");
            statFonts.AddFontFile("Fonts/MyriadPro-Regular.otf");
            statFonts.AddFontFile("Fonts/MyriadPro-Semibold.otf");
            statFonts.AddFontFile("Fonts/SegoeUILight.ttf");
            statFonts.AddFontFile("Fonts/seguisb.ttf");
        }

        public string GenOxy(string pathToPhoto)
        {
            var str = (new StreamReader(pathToPhoto)).BaseStream;
            Bitmap bitmap = new Bitmap(str);

            var ssss = @"";

            var spl = ssss.Split(',');

            Random r = new Random();
            int i = r.Next(spl.Length);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawString(spl[i].Trim().ToUpper(), new Font(FontFamily.GenericSansSerif, 30), Brushes.Chartreuse, 50, 600);
            }

            var file = "temp.jpg";
            bitmap.Save(file, ImageFormat.Jpeg);
            return file;
        }

        public string GameStat(Game game)
        {
            Image bitmap = Image.FromFile("Images\\stata_shl2.jpg");

            var enemyFont = new Font(statFonts.Families[2], 24);
            var viewersFont = new Font(statFonts.Families[2], 38);
            var placeFont = new  Font(statFonts.Families[1], 38);
            var dateFont = new Font(statFonts.Families[1], 50);
            var scoreFont = new Font("Segoe UI", 56);
            var statFont = new Font(statFonts.Families[4], 42);
            var nextGameFont = new Font(statFonts.Families[3], 25);



            using (Graphics g = Graphics.FromImage(bitmap))
            {
                StringFormat centerFormat = new StringFormat();
                centerFormat.Alignment = StringAlignment.Center;
                centerFormat.LineAlignment = StringAlignment.Center;

                StringFormat leftFormat = new StringFormat();
                leftFormat.Alignment = StringAlignment.Near;
                leftFormat.LineAlignment = StringAlignment.Near;

                StringFormat rightFormat = new StringFormat();
                rightFormat.Alignment = StringAlignment.Far;
                rightFormat.LineAlignment = StringAlignment.Far;

                var kek = new Random();

                #region Зрители
                var viewers = new Rectangle(720, 155, 100, 45);

                g.DrawString(kek.Next(1, 3000).ToString(), viewersFont, Brushes.White, viewers, rightFormat);
                //g.DrawRectangle(Pens.Red, viewers);
                #endregion

                #region Соперник
                var enemy = new Rectangle(310, 24, 350, 30);

                g.DrawString("ХК ДИНАМО", enemyFont, Brushes.White, enemy, leftFormat);
                //g.DrawRectangle(Pens.Red, enemy);
                
                #endregion

                #region Место + дата
                var place = new Rectangle(543, 73, 458, 40);
                var date = new Rectangle(600, 20, 400, 60);


                //g.DrawString("г. Зеленоград, ФОК \"Ледовый\"", placeFont, Brushes.White, place, rightFormat);
                g.DrawString("г. Москва, ЛД \"Медведково\"", placeFont, Brushes.White, place, rightFormat);
                g.DrawString("21.12.17", dateFont, Brushes.White, date, rightFormat);

                //g.DrawRectangle(Pens.Red, place);
                //g.DrawRectangle(Pens.Red, date);

                #endregion

                #region Счет
                var aviPucks = new Rectangle(167, 75, 90, 90); // Х У вернего левого, ширина высота
                var enemyPucks= new Rectangle(300, 75, 90, 90);

                //TextRenderer.DrawText(g, "dfgdf", Myraid, aviPucks,Color.Red);

                if (game.Score != null)
                {
                    g.DrawString(game.Score.Item1.ToString(), scoreFont, Brushes.White, aviPucks, centerFormat);
                    g.DrawString(game.Score.Item2.ToString(), scoreFont, Brushes.White, enemyPucks, centerFormat);
                }

                //g.DrawString(kek.Next(1, 50).ToString(), scoreFont, Brushes.White, aviPucks, centerFormat);
                //g.DrawString(kek.Next(1, 50).ToString(), scoreFont, Brushes.White, enemyPucks, centerFormat);
#endregion

                #region Статистика
                var statY = 240;
                var statX = 15;

                //var arrOfStat = new int[6, 2]
                //{
                //    {game.Stat1.Shots, game.Stat2.Shots},
                //    {game.Stat1.ShotsIn, game.Stat2.ShotsIn},
                //    {game.Stat1.Faceoff, game.Stat2.Faceoff},
                //    {game.Stat1.Hits, game.Stat2.Shots},
                //    {game.Stat1.Penalty, game.Stat2.Penalty},
                //    {game.Stat1.BlockShots, game.Stat2.BlockShots},
                //};

                var arrOfStat = new int[6, 2]
                {
                    {kek.Next(1, 50), kek.Next(1, 50)},
                     {kek.Next(1, 50), kek.Next(1, 50)},
                      {kek.Next(1, 50), kek.Next(1, 50)},
                       {kek.Next(1, 50), kek.Next(1, 50)},
                        {kek.Next(1, 50), kek.Next(1, 50)},
                         {kek.Next(1, 50), kek.Next(1, 50)}
                };


                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var tmp = new Rectangle(statX + j * 399, statY + i * 96, 80, 80);
                        g.DrawString(arrOfStat[i, j].ToString(), statFont, Brushes.Black, tmp, centerFormat);

                    }
                }

                #endregion

                #region Зрители
                var nextGame = new Rectangle(47, 865, 404, 115);

                g.DrawString("ХК АВИАТОРЫ МАИ - ХК РЭУ\n" +
                             "21,05,2015 | 20:30\n" +
                             "ЛД МЕДВЕДКОВО", nextGameFont, Brushes.White, nextGame, centerFormat);
                //g.DrawRectangle(Pens.Red, viewers);
                #endregion


                //g.DrawRectangle(Pens.Red, aviGoals);
                //g.DrawRectangle(Pens.Red, enemyGoals);


            }

            var file = $"Images\\game_as1.jpg";
            bitmap.Save(file, ImageFormat.Jpeg);
            return file;
        }

    }
}
