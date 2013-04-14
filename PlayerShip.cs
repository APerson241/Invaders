using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public class PlayerShip
    {
        public Point Location;
        public Point NewShotLocation { get { return new Point(Location.X + 26, Location.Y - 11); } }
        public Rectangle Hitbox { get { return new Rectangle(Location, new Size(50, 50)); } }

        public const int MOVE_INCREMENT = 2;

        public PlayerShip()
        {
            Location = new Point(423, 400); // 449, 339
        }

        public void Draw(Graphics g)
        {
            g.DrawImageUnscaled(Properties.Resources.player, Location);
        }

        public void Move(Direction direction)
        {
            if (direction == Direction.Left)
                Location.X -= MOVE_INCREMENT;
            else
                Location.X += MOVE_INCREMENT;
        }
    }
}
