using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public class Explosions
    {
        private List<Explosion> explosions = new List<Explosion>();
        private Random random;
        private Rectangle clientRectangle;

        public Explosions(Rectangle clientRectangle)
        {
            this.random = new Random();
            this.clientRectangle = clientRectangle;
        }

        public void Draw(Graphics g) {
            foreach (Explosion explosion in explosions)
                explosion.Draw(g);
        }

        public void SpawnExplosion(Point point, Size size, string text="")
        {
            explosions.Add(new Explosion(point, size, text));
        }

        public void SpawnExplosion(Invader invader)
        {
            explosions.Add(new Explosion(invader));
        }

        public void ClearAllExplosions()
        {
            explosions.Clear();
        }

        private struct Explosion
        {
            public Point Point;
            public int X { get { return Point.X; } }
            public int Y { get { return Point.Y; } }
            public Size Size;
            private DateTime created;
            public DateTime Created { get { return created; } }
            public double AgeInSeconds { get { return (DateTime.Now - Created).TotalSeconds; } }
            public String Text;

            public Explosion(Point point, Size size, string text="")
            {
                this.Size = size;
                this.Point = point;
                created = DateTime.Now;
                this.Text = text;
            }

            public Explosion(Invader invader)
            {
                this.Size = invader.Hitbox.Size;
                this.Point = new Point(invader.Hitbox.X + (int)(invader.Hitbox.Width / 2),
                    invader.Hitbox.Y + (int)(invader.Hitbox.Height / 2));
                created = DateTime.Now;
                this.Text = invader.Score.ToString();
            }

            public void Draw(Graphics g)
            {
                if (AgeInSeconds < 2) g.DrawString(Text, Form1.DefaultFont,
                        Brushes.White, (float)X, (float)(Y - AgeInSeconds * 10));
            }
        }
    }
}
