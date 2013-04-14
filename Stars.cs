using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public class Stars
    {
        private List<Star> stars = new List<Star>();
        private Random random;
        private Rectangle clientRectangle;

        public Stars(Rectangle clientRectangle)
        {
            this.random = new Random();
            this.clientRectangle = clientRectangle;
            for (int i = 0; i < 300; i++)
            {
                stars.Add(new Star(new Point(random.Next(clientRectangle.X, clientRectangle.X + clientRectangle.Width),
                    random.Next(clientRectangle.Y, clientRectangle.Y + clientRectangle.Height)),
                    RandomPen()));
            }
        }

        private Pen[] pens = { Pens.SkyBlue, Pens.White, Pens.Yellow, Pens.Green, Pens.Blue };
        private Pen RandomPen() {
            return pens[random.Next(pens.Length - 1)];
        }

        public void Draw(Graphics g) {
            foreach (Star star in stars) {
                g.DrawRectangle(star.Pen, star.X, star.Y, 1, 1);
            }
        }

        public void Twinkle(Random random)
        {
            List<Star> starsToRemove = new List<Star>();
            for (int i = 0; i < 5; i++)
                starsToRemove.Add(stars[random.Next(stars.Count - 1)]);
            foreach (Star doomed in starsToRemove)
                stars.Remove(doomed);
            for (int i = 0; i < 5; i++)
                stars.Add(new Star(new Point(random.Next(clientRectangle.X, clientRectangle.X + clientRectangle.Width),
                    random.Next(clientRectangle.Y, clientRectangle.Y + clientRectangle.Height)),
                    RandomPen()));
        }
        
        private struct Star
        {
            public Point Point;
            public Pen Pen;
            public int X { get { return Point.X; } }
            public int Y { get { return Point.Y; } }

            public Star(Point point, Pen pen)
            {
                this.Point = point;
                this.Pen = pen;
            }
        }
    }
}
