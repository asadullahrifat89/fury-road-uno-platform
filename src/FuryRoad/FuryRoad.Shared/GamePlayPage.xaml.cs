using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.System;

namespace FuryRoad
{
    public sealed partial class GamePlayPage : Page
    {
        #region Fields

        PeriodicTimer GameViewTimer;

        readonly List<GameObject> GameViewRemovableObjects = new();
        readonly Random rand = new();

        Rect playerHitBox;

        int gameSpeed = 6;
        readonly int defaultGameSpeed = 6;
        readonly int playerSpeed = 6;
        int markNum;
        int powerUpCounter = 30;
        int powerModeCounter = 250;
        readonly int powerModeDelay = 250;

        int healthCounter = 500;
        int lives = 3;
        readonly int maxLives = 3;

        double score;

        double CarWidth;
        double CarHeight;

        double TruckWidth;
        double TruckHeight;

        double RoadMarkWidth;
        double RoadMarkHeight;

        double RoadSideWidth;
        double RoadSideHeight;

        double TreeWidth;
        double TreeHeight;

        double LampPostWidth;
        double LampPostHeight;

        double HighWayDividerWidth;
        private bool moveLeft;
        private bool moveRight;
        private bool moveUp;
        private bool moveDown;
        private bool isGameOver;
        private bool isPowerMode;
        private bool isGamePaused;

        private bool isRecoveringFromDamage;
        private bool isPointerActivated;
        readonly TimeSpan frameTime = TimeSpan.FromMilliseconds(18);

        private int accelerationCounter;

        private int damageRecoveryCounter = 100;
        private readonly int damageRecoveryDelay = 500;

        double windowHeight, windowWidth;

        Point pointerPosition;

        #endregion

        #region Ctor

        public GamePlayPage()
        {
            this.InitializeComponent();

            isGameOver = true;

            windowHeight = Window.Current.Bounds.Height;
            windowWidth = Window.Current.Bounds.Width;

            AdjustView();

            this.Loaded += GamePlayPage_Loaded;
            this.Unloaded += GamePlayPage_Unloaded;
        }

        #endregion

        #region Events

        private void GamePlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += GamePlayPage_SizeChanged;
        }

        private void GamePlayPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= GamePlayPage_SizeChanged;
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            windowWidth = args.NewSize.Width;
            windowHeight = args.NewSize.Height;

            Console.WriteLine($"WINDOWS SIZE: {windowWidth}x{windowHeight}");

            AdjustView();
        }

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (isGameOver)
            {
                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                isPointerActivated = true;

                //TODO: capture pointer activation and move car to pointer position

                //if (isGamePaused)
                //{
                //    RunGame();
                //    isGamePaused = false;
                //}
                //else
                //{
                //    StopGame();
                //    isGamePaused = true;
                //}
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                pointerPosition = point.Position;
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isPointerActivated = false;
            pointerPosition = null;
        }

        private void StopGame()
        {
            GameViewTimer.Dispose();
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
            {
                moveLeft = true;
                moveRight = false;
            }
            if (e.Key == VirtualKey.Right)
            {
                moveRight = true;
                moveLeft = false;
            }
            if (e.Key == VirtualKey.Up)
            {
                moveUp = true;
                moveDown = false;
            }
            if (e.Key == VirtualKey.Down)
            {
                moveDown = true;
                moveUp = false;
            }
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {
            // when the player releases the left or right key it will set the designated boolean to false
            if (e.Key == VirtualKey.Left)
            {
                moveLeft = false;
            }
            if (e.Key == VirtualKey.Right)
            {
                moveRight = false;
            }
            if (e.Key == VirtualKey.Up)
            {
                moveUp = false;
            }
            if (e.Key == VirtualKey.Down)
            {
                moveDown = false;
            }

            if (!moveLeft && !moveRight && !moveUp && !moveDown)
                accelerationCounter = 0;

            // in this case we will listen for the enter key aswell but for this to execute we will need the game over boolean to be true
            if (e.Key == VirtualKey.Enter && isGameOver == true)
            {
                StartGame();
            }
        }

        #endregion

        #region Methods

        #region View

        private void AdjustView()
        {
            GameView.Width = windowWidth < 500 ? 500 : windowWidth < 1200 ? 700 : windowWidth < 1400 ? windowWidth / 1.6 : windowWidth / 1.6;
            GameView.Height = windowHeight * 2;

            double scale = GetGameObjectScale();

            GameView.Width = GameView.Width * scale;

            RootGrid.Width = windowWidth;
            RootGrid.Height = windowHeight;

            //GameView.Width = GameView.Width - 40 * scale;
            //GameView.Height = GameView.Height;

            SoilView.Width = windowWidth * 2;
            SoilView.Height = GameView.Height;

            SoilView.Children.Clear();

            // draw grass stripes
            for (int i = -5; i < 60; i++)
            {
                Border border = new()
                {
                    Width = 30 * scale,
                    Height = SoilView.Height,
                    Background = this.Resources["GrassStripeColor"] as SolidColorBrush,
                };

                Canvas.SetLeft(border, i * 60);
                Canvas.SetTop(border, 0);

                SoilView.Children.Add(border);
            }

            CarWidth = Convert.ToDouble(this.Resources["CarWidth"]);
            CarHeight = Convert.ToDouble(this.Resources["CarHeight"]);

            TruckWidth = Convert.ToDouble(this.Resources["TruckWidth"]);
            TruckHeight = Convert.ToDouble(this.Resources["TruckHeight"]);

            TreeWidth = Convert.ToDouble(this.Resources["TreeWidth"]);
            TreeHeight = Convert.ToDouble(this.Resources["TreeHeight"]);

            LampPostWidth = Convert.ToDouble(this.Resources["LampPostWidth"]);
            LampPostHeight = Convert.ToDouble(this.Resources["LampPostHeight"]);

            CarWidth *= scale;
            CarHeight *= scale;

            TruckWidth *= scale;
            TruckHeight *= scale;

            TreeWidth *= scale;
            TreeHeight *= scale;

            LampPostWidth *= scale;
            LampPostHeight *= scale;

            Console.WriteLine($"CAR WIDTH {CarWidth}");
            Console.WriteLine($"CAR HEIGHT {CarHeight}");

            Console.WriteLine($"TRUCK WIDTH {TruckWidth}");
            Console.WriteLine($"TRUCK HEIGHT {TruckHeight}");

            RoadMarkWidth = Convert.ToDouble(this.Resources["RoadMarkWidth"]);
            RoadMarkHeight = Convert.ToDouble(this.Resources["RoadMarkHeight"]);

            RoadSideWidth = Convert.ToDouble(this.Resources["RoadSideWidth"]);
            RoadSideHeight = Convert.ToDouble(this.Resources["RoadSideHeight"]);

            RoadMarkWidth *= scale;
            RoadMarkHeight *= scale;

            RoadSideWidth *= scale;
            RoadSideHeight *= scale;

            Console.WriteLine($"ROAD MARK WIDTH {RoadMarkWidth}");
            Console.WriteLine($"ROAD MARK HEIGHT {RoadMarkHeight}");

            Console.WriteLine($"ROAD SIDE WIDTH {RoadSideWidth}");
            Console.WriteLine($"ROAD SIDE HEIGHT {RoadSideHeight}");

            // run a initial foreach loop to set up the cars and remove any star in the game
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                string tag = (string)x.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            x.SetSize(RoadMarkWidth, RoadMarkHeight);
                        }
                        break;
                    case Constants.TREE_TAG:
                        {
                            x.SetSize(TreeWidth, TreeHeight);
                        }
                        break;
                    case Constants.LAMPPOST_LEFT_TAG:
                        {
                            x.SetSize(LampPostWidth, LampPostHeight);
                            x.SetLeft(0 + (19 * scale));
                        }
                        break;
                    case Constants.LAMPPOST_RIGHT_TAG:
                        {
                            x.SetSize(LampPostWidth, LampPostHeight);
                            x.SetLeft(GameView.Width - (57 * scale));
                        }
                        break;
                    case Constants.CAR_TAG:
                        {
                            x.SetSize(CarWidth, CarHeight);
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            x.SetSize(TruckWidth, TruckHeight);
                        }
                        break;
                    case Constants.POWERUP_TAG:
                        {
                            x.SetSize(50 * scale, 50 * scale);
                        }
                        break;
                    default:
                        break;
                }
            }

            highWayLeftSide.Width = RoadSideWidth;
            highWayRightSide.Width = RoadSideWidth;

            Canvas.SetLeft(highWayRightSide, GameView.Width - RoadSideWidth);

            player.SetSize(CarWidth, CarHeight);

            double carY = (GameView.Height / 1.3) - (370 * scale);
            Console.WriteLine($"CAR Y: {carY}");

            player.SetTop(carY);

            HighWayDividerWidth = Convert.ToDouble(this.Resources["HighWayDividerWidth"]);
            HighWayDividerWidth *= scale;
            Console.WriteLine($"HIGHWAY DIVIDER WIDTH {HighWayDividerWidth}");

            highWayDivider.Width = HighWayDividerWidth;
            Canvas.SetLeft(highWayDivider, (GameView.Width / 2) - (highWayDivider.Width / 2));
        }

        public double GetGameObjectScale()
        {
            return GameView.Width switch
            {
                <= 300 => 0.60,
                <= 400 => 0.65,
                <= 500 => 0.70,
                <= 700 => 0.75,
                <= 900 => 0.80,
                <= 1000 => 0.85,
                <= 1400 => 0.90,
                <= 2000 => 0.95,
                _ => 1,
            };
        }

        #endregion

        #region Game Start, Run, Loop, Over

        private void StartGame()
        {
            Console.WriteLine("GAME STARTED");

            lives = maxLives;
            SetLives();

            gameSpeed = 6;
            RunGame();

            player.SetContent(new Uri("ms-appx:///Assets/Images/player.png"));
            player.SetSize(CarWidth, CarHeight);
            player.Opacity = 1;

            GameView.Background = this.Resources["RoadBackgroundColor"] as SolidColorBrush;

            moveLeft = false;
            moveRight = false;
            moveUp = false;
            moveDown = false;

            isGameOver = false;
            isPowerMode = false;
            powerModeCounter = powerModeDelay;
            isRecoveringFromDamage = false;
            damageRecoveryCounter = damageRecoveryDelay;

            score = 0;
            scoreText.Text = "Score: 0";

            // set game view objects
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                string tag = (string)x.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            RandomizeRoadMark(x);
                        }
                        break;
                    case Constants.TREE_TAG:
                        {
                            RandomizeTree(x);
                        }
                        break;
                    case Constants.LAMPPOST_LEFT_TAG:
                        {
                            RandomizeLampPostLeft(x);
                        }
                        break;
                    case Constants.LAMPPOST_RIGHT_TAG:
                        {
                            RandomizeLampPostRight(x);
                        }
                        break;
                    case Constants.CAR_TAG:
                        {
                            RecyleCar(x);
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            RecyleTruck(x);
                        }
                        break;
                    case Constants.HEALTH_TAG:
                    case Constants.POWERUP_TAG:
                        {
                            GameViewRemovableObjects.Add(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (GameObject y in GameViewRemovableObjects)
            {
                GameView.Children.Remove(y);
            }

            GameViewRemovableObjects.Clear();
        }

        private void RunGame()
        {
            RunGameView();

        }

        private async void RunGameView()
        {
            GameViewTimer = new PeriodicTimer(frameTime);

            while (await GameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private void GameViewLoop()
        {
            score += .05; // increase the score by .5 each tick of the timer

            powerUpCounter -= 1;

            scoreText.Text = "Score: " + score.ToString("#");

            playerHitBox = new Rect(player.GetLeft(), player.GetTop(), player.Width, player.Height);

            if (moveLeft || moveRight || moveUp || moveDown || isPointerActivated)
            {
                UpdatePlayer();
            }

            if (powerUpCounter < 0)
            {
                SpawnPowerUp();
                powerUpCounter = rand.Next(500, 800);
            }

            if (lives < maxLives)
            {
                healthCounter--;

                if (healthCounter < 0)
                {
                    SpawnHealth();
                    healthCounter = rand.Next(500, 800);
                }
            }

            // below is the main game loop, inside of this loop we will go through all of the rectangles available in this game
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                string tag = (string)x.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            UpdateRoadMark(x);
                        }
                        break;
                    case Constants.TREE_TAG:
                        {
                            UpdateTree(x);
                        }
                        break;
                    case Constants.LAMPPOST_LEFT_TAG:
                    case Constants.LAMPPOST_RIGHT_TAG:
                        {
                            UpdateLampPost(x);
                        }
                        break;
                    case Constants.CAR_TAG:
                    case Constants.TRUCK_TAG:
                        {
                            UpdateVehicle(x);
                        }
                        break;
                    case Constants.POWERUP_TAG:
                        {
                            UpdatePowerUp(x);
                        }
                        break;
                    case Constants.HEALTH_TAG:
                        {
                            UpdateHealth(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (isGameOver)
                return;

            if (isPowerMode)
            {
                PowerUpCoolDown();

                if (powerModeCounter <= 0)
                {
                    PowerDown();
                }
            }

            foreach (GameObject y in GameViewRemovableObjects)
            {
                GameView.Children.Remove(y);
            }

            // as you progress in the game you will score higher and game speed will go up
            ScaleDifficulty();
        }

        private void GameOver()
        {
            player.SetContent(new Uri("ms-appx:///Assets/Images/player-crashed.png"));

            StopGame();

            scoreText.Text += " Press Enter to replay";
            isGameOver = true;
        }

        #endregion

        #region Player

        private void UpdatePlayer()
        {
            double effectiveSpeed = accelerationCounter >= playerSpeed ? playerSpeed : accelerationCounter / 1.3;

            // increase acceleration and stop when player speed is reached
            if (accelerationCounter <= playerSpeed)
                accelerationCounter++;

            //Console.WriteLine("ACC:" + _accelerationCounter);

            double scale = GetGameObjectScale();

            double left = player.GetLeft();
            double top = player.GetTop();

            double playerMiddleX = left + player.Width / 2;
            double playerMiddleY = top + player.Height / 2;

            if (isPointerActivated)
            {
                // move up
                if (pointerPosition.Y < playerMiddleY - playerSpeed)
                {
                    player.SetTop(top - effectiveSpeed);
                }
                // move left
                if (pointerPosition.X < playerMiddleX - playerSpeed && left > 0)
                {
                    player.SetLeft(left - effectiveSpeed);
                }

                // move down
                if (pointerPosition.Y > playerMiddleY + playerSpeed)
                {
                    player.SetTop(top + effectiveSpeed);
                }
                // move right
                if (pointerPosition.X > playerMiddleX + playerSpeed && left + player.Width < GameView.Width)
                {
                    player.SetLeft(left + effectiveSpeed);
                }
            }
            else
            {
                if (moveLeft && left > 0)
                {
                    player.SetLeft(left - effectiveSpeed);
                }
                if (moveRight && left + player.Width < GameView.Width)
                {
                    player.SetLeft(left + effectiveSpeed);
                }
                if (moveUp && top > (GameView.Height / 3.3) + 100 * scale)
                {
                    player.SetTop(top - effectiveSpeed);
                }
                if (moveDown && top < ((GameView.Height / 1.3) - 370 * scale))
                {
                    player.SetTop(top + effectiveSpeed);
                }
            }
        }

        #endregion

        #region Road Marks

        private void UpdateRoadMark(GameObject roadMark)
        {
            roadMark.SetTop(roadMark.GetTop() + gameSpeed);

            if (roadMark.GetTop() > GameView.Height)
            {
                RecyleRoadMark(roadMark);
            }
        }

        private void RecyleRoadMark(GameObject roadMark)
        {
            RandomizeRoadMark(roadMark);

            roadMark.SetSize(RoadMarkWidth, RoadMarkHeight);
            roadMark.SetTop(((roadMark.Height * 2) * -1));
        }

        private void RandomizeRoadMark(GameObject roadMark)
        {
            markNum = rand.Next(0, AssetTemplates.ROADMARK_TEMPLATES.Length);
            roadMark.SetContent(AssetTemplates.ROADMARK_TEMPLATES[markNum]);
        }

        #endregion

        #region Tree

        private void UpdateTree(GameObject roadSide)
        {
            roadSide.SetTop(roadSide.GetTop() + gameSpeed);

            if (roadSide.GetTop() > GameView.Height)
            {
                RecyleTree(roadSide);
            }
        }

        private void RecyleTree(GameObject tree)
        {
            RandomizeTree(tree);

            tree.SetSize(TreeWidth, TreeHeight);
            tree.SetTop((rand.Next(100, (int)GameView.Height) * -1));
        }

        private void RandomizeTree(GameObject tree)
        {
            markNum = rand.Next(0, AssetTemplates.TREE_TEMPLATES.Length);
            tree.SetContent(AssetTemplates.TREE_TEMPLATES[markNum]);
        }

        #endregion

        #region Lamp Posts

        private void UpdateLampPost(GameObject roadSide)
        {
            roadSide.SetTop(roadSide.GetTop() + gameSpeed);

            if (roadSide.GetTop() > GameView.Height)
            {
                RecyleLampPost(roadSide);
            }
        }

        private void RecyleLampPost(GameObject lampPostLeft)
        {
            switch ((string)lampPostLeft.Tag)
            {
                case Constants.LAMPPOST_LEFT_TAG: { RandomizeLampPostLeft(lampPostLeft); } break;
                case Constants.LAMPPOST_RIGHT_TAG: { RandomizeLampPostRight(lampPostLeft); } break;
                default:
                    break;
            }

            lampPostLeft.SetSize(LampPostWidth, LampPostHeight);
            lampPostLeft.SetTop(((lampPostLeft.Height * 2) * -1));
        }

        private void RandomizeLampPostLeft(GameObject lampPostLeft)
        {
            lampPostLeft.SetContent(new Uri("ms-appx:///Assets/Images/lamppost-left.png"));
        }

        private void RandomizeLampPostRight(GameObject lampPostLeft)
        {
            lampPostLeft.SetContent(new Uri("ms-appx:///Assets/Images/lamppost-right.png"));
        }

        #endregion

        #region Vehicles

        private void UpdateVehicle(GameObject vehicle)
        {
            // move down vehicle
            vehicle.SetTop(vehicle.GetTop() + vehicle.Speed);

            // if vechicle goes out of bounds
            if (vehicle.GetTop() > GameView.Height)
            {
                if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                    RecyleTruck(vehicle);
                else
                    RecyleCar(vehicle);
            }

            if (isRecoveringFromDamage)
            {
                player.Opacity = 0.66;
                damageRecoveryCounter--;

                if (damageRecoveryCounter <= 0)
                {
                    player.Opacity = 1;
                    isRecoveringFromDamage = false;
                }
            }
            else
            {
                // if vehicle collides with player
                if (playerHitBox.IntersectsWith(vehicle.GetHitBox()))
                {
                    if (!isPowerMode)
                    {
                        lives--;
                        damageRecoveryCounter = damageRecoveryDelay;
                        isRecoveringFromDamage = true;
                        SetLives();

                        if (lives == 0)
                            GameOver();
                    }
                }
            }

            if (isGameOver)
                return;

            //TODO: this is expensive
            // if vechicle will collide with another vehicle
            if (GameView.Children.OfType<GameObject>()
                .Where(x => (string)x.Tag is Constants.CAR_TAG or Constants.TRUCK_TAG)
                .LastOrDefault(v => v.GetDistantHitBox()
                .IntersectsWith(vehicle.GetDistantHitBox())) is GameObject collidingVehicle)
            {
                // slower vehicles will slow down faster vehicles
                if (collidingVehicle.Speed > vehicle.Speed)
                {
                    vehicle.Speed = collidingVehicle.Speed;
                }
                else
                {
                    collidingVehicle.Speed = vehicle.Speed;
                }
            }
        }

        private void SetLives()
        {
            livesText.Text = "";
            for (int i = 0; i < lives; i++)
            {
                livesText.Text += "❤️";
            }
        }

        private void RecyleCar(GameObject car)
        {
            markNum = rand.Next(0, AssetTemplates.CAR_TEMPLATES.Length);

            car.SetContent(AssetTemplates.CAR_TEMPLATES[markNum]);
            car.SetSize(CarWidth, CarHeight);
            car.Speed = gameSpeed - rand.Next(0, 7);

            RandomizeVehiclePostion(car);
        }

        private void RecyleTruck(GameObject truck)
        {
            markNum = rand.Next(0, AssetTemplates.TRUCK_TEMPLATES.Length);

            truck.SetContent(AssetTemplates.TRUCK_TEMPLATES[markNum]);
            truck.SetSize(TruckWidth, TruckHeight);
            truck.Speed = gameSpeed - rand.Next(0, 5);

            RandomizeVehiclePostion(truck);
        }

        private void RandomizeVehiclePostion(GameObject vehicle)
        {
            // set a random top and left position for the traffic car
            vehicle.SetPosition(rand.Next(100, (int)GameView.Height) * -1, rand.Next(0, (int)GameView.Width - 50));

            //if (GameView.Children.OfType<GameObject>().Where(x => x is Car or Truck).Any(y => y.GetDistantHitBox().IntersectsWith(hitBox)))
            //{

            //}
        }

        #endregion

        #region Power Ups

        private void UpdatePowerUp(GameObject powerUp)
        {
            powerUp.SetTop(powerUp.GetTop() + 5);

            // if player gets a power up
            if (playerHitBox.IntersectsWith(powerUp.GetHitBox()))
            {
                GameViewRemovableObjects.Add(powerUp);

                TriggerPowerUp();
            }

            if (powerUp.GetTop() > GameView.Height)
            {
                GameViewRemovableObjects.Add(powerUp);
            }
        }

        private void TriggerPowerUp()
        {
            powerUpText.Visibility = Visibility.Visible;
            isPowerMode = true;
            powerModeCounter = powerModeDelay;
        }

        private void PowerUpCoolDown()
        {
            powerModeCounter -= 1;
            GameView.Background = new SolidColorBrush(Colors.Goldenrod);
            player.Opacity = 0.7d;

            double remainingPow = (double)powerModeCounter / (double)powerModeDelay * 4;

            powerUpText.Text = "";
            for (int i = 0; i < remainingPow; i++)
            {
                powerUpText.Text += "⚡";
            }
        }

        private void PowerDown()
        {
            isPowerMode = false;
            player.Opacity = 1;
            powerUpText.Visibility = Visibility.Collapsed;

            GameView.Background = this.Resources["RoadBackgroundColor"] as SolidColorBrush;
        }

        private void SpawnPowerUp()
        {
            double scale = GetGameObjectScale();

            PowerUp powerUp = new()
            {
                Height = 50 * scale,
                Width = 50 * scale,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { Angle = Convert.ToDouble(this.Resources["FoliageViewRotationAngle"]) },
            };

            powerUp.SetPosition(rand.Next(100, (int)GameView.Height) * -1, rand.Next(0, (int)(GameView.Width - 55)));

            GameView.Children.Add(powerUp);
        }

        #endregion

        #region Healths

        private void SpawnHealth()
        {
            double scale = GetGameObjectScale();

            Health health = new()
            {
                Height = 80 * scale,
                Width = 80 * scale,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform() { Angle = Convert.ToDouble(this.Resources["FoliageViewRotationAngle"]) },
            };

            health.SetPosition(rand.Next(100, (int)GameView.Height) * -1, rand.Next(0, (int)(GameView.Width - 55)));
            GameView.Children.Add(health);
        }

        private void UpdateHealth(GameObject health)
        {
            health.SetTop(health.GetTop() + 5);

            // if player gets a health
            if (playerHitBox.IntersectsWith(health.GetHitBox()))
            {
                GameViewRemovableObjects.Add(health);

                lives++;
                SetLives();
            }

            if (health.GetTop() > GameView.Height)
            {
                GameViewRemovableObjects.Add(health);
            }
        }

        #endregion

        #region Game Difficulty

        private void ScaleDifficulty()
        {
            if (score >= 10 && score < 20)
            {
                gameSpeed = defaultGameSpeed + 2;
            }

            if (score >= 20 && score < 30)
            {
                gameSpeed = defaultGameSpeed + 4;
            }
            if (score >= 30 && score < 40)
            {
                gameSpeed = defaultGameSpeed + 6;
            }
            if (score >= 40 && score < 50)
            {
                gameSpeed = defaultGameSpeed + 8;
            }
            if (score >= 50 && score < 80)
            {
                gameSpeed = defaultGameSpeed + 10;
            }
            if (score >= 80 && score < 100)
            {
                gameSpeed = defaultGameSpeed + 12;
            }
            if (score >= 100 && score < 130)
            {
                gameSpeed = defaultGameSpeed + 14;
            }
            if (score >= 130 && score < 150)
            {
                gameSpeed = defaultGameSpeed + 16;
            }
            if (score >= 150 && score < 180)
            {
                gameSpeed = defaultGameSpeed + 18;
            }
            if (score >= 180 && score < 200)
            {
                gameSpeed = defaultGameSpeed + 20;
            }
        }

        #endregion

        #endregion
    }
}
