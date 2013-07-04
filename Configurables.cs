using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public abstract class Configurables
    {
        public const bool SHOW_LOCATION_ON_CLICK = false;
        public const bool ALLOW_ARBITRARY_SCORE_INCREASES = true;
        public const bool SHOW_INVADER_HITBOXES = false;

        public const int NUMBER_OF_PLAYER_SHOTS_ALLOWED = 3;
        public const int SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP = 1000;
        /// <summary>
        /// How many cycles the wave setup goes through before it returns to that of Wave 1.
        /// </summary>
        public const int INVADER_TURNOVER_INTERVAL = 3;
        public const int BULLET_LIMIT_PRICE = 3;
        public const int GUIDED_MISSLE_BASE_PRICE = 5;
        public const int EXTRA_LIFE_PRICE = 1;

        private static Brush shopBackground = (new Pen(Color.FromArgb(200, Color.Gray))).Brush;
        public static Brush SHOP_BACKGROUND { get { return shopBackground; } }
        private static Font hudFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);
        public static Font HUD_FONT { get { return hudFont; } }
        private static Font biggerFont = new Font(FontFamily.GenericSerif, 24, FontStyle.Regular);
        public static Font BIGGER_FONT { get { return biggerFont; } }
        private static Font massiveFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular);
        public static Font MASSIVE_FONT { get { return massiveFont; } }
    }
}
