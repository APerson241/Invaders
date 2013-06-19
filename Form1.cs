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
            const int SEPARATION = 50; // Half of the vertical separation between the two lines of text.
            using (Font bigFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular))
                g.DrawString("INVADERS", bigFont, Brushes.White,
                        new Point((int)(ClientRectangle.Width - g.MeasureString("INVADERS", bigFont).Width) / 2,
                                   (int)(ClientRectangle.Height - g.MeasureString("INVADERS", bigFont).Height - SEPARATION) / 2));
            using (Font smallerFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular))
                g.DrawString("Press S to begin or Q to quit", smallerFont, Brushes.White,
                        new Point((int)(ClientRectangle.Width - g.MeasureString("Press S to begin or Q to quit", smallerFont).Width) / 2,
                                   (int)(ClientRectangle.Height - g.MeasureString("Press S to begin or Q to quit",
                                        smallerFont).Height + SEPARATION) / 2));
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
            if (e.KeyCode == Keys.U)
                game.UpgradeRequested();
            if (game.UpgradesOpen && e.KeyCode == Keys.D1 || e.KeyCode == Keys.D2 || e.KeyCode == Keys.D3)
                game.UpgradeRequested(e.KeyCode);
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
            if (Configurables.SHOW_LOCATION_ON_CLICK) MessageBox.Show("Location: " + e.Location.ToString());
        }
    }
}
