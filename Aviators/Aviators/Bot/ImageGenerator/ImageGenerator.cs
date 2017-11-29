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
    public class ImageGenerator
    {
        TextHelper th = new TextHelper();

        void DrawOutlineText(Graphics g, String text, Font font, Rectangle r, Brush b, Color line)
        {
            // set atialiasing
            g.SmoothingMode = SmoothingMode.HighQuality;

            // make thick pen for outlining
            Pen pen = new Pen(line, 3);
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
            Brush br = new SolidBrush(line);
            // fill outline path with some color
            g.FillPath(br, outlinePath);
            // fill original text path with some color
            g.FillPath(b, textPath);
        }

        public string Roster(Game game)
        {
            Image bitmap = Image.FromFile("Images\\Blanks\\roster.jpg");
            RosterImg r = new RosterImg("Images\\roster.txt");

            using (Graphics g = Graphics.FromImage(bitmap))
            {

                #region Лого турнира

                Image logo;

                if (game.Tournament != null)
                {

                    logo = getTournamentLogo(game.Tournament.Name);

                    if (logo != null)
                    {
                        var resRect = GetInscribed(r.TournamentLogo, logo.Size);

                        g.DrawImage(logo, resRect);
                    }
                }

                #endregion

                #region Дата + описание

                var description = game.Description;
                if (description.Length != 0)
                {    int fontSize = (r.Description.Position.Width + 100)/description.Length;
                r.Description.Font = new Font(r.Description.Font.FontFamily, fontSize, r.Description.Font.Style);

                DrawStr(g, description, r.Description);
                DrawStr(g, game.Date.ToString(), r.Date);
                }

            #endregion
                #region Лого противника

                Image enLogo;
                var enemyName = game.Team2;
                enLogo = getTeamLogo(enemyName);

                if (enLogo != null)
                {
                    var resRect = GetInscribed(r.EnemyLogo, enLogo.Size);
                    g.DrawImage(enLogo, resRect);
                }
                #endregion


                #region Состав

                var roster = game.Roster;

                for (int i = 0, gl = 0, dfd = 0, fwd = 0; i < roster.Count; ++i)
                {
                    var player = roster[i];

                    if (player.Position == PlayerPosition.Нападающий)
                    {
                        DrawPlayer(g, r.Forward, player, fwd);
                        ++fwd;
                    }
                    if (player.Position == PlayerPosition.Защитник)
                    {
                        DrawPlayer(g, r.Defender, player, dfd);
                        ++dfd;
                    }

                    if (player.Position == PlayerPosition.Вратарь)
                    {
                        DrawPlayer(g, r.Goalie, player, gl);
                        ++gl;
                    }
                }

                #endregion
            }

            var file = $"Images\\{game.Id}Rost.jpg";

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            bitmap.Save(file, jpgEncoder, myEncoderParameters);
            return file;
        }

        private void DrawPlayer(Graphics g, RosterImg.Player r, Player player, int param)
        {
            int i = 0;
            int j = 0;

            if (player.Position == PlayerPosition.Нападающий)
            {
                i = param / 3;
                j = param % 3;
            }
            if (player.Position == PlayerPosition.Защитник || player.Position == PlayerPosition.Вратарь)
            {
                i = param / 2;
                j = param % 2;
            }

            


            var rect = UpdateRectangle(r.Position, r.OffsetX, r.OffsetY, i, j);
            DrawImageInCircle(g, new Bitmap(player.PhotoFile), rect, 3);

            Point pntName = new Point(rect.X + r.Name.OffsetX, rect.Y + r.Name.OffsetY);
            Point pntNum = new Point(rect.X + r.Number.OffsetX, rect.Y + r.Number.OffsetY);
            Point pntAorK = new Point(rect.X + r.AorK.OffsetX, rect.Y + r.AorK.OffsetY);

            r.Name.Position = new Rectangle(pntName, r.Name.RectSize);
            DrawStr(g, $"{player.Name}\n{player.Surname}", r.Name);

            r.Number.Position = new Rectangle(pntNum, r.Number.RectSize);
            DrawStr(g, $"#{player.Number}", r.Number);

            if (player.isA || player.isK)
            {
                r.AorK.Position = new Rectangle(pntAorK, r.AorK.RectSize);

                DrawStr(g, player.isK ? "K" : "A", r.AorK);
            }
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

                #region Соперник + мы
                
                var enemyName = game.Team2;
                var homeName = game.Description;

                Image enLogo = getTeamLogo(enemyName);

                enemyName = th.FullNameFinder(game.Team2);
                DrawStr(g, homeName, r.Team1);
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
                    r.Score.Position = UpdateRectangle(r.Score.Position, r.Score.OffsetX, r.Score.OffsetY, 0, 1);
                    DrawStr(g, game.Score.Item2.ToString(), r.Score);

                    if (game.PenaltyGame)
                    {
                        if (game.Score.Item1 < game.Score.Item2)
                            r.OverPenalty.Position = UpdateRectangle(r.OverPenalty.Position, r.OverPenalty.OffsetX, r.OverPenalty.OffsetY, 0, 1);

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

                        r.Stat.Position = UpdateRectangle(r.Stat.Position, r.Stat.OffsetX, r.Stat.OffsetY, i, j);
                        //if (arrOfStat[i, j] == 0)
                        //{
                        //    DrawStr(g, "-", r.Stat);
                        //}
                        //else
                            DrawStr(g, arrOfStat[i, j].ToString(), r.Stat);
                        r.Stat.Position = BackRectangleAtr(r.Stat.Position, r.Stat.OffsetX, r.Stat.OffsetY, i, j); // потому что соскакивают все атрибуты
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


                    r.Pucks.Position = UpdateRectangle(r.Pucks.Position, r.Pucks.OffsetX, r.Pucks.OffsetY, k, 0);

                    if (goalString.Length > 30) // невероятный костыль
                    {
                        var splitStr = goalString.Split(',');
                        splitStr[0] += ",";
                        splitStr[1] = splitStr[1].Insert(0, " ");
                        DrawStr(g, splitStr[0], r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks.Position, r.Pucks.OffsetX, r.Pucks.OffsetY, k, 0);
                        ++k;
                        r.Pucks.Position = UpdateRectangle(r.Pucks.Position, r.Pucks.OffsetX, r.Pucks.OffsetY, k, 0);
                        DrawStr(g, splitStr[1], r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks.Position, r.Pucks.OffsetX, r.Pucks.OffsetY, k, 0);
                        ++k;
                    }
                    else
                    {

                        DrawStr(g, goalString, r.Pucks);
                        r.Pucks.Position = BackRectangleAtr(r.Pucks.Position, r.Pucks.OffsetX, r.Pucks.OffsetY, k, 0);
                        ++k;
                    }
                }

                #endregion

                #region Лучший игрок

                var bestPlayer = game.BestPlayer;

                if (bestPlayer == null)
                    bestPlayer = new Player(100, "Алексей", "Данилин");

                var goalieStat = Math.Abs(1.0 - ((float)game.Score.Item2 / (float)game.Stat2.ShotsIn)).ToString("N3") + "%";
                //var goalieStat = "1.000%";


                string bestStat = bestPlayer.Position == PlayerPosition.Вратарь ? goalieStat :
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
                    r.Best.Position = UpdateRectangle(r.Best.Position, r.Best.OffsetX, r.Best.OffsetY, i, 0);
                    DrawStr(g, arrOfBestPlayerAttributes[i], r.Best);
                    r.Best.Position = BackRectangleAtr(r.Best.Position, r.Best.OffsetX, r.Best.OffsetY, i, 0);
                }


                int ramka = 8;
                //TODO проверить на наличие файла

                
                DrawImageInCircle(g, new Bitmap(bestPlayer.PhotoFile), r.BestPlayerImage, ramka);

                #endregion
            }

            var file = $"Images\\{game.Id}Stat.jpg";

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            bitmap.Save(file, jpgEncoder, myEncoderParameters);
            return file;
        }

        private Rectangle BackRectangleAtr(Rectangle rect, int offX, int offY, int i, int j)
        {
            return new Rectangle(rect.X - j * offX,
                                  rect.Y - i * offY,
                                  rect.Width,
                                  rect.Height);
        }

        private Rectangle UpdateRectangle(Rectangle rect, int offX, int offY, int i, int j)
        {
            return new Rectangle(rect.X + j * offX,
                                  rect.Y + i * offY,
                                  rect.Width,
                                  rect.Height);
        }



        private void DrawStr(Graphics g, string s, TextInImg text)
        {
            if (text.IsOutline)
                DrawOutlineText(g, s, text.Font, text.Position, text.Color, text.OutlineColor);

            else
                g.DrawString(s, text.Font, text.Color, text.Position, text.StrFormatting);

            //g.DrawRectangle(Pens.Red, stat.Position);
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
