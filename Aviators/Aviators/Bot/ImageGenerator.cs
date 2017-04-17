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
using Aviators.Properties;

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

        public string GameStat(Game game, bool isLastGame = false)
        {
            Image bitmap = Image.FromFile("Images\\Blanks\\gameStat.jpg");

            var enemyFont = new Font(statFonts.Families[2], 24);
            var viewersFont = new Font(statFonts.Families[1], 45);
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
               

                #region Зрители
                var onGame = new Rectangle(598, 115, 400, 50);
                var sOnGame = "На матче присутствовало";

                var viewers = new Rectangle(598, 160, 400, 50);
                //var sViewers = kek.Next(1, 3000).ToString();
                var sViewers = game.Viewers.ToString();
                sViewers += " зрителей";

                g.DrawString(sOnGame, viewersFont, Brushes.White, onGame, rightFormat);
                g.DrawString(sViewers, viewersFont, Brushes.White, viewers, rightFormat);
                //g.DrawRectangle(Pens.Red, viewers);
                //g.DrawRectangle(Pens.Red, onGame);

                #endregion

                #region Соперник
                var enemy = new Rectangle(310, 24, 350, 30);
                var enemyLogo = new Rectangle(395, 50, 130, 130);
                var enemyName = game.Team2;
                Image enLogo;

                //g.DrawString("ХК ДИНАМО", enemyFont, Brushes.White, enemy, leftFormat);
                g.DrawString(enemyName, enemyFont, Brushes.White, enemy, leftFormat);

                
                enLogo = getTeamLogo(enemyName);

                if (enLogo != null)
                {
                    
                    //g.DrawRectangle(Pens.Red, enemyLogo);

                    var resRect = GetInscribed(enemyLogo, enLogo.Size);

                    //g.DrawRectangle(Pens.Red, resRect);

                    g.DrawImage(enLogo, resRect);
                }


                //g.DrawRectangle(Pens.Red, enemy);

                #endregion

                #region Место + дата
                var place = new Rectangle(500, 23, 500, 43);
                var date = new Rectangle(800, 55, 200, 60);


                //g.DrawString(game.Place.Name, placeFont, Brushes.White, place, rightFormat);
                g.DrawString(game.Date.ToString("dd.MM.yyyy"), dateFont, Brushes.White, date, rightFormat);
                g.DrawString("ФОК \"Мещерский\", г. Н. Новгород", placeFont, Brushes.White, place, rightFormat);

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
                        g.DrawString(arrOfStat[i, j].ToString(), statFont, Brushes.Black, tmp, centerFormat);

                    }
                }

                #endregion

                #region Следующий матч или лого

                if (isLastGame)
                {


                    var nextGame = new Rectangle(47, 865, 404, 115);

                    g.DrawString("ХК АВИАТОРЫ МАИ - ХК РЭУ\n" +
                                 "21,05,2015 | 20:30\n" +
                                 "ЛД МЕДВЕДКОВО", nextGameFont, Brushes.White, nextGame, centerFormat);
                    //g.DrawRectangle(Pens.Red, viewers);


                    //g.DrawRectangle(Pens.Red, aviGoals);
                    //g.DrawRectangle(Pens.Red, enemyGoals);
                }
                else
                {
                    Image logo;

                    if (game.Tournament != null)
                    {
                     
                        logo = getTournamentLogo(game.Tournament.Name);

                        if (logo != null)
                        {
                            Rectangle needRect = new Rectangle(50, 810, 410, 180);
                            //g.DrawRectangle(Pens.Red, needRect);

                            var resRect = GetInscribed(needRect, logo.Size);

                            //g.DrawRectangle(Pens.Red, resRect);

                            g.DrawImage(logo, resRect);
                        }
                    }

                }
                #endregion

                #region Лучший игрок

                var rectToDraw = new Rectangle(785,645,220,220);
                int ramka = 8;
                DrawImageInCircle(g, new Bitmap("DB\\PlayersPhoto\\88_сорокин.jpg"), rectToDraw, ramka);

                //g.DrawImage(playerCircle, 500, 500);

                #endregion
            }

            var file = $"Images\\game_as1.jpg";
            bitmap.Save(file, ImageFormat.Jpeg);
            return file;
        }
        private Image getTournamentLogo(string name)
        {
            if (name == "МСХЛ. Плей-офф")
                name = "МСХЛ";

            var logoPath = $@"Images\Logo\{name}_logo.png";

            if (File.Exists(logoPath))
                return Image.FromFile(logoPath);
           
           return null;
        }

        private Image getTeamLogo(string name)
        {
            var logoPath = $@"Images\Teams\{name}.png";

            if (File.Exists(logoPath))
                return Image.FromFile(logoPath);

            return null;
        }

        private Rectangle GetInscribed(Rectangle baseRect, Size inputsize)
        {
            Rectangle resRect = baseRect;

            //соотнашение сторон
            float ratio = inputsize.Width / (float) inputsize.Height;

            int height = baseRect.Height;
            int width = (int) (height * ratio);

            if (width > baseRect.Width)
            {
                width = baseRect.Width;
                height = (int) (width / ratio);
            }

            var x = baseRect.X + baseRect.Width / 2 - width / 2;
            var y = baseRect.Y + baseRect.Height / 2 - height / 2;

            resRect = new Rectangle(x, y, width, height);
            
            return resRect;
        }

        public Bitmap CropToSize(Bitmap srcImage, Size size)
        {
            Bitmap dstImage = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(dstImage);
            
            var destRect = new Rectangle(0, 0, size.Width, size.Height);
            var srcRect = new Rectangle(0, 0, srcImage.Width, srcImage.Width);

            g.DrawImage(srcImage, destRect, srcRect, GraphicsUnit.Pixel);
            //g.DrawEllipse(Pens.Red, 10, 10, 50, 50);

            //g.FillPath(bra, path);
            return dstImage;
            //return srcImage.Clone(srcRect, srcImage.PixelFormat);
        }

        private void DrawImageInCircle(Graphics g, Bitmap bitmap, Rectangle rectToDraw, int ramka)
        {
            var rectsize = new Rectangle(0, 0, rectToDraw.Width - ramka, rectToDraw.Height - ramka);


            Bitmap playerCircle = CropToSize(bitmap, rectToDraw.Size);
            //playerCircle.Save("Images\\circle.jpg", ImageFormat.Jpeg);

            var bra = new TextureBrush(playerCircle, rectsize);

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(rectsize);

            g.TranslateTransform(rectToDraw.X + ramka / 2, rectToDraw.Y + ramka / 2);
            g.FillPath(bra, path);
            g.TranslateTransform(-rectToDraw.X - ramka / 2, -rectToDraw.Y - ramka / 2);
        }
    }
}
