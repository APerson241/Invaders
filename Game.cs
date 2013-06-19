using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Invaders
{
    public class Game
    {
        private int livesLeft = 2;
        private int beans = 0;  // beans = in-game currency
        private int beansGainedDueToScore = 0;
        private int bulletLimitGained = 0;
        private int guidedMissleLevel = 0;
        private int score; // DO NOT SET DIRECTLY!!
        public int Score
        {
            get { return score; }
            set
            {
                score = value;
                if (ScoreLeftUntilFree1Up <= 5)
                {
                    beansGainedDueToScore++;
                    beans++;
                    explosions.SpawnExplosion(new Point(500, 100), new Size(), "+1 BEAN");
                }
            }
        }
        public int Free1UpThreshold { get {
                return ((Configurables.SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP + beansGainedDueToScore * 10) *
                        (beansGainedDueToScore + 1));
        } }
        public int ScoreLeftUntilFree1Up { get {
            return Free1UpThreshold - score;
        } }
        public double ScoreLeftPercentage { get {
            return 100 - ((double)ScoreLeftUntilFree1Up / (double)(Free1UpThreshold -
                    ((Configurables.SCORE_THRESHOLD_MULTIPLIER_FOR_FREE_1UP + beansGainedDueToScore * 10) * beansGainedDueToScore)) * 100);
        } }
        private int wave = 0;
        private int framesSkipped = 5;
        private int currentFrame = 0;

        private Rectangle clientRectangle;
        private Random random = new Random();

        private Direction invaderDirection = Direction.Right;
        private Boolean invadersMovingDown = false;
        private const int invaderMargin = 10;
        private List<Invader> invaders = new List<Invader>();
        public bool UpgradesOpen = false;

        private PlayerShip playerShip = new PlayerShip();
        private List<Shot> playerShots = new List<Shot>();
        private List<Shot> enemyShots = new List<Shot>();

        public Explosions explosions;

        public Boolean GameOver { get; private set; }

        private int livesLeftY;

        public Game (Rectangle clientRectangle) {
            this.clientRectangle = clientRectangle;
            explosions = new Explosions(clientRectangle);
            livesLeftY = clientRectangle.Y + clientRectangle.Height - Properties.Resources.player.Height - 10;
        }

        public void Go(Random random)
        {
            if (UpgradesOpen) return;
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
            explosions.Draw(g);
            foreach (Invader invader in invaders)
                invader.Draw(g, animationCell);
            playerShip.Draw(g);
            foreach (Shot shot in playerShots)
                shot.Draw(g);
            foreach (Shot shot in enemyShots)
                shot.Draw(g);
            #region upper-left-corner
            g.DrawString(score.ToString(), Configurables.HUD_FONT, Brushes.White, 10, 10);
            float scoreLeftOffset = 20 + g.MeasureString(score.ToString(), Configurables.HUD_FONT).Width;
            g.FillRectangle(Brushes.Orange, scoreLeftOffset, 10, (float)((ScoreLeftPercentage<0?0:ScoreLeftPercentage) * 2), 23);
            g.DrawRectangle(Pens.Purple, scoreLeftOffset, 10, 200, 23);
            g.DrawString(Math.Round(ScoreLeftPercentage).ToString() + "%", Form1.DefaultFont, Brushes.White, scoreLeftOffset +
                100 - g.MeasureString(Math.Round(ScoreLeftPercentage).ToString() + "%", Form1.DefaultFont).Width / 2, 15);
            if (beans > 0 || UpgradesOpen)
            {
                g.DrawString(UpgradesOpen?"Close Store (Press U)":"Upgrades available! (Press U)", Form1.DefaultFont, Brushes.LightGreen, scoreLeftOffset + 210, 15);
            }
            #endregion
            g.DrawString("Wave " + wave, Configurables.HUD_FONT, Brushes.White, 10, clientRectangle.Bottom - 30);
            int x = clientRectangle.X + clientRectangle.Width - Properties.Resources.player.Width - 20;
            for (int i = livesLeft; i > 0; i--)
            {
                g.DrawImageUnscaled(Properties.Resources.player, new Point(x, livesLeftY));
                x -= Properties.Resources.player.Width + 10;
            }
            if (UpgradesOpen)
            {
                g.FillRectangle(Configurables.SHOP_BACKGROUND, 50, 50, clientRectangle.Width - 100, clientRectangle.Height - 100);
                g.DrawString("MR. SHOP", Configurables.BIGGER_FONT, Brushes.White, (clientRectangle.Width - 160) / 2, 60);
                g.DrawString("You have " + beans + " bean" + (beans == 1 ? "" : "s") + ".", Configurables.HUD_FONT,
                        Brushes.AntiqueWhite, clientRectangle.Right - 200, 70);
                g.DrawString("Item\t\tDescription\t\t\tPrice\t\tPurchase", SystemFonts.IconTitleFont, Brushes.White, 60, 100);
                g.DrawString("----\t\t-----------\t\t\t-----\t\t--------", SystemFonts.IconTitleFont, Brushes.White, 60, 120);
                drawShopItem(g, "Extra Life", "Gives you an extra life\t", Configurables.EXTRA_LIFE_PRICE, 1);
                drawShopItem(g, "More Bullets", "Increases your bullet limit to " + (Configurables.NUMBER_OF_PLAYER_SHOTS_ALLOWED +
                        bulletLimitGained + 1), Configurables.BULLET_LIMIT_PRICE, 2);
                drawShopItem(g, "Guided Missles", (guidedMissleLevel==0)?"Have fun!\t\t":"Increases level to " + (guidedMissleLevel +
                    1), Configurables.GUIDED_MISSLE_BASE_PRICE + guidedMissleLevel, 3);
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

        private void drawShopItem(Graphics g, string name, string description, int price, int keyboardShortcut)
        {
            g.DrawString(name + "\t" + description + "\t" + price + " bean" + (price==1?"":"s") + "\t\t" + (beans>=price?"Press " +
                    keyboardShortcut:"Insufficient beans"), SystemFonts.IconTitleFont, Brushes.White, 60, 140 +
                    (keyboardShortcut - 1) * 20);
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
            if (playerShots.Count < (Configurables.NUMBER_OF_PLAYER_SHOTS_ALLOWED + bulletLimitGained))
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
            explosions.SpawnExplosion(new Point(30, clientRectangle.Bottom - 30), new Size(), "+1");
            playerShots.Clear();
            enemyShots.Clear();
            explosions.ClearAllExplosions();
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
                        explosions.SpawnExplosion(invader);
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
            explosions.ClearAllExplosions();
        }

        public void RestartRequested()
        {
            if (GameOver)
            {
                wave = 0;
                score = 0;
                framesSkipped = 6;
                livesLeft = 2;
                beans = 0;
                beansGainedDueToScore = 0;
                bulletLimitGained = 0;
                GameOver = false;
                NextWave();
            }
        }

        public void UpgradeRequested(Keys key=Keys.U)
        {
            if (UpgradesOpen)
            {
                if (key == Keys.U)
                {
                    UpgradesOpen = false;
                    return;
                }
                else if (key == Keys.D1 && beans >= Configurables.EXTRA_LIFE_PRICE)
                {
                    livesLeft++;
                    beans--;
                }
                else if (key == Keys.D2 && beans >= Configurables.BULLET_LIMIT_PRICE)
                {
                    bulletLimitGained++;
                    beans -= Configurables.BULLET_LIMIT_PRICE;
                }
                else if (key == Keys.D3 && beans >= Configurables.GUIDED_MISSLE_BASE_PRICE + guidedMissleLevel)
                {
                    guidedMissleLevel++;
                    beans -= Configurables.GUIDED_MISSLE_BASE_PRICE;
                }
                if (beans == 0) UpgradesOpen = false;
            }
            if (beans > 0 && !GameOver)
            {
                UpgradesOpen = true;
            }
        }
    }
}
