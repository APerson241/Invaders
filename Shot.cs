using System.Drawing;

namespace Invaders
{
    public class Shot
    {
        public Point Location;
        internal Direction upOrDown;
        public static Size SIZE = new Size(4, 12);
        public static int MOVE_INTERVAL = 3;

        public Shot(Point location, Direction direction)
        {
            this.Location = location;
            this.upOrDown = direction;
        }

        public virtual void Draw(Graphics g)
        {
            g.FillRectangle((upOrDown == Direction.Up)?Brushes.Orange:Brushes.Red, Location.X - (int)(SIZE.Width/2),
                Location.Y - (int)(SIZE.Height/2), SIZE.Width, SIZE.Height);
        }

        public virtual void Move()
        {
            if (upOrDown == Direction.Up)
                Location.Y -= MOVE_INTERVAL;
            else
                Location.Y += MOVE_INTERVAL;
        }
    }
}
