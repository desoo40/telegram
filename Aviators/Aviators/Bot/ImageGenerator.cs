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
            Image bitmap = Image.FromFile("Images\\stata_blank.jpg");

            var fontScore = new Font("Segoe UI Ligth", 56);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                //g.DrawString("1", font, Brushes.White, 200, 105);

                var r = new Rectangle(180, 90, 65, 65);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;


                //g.DrawRectangle(Pens.Red,r);
                g.DrawString("1", fontScore, Brushes.White, r, stringFormat);
                g.DrawString("2", fontScore, Brushes.White, 330, 105);
            }

            var file = $"Images\\game_{game.Id}.jpg";
            bitmap.Save(file, ImageFormat.Jpeg);
            return file;
        }

    }
}
