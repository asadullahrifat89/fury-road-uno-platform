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

        int gameSpeed = 15;
        int playerSpeed = 6;
        int carNum;
        int powerUpCounter = 30;
        int powerModeCounter = 2000;

        double columns = 0;
        double rows = 0;

        double score;

        bool moveLeft, moveRight, isGameOver, isPowerMode, isGamePaused;

        TimeSpan frameTime = TimeSpan.FromMilliseconds(18);

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            isGameOver = true;

            AdjustView();

            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= MainPage_SizeChanged;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            AdjustView();
        }

        #endregion

        #region Methods

        #region Game Start, Run, Loop, Over

        private void StartGame()
        {
            isGameOver = false;
            Console.WriteLine("GAME STARTED");

            gameSpeed = 8; // set speed to 8
            RunGame();

            // set all of the boolean to false
            moveLeft = false;
            moveRight = false;
            isGameOver = false;
            isPowerMode = false;

            // set score to 0
            score = 0;

            // set the score text to its default content
            scoreText.Text = "Survived: 0 Seconds";

            // assign the player image to the player rectangle from the canvas
            player.SetContent(new Uri("ms-appx:///Assets/Images/player.png"));

            // set the default background colour to gray
            RoadView.Background = App.Current.Resources["RoadBackgroundColor"] as SolidColorBrush;

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
                    isPowerMode = false;
                }
            }
            else
            {
                //playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/player.png"));
                RoadView.Background = App.Current.Resources["RoadBackgroundColor"] as SolidColorBrush;
            }
        }

        private void GameViewLoop()
        {
            score += .05; // increase the score by .5 each tick of the timer

            powerUpCounter -= 1;

            scoreText.Text = "Survived " + score.ToString("#.#") + " Seconds";

            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            UpdatePlayer();

            if (powerUpCounter < 1)
            {
                SpawnPowerUp();
                powerUpCounter = rand.Next(600, 900);
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
            if (moveLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight == true && Canvas.GetLeft(player) + 55 < GameView.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
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

            Canvas.SetTop(roadMark, (roadMark.Height * 2) * -1);
        }

        private void RandomizeRoadMark(GameObject roadMark)
        {
            carNum = rand.Next(1, 4);

            switch (carNum)
            {
                case 1:
                    roadMark.SetContent(new Uri("ms-appx:///Assets/Images/road-dash1.png"));
                    break;
                case 2:
                    roadMark.SetContent(new Uri("ms-appx:///Assets/Images/road-dash2.png"));
                    break;
                case 3:
                    roadMark.SetContent(new Uri("ms-appx:///Assets/Images/road-dash3.png"));
                    break;
            }
        }

        private void RecyleRoadSide(GameObject roadSide)
        {
            RandomizeRoadSide(roadSide);
            Canvas.SetTop(roadSide, roadSide.Height * -1);
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
                    if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                        RecyleTruck(vehicle);
                    else
                        RecyleCar(vehicle);
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
            carNum = rand.Next(1, 6);

            switch (carNum)
            {
                case 1:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car1.png"));
                    break;
                case 2:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car2.png"));
                    break;
                case 3:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car3.png"));
                    break;
                case 4:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car4.png"));
                    break;
                case 5:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car5.png"));
                    break;
                case 6:
                    car.SetContent(new Uri("ms-appx:///Assets/Images/car6.png"));
                    break;
            }

            car.Speed = gameSpeed - rand.Next(0, 7);

            RandomizeVehiclePostion(car);
        }

        private void RecyleTruck(GameObject truck)
        {
            carNum = rand.Next(1, 5);

            switch (carNum)
            {
                case 1:
                    truck.SetContent(new Uri("ms-appx:///Assets/Images/truck1.png"));
                    break;
                case 2:
                    truck.SetContent(new Uri("ms-appx:///Assets/Images/truck2.png"));
                    break;
                case 3:
                    truck.SetContent(new Uri("ms-appx:///Assets/Images/truck3.png"));
                    break;
                case 4:
                    truck.SetContent(new Uri("ms-appx:///Assets/Images/truck4.png"));
                    break;
            }

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

        #region Powerups

        private void UpdatePowerUp(GameObject powerUp)
        {
            // move it down the screen 5 pixels at a time
            Canvas.SetTop(powerUp, Canvas.GetTop(powerUp) + 5);

            Rect starHitBox = powerUp.GetHitBox();

            if (playerHitBox.IntersectsWith(starHitBox))
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
        }

        private void SpawnPowerUp()
        {
            PowerUp newStar = new PowerUp
            {
                Height = 50,
                Width = 50,
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
                gameSpeed = 10;
            }

            if (score >= 20 && score < 30)
            {
                gameSpeed = 12;
            }
            if (score >= 30 && score < 40)
            {
                gameSpeed = 14;
            }
            if (score >= 40 && score < 50)
            {
                gameSpeed = 16;
            }
            if (score >= 50 && score < 80)
            {
                gameSpeed = 18;
            }
            if (score >= 80 && score < 100)
            {
                gameSpeed = 20;
            }
            if (score >= 100 && score < 130)
            {
                gameSpeed = 22;
            }
            if (score >= 130 && score < 150)
            {
                gameSpeed = 24;
            }
            if (score >= 150 && score < 180)
            {
                gameSpeed = 26;
            }
            if (score >= 180 && score < 200)
            {
                gameSpeed = 28;
            }
        }

        #endregion

        #region View

        private void AdjustView()
        {
            RoadView.Width = Window.Current.Bounds.Width > 900 ? Window.Current.Bounds.Width / 1.5 : Window.Current.Bounds.Width;
            RoadView.Height = Window.Current.Bounds.Height;

            GameView.Width = RoadView.Width;
            GameView.Height = RoadView.Height;

            columns = RoadView.Width / 200;
            rows = RoadView.Height / 240;

            Console.WriteLine($"ROAD SIZE {RoadView.Width}x{RoadView.Height}");
        }

        #endregion

        #endregion

        #region Events

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
