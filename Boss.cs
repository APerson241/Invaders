using System;
using System.Drawing;

namespace Invaders
{
    public class Boss
    {
        public Point Location;
        private Size hitboxSize;
        public Rectangle Hitbox { get { return new Rectangle(Location, hitboxSize); } }
        public int Score { get { return Configurables.BOSS_SCORE; } }
        private bool onScreen; // Never set this directly.
        public bool OnScreen
        {
            get { return onScreen; }
            set
            {
                if (value == true)
                {
                    Game game = Game.GetInstance();
                    Location = new Point(game.ClientRectangle.Right - 50, 20);
                }
                onScreen = value;
            }
        }

        public Boss()
        {
            Location = new Point(500, 20);
            hitboxSize = Properties.Resources.block.Size;
            OnScreen = false;
        }

        public void Draw(Graphics g)
        {
            if (OnScreen) g.DrawImageUnscaled(Properties.Resources.block, Location);
        }

        public void Move()
        {
            if (OnScreen)
            {
                Location.X--;
                if (Location.X <= -hitboxSize.Width / 2)
                {
                    OnScreen = false;
                }
            }
        }
    }
}
