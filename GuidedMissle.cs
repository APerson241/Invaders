//using System.Drawing;

//namespace Invaders
//{
//    public class GuidedMissle : Shot
//    {
//        public GuidedMissle(Point location, Direction direction)
//            : base(location, direction)
//        {

//        }

//        public override void Move()
//        {
//            base.Move();
            
//            Game game = Game.GetInstance();
//            if (this.upOrDown == Direction.Down)
//            {
//                if (this.Location.X < game.ShipLocation.X)
//                    this.Location.X++;
//                else if (this.Location.X > game.ShipLocation.X)
//                    this.Location.X--;
//            }
//        }

//        public override void Draw(Graphics g)
//        {
//            g.FillRectangle((upOrDown == Direction.Up) ? Brushes.Orange : Brushes.Red, Location.X - (int)(SIZE.Width / 2),
//                Location.Y - (int)(SIZE.Height / 2), SIZE.Width, SIZE.Height);
//            g.FillPolygon((upOrDown == Direction.Up) ? Brushes.Orange : Brushes.Red,
//                    new Point[] {new Point(Location.X, Location.Y - (int)(SIZE.Height / 2)),
//                                new Point(Location.X + SIZE.Width, Location.Y - (int)(SIZE.Height / 2)),
//                                new Point (Location.X - (int)(SIZE.Width / 2),Location.Y + (int)(SIZE.Height / 2))});
//        }
//    }
//}
