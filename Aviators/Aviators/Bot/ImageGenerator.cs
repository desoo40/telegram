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
        PrivateFontCollection rosterFonts;

        public ImageGenerator ()
        {
            //Добавляем шрифт из указанного файла в em.Drawing.Text.PrivateFontCollection
            statFonts = new PrivateFontCollection();

            statFonts.AddFontFile("Fonts/MyriadPro-Cond.otf");
            statFonts.AddFontFile("Fonts/MyriadPro-Regular.otf");
            statFonts.AddFontFile("Fonts/MyriadPro-Semibold.otf");
            statFonts.AddFontFile("Fonts/SegoeUILight.ttf");
            statFonts.AddFontFile("Fonts/seguisb.ttf");

            rosterFonts = new PrivateFontCollection();
            rosterFonts.AddFontFile("Fonts/segoeui.ttf");
            rosterFonts.AddFontFile("Fonts/tahoma.ttf");

        }
        void DrawOutlineText(Graphics g, String text, Font font, Rectangle r, Brush b)
        {
            // set atialiasing
            g.SmoothingMode = SmoothingMode.HighQuality;

            // make thick pen for outlining
            Pen pen = new Pen(Color.Black, 3);
            // round line joins of the pen
            pen.LineJoin = LineJoin.Round;

            // create graphics path
            GraphicsPath textPath = new GraphicsPath();

            // convert string to path
            textPath.AddString(text, font.FontFamily, (int)font.Style, font.Size, r, StringFormat.GenericTypographic);

            // clone path to make outlining path
            GraphicsPath outlinePath = (GraphicsPath)textPath.Clone();

            // outline the path
            outlinePath.Widen(pen);

            // fill outline path with some color
            g.FillPath(Brushes.Black, outlinePath);
            // fill original text path with some color
            g.FillPath(b, textPath);
        }

        public string Roster(Game game)
        {
            Image bitmap = Image.FromFile("Images\\Blanks\\roster.jpg");

            var dateFont = new Font(rosterFonts.Families[0], 52, FontStyle.Bold);

            var numberFont = new Font(rosterFonts.Families[1], 29);
            var nameFont = new Font(rosterFonts.Families[1], 26, FontStyle.Bold);
            var numberColor = new SolidBrush(Color.FromArgb(9, 55, 143));



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

                #region Лого турнира
                Image logo;

                if (game.Tournament != null)
                {

                    logo = getTournamentLogo(game.Tournament.Name);

                    if (logo != null)
                    {
                        Rectangle needRect = new Rectangle(355, 28, 115, 115);
                        //g.DrawRectangle(Pens.Red, needRect);

                        var resRect = GetInscribed(needRect, logo.Size);

                        //g.DrawRectangle(Pens.Red, resRect);

                        g.DrawImage(logo, resRect);
                    }
                }
                #endregion

                #region Дата + описание

                Rectangle dateRect = new Rectangle(445, 25, 300, 50);
                Rectangle descrRect = new Rectangle(445, 55, 300, 100);
                var description = game.Description;
                var descrFont = new Font(rosterFonts.Families[0], 72, FontStyle.Bold);

                g.DrawString(game.Date.ToString(), dateFont, Brushes.White, dateRect, centerFormat);
                //DrawOutlineText(g, "Финал", descrFont, descrRect, Brushes.White);
                g.DrawString(description, descrFont, Brushes.White, descrRect, centerFormat);

                //g.DrawRectangle(Pens.Crimson,dateRect);
                #endregion
                #region Лого противника

                Image enLogo;
                var enemyName = game.Team2;
                var enemyLogo = new Rectangle(970, 40, 120, 120);
                enLogo = getTeamLogo(enemyName);

                if (enLogo != null)
                {

                    //g.DrawRectangle(Pens.Red, enemyLogo);

                    var resRect = GetInscribed(enemyLogo, enLogo.Size);

                    //g.DrawRectangle(Pens.Red, resRect);

                    g.DrawImage(enLogo, resRect);
                }
                #endregion

                
                #region Состав
                
                var roster = game.Roster;
                var sizePhoto = new Size(132, 132);
                var sizeNum = new Size(70, 50);
                var sizeName = new Size(200, 100);
                
                for (int i = 0; i < roster.Count; ++i)
                {
                    var point = GetPointOfPlayer(i);
                    var player = roster[i];

                    var shiftNumX = 125;
                    var shiftNumY = 5;

                    var shiftNameY = 128;

                    Rectangle rectToDraw;
                    Rectangle numRect;
                    Rectangle nameRect;

                    if (i < 2)
                    {
                        var distNumXG = 180;

                        var distNameXG = 300;

                        var shfitNameX = 150;
                        var shfitNameY = 60;

                        var shfitNumX = 50;
                        var shfitNumY = 5;

                        rectToDraw = new Rectangle(point, sizePhoto);
                        numRect = new Rectangle(point.X + distNumXG * i - shfitNumX, point.Y + shfitNumY, sizeNum.Width, sizeNum.Height);
                        nameRect = new Rectangle(point.X + distNameXG * i - shfitNameX, point.Y + shfitNameY, sizeName.Width, sizeName.Height);
                    }
                    else
                    {
                        rectToDraw = new Rectangle(point, sizePhoto);
                        numRect = new Rectangle(point.X + shiftNumX, point.Y + shiftNumY, sizeNum.Width, sizeNum.Height);
                        nameRect = new Rectangle(point.X, point.Y + shiftNameY, sizeName.Width, sizeName.Height);
                    }

                    string path = $"DB\\PlayersPhoto\\{player.Number}_{player.Surname}.jpg";

                    if (!File.Exists(path))
                        path = $"DB\\PlayersPhoto\\no_photo.jpg";


                    DrawImageInCircle(g, new Bitmap(path), rectToDraw, 0);
                    DrawOutlineText(g, $"#{player.Number}", numberFont, numRect, numberColor);
                    DrawOutlineText(g, $"{player.Name}\n{player.Surname}", nameFont, nameRect, Brushes.White);
                }
                #endregion
            }

            var file = $"Images\\roster.jpg";

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            bitmap.Save(file, jpgEncoder, myEncoderParameters);
            return file;
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
            var bestPlayerFont = new Font(statFonts.Families[1], 45);


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


                g.DrawString(game.Place.Name, placeFont, Brushes.White, place, rightFormat);
                g.DrawString(game.Date.ToString("dd.MM.yyyy"), dateFont, Brushes.White, date, rightFormat);

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

                #region Заброшенные шайбы

                var k = 0;
                var fontSize = 25;

                if (game.Goal.Count > 12)
                {
                    fontSize -= 5;
                }
                var pucksFont = new Font(statFonts.Families[3], fontSize, FontStyle.Bold);

                foreach (var goal in game.Goal)
                {
                    var pucksRectangle = new Rectangle(580, k * (fontSize + 2) + 282, 420, 33);
                    var goalString = "• " + goal.Author.Surname;

                    if (goal.Assistant1 != null)
                    {
                        goalString += " (";
                        goalString += goal.Assistant1.Surname;

                        if (goal.Assistant2 != null)
                        {
                            goalString += ", ";
                            goalString += goal.Assistant2.Surname;
                        }
                        goalString += ")";
                    }
                     
                    g.DrawString(goalString, pucksFont, Brushes.White, pucksRectangle, leftFormat);
                    ++k;
                }

                #endregion

                #region Лучший игрок

                var bestPlayer = game.BestPlayer;

                if (bestPlayer == null)
                    bestPlayer = new Player(100, "Алексей", "Данилин");

                string bestStat =
                    $"{game.Goal.Count(go => go.Author.Id == bestPlayer.Id)}+{game.Goal.Count(go => go.Assistant1 != null && go.Assistant1.Id == bestPlayer.Id) + game.Goal.Count(go => go.Assistant2 != null && go.Assistant2.Id == bestPlayer.Id)}";

                var arrOfBestPlayerAttributes = new string[4]
                {
                    bestPlayer.Name,
                    bestPlayer.Surname,
                    "#" + bestPlayer.Number,
                    bestStat
                };

                var bestX = 598;
                var bestY = 700;

                for (int i = 0; i < 4; i++)
                {
                    var tmp = new Rectangle(bestX, bestY + i * 41, 200, 80);
                    g.DrawString(arrOfBestPlayerAttributes[i], bestPlayerFont, Brushes.White, tmp, leftFormat);
                }

                //Image playerCircle = CropToCircle(Image.FromFile("DB\\PlayersPhoto\\1_черненков.jpg"), Color.FromArgb(0,0,0));

                //g.DrawImage(playerCircle, 500, 500);

                var rectToDraw = new Rectangle(785,645,220,220);
                int ramka = 8;
                DrawImageInCircle(g, new Bitmap($"DB\\PlayersPhoto\\{bestPlayer.Number}_{bestPlayer.Surname}.jpg"), rectToDraw, ramka);

                #endregion
            }

            var file = $"Images\\game_as1.jpg";

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            bitmap.Save(file, jpgEncoder, myEncoderParameters);
            return file;
        }

        private Point GetPointOfPlayer(int ind)
        {
            var point = new Point();

            var startX = 36;
            var startY = 290;

            var distX = 190;
            var distY = 196;

            var kekterval = 0;

            if (ind < 2)
            {
                var distXGoalie = 180;
                var startXGoalie = 420;
                var startYGoalie = 150;

                point.X = startXGoalie + ind*distXGoalie;
                point.Y = startYGoalie;
            }
            else
            {
                var column = (ind - 2)%5;
                var row = (ind - 2)/5;

                if (column > 2)
                    kekterval = 140;

                point.X = startX + distX*column + kekterval;
                point.Y = startY + distY*row;
            }

            return point;
        }


        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
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
