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
using Aviators.Bot;
using Aviators.Bot.ImageGenerator;
using Aviators.Properties;

namespace Aviators
{
    public class ImageGenerator2
    {
        PrivateFontCollection rosterFonts;
        TextHelper th = new TextHelper();

        public ImageGenerator2()
        {
            //Добавляем шрифт из указанного файла в em.Drawing.Text.PrivateFontCollection

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
            var assOrKfont = new Font(rosterFonts.Families[1], 50, FontStyle.Bold);



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
                int fontSize = (descrRect.Width + 100) / description.Length;
                var descrFont = new Font(rosterFonts.Families[0], fontSize, FontStyle.Bold);

                g.DrawString(game.Date.ToString(), dateFont, Brushes.White, dateRect, centerFormat);
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
                    var player = roster[i];

                    if (i == roster.Count - 1)
                        i++;

                    var point = GetPointOfPlayer(i);

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

                    if (player.isK || player.isA)
                    {
                        Rectangle assOrK = new Rectangle(point.X + 100, point.Y + 80, sizeNum.Width, sizeNum.Height);

                        g.DrawString(player.isK ? "K" : "A", assOrKfont, Brushes.Red, assOrK, leftFormat);
                    }
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

        public string GameStatistic(Game game, bool isLastGame = false)
        {
            Image bitmap = Image.FromFile("Images\\Blanks\\gameStat.jpg");

            if (isLastGame)
                bitmap = Image.FromFile("Images\\Blanks\\lastStat.jpg");

            GameStat r = new GameStat("Images\\GameStat.txt");


            using (Graphics g = Graphics.FromImage(bitmap))
            {

                #region Зрители
                var onGame = new Rectangle(598, 115, 400, 50);
                var sOnGame = "На матче присутствовало";

                var sViewers = game.Viewers.ToString();
                sViewers += " зрителей";

                DrawStr(g, sViewers, r.Viewers);
                g.DrawString(sOnGame, r.Viewers.Font, Brushes.White, onGame, r.Viewers.StrFormatting);

                #endregion

                #region Соперник

                var enemyName = game.Team2;

                Image enLogo = getTeamLogo(enemyName);

                enemyName = th.FullNameFinder(game.Team2);
                DrawStr(g, enemyName, r.Team2);

                if (enLogo != null)
                {
                    var resRect = GetInscribed(r.EnemyLogo, enLogo.Size);
                    g.DrawImage(enLogo, resRect);
                }

                #endregion

                #region Место + дата

                DrawStr(g, game.Place.Name, r.Place);
                DrawStr(g, game.Date.ToString("dd.MM.yyyy"), r.Date);

                #endregion

                #region Счет


                if (game.Score != null)
                {
                    DrawStr(g, game.Score.Item1.ToString(), r.Score);
                    r.Score.Position = UpdateRectangle(r.Score, 0, 1);
                    DrawStr(g, game.Score.Item2.ToString(), r.Score);

                    if (game.PenaltyGame)
                    {
                        if (game.Score.Item1 < game.Score.Item2)
                            r.OverPenalty.Position = UpdateRectangle(r.OverPenalty, 0, 1);

                        DrawStr(g, "Б", r.OverPenalty);
                    }
                }

                #endregion

                #region Статистика

                var arrOfStat = new int[6, 2]
                {
                    {game.Stat1.Shots, game.Stat2.Shots},
                    {game.Stat1.ShotsIn, game.Stat2.ShotsIn},
                    {game.Stat1.Faceoff, game.Stat2.Faceoff},
                    {game.Stat1.Hits, game.Stat2.Hits},
                    {game.Stat1.Penalty, game.Stat2.Penalty},
                    {game.Stat1.BlockShots, game.Stat2.BlockShots},
                };

                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {

                        r.Stat.Position = UpdateRectangle(r.Stat, i, j);
                        DrawStr(g, arrOfStat[i, j].ToString(), r.Stat);
                        r.Stat.Position = BackRectangleAtr(r.Stat, i, j); // потому что соскакивают все атрибуты
                    }
                }

                #endregion

                #region Следующий матч или лого

                if (isLastGame)
                    DrawStr(g, "СЕЗОН ОКОНЧЕН", r.NextGameOrLogo);

                else
                {
                    if (game.Tournament != null)
                    {

                        Image logo = getTournamentLogo(game.Tournament.Name);

                        if (logo != null)
                        {
                            var resRect = GetInscribed(r.TournamentLogo, logo.Size);

                            g.DrawImage(logo, resRect);
                        }
                    }

                }
                #endregion

                #region Заброшенные шайбы

                var k = 0;
                var extermSizeOfFont = 12;

                if (game.Goal.Count > extermSizeOfFont)
                {
                    var newSize = (int)r.Pucks.Font.Size - 5;
                    r.Pucks.Font = new Font(r.Pucks.Font.FontFamily, newSize);
                }

                r.Pucks.OffsetY = (int)r.Pucks.Font.Size + 2;

                foreach (var goal in game.Goal)
                {
                    var goalString = "• " + th.SimpleNameFinder(game, goal.Author); ;

                    if (goal.Assistant1 != null)
                    {
                        goalString += " (" + th.SimpleNameFinder(game, goal.Assistant1);

                        if (goal.Assistant2 != null)
                            goalString += ", " + th.SimpleNameFinder(game, goal.Assistant2);

                        goalString += ")";
                    }

                    if (goal.isPenalty)
                        goalString += " [Б]";

                    
                    r.Pucks.Position = UpdateRectangle(r.Pucks, k, 0);

                    if (goalString.Length > 33) // невероятный костыль
                    {
                        var splitStr = goalString.Split(',');
                        splitStr[0] += ",";
                        splitStr[1] = splitStr[1].Insert(0, " ");
                        DrawStr(g, splitStr[0], r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks, k, 0);
                        ++k;
                        r.Pucks.Position = UpdateRectangle(r.Pucks, k, 0);
                        DrawStr(g, splitStr[1], r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks, k, 0);
                        ++k;
                    }
                    else
                    {

                        DrawStr(g, goalString, r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks, k, 0);
                        ++k;
                    }
                }

                #endregion

                #region Лучший игрок

                var bestPlayer = game.BestPlayer;

                if (bestPlayer == null)
                    bestPlayer = new Player(100, "Алексей", "Данилин");

                //var goalieStat = Math.Abs(1.0 - ((float)game.Score.Item2 / (float)game.Stat2.ShotsIn)).ToString("N3") + "%";

                string bestStat = 
                    $"{game.Goal.Count(go => go.Author.Id == bestPlayer.Id)}+{game.Goal.Count(go => go.Assistant1 != null && go.Assistant1.Id == bestPlayer.Id) + game.Goal.Count(go => go.Assistant2 != null && go.Assistant2.Id == bestPlayer.Id)}";

                var arrOfBestPlayerAttributes = new string[4]
                {
                    bestPlayer.Name,
                    bestPlayer.Surname,
                    "#" + bestPlayer.Number,
                    bestStat
                };

                for (int i = 0; i < arrOfBestPlayerAttributes.Length; i++)
                {
                    r.Best.Position = UpdateRectangle(r.Best, i, 0);
                    DrawStr(g, arrOfBestPlayerAttributes[i], r.Best);
                    r.Best.Position = BackRectangleAtr(r.Best, i, 0);
                }


                int ramka = 8;
                DrawImageInCircle(g, new Bitmap($"DB\\PlayersPhoto\\{bestPlayer.Number}_{bestPlayer.Surname}.jpg"), r.BestPlayerImage, ramka);

                #endregion
            }

            var file = $"Images\\{game.Id}.jpg";

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            bitmap.Save(file, jpgEncoder, myEncoderParameters);
            return file;
        }

        private Rectangle BackRectangleAtr(TextInImg stat, int i, int j)
        {
            return new Rectangle(stat.Position.X - j * stat.OffsetX,
                                  stat.Position.Y - i * stat.OffsetY,
                                  stat.Position.Width,
                                  stat.Position.Height);
        }

        private Rectangle UpdateRectangle(TextInImg stat, int i, int j)
        {
            return new Rectangle(j * stat.OffsetX + stat.Position.X,
                                  i * stat.OffsetY + stat.Position.Y,
                                  stat.Position.Width,
                                  stat.Position.Height);
        }



        private void DrawStr(Graphics g, string s, TextInImg stat)
        {
            g.DrawString(s, stat.Font, stat.Color, stat.Position, stat.StrFormatting);
            //g.DrawRectangle(Pens.Red, stat.Position);
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

                point.X = startXGoalie + ind * distXGoalie;
                point.Y = startYGoalie;
            }
            else
            {
                var column = (ind - 2) % 5;
                var row = (ind - 2) / 5;

                if (column > 2)
                    kekterval = 140;

                point.X = startX + distX * column + kekterval;
                point.Y = startY + distY * row;
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

            //соотношение сторон
            float ratio = inputsize.Width / (float)inputsize.Height;

            int height = baseRect.Height;
            int width = (int)(height * ratio);

            if (width > baseRect.Width)
            {
                width = baseRect.Width;
                height = (int)(width / ratio);
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

            return dstImage;
        }

        private void DrawImageInCircle(Graphics g, Bitmap bitmap, Rectangle rectToDraw, int ramka)
        {
            var rectsize = new Rectangle(0, 0, rectToDraw.Width - ramka, rectToDraw.Height - ramka);


            Bitmap playerCircle = CropToSize(bitmap, rectToDraw.Size);

            var bra = new TextureBrush(playerCircle, rectsize);

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(rectsize);

            g.TranslateTransform(rectToDraw.X + ramka / 2, rectToDraw.Y + ramka / 2);
            g.FillPath(bra, path);
            g.TranslateTransform(-rectToDraw.X - ramka / 2, -rectToDraw.Y - ramka / 2);
        }
    }
}
