using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invaders
{
    public class Game
    {
        private int score; // DO NOT SET DIRECTLY!!
        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                if (ScoreLeftUntilFree1Up <= 5)
                {
                    livesGainedDueToScore++;
                    livesLeft++;
                }
            }
        }
        public int ScoreLeftUntilFree1Up { get {
            return ((Configurables.SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP + livesGainedDueToScore * 10) *
                (livesGainedDueToScore + 1)) - score;
        } }
        private int livesLeft = 2;
        private int livesGainedDueToScore = 0;
        private int wave = 0;
        private int framesSkipped = 5;
        private int currentFrame = 0;

        private Rectangle clientRectangle;
        private Random random = new Random();

        private Direction invaderDirection = Direction.Right;
        private Boolean invadersMovingDown = false;
        private const int invaderMargin = 10;
        private List<Invader> invaders = new List<Invader>();

        private PlayerShip playerShip = new PlayerShip();
        private List<Shot> playerShots = new List<Shot>();
        private List<Shot> enemyShots = new List<Shot>();
        

        public Boolean GameOver { get; private set; }

        private int livesLeftY;
        private Font hudFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);

        public Game (Rectangle clientRectangle) {
            this.clientRectangle = clientRectangle;
            livesLeftY = clientRectangle.Y + clientRectangle.Height - Properties.Resources.player.Height - 10;
        }

        public void Go(Random random)
        {
            MoveInvaders();
            UpdateShots();
            CheckForInvaderCollisions();
            CheckForPlayerCollisions();
            if (!GameOver)
            {
                if (invaders.Count == 0)
                    NextWave();
                ReturnFire();
            }
        }

        public void Draw(Graphics g, int animationCell)
        {
            foreach (Invader invader in invaders)
                invader.Draw(g, animationCell);
            playerShip.Draw(g);
            foreach (Shot shot in playerShots)
                shot.Draw(g);
            foreach (Shot shot in enemyShots)
                shot.Draw(g);
            g.DrawString(score.ToString() + "(" + ScoreLeftUntilFree1Up + " left until free 1up)", hudFont, Brushes.White, 10, 10);
            g.DrawString("Wave " + wave, hudFont, Brushes.White, 10, clientRectangle.Bottom - 30);
            int x = clientRectangle.X + clientRectangle.Width - Properties.Resources.player.Width - 20;
            for (int i = livesLeft; i > 0; i--)
            {
                g.DrawImageUnscaled(Properties.Resources.player, new Point(x, livesLeftY));
                x -= Properties.Resources.player.Width + 10;
            }
            if (GameOver)
            {
                const int SEPARATION = 50; // Half of the vertical separation between the two lines of text.
                using (Font bigFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular))
                    g.DrawString("GAME OVER", bigFont, Brushes.White,
                            new Point((int)(clientRectangle.Width - g.MeasureString("GAME OVER", bigFont).Width) / 2,
                                       (int)(clientRectangle.Height - g.MeasureString("GAME OVER", bigFont).Height - SEPARATION) / 2));
                using (Font smallerFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular))
                    g.DrawString("Press R to restart or Q to quit", smallerFont, Brushes.White,
                            new Point((int)(clientRectangle.Width - g.MeasureString("Press R to restart or Q to quit", smallerFont).Width) / 2,
                                       (int)(clientRectangle.Height - g.MeasureString("Press R to restart or Q to quit",
                                            smallerFont).Height + SEPARATION) / 2));
            }
        }

        public void AddScore(int value)
        {
            Score += value;
        }

        private void UpdateShots()
        {
            List<Shot> doomedShots = new List<Shot>();
            foreach (Shot shot in playerShots)
            {
                shot.Move();
                if (shot.Location.Y <= 0 || shot.Location.Y >= clientRectangle.Y + clientRectangle.Height)
                {
                    doomedShots.Add(shot);
                }
            }
            foreach (Shot shot in enemyShots)
            {
                shot.Move();
                if (shot.Location.Y <= 0 || shot.Location.Y >= clientRectangle.Y + clientRectangle.Height)
                {
                    doomedShots.Add(shot);
                }
            }
            foreach (Shot shot in doomedShots)
            {
                if (playerShots.Contains(shot))
                    playerShots.Remove(shot);
                else
                    enemyShots.Remove(shot);
            }
        }

        public void FireShot()
        {
            if (playerShots.Count < Configurables.NUMBER_OF_PLAYER_SHOTS_ALLOWED)
            {
                playerShots.Add(new Shot(playerShip.NewShotLocation, Direction.Up));
            }
        }

        public void MovePlayer(Direction direction)
        {
            if (direction == Direction.Left || direction == Direction.Right)
                playerShip.Move(direction);
        }

        private InvaderType[] lookup = {InvaderType.Bug, InvaderType.FlyingSaucer, InvaderType.Satellite,
                                                 InvaderType.Star, InvaderType.Watchit};
        public void NextWave()
        {
            wave++;
            playerShots.Clear();
            enemyShots.Clear();
            invaders = new List<Invader>();
            InvaderType type = InvaderType.Bug + (wave % Configurables.INVADER_TURNOVER_INTERVAL);
            for (int y = 50; y < 250; y += 80)
            {
                for (int x = 50; x < 600; x += 80)
                {
                    invaders.Add(new Invader(type, new Point(x, y)));
                }
                type++;
            }
            if (framesSkipped > 0) framesSkipped--;
        }

        /// <summary>
        /// Check if any invaders have been hit
        /// </summary>
        private void CheckForInvaderCollisions()
        {
            List<Invader> deadInvaders = new List<Invader>();
            List<Shot> usedShots = new List<Shot>();
            foreach (Shot shot in playerShots)
            {
                foreach (Invader invader in invaders)
                {
                    if (invader.Hitbox.Contains(shot.Location))
                    {
                        deadInvaders.Add(invader);
                        usedShots.Add(shot);
                    }
                }
            }
            foreach (Invader invader in deadInvaders)
            {
                Score += invader.Score;
                invaders.Remove(invader);
            }
            foreach (Shot shot in usedShots)
                playerShots.Remove(shot);
        }

        private void CheckForPlayerCollisions()
        {
            List<Shot> usedShots = new List<Shot>();
            foreach (Shot shot in enemyShots)
            {
                if (playerShip.Hitbox.Contains(shot.Location))
                {
                    livesLeft--;
                    usedShots.Add(shot);
                }
            }
            foreach (Shot shot in usedShots)
                enemyShots.Remove(shot);
            if (livesLeft == 0)
                OnGameOver();
        }

        private void MoveInvaders()
        {
            if (framesSkipped > 0)
            {
                currentFrame++;
                if (currentFrame >= framesSkipped)
                    currentFrame = 0;
                else
                    return;
            }
            Direction shouldMove = invaderDirection; // What direction the invaders should move in this round
            if (!invadersMovingDown)
            {
                foreach (Invader invader in invaders)
                {
                    if (invader.Location.X + invader.Hitbox.Width >= clientRectangle.X + clientRectangle.Width - (invaderMargin + 10))
                    {
                        shouldMove = Direction.Down;
                        invaderDirection = Direction.Left;
                        break;
                    }
                    else if (invader.Location.X <= invaderMargin)
                    {
                        shouldMove = Direction.Down;
                        invaderDirection = Direction.Right;
                        break;
                    }
                }
            }
            invadersMovingDown = shouldMove == Direction.Down;
            foreach (Invader invader in invaders)
                invader.Move(shouldMove);
        }

        private void ReturnFire()
        {
            if (enemyShots.Count >= wave + 1) return;
            if (random.Next(10) < wave) return;
            var invaderGroups = from invader in invaders
                                group invader by invader.Location.X
                                    into invaderGroup
                                    orderby invaderGroup.Key descending
                                    select invaderGroup;
            var chosenInvaderGroup = invaderGroups.ToList()[random.Next(invaderGroups.ToList().Count - 1)];
            Invader chosenInvader = chosenInvaderGroup.Last() as Invader;
            enemyShots.Add(new Shot(new Point(chosenInvader.Location.X + (int)(chosenInvader.Hitbox.Width / 2),
                chosenInvader.Location.Y + chosenInvader.Hitbox.Height), Direction.Down));
        }

        /// <summary>
        /// Should only be called by CheckForPlayerCollisions() when livesLeft is 0.
        /// </summary>
        private void OnGameOver()
        {
            GameOver = true;
            invaders.Clear();
            enemyShots.Clear();
            playerShots.Clear();
        }

        public void RestartRequested()
        {
            if (GameOver)
            {
                wave = 0;
                score = 0;
                framesSkipped = 6;
                livesLeft = 2;
                livesGainedDueToScore = 0;
                GameOver = false;
                NextWave();
            }
        }
    }
}
