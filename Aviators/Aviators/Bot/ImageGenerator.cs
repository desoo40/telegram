using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aviators
{
    public class ImageGenerator
    {
        public string GenOxy(string pathToPhoto)
        {
            var str = (new StreamReader(pathToPhoto)).BaseStream;
            Bitmap bitmap = new Bitmap(str);

            var ssss = @"Говно, залупа, пенис, хер, давалка, хуй, блядина,
Головка, шлюха, жопа, член, еблан, петух, мудила,
Рукоблуд, ссанина, очко, блядун, вагина,
Сука, ебланище, влагалище, пердун, дрочила,
Пидор, пизда, туз, малафья, гомик, мудила, пилотка, манда,
Анус, вагина, путана, педрила, шалава, хуила, мошонка, елда";

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
            Image bitmap = Image.FromFile("Images\\stata_shl.jpg");

            var Segoe56 = new Font("Segoe UI", 56);
            var Segoe = new Font("Segoe UI Semibold", 42);

            var Myraid = new Font("Myriad Pro", 42);
            var Segoe36 = new Font("Segoe UI Semibold", 34, FontStyle.Regular);


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

                var viewers = new Rectangle(740, 165, 100, 25);

                g.DrawString("300", Segoe36, Brushes.White, viewers, rightFormat);

                var aviPucks = new Rectangle(167, 75, 90, 90); // Х У вернего левого, ширина высота
                var enemyPucks= new Rectangle(300, 75, 90, 90);


                if (game.Score != null)
                {
                    g.DrawString(game.Score.Item1.ToString(), Segoe56, Brushes.White, aviPucks, centerFormat);
                    g.DrawString(game.Score.Item2.ToString(), Segoe56, Brushes.White, enemyPucks, centerFormat);
                }
                g.DrawString("56", Segoe56, Brushes.White, aviPucks, centerFormat);
                g.DrawString("0", Segoe56, Brushes.White, enemyPucks, centerFormat);

                var statY = 240;
                var statX = 15;

                var arrOfStat = new int[6, 2]
                {
                    {game.Stat1.Shots, game.Stat2.Shots},
                    {game.Stat1.ShotsIn, game.Stat2.ShotsIn},
                    {game.Stat1.Faceoff, game.Stat2.Faceoff},
                    {game.Stat1.Hits, game.Stat2.Shots},
                    {game.Stat1.Penalty, game.Stat2.Penalty},
                    {game.Stat1.BlockShots, game.Stat2.BlockShots},
                };


                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var tmp = new Rectangle(statX + j * 399, statY + i * 96, 80, 80);
                        g.DrawString(arrOfStat[i, j].ToString(), Segoe, Brushes.Black, tmp, centerFormat);

                    }
                }


                //g.DrawRectangle(Pens.Red, aviGoals);
                //g.DrawRectangle(Pens.Red, enemyGoals);


            }

            var file = $"Images\\game_as.jpg";
            bitmap.Save(file, ImageFormat.Jpeg);
            return file;
        }

    }
}
