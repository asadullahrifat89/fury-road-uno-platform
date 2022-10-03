using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Windows.Foundation;
using Windows.System;

namespace FuryRoad
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        PeriodicTimer gameViewTimer;
        PeriodicTimer roadViewTimer;

        List<GameObject> gameViewRemovableObjects = new List<GameObject>();

        Random rand = new Random();

        Rect playerHitBox;

        int gameSpeed = 6;
        int defaultGameSpeed = 6;
        int playerSpeed = 6;
        int markNum;
        int powerUpCounter = 30;
        int powerModeCounter = 10000;

        double score;

        double CarWidth;
        double CarHeight;

        double TruckWidth;
        double TruckHeight;

        double RoadMarkWidth;
        double RoadMarkHeight;

        double RoadSideWidth;
        double RoadSideHeight;

        double HighWayDividerWidth;

        bool moveLeft, moveRight, isGameOver, isPowerMode, isGamePaused;

        TimeSpan frameTime = TimeSpan.FromMilliseconds(18);

        int _accelerationCounter;

        double windowHeight, windowWidth;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            isGameOver = true;

            windowHeight = Window.Current.Bounds.Height;
            windowWidth = Window.Current.Bounds.Width;

            AdjustView();

            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        #endregion

        #region Methods

        #region Game Start, Run, Loop, Over

        private void StartGame()
        {
            Console.WriteLine("GAME STARTED");

            gameSpeed = 6; // set speed to 8
            RunGame();

            // set all of the boolean to false
            moveLeft = false;
            moveRight = false;
            isGameOver = false;
            isPowerMode = false;

            // set score to 0
            score = 0;

            // set the score text to its default content
            scoreText.Text = "Score: 0";

            // assign the player image to the player rectangle from the canvas
            player.SetContent(new Uri("ms-appx:///Assets/Images/player.png"));

            // set the default background colour to gray
            RoadView.Background = this.Resources["RoadBackgroundColor"] as SolidColorBrush;

            player.Width = CarWidth;
            player.Height = CarHeight;

            // run a initial foreach loop to set up the cars and remove any star in the game
            foreach (var x in GameView.Children.OfType<GameObject>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    // if we find any rectangle with the car tag on it then we will
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
                    case Constants.POWERUP_TAG:
                        {
                            gameViewRemovableObjects.Add(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (var x in RoadView.Children.OfType<GameObject>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG: { RandomizeRoadMark(x); } break;
                    case Constants.ROADSIDE_TAG: { RandomizeRoadSide(x); } break;
                    default:
                        break;
                }
            }

            foreach (GameObject y in gameViewRemovableObjects)
            {
                GameView.Children.Remove(y);
            }

            gameViewRemovableObjects.Clear();
        }

        private void RunGame()
        {
            RunGameView();
            RunRoadView();
        }

        private async void RunGameView()
        {
            gameViewTimer = new PeriodicTimer(frameTime);

            while (await gameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private async void RunRoadView()
        {
            roadViewTimer = new PeriodicTimer(frameTime);

            while (await roadViewTimer.WaitForNextTickAsync())
            {
                RoadViewLoop();
            }
        }

        private void RoadViewLoop()
        {
            // below is the main game loop, inside of this loop we will go through all of the rectangles available in this game
            foreach (var gameObject in RoadView.Children.OfType<GameObject>())
            {
                var tag = (string)gameObject.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            UpdateRoadMark(gameObject);
                        }
                        break;
                    case Constants.ROADSIDE_TAG:
                        {
                            UpdateRoadSide(gameObject);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (isGameOver)
                return;

            if (isPowerMode == true)
            {
                powerModeCounter -= 1;

                PowerUp();

                if (powerModeCounter < 1)
                {
                    PowerDown();
                }
            }
            //else
            //{
            //    //playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/player.png"));
            //    RoadView.Background = this.Resources["RoadBackgroundColor"] as SolidColorBrush;
            //}
        }

        private void GameViewLoop()
        {
            score += .05; // increase the score by .5 each tick of the timer

            powerUpCounter -= 1;

            scoreText.Text = "Score: " + score.ToString("#");

            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            if (moveLeft || moveRight)
            {
                UpdatePlayer();
            }

            if (powerUpCounter < 1)
            {
                SpawnPowerUp();
                powerUpCounter = rand.Next(500, 800);
            }

            // below is the main game loop, inside of this loop we will go through all of the rectangles available in this game
            foreach (var gameObject in GameView.Children.OfType<GameObject>())
            {
                var tag = (string)gameObject.Tag;

                switch (tag)
                {
                    case Constants.CAR_TAG:
                    case Constants.TRUCK_TAG:
                        {
                            UpdateVehicle(gameObject);
                        }
                        break;
                    case Constants.POWERUP_TAG:
                        {
                            UpdatePowerUp(gameObject);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (isGameOver)
                return;

            foreach (GameObject y in gameViewRemovableObjects)
            {
                GameView.Children.Remove(y);
            }

            // as you progress in the game you will score higher and game speed will go up
            ScaleDifficulty();
        }

        private void GameOver()
        {
            player.SetContent(new Uri("ms-appx:///Assets/Images/player-crashed.png"));

            gameViewTimer.Dispose();
            roadViewTimer.Dispose();

            scoreText.Text += " Press Enter to replay";
            isGameOver = true;
        }

        #endregion

        #region Player

        private void UpdatePlayer()
        {
            var effectiveSpeed = _accelerationCounter >= playerSpeed ? playerSpeed : _accelerationCounter / 1.3;

            // increase acceleration and stop when player speed is reached
            if (_accelerationCounter <= playerSpeed)
                _accelerationCounter++;

            //Console.WriteLine("ACC:" + _accelerationCounter);

            var left = Canvas.GetLeft(player);

            if (moveLeft == true && left > 0)
            {
                Canvas.SetLeft(player, left - effectiveSpeed);
            }
            if (moveRight == true && left + player.Width < GameView.Width)
            {
                Canvas.SetLeft(player, left + effectiveSpeed);
            }
        }

        #endregion

        #region Road Marks

        private void UpdateRoadMark(GameObject roadMark)
        {
            Canvas.SetTop(roadMark, Canvas.GetTop(roadMark) + gameSpeed);

            if (Canvas.GetTop(roadMark) > RoadView.Height)
            {
                RecyleRoadMark(roadMark);
            }
        }

        private void UpdateRoadSide(GameObject roadSide)
        {
            Canvas.SetTop(roadSide, Canvas.GetTop(roadSide) + gameSpeed);

            if (Canvas.GetTop(roadSide) > RoadView.Height)
            {
                RecyleRoadSide(roadSide);
            }
        }

        private void RecyleRoadMark(GameObject roadMark)
        {
            RandomizeRoadMark(roadMark);

            roadMark.Width = RoadMarkWidth;
            roadMark.Height = RoadMarkHeight;

            Canvas.SetTop(roadMark, ((roadMark.Height * 2) * -1));
        }

        private void RecyleRoadSide(GameObject roadSide)
        {
            RandomizeRoadSide(roadSide);

            //roadSide.Width = RoadSideWidth;
            //roadSide.Height = RoadSideHeight;

            Canvas.SetTop(roadSide, (roadSide.Height * -1));
        }

        private void RandomizeRoadMark(GameObject roadMark)
        {
            markNum = rand.Next(0, AssetTemplates.ROADMARK_TEMPLATES.Length);
            roadMark.SetContent(AssetTemplates.ROADMARK_TEMPLATES[markNum]);
        }

        private void RandomizeRoadSide(GameObject roadSide)
        {
            roadSide.SetContent(new Uri("ms-appx:///Assets/Images/road-side.png"));
        }

        #endregion

        #region Vehicles

        private void UpdateVehicle(GameObject vehicle)
        {
            // move down vehicle
            Canvas.SetTop(vehicle, Canvas.GetTop(vehicle) + vehicle.Speed);

            // if vechicle goes out of bounds
            if (Canvas.GetTop(vehicle) > GameView.Height)
            {
                if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                    RecyleTruck(vehicle);
                else
                    RecyleCar(vehicle);
            }

            // if vehicle collides with player
            if (playerHitBox.IntersectsWith(vehicle.GetHitBox()))
            {
                if (isPowerMode)
                {
                    //if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                    //    RecyleTruck(vehicle);
                    //else
                    //    RecyleCar(vehicle);
                }
                else
                {
                    GameOver();
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

        private void RecyleCar(GameObject car)
        {
            markNum = rand.Next(0, AssetTemplates.CAR_TEMPLATES.Length);

            car.SetContent(AssetTemplates.CAR_TEMPLATES[markNum]);

            car.Width = CarWidth;
            car.Height = CarHeight;

            car.Speed = gameSpeed - rand.Next(0, 7);

            RandomizeVehiclePostion(car);
        }

        private void RecyleTruck(GameObject truck)
        {
            markNum = rand.Next(0, AssetTemplates.TRUCK_TEMPLATES.Length);

            truck.SetContent(AssetTemplates.TRUCK_TEMPLATES[markNum]);

            truck.Width = TruckWidth;
            truck.Height = TruckHeight;

            truck.Speed = gameSpeed - rand.Next(0, 5);

            RandomizeVehiclePostion(truck);
        }

        private void RandomizeVehiclePostion(GameObject vehicle)
        {
            // set a random top and left position for the traffic car
            Canvas.SetTop(vehicle, (rand.Next(100, (int)GameView.Height) * -1));
            Canvas.SetLeft(vehicle, rand.Next(0, (int)GameView.Width - 50));

            //if (GameView.Children.OfType<GameObject>().Where(x => x is Car or Truck).Any(y => y.GetDistantHitBox().IntersectsWith(hitBox)))
            //{

            //}
        }

        #endregion

        #region Power Ups

        private void UpdatePowerUp(GameObject powerUp)
        {
            // move it down the screen 5 pixels at a time
            Canvas.SetTop(powerUp, Canvas.GetTop(powerUp) + 5);

            if (playerHitBox.IntersectsWith(powerUp.GetHitBox()))
            {
                gameViewRemovableObjects.Add(powerUp);
                isPowerMode = true;
                powerModeCounter = 200;
            }

            if (Canvas.GetTop(powerUp) > GameView.Height)
            {
                gameViewRemovableObjects.Add(powerUp);
            }
        }

        private void PowerUp()
        {
            RoadView.Background = new SolidColorBrush(Colors.Goldenrod);
            player.Opacity = 0.7d;
        }

        private void PowerDown()
        {
            isPowerMode = false;
            player.Opacity = 1;

            RoadView.Background = this.Resources["RoadBackgroundColor"] as SolidColorBrush;
        }

        private void SpawnPowerUp()
        {
            var scale = GetGameObjectScale();

            PowerUp newStar = new PowerUp
            {
                Height = 50 * scale,
                Width = 50 * scale,
            };

            newStar.SetContent(new Uri("ms-appx:///Assets/Images/star.png"));

            Canvas.SetLeft(newStar, rand.Next(0, (int)(GameView.Width - 55)));
            Canvas.SetTop(newStar, (rand.Next(100, (int)GameView.Height) * -1));

            GameView.Children.Add(newStar);
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

        #region View

        private void AdjustView()
        {
            RoadView.Width = windowWidth > 200 && windowWidth < 700 ? windowWidth * 1.2 : windowWidth > 900 ? windowWidth / 1.5 : windowWidth;
            RoadView.Height = windowHeight * 2;

            var scale = GetGameObjectScale();

            RoadView.Width = RoadView.Width - 50;
            RoadView.Height = RoadView.Height - 50;

            GameView.Width = RoadView.Width - 40 * scale;
            GameView.Height = RoadView.Height;

            BackgroundView.Width = windowWidth * 3;
            BackgroundView.Height = windowHeight * 2;

            CarWidth = Convert.ToDouble(this.Resources["CarWidth"]);
            CarHeight = Convert.ToDouble(this.Resources["CarHeight"]);

            TruckWidth = Convert.ToDouble(this.Resources["TruckWidth"]);
            TruckHeight = Convert.ToDouble(this.Resources["TruckHeight"]);

            CarWidth = CarWidth * scale; CarHeight = CarHeight * scale;
            TruckWidth = TruckWidth * scale; TruckHeight = TruckHeight * scale;

            Console.WriteLine($"CAR WIDTH {CarWidth}");
            Console.WriteLine($"CAR HEIGHT {CarHeight}");

            Console.WriteLine($"TRUCK WIDTH {TruckWidth}");
            Console.WriteLine($"TRUCK HEIGHT {TruckHeight}");

            RoadMarkWidth = Convert.ToDouble(this.Resources["RoadMarkWidth"]);
            RoadMarkHeight = Convert.ToDouble(this.Resources["RoadMarkHeight"]);

            RoadSideWidth = Convert.ToDouble(this.Resources["RoadSideWidth"]);
            RoadSideHeight = Convert.ToDouble(this.Resources["RoadSideHeight"]);

            RoadMarkWidth = RoadMarkWidth * scale; RoadMarkHeight = RoadMarkHeight * scale;
            RoadSideWidth = RoadSideWidth * scale; RoadSideHeight = RoadSideHeight * scale;

            Console.WriteLine($"ROAD MARK WIDTH {RoadMarkWidth}");
            Console.WriteLine($"ROAD MARK HEIGHT {RoadMarkHeight}");

            Console.WriteLine($"ROAD SIDE WIDTH {RoadSideWidth}");
            Console.WriteLine($"ROAD SIDE HEIGHT {RoadSideHeight}");

            // run a initial foreach loop to set up the cars and remove any star in the game
            foreach (var x in GameView.Children.OfType<GameObject>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    // if we find any rectangle with the car tag on it then we will
                    case Constants.CAR_TAG:
                        {
                            x.Height = CarHeight;
                            x.Width = CarWidth;
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            x.Height = TruckHeight;
                            x.Width = TruckWidth;
                        }
                        break;
                    case Constants.POWERUP_TAG:
                        {
                            x.Width = 50 * scale;
                            x.Height = 50 * scale;
                        }
                        break;
                    default:
                        break;
                }
            }

            highWayLeftSide.Width = RoadSideWidth;
            highWayRightSide.Width = RoadSideWidth;

            Canvas.SetLeft(highWayRightSide, RoadView.Width - RoadSideWidth);

            foreach (var x in RoadView.Children.OfType<GameObject>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            x.Width = RoadMarkWidth;
                            x.Height = RoadMarkHeight;
                        }
                        break;
                    //case Constants.ROADSIDE_TAG: { RandomizeRoadSide(x); } break;
                    default:
                        break;
                }
            }

            player.Width = CarWidth;
            player.Height = CarHeight;

            var carY = (GameView.Height / 1.3) - (370 * scale);
            Console.WriteLine($"CAR Y: {carY}");

            Canvas.SetTop(player, carY);

            HighWayDividerWidth = Convert.ToDouble(this.Resources["HighWayDividerWidth"]);
            HighWayDividerWidth = HighWayDividerWidth * scale;
            Console.WriteLine($"HIGHWAY DIVIDER WIDTH {HighWayDividerWidth}");

            highWayDivider.Width = HighWayDividerWidth;
            Canvas.SetLeft(highWayDivider, (RoadView.Width / 2) - (highWayDivider.Width / 2));
        }

        public double GetGameObjectScale()
        {
            switch (RoadView.Width)
            {
                case <= 300:
                    return 0.65;
                case <= 400:
                    return 0.70;
                case <= 500:
                    return 0.75;
                case <= 700:
                    return 0.80;
                case <= 900:
                    return 0.85;
                case <= 1000:
                    return 0.90;
                case <= 1400:
                    return 0.95;
                case <= 2000:
                    return 1;
                default:
                    return 1;
            }
        }

        #endregion

        #endregion

        #region Events

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            windowWidth = args.NewSize.Width;
            windowHeight = args.NewSize.Height;

            Console.WriteLine($"WINDOWS SIZE: {windowWidth}x{windowHeight}");

            AdjustView();
        }

        private void GameView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (isGameOver)
            {
                GameView.Focus(FocusState.Programmatic);

                StartGame();
            }
            else
            {
                if (isGamePaused)
                {
                    RunGame();
                    isGamePaused = false;
                }
                else
                {
                    gameViewTimer.Dispose();
                    roadViewTimer.Dispose();
                    isGamePaused = true;
                }
            }
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
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {
            _accelerationCounter = 0;

            // when the player releases the left or right key it will set the designated boolean to false
            if (e.Key == VirtualKey.Left)
            {
                moveLeft = false;
            }
            if (e.Key == VirtualKey.Right)
            {
                moveRight = false;
            }

            // in this case we will listen for the enter key aswell but for this to execute we will need the game over boolean to be true
            if (e.Key == VirtualKey.Enter && isGameOver == true)
            {
                StartGame();
            }
        }

        #endregion
    }
}
