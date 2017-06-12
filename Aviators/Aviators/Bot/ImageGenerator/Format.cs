using System.Drawing;
using System.Drawing.Text;

namespace Aviators.Bot.ImageGenerator
{
    public class Format
    {
        StringFormat centerFormat;
        StringFormat leftFormat;
        StringFormat rightFormat;

        public Format()
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
        }
    }
}