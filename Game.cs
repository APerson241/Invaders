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
        private static Game instance;
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
        private DateTime waveStarted;
        private int framesSkipped = 5;
        private int currentFrame = 0;

        private Rectangle clientRectangle;
        private Random random = new Random();

        /// <summary>
        /// What direction the invaders are currently moving in - either LEFT or RIGHT.
        /// </summary>
        private Direction invaderDirection = Direction.Right;
        private Direction directionInvadersJustMovedIn = Direction.Right;
        private const int INVADER_MARGIN = 10;
        private List<Invader> invaders = new List<Invader>();
        public bool UpgradesOpen = false;

        private PlayerShip playerShip = new PlayerShip();
        public Point ShipLocation { get { return playerShip.Location; } }
        private List<Shot> playerShots = new List<Shot>();
        private List<Shot> enemyShots = new List<Shot>();

        public Explosions explosions;

        public Boolean GameOver { get; private set; }
        
        private int waveAnnouncementAlpha { get { return ((DateTime.Now - waveStarted).TotalSeconds >= 5)?0:255 -
                (int)Math.Round((DateTime.Now - waveStarted).TotalSeconds * (255 / 3)); } }
        private string[] inspirationalMessages = new string[] {"You passed the first level!  Congratulations!",
                "Enjoy: 50-point invaders!", "Having fun yet?", "Rockets!  Gotta have them!"};

        public void setClientRectangle(Rectangle clientRectangle)
        {
            this.clientRectangle = clientRectangle;
        }

        private Game () {
            explosions = new Explosions(clientRectangle);
        }

        public static Game GetInstance()
        {
            if (instance == null)
            {
                instance = new Game();
            }
            return instance;
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
            if (Configurables.SHOW_INVADER_HITBOXES)
                foreach (Invader invader in invaders)
                    g.DrawRectangle(Pens.Olive, invader.Hitbox);
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
                g.DrawString(UpgradesOpen?"Close Store (Press U)":"Upgrades available! (Press U)", Form1.DefaultFont,
                    UpgradesOpen?Brushes.LightGoldenrodYellow:Brushes.LightGreen, scoreLeftOffset + 210, 15);
            }
            #endregion
            g.DrawString("Wave " + wave, Configurables.HUD_FONT, Brushes.White, 10, clientRectangle.Bottom - 30);
            int x = clientRectangle.X + clientRectangle.Width - Properties.Resources.player.Width - 20;
            for (int i = livesLeft; i > 0; i--)
            {
                g.DrawImageUnscaled(Properties.Resources.player, new Point(x, clientRectangle.Bottom - Properties.Resources.player.Height - 5));
                x -= Properties.Resources.player.Width + 10;
            }
            if (UpgradesOpen)
            {
                g.FillRectangle(Configurables.SHOP_BACKGROUND, 50, 50, clientRectangle.Width - 100, clientRectangle.Height - 100);
                g.DrawString("MR. SHOP", Configurables.BIGGER_FONT, Brushes.White, (clientRectangle.Width - 160) / 2, 60);
                g.DrawString("You have    " + beans + " bean" + (beans == 1 ? "" : "s") + ".", Configurables.HUD_FONT,
                        Brushes.AntiqueWhite, 70, 70);
                g.FillEllipse(Brushes.LightGreen, clientRectangle.Right - 210, 70, 5, 5);
                g.DrawString("Item\t\tDescription\t\t\tPrice\t\tPurchase", SystemFonts.IconTitleFont, Brushes.White, 60, 100);
                g.DrawString("----\t\t-----------\t\t\t-----\t\t--------", SystemFonts.IconTitleFont, Brushes.White, 60, 120);
                drawShopItem(g, "Extra Life", "Gives you an extra life\t", Configurables.EXTRA_LIFE_PRICE, 1);
                drawShopItem(g, "More Bullets", "Increases your bullet limit to " + (Configurables.NUMBER_OF_PLAYER_SHOTS_ALLOWED +
                        bulletLimitGained + 1), Configurables.BULLET_LIMIT_PRICE, 2);
                drawShopItem(g, "Guided Missles", (guidedMissleLevel==0)?"Have fun!\t\t":"Increases level to " + (guidedMissleLevel +
                    1), Configurables.GUIDED_MISSLE_BASE_PRICE + guidedMissleLevel, 3);
            }
            if (waveAnnouncementAlpha > 0)
            {
                const int MARGIN_BETWEEN_LINES_OF_TEXT = 50;
                Brush brush = (new Pen(Color.FromArgb(waveAnnouncementAlpha, Color.White))).Brush;
                g.DrawString("WAVE " + wave, Configurables.MASSIVE_FONT, brush, (clientRectangle.Right -
                        g.MeasureString("Wave " + wave, Configurables.MASSIVE_FONT).Width) / 2, (clientRectangle.Bottom -
                        g.MeasureString("Wave " + wave, Configurables.MASSIVE_FONT).Height) / 2 - MARGIN_BETWEEN_LINES_OF_TEXT);
                if (wave >= 2 && wave < 10)
                    g.DrawString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT, brush, (clientRectangle.Right -
                            g.MeasureString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT).Width) / 2, (clientRectangle.Bottom -
                            g.MeasureString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT).Height) / 2 + MARGIN_BETWEEN_LINES_OF_TEXT);
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
            waveStarted = DateTime.Now;
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

        private bool invadersShouldMoveDown { get {
            foreach (Invader invader in invaders)
                if (invader.Hitbox.Right >= clientRectangle.Right - INVADER_MARGIN)
                    return true;
                else if (invader.Location.X <= INVADER_MARGIN)
                    return true;
            return false; } }

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
            if (invadersShouldMoveDown && directionInvadersJustMovedIn != Direction.Down)
            {
                invaderDirection = (invaderDirection == Direction.Left) ? Direction.Right : Direction.Left;
                foreach (Invader invader in invaders)
                    invader.Move(Direction.Down);
                directionInvadersJustMovedIn = Direction.Down;
            }
            else {
                foreach (Invader invader in invaders)
                    invader.Move(invaderDirection);
                directionInvadersJustMovedIn = invaderDirection;
            }
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
            /*
            if (wave >= 5 && random.Next(5) <= 1)
                enemyShots.Add(new GuidedMissle(new Point(chosenInvader.Location.X + (int)(chosenInvader.Hitbox.Width / 2),
                    chosenInvader.Location.Y + chosenInvader.Hitbox.Height), Direction.Down));
            else
                */enemyShots.Add(new Shot(new Point(chosenInvader.Location.X + (int)(chosenInvader.Hitbox.Width / 2),
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
