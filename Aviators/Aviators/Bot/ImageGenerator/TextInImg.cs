using System;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Text;

namespace Aviators.Bot.ImageGenerator
{
    public class TextInImg
    {
        // в дальнейшем может пригодиться ввести флаг отличающий оффсетные тексты от обычных...
        public bool IsOffset { get; set; }
        public Rectangle Position { get; set; }
        public Size RectSize { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public Font Font { get; set; }
        public Brush Color { get; set; }
        public StringFormat StrFormatting { get; set; }

        PrivateFontCollection coll = new PrivateFontCollection();

        public TextInImg() {}

        public TextInImg(string[] lines, int i)
        {
            Format format = new Format();
            if (CheckForNothing(lines[i]))
                return;

            var tmpFont = "Times New Roman";
            var tmpFontSize = 14;
            FontStyle tmpFontStyle = FontStyle.Regular;

            while (lines[i] != "*")
            {
                if (lines[i] == "Position")
                    Position = GetRectFromLine(lines[++i]);

                if (lines[i] == "Size")
                    RectSize = GetSizeFromLine(lines[++i]);


                if (lines[i] == "Repeatability" || lines[i] == "Offset")
                {
                    if (CheckForNothing(lines[++i]))
                    {
                        OffsetX = 0;
                        OffsetY = 0;
                        continue;
                    }

                    var spl = lines[i].Split(' ');

                    OffsetX = Convert.ToInt32(spl[0]);
                    OffsetY = Convert.ToInt32(spl[1]);
                }

                if (lines[i] == "Font")
                {
                    if (!CheckForNothing(lines[++i]))
                        tmpFont = lines[i];
                }

                if (lines[i] == "Font size")
                {
                    if (!CheckForNothing(lines[++i]))
                        tmpFontSize = Convert.ToInt32(lines[i]);
                }

                if (lines[i] == "Font Style")
                {
                    if (!CheckForNothing(lines[++i]))
                    {
                        if (lines[i].ToLower() == "regular")
                            tmpFontStyle = FontStyle.Regular;

                        if (lines[i].ToLower() == "bold")
                            tmpFontStyle = FontStyle.Bold;
                    }
                }

                if (lines[i] == "Color")
                {
                    if (!CheckForNothing(lines[++i]))
                    {
                        if (lines[i].Contains(","))
                        {
                            Color = GetColorFromLine(lines[i]);
                        }
                        else
                        {
                            if (lines[i].ToLower() == "white")
                                Color = Brushes.White;

                            if (lines[i].ToLower() == "black")
                                Color = Brushes.Black;

                            if (lines[i].ToLower() == "blue")
                                Color = Brushes.Blue;

                            if (lines[i].ToLower() == "red")
                                Color = Brushes.Red;
                        }
                    }
                }

                if (lines[i] == "Formatting")
                {
                    if (!CheckForNothing(lines[++i]))
                    {
                        if (lines[i].ToLower() == "center")
                            StrFormatting = format.centerFormat;

                        if (lines[i].ToLower() == "left")
                            StrFormatting = format.leftFormat;

                        if (lines[i].ToLower() == "right")
                            StrFormatting = format.rightFormat;
                    }
                }

                ++i;
            }

            if (IsFontFromFile(tmpFont))
            {
                coll.AddFontFile(tmpFont);
                Font = new Font(coll.Families[0], tmpFontSize, tmpFontStyle);
            }
            else
                Font = new Font(tmpFont, tmpFontSize, tmpFontStyle);
        }

        private Brush GetColorFromLine(string s)
        {
            var spl = s.Split(',');

            return new SolidBrush(System.Drawing.Color.FromArgb(Convert.ToInt32(spl[0]),
                Convert.ToInt32(spl[1]),
                Convert.ToInt32(spl[2])));
        }

        internal Size GetSizeFromLine(string s)
        {
            if (CheckForNothing(s))
                return new Size();

            var spl = s.Split(' ');
            return new Size(Convert.ToInt32(spl[0]),
                Convert.ToInt32(spl[1]));
        }

        private bool IsFontFromFile(string tmpFont)
        {
            return tmpFont.Contains(".") || tmpFont.Contains("/");
        }

        internal Rectangle GetRectFromLine(string s)
        {
            if (CheckForNothing(s))
                return new Rectangle();

            var spl = s.Split(',');
            return new Rectangle(Convert.ToInt32(spl[0]),
                Convert.ToInt32(spl[1]),
                Convert.ToInt32(spl[2]),
                Convert.ToInt32(spl[3]));
        }

        internal bool CheckForNothing(string s)
        {
            return s == "-";
        }
    }
}