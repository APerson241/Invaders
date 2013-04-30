using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invaders
{
    public partial class Form1 : Form
    {
        private Stars stars;
        private Game game;
        private Random random;
        private bool gameStarted = false;

        List<Keys> keysPressed = new List<Keys>();

        private int animationCell;

        public Form1()
        {
            InitializeComponent();
            random = new Random();
            stars = new Stars(ClientRectangle);
            game = new Game(ClientRectangle);
            game.NextWave();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
            stars.Draw(g);
            if (gameStarted)
                game.Draw(g, animationCell);
            else
                drawSplashScreen(g);
        }

        private void drawSplashScreen(Graphics g)
        {
            g.DrawString("INVADERS", new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular), Brushes.White,
                     ClientRectangle.Width / 2 - 300, ClientRectangle.Height / 2 - 50);
            g.DrawString("Press S to begin or Q to quit", new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular),
                Brushes.White, new Point(ClientRectangle.Width / 2 - 100,
                ClientRectangle.Height / 2 + 50));
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            game.Go(random);
            foreach (Keys key in keysPressed)
            {
                if (key == Keys.Left)
                {
                    game.MovePlayer(Direction.Left);
                    return;
                }
                else if (key == Keys.Right)
                {
                    game.MovePlayer(Direction.Right);
                    return;
                }
                else if (Configurables.ALLOW_ARBITRARY_SCORE_INCREASES && key == Keys.S)
                {
                    game.AddScore(10);
                    return;
                }
            }
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            stars.Twinkle(random);
            animationCell++;
            if (animationCell >= 6)
                animationCell = 0;
            Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameStarted && e.KeyCode == Keys.S)
            {
                gameStarted = true;
                return;
            }
            if (e.KeyCode == Keys.Q)
                Application.Exit();
            if (e.KeyCode == Keys.Space)
                game.FireShot();
            if (e.KeyCode == Keys.R)
                game.RestartRequested();
            if (keysPressed.Contains(e.KeyCode))
                keysPressed.Remove(e.KeyCode);
            keysPressed.Add(e.KeyCode);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (keysPressed.Contains(e.KeyCode))
                keysPressed.Remove(e.KeyCode);
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Location: " + e.Location.ToString());
        }
    }
}
