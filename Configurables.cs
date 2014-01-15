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
        /// <summary>
        /// Whether or not all guided missles target the Boss.
        /// </summary>
        public const bool GUIDED_MISSLES_HIT_BOSS = true;

        public const int NUMBER_OF_PLAYER_SHOTS_ALLOWED = 3;
        public const int SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP = 1000;
        /// <summary>
        /// How many cycles the wave setup goes through before it returns to that of Wave 1.
        /// </summary>
        public const int INVADER_TURNOVER_INTERVAL = 3;
        /// <summary>
        /// The score awarded for killing a Boss.
        /// </summary>
        public const int BOSS_SCORE = 500;

        private static Brush shopBackground = (new Pen(Color.FromArgb(200, Color.LightGray))).Brush;
        public static Brush SHOP_BACKGROUND { get { return shopBackground; } }
        private static Font hudFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);
        public static Font HUD_FONT { get { return hudFont; } }
        private static Font biggerFont = new Font(FontFamily.GenericSerif, 24, FontStyle.Regular);
        public static Font BIGGER_FONT { get { return biggerFont; } }
        private static Font massiveFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular);
        public static Font MASSIVE_FONT { get { return massiveFont; } }

        // ============ SHOP ============
        public const int NUMBER_OF_SHOP_ITEMS = 3;
        public const int BULLET_LIMIT_PRICE = 3;
        public const int GUIDED_MISSLE_BASE_PRICE = 5;
        public const int EXTRA_LIFE_PRICE = 1;
        /// <summary>
        /// The maximum bullet limit that can be gained.
        /// </summary>
        public const int MAX_LIMIT_INCREASE = 7;
        /// <summary>
        /// The number of pixels the shop screen initially moves sideways during its opening / closing animation.
        /// </summary>
        public const int SHOP_ANIMATION_SPEED = 1;
    }
}
