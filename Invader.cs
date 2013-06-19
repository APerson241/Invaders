using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public class Invader
    {
        public Point Location;
        public InvaderType Type { get; private set; }
        private Size hitboxSize;
        public Rectangle Hitbox { get { return new Rectangle(Location, hitboxSize); } }
        public int Score { get { return ((int)Type * 10) + 10; } }

        public Invader(InvaderType type, Point location)
        {
            this.Type = type;
            this.Location = location;
            hitboxSize = InvaderImage(0).Size;
        }

        public void Draw(Graphics g, int animationCell)
        {
            g.DrawImageUnscaled(InvaderImage(animationCell), Location);
        }

        public void Move(Direction direction)
        {
            int increment = (direction == Direction.Down)?10:1;
            switch (direction)
            {
                case Direction.Up:
                    Location.Y -= increment; break;
                case Direction.Down:
                    Location.Y += increment; break;
                case Direction.Left:
                    Location.X -= increment; break;
                case Direction.Right:
                    Location.X += increment; break;
            }
        }

        private Bitmap InvaderImage(int animationCell)
        {
            switch (Type)
            {
                case InvaderType.Bug:
                    switch (animationCell)
                    {
                        case 0:
                            return Properties.Resources.bug1;
                        case 1:
                            return Properties.Resources.bug2;
                        case 2:
                            return Properties.Resources.bug3;
                        case 3:
                            return Properties.Resources.bug4;
                        case 4:
                            return Properties.Resources.bug3;
                        case 5:
                            return Properties.Resources.bug2;
                        default:
                            return Properties.Resources.bug1;
                    }
                case InvaderType.FlyingSaucer:
                    switch (animationCell)
                    {
                        case 0:
                            return Properties.Resources.flyingsaucer1;
                        case 1:
                            return Properties.Resources.flyingsaucer2;
                        case 2:
                            return Properties.Resources.flyingsaucer3;
                        case 3:
                            return Properties.Resources.flyingsaucer4;
                        case 4:
                            return Properties.Resources.flyingsaucer3;
                        case 5:
                            return Properties.Resources.flyingsaucer2;
                        default:
                            return Properties.Resources.flyingsaucer1;
                    }
                case InvaderType.Satellite:
                    switch (animationCell)
                    {
                        case 0:
                            return Properties.Resources.satellite1;
                        case 1:
                            return Properties.Resources.satellite2;
                        case 2:
                            return Properties.Resources.satellite3;
                        case 3:
                            return Properties.Resources.satellite4;
                        case 4:
                            return Properties.Resources.satellite3;
                        case 5:
                            return Properties.Resources.satellite2;
                        default:
                            return Properties.Resources.satellite1;
                    }
                case InvaderType.Star:
                    switch (animationCell)
                    {
                        case 0:
                            return Properties.Resources.star1;
                        case 1:
                            return Properties.Resources.star2;
                        case 2:
                            return Properties.Resources.star3;
                        case 3:
                            return Properties.Resources.star4;
                        case 4:
                            return Properties.Resources.star3;
                        case 5:
                            return Properties.Resources.star2;
                        default:
                            return Properties.Resources.star1;
                    }
                case InvaderType.Watchit:
                    switch (animationCell)
                    {
                        case 0:
                            return Properties.Resources.watchit1;
                        case 1:
                            return Properties.Resources.watchit2;
                        case 2:
                            return Properties.Resources.watchit3;
                        case 3:
                            return Properties.Resources.watchit4;
                        case 4:
                            return Properties.Resources.watchit3;
                        case 5:
                            return Properties.Resources.watchit2;
                        default:
                            return Properties.Resources.watchit1;
                    }
                default:
                    return Properties.Resources.block;
            }
        }
    }
}
