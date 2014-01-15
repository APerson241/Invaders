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
        public static bool GameStarted = false;
        public static bool GameReady = false;

        List<Keys> keysPressed = new List<Keys>();

        private int animationCell;

        public Form1()
        {
            InitializeComponent();
            random = new Random();
            stars = new Stars(ClientRectangle);
            game = Game.GetInstance();
            game.setClientRectangle(ClientRectangle);
            game.NextWave();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, 0, 0, Width, Height);
            stars.Draw(g);
            if (GameStarted)
                game.Draw(g, animationCell);
            else
                if (GameReady)
                    drawSplashScreen(g, "Instructions", "Avoid bullets. Shoot invaders.",
                        "Arrow keys to move. Space to fire. That's it. (Press S to start.)", 70);
                else
                    drawSplashScreen(g, "INVADERS", "Press S to start or Q to quit.", "");
        }
        
        /// <summary>
        /// Draws a splash screen with 3 lines, centered in the screen.
        /// </summary>
        /// <param name="g">A GDI+ drawing surface.</param>
        /// <param name="title">The top line of text, drawn in a bigger font.</param>
        /// <param name="line1">The middle line of text.</param>
        /// <param name="line2">The bottom line of text.</param>
        /// <param name="separation">Half the vertical separation between the first two lines of text.</param>
        private void drawSplashScreen(Graphics g, string title, string line1, string line2, int separation=50)
        {
            using (Font bigFont = new Font(FontFamily.GenericSansSerif, (line2 != "")?48:72, FontStyle.Regular))
                g.DrawString(title, bigFont, Brushes.White,
                        new Point((int)(ClientRectangle.Width - g.MeasureString(title, bigFont).Width) / 2,
                                   (int)(ClientRectangle.Height - g.MeasureString(title, bigFont).Height - separation) / 2));
            using (Font smallerFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular))
            {
                g.DrawString(line1, smallerFont, Brushes.White,
                        new Point((int)(ClientRectangle.Width - g.MeasureString(line1, smallerFont).Width) / 2,
                                   (int)(ClientRectangle.Height - g.MeasureString(line1,
                                        smallerFont).Height + separation) / 2));
                g.DrawString(line2, smallerFont, Brushes.White,
                        new Point((int)(ClientRectangle.Width - g.MeasureString(line2, smallerFont).Width) / 2,
                                   (int)(ClientRectangle.Height - g.MeasureString(line2,
                                        smallerFont).Height + separation * 2) / 2));
            }
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
            if (!GameStarted && e.KeyCode == Keys.S)
            {
                if (!GameReady)
                    GameReady = true;
                else
                    GameStarted = true;
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
            if (e.KeyCode == Keys.B)
                game.SpawnBoss();
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
            if (game.NotifyMeOfMouseEvents) game.OnMouseClick(e.Location);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (game.NotifyMeOfMouseEvents) game.OnMouseMove(e.Location);
        }
    }
}
