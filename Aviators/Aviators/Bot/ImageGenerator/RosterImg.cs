using System.Drawing;

namespace Aviators.Bot.ImageGenerator
{
    public class RosterImg
    {
        public Rectangle EnemyLogo { get; set; }
        public Rectangle TournamentLogo { get; set; }

        public class Player
        {
            public Photo Photo { get; set; }
            public TextInImg Name { get; set; }
            public TextInImg Number { get; set; }
            public TextInImg AorK { get; set; }



        }

        public class Photo
        {
            public Rectangle Position { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
        }
    }
}