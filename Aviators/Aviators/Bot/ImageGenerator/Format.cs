using System.Drawing;
using System.Drawing.Text;

namespace Aviators.Bot.ImageGenerator
{
    public class Format
    {
        public StringFormat centerFormat { get; set; }
        public StringFormat leftFormat { get; set; }
        public StringFormat rightFormat { get; set; }

        public Format()
        {
            centerFormat = new StringFormat();
            leftFormat = new StringFormat();
            rightFormat = new StringFormat();

            centerFormat.Alignment = StringAlignment.Center;
            centerFormat.LineAlignment = StringAlignment.Center;

            leftFormat.Alignment = StringAlignment.Near;
            leftFormat.LineAlignment = StringAlignment.Near;

            rightFormat.Alignment = StringAlignment.Far;
            rightFormat.LineAlignment = StringAlignment.Far;
        }
    }
}