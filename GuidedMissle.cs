using System.Drawing;

namespace Invaders
{
    public class GuidedMissle : Shot
    {
        private Invader target;
        private Point targetPoint { get {
            if (Configurables.GUIDED_MISSLES_HIT_BOSS)
            {
                Game game = Game.GetInstance();
                return game.Boss.Location;
            } else
                return new Point(target.Hitbox.Bottom - target.Hitbox.Top / 2,
            target.Hitbox.Right - target.Hitbox.Left / 2); } }

        public GuidedMissle(Point location, Direction direction)
            : base(location, direction)
        {
            if (this.upOrDown == Direction.Up)
            {
                Game game = Game.GetInstance();
                target = game.Invaders[0];
            }
        }

        public override void Move()
        {
            base.Move();
            Game game = Game.GetInstance();
            if (this.upOrDown == Direction.Down)
            {
                if (this.Location.X < game.ShipLocation.X)
                    this.Location.X++;
                else if (this.Location.X > game.ShipLocation.X)
                    this.Location.X--;
            }
            else
            {
                if (!game.Invaders.Contains(target)) target = game.Invaders[game.Invaders.Count > 1?1:0]; // If our target has died, get a new one.
                if (this.Location.X > targetPoint.X)
                    this.Location.X--;
                else if (this.Location.X < targetPoint.X)
                    this.Location.X++;
            }
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle((upOrDown == Direction.Up) ? Brushes.DarkGreen : Brushes.Aquamarine, Location.X - (int)(SIZE.Width / 2),
                Location.Y - (int)(SIZE.Height / 2), SIZE.Width, SIZE.Height);
            g.FillPolygon((upOrDown == Direction.Up) ? Brushes.DarkGreen : Brushes.Aquamarine,
                    (upOrDown == Direction.Up)?
                        new Point[] {new Point(Location.X - (int)(SIZE.Width / 2), Location.Y - (int)(SIZE.Height / 2)),
                                new Point(Location.X + (int)(SIZE.Width / 2), Location.Y - (int)(SIZE.Height / 2)),
                                new Point (Location.X, Location.Y - (int)(SIZE.Height / 2))}:
                        new Point[] {new Point(Location.X - (int)(SIZE.Width / 2), Location.Y + (int)(SIZE.Height / 2)),
                                new Point(Location.X + (int)(SIZE.Width / 2), Location.Y + (int)(SIZE.Height / 2)),
                                new Point (Location.X, Location.Y + SIZE.Width)});
        }
    }
}
