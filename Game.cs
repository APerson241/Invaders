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
        private int shopXOff = 0; // The x-offset of the shop window.
        public ShopStatus CurrentShopStatus = ShopStatus.CLOSED;
        public enum ShopStatus {CLOSED, CLOSING, OPENING, OPEN };

        public Rectangle ClientRectangle;
        private Random random = new Random();

        /// <summary>
        /// What direction the invaders are currently moving in - either LEFT or RIGHT.
        /// </summary>
        private Direction invaderDirection = Direction.Right;
        private Direction directionInvadersJustMovedIn = Direction.Right;
        private const int INVADER_MARGIN = 10;
        private List<Invader> invaders = new List<Invader>();
        public List<Invader> Invaders { get { return invaders; } }
        public Boss Boss;

        public bool NotifyMeOfMouseEvents { get { return CurrentShopStatus == ShopStatus.OPEN; } }
        private Point lastKnownMouseLocation = new Point(5, 5);

        private PlayerShip playerShip = new PlayerShip();
        public Point ShipLocation { get { return playerShip.Location; } }
        private List<Shot> playerShots = new List<Shot>();
        private int playerShotsTaken;
        private int playerShotsGood; // How many player shots actually shot something
        public decimal Accuracy { get { return ((decimal)playerShotsGood / (decimal)playerShotsTaken); } }
        private List<Shot> enemyShots = new List<Shot>();

        public Explosions explosions;

        public Boolean GameOver { get; private set; }
        
        private int waveAnnouncementAlpha { get { return ((DateTime.Now - waveStarted).TotalSeconds >= 5)?0:255 -
                (int)Math.Round((DateTime.Now - waveStarted).TotalSeconds * (255 / 3)); } }
        private string[] inspirationalMessages = new string[] {"You passed the first level - congratulations.",
                "Enjoy: 10-point invaders! (Worth it!)", "Having fun yet?", "Rockets!  Gotta have them!", "Yes, they're getting faster.",
                "At least they won't be getting faster from now on.", "I might have lied.", "Who do you think is going to be the next mayor?"};

        public void setClientRectangle(Rectangle clientRectangle)
        {
            this.ClientRectangle = clientRectangle;
        }

        private Game () {
            explosions = new Explosions(ClientRectangle);
            Boss = new Boss();
        }

        /// <summary>
        /// An attempt at the Singleton design pattern but really just a way for other classes to get an instance of this class.
        /// </summary>
        /// <returns>An instance of The Game. (You just lost The Game.)</returns>
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
            if (CurrentShopStatus == ShopStatus.OPEN) return;
            if (CurrentShopStatus == ShopStatus.OPENING) {
                shopXOff -= Configurables.SHOP_ANIMATION_SPEED + (int)((ClientRectangle.Right - shopXOff) / 10);
                if (shopXOff <= 0)
                {
                    CurrentShopStatus = ShopStatus.OPEN;
                    shopXOff = 0;
                }
                return;
            }
            if (CurrentShopStatus == ShopStatus.CLOSING) {
                shopXOff += Configurables.SHOP_ANIMATION_SPEED + (int)(shopXOff / 10);
                if (shopXOff >= ClientRectangle.Right) CurrentShopStatus = ShopStatus.CLOSED;
                return;
            }
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
            if ((beans > 0 && CurrentShopStatus == ShopStatus.CLOSED) || CurrentShopStatus == ShopStatus.OPEN)
            {
                g.DrawString(CurrentShopStatus == ShopStatus.OPEN ? "Close Store (Press U)" : "Upgrades available! (Press U)", Form1.DefaultFont,
                    CurrentShopStatus == ShopStatus.OPEN ? Brushes.LightGoldenrodYellow : Brushes.LightGreen, scoreLeftOffset + 210, 15);
            }
            #endregion
            Boss.Draw(g);
            g.DrawString("Wave " + wave, Configurables.HUD_FONT, Brushes.White, 10, ClientRectangle.Bottom - 30);
            int x = ClientRectangle.X + ClientRectangle.Width - Properties.Resources.player.Width - 20;
            for (int i = livesLeft; i > 0; i--)
            {
                g.DrawImageUnscaled(Properties.Resources.player, new Point(x, ClientRectangle.Bottom - Properties.Resources.player.Height - 5));
                x -= Properties.Resources.player.Width + 10;
            }
            if (CurrentShopStatus != ShopStatus.CLOSED)
            {
                drawShop(g, shopXOff);
            }
            if (waveAnnouncementAlpha > 0)
            {
                const int MARGIN_BETWEEN_LINES_OF_TEXT = 50;
                bool drawingMessage = wave >= 2 && wave < 10;
                Brush brush = (new Pen(Color.FromArgb(waveAnnouncementAlpha, Color.White))).Brush;
                g.DrawString("WAVE " + wave, Configurables.MASSIVE_FONT, brush, (ClientRectangle.Right -
                        g.MeasureString("Wave " + wave, Configurables.MASSIVE_FONT).Width) / 2, (ClientRectangle.Bottom -
                        g.MeasureString("Wave " + wave, Configurables.MASSIVE_FONT).Height) / 2 - (drawingMessage?MARGIN_BETWEEN_LINES_OF_TEXT:0));
                if (drawingMessage)
                    g.DrawString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT, brush, (ClientRectangle.Right -
                            g.MeasureString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT).Width) / 2, (ClientRectangle.Bottom -
                            g.MeasureString(inspirationalMessages[wave - 2], Configurables.BIGGER_FONT).Height) / 2 + MARGIN_BETWEEN_LINES_OF_TEXT);
            }
            if (GameOver)
            {
                const int SEPARATION = 100; // Half of the vertical separation between the top and bottom lines of text.
                using (Font bigFont = new Font(FontFamily.GenericSansSerif, 72, FontStyle.Regular))
                    g.DrawString("GAME OVER", bigFont, Brushes.White,
                            new Point((int)(ClientRectangle.Width - g.MeasureString("GAME OVER", bigFont).Width) / 2,
                                       (int)(ClientRectangle.Height - g.MeasureString("GAME OVER", bigFont).Height - SEPARATION) / 2));
                string accuracy = "Accuracy: " + ((playerShotsTaken != 0)?Accuracy.ToString("P") +
                    " [" + playerShotsGood + " / " + playerShotsTaken + "]":"You didn't take any shots!");
                using (Font smallerFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular))
                {
                    g.DrawString(accuracy, smallerFont, Brushes.White,
                            new Point((int)(ClientRectangle.Width - g.MeasureString(accuracy, smallerFont).Width) / 2,
                                       (int)(ClientRectangle.Height - g.MeasureString(accuracy, smallerFont).Height) / 2));
                    g.DrawString("Press R to restart or Q to quit", smallerFont, Brushes.White,
                            new Point((int)(ClientRectangle.Width - g.MeasureString("Press R to restart or Q to quit", smallerFont).Width) / 2,
                                       (int)(ClientRectangle.Height - g.MeasureString("Press R to restart or Q to quit",
                                            smallerFont).Height + SEPARATION) / 2));
                }
            }
            explosions.Draw(g);
        }

        /// <summary>
        /// Draws the shop screen
        /// </summary>
        /// <param name="g">A GDI+ panel.</param>
        /// <param name="yOff">The x-offset.</param>
        private void drawShop(Graphics g, int xOff)
        {
            g.FillRectangle(Configurables.SHOP_BACKGROUND, 50 + xOff, 50, ClientRectangle.Width - 100, ClientRectangle.Height - 100);
            g.DrawString("MR. SHOP", Configurables.BIGGER_FONT, Brushes.Black, (ClientRectangle.Width - 160) / 2 + xOff, 60);
            g.DrawString("You have " + beans + " bean" + (beans == 1 ? "" : "s") + ".", Configurables.HUD_FONT,
                    Brushes.DarkGreen, 70 + xOff, 70);
            drawShopRow(g, xOff, 100, "Item", "Description", "Price", "Upgrade Level");
            drawShopRow(g, xOff, 120, "----", "-----------", "-----", "-------------");
            drawShopItem(g, "Extra Life", "Gives you an extra life\t\t", Configurables.EXTRA_LIFE_PRICE, 1, xOff);
            drawShopItem(g, "More Bullets", "Increases your bullet limit to " + (Configurables.NUMBER_OF_PLAYER_SHOTS_ALLOWED +
                    bulletLimitGained + 1) + "\t", Configurables.BULLET_LIMIT_PRICE, 2, xOff);
            drawShopItem(g, "Guided Missles", (guidedMissleLevel == 0) ? "Have fun!\t\t" : "Increases level to " + (guidedMissleLevel +
                1), Configurables.GUIDED_MISSLE_BASE_PRICE + guidedMissleLevel, 3, xOff);
            // Upgrade Level bars
            g.DrawString("n/a", SystemFonts.IconTitleFont, Brushes.Black, COLUMN_X_VALUES[3] + xOff, 140);
            g.FillRectangle(Brushes.Gold, COLUMN_X_VALUES[3] + xOff, 185, bulletLimitGained == Configurables.MAX_LIMIT_INCREASE
                ?100:(100 / Configurables.MAX_LIMIT_INCREASE) * bulletLimitGained, 26);
            g.DrawRectangle(Pens.Wheat, COLUMN_X_VALUES[3] + xOff, 185, 100, 26);
            g.FillRectangle(Brushes.Gold, COLUMN_X_VALUES[3] + xOff, 235, (10) * guidedMissleLevel, 26);
            g.DrawRectangle(Pens.Wheat, COLUMN_X_VALUES[3] + xOff, 235, 100, 26);
        }

        private const int BUTTON_PADDING = 5;
        private Brush disabledBrush = (new Pen(Color.FromArgb(150, Color.Gray)).Brush);
        private void drawShopItem(Graphics g, string name, string description, int price, int keyboardShortcut, int xOff=0)
        {
            string p = (price == 1 ? "" : "s"); // Plural of price
            drawShopRow(g, xOff, 140 + (keyboardShortcut - 1) * 50, name, description, price + " bean" + p);
            bool bulletLimitGood = (name == "More Bullets"?bulletLimitGained < Configurables.MAX_LIMIT_INCREASE:true);
            string buttonString = (beans>=price?(bulletLimitGood?"Buy (" + price + " bean" + p + ")":"Maximum level!"):"Insufficient beans");
            Rectangle buttonRectangle = new Rectangle(COLUMN_X_VALUES[4] - BUTTON_PADDING + xOff, (140 + (keyboardShortcut - 1) * 50) -
                BUTTON_PADDING, (int)(g.MeasureString(buttonString, SystemFonts.IconTitleFont).Width + 2 * BUTTON_PADDING),
                (int)(g.MeasureString(buttonString, SystemFonts.IconTitleFont).Height + 2 * BUTTON_PADDING));
            if (buttonRectangle.Contains(lastKnownMouseLocation) && beans>=price) g.FillRectangle(Brushes.LightGreen, buttonRectangle);
            if (beans < price || !bulletLimitGood) g.FillRectangle(disabledBrush, buttonRectangle);
            g.DrawRectangle(Pens.Black, buttonRectangle);
            g.DrawString(buttonString, SystemFonts.IconTitleFont,
                Brushes.Black, COLUMN_X_VALUES[4] + xOff, 140 + (keyboardShortcut - 1) * 50);
        }
        // { Name, Description, Price, Bullet Limit Progress Bar, Buy Button }
        private int[] COLUMN_X_VALUES = new int[] { 60, 240, 480, 570, 710 };
        private void drawShopRow(Graphics g, int xOff, int y, string a, string b, string c, string d = "")
        {
            g.DrawString(a, SystemFonts.IconTitleFont, Brushes.Black, COLUMN_X_VALUES[0] + xOff, y);
            g.DrawString(b, SystemFonts.IconTitleFont, Brushes.Black, COLUMN_X_VALUES[1] + xOff, y);
            g.DrawString(c, SystemFonts.IconTitleFont, Brushes.Black, COLUMN_X_VALUES[2] + xOff, y);
            g.DrawString(d, SystemFonts.IconTitleFont, Brushes.Black, COLUMN_X_VALUES[3] + xOff, y);
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
                if (shot.Location.Y <= 0 || shot.Location.Y >= ClientRectangle.Y + ClientRectangle.Height)
                {
                    doomedShots.Add(shot);
                }
            }
            foreach (Shot shot in enemyShots)
            {
                shot.Move();
                if (shot.Location.Y <= 0 || shot.Location.Y >= ClientRectangle.Y + ClientRectangle.Height)
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
                if (guidedMissleLevel != 0 && random.Next(guidedMissleLevel >= 10 ? 2 : 11 - guidedMissleLevel) == 0)
                    playerShots.Add(new GuidedMissle(playerShip.NewShotLocation, Direction.Up));
                else
                    playerShots.Add(new Shot(playerShip.NewShotLocation, Direction.Up));
                playerShotsTaken++;
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
                        playerShotsGood++;
                    }
                }
                if (Boss.Hitbox.Contains(shot.Location))
                {
                    Boss.OnScreen = false;
                    usedShots.Add(shot);
                    explosions.SpawnExplosion(Boss.Location, Boss.Hitbox.Size, "LIKE A BOSS");
                    playerShotsGood++;
                    Score += Boss.Score;
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
                if (invader.Hitbox.Right >= ClientRectangle.Right - INVADER_MARGIN)
                    return true;
                else if (invader.Location.X <= INVADER_MARGIN)
                    return true;
            return false; } }

        private void MoveInvaders()
        {
            Boss.Move();
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
            if (wave >= 5 && random.Next(5) <= 1)
                enemyShots.Add(new GuidedMissle(new Point(chosenInvader.Location.X + (int)(chosenInvader.Hitbox.Width / 2),
                    chosenInvader.Location.Y + chosenInvader.Hitbox.Height), Direction.Down));
            else
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
                playerShotsTaken = 0;
                playerShotsGood = 0;
                guidedMissleLevel = 0;
                GameOver = false;
                NextWave();
            }
        }

        public void UpgradeRequested()
        {
            if (CurrentShopStatus == ShopStatus.OPEN)
            {
                CurrentShopStatus = ShopStatus.CLOSING;
                return;
            }
            if (beans > 0 && !GameOver)
            {
                shopXOff = ClientRectangle.Right;
                CurrentShopStatus = ShopStatus.OPENING;
            }
        }

        public void OnMouseClick(Point location)
        {
            if (CurrentShopStatus == ShopStatus.OPEN)
            {
                for (int i = 1; i < Configurables.NUMBER_OF_SHOP_ITEMS + 1; i++)
                {
                    Rectangle buttonRectangle = new Rectangle(COLUMN_X_VALUES[4] - BUTTON_PADDING,
                        (140 + (i - 1) * 50) - BUTTON_PADDING, 50 + 2 * BUTTON_PADDING, 50 + 2 * BUTTON_PADDING);
                    if (buttonRectangle.Contains(location))
                    {
                        switch (i)
                        {
                            case 1:
                                if (beans >= Configurables.EXTRA_LIFE_PRICE)
                                {
                                    livesLeft++;
                                    beans -= Configurables.EXTRA_LIFE_PRICE;
                                    explosions.SpawnExplosion(new Point(buttonRectangle.Right + BUTTON_PADDING,
                                        buttonRectangle.Top + BUTTON_PADDING), buttonRectangle.Size, "+1 Extra Life");
                                } break;
                            case 2:
                                if (beans >= Configurables.BULLET_LIMIT_PRICE && bulletLimitGained < Configurables.MAX_LIMIT_INCREASE)
                                {
                                    bulletLimitGained++;
                                    beans -= Configurables.BULLET_LIMIT_PRICE;
                                    explosions.SpawnExplosion(new Point(buttonRectangle.Right + BUTTON_PADDING,
                                        buttonRectangle.Top + BUTTON_PADDING), buttonRectangle.Size, "Bullet Limit +1");
                                } break;
                            case 3:
                                if (beans >= Configurables.GUIDED_MISSLE_BASE_PRICE)
                                {
                                    guidedMissleLevel++;
                                    beans -= Configurables.GUIDED_MISSLE_BASE_PRICE;
                                    explosions.SpawnExplosion(new Point(buttonRectangle.Right + BUTTON_PADDING,
                                        buttonRectangle.Top + BUTTON_PADDING), buttonRectangle.Size,
                                        "Guided Missles are now at level " + guidedMissleLevel + "!");
                                } break;
                        }
                        if (beans == 0) CurrentShopStatus = ShopStatus.OPEN;
                    }
                }
            }
        }

        public void OnMouseMove(Point location)
        {
            if (CurrentShopStatus == ShopStatus.OPEN)
            {
                lastKnownMouseLocation = location;
            }
        }

        public void SpawnBoss()
        {
            if (!Boss.OnScreen) Boss.OnScreen = true;
        }
    }
}
