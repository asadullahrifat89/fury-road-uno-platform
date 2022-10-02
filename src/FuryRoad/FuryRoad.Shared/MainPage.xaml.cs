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

        PeriodicTimer gameTimer;
        List<GameObject> removableObjects = new List<GameObject>();

        Random rand = new Random();

        Image playerImage = new Image();
        Image powerUpImage = new Image();

        Rect playerHitBox;

        int gameSpeed = 15;
        int playerSpeed = 6;
        int carNum;
        int powerUpCounter = 30;
        int powerModeCounter = 1000;

        double columns = 0;
        double rows = 0;

        double score;

        bool moveLeft, moveRight, isGameOver, isPowerMode;

        TimeSpan frameTime = TimeSpan.FromMilliseconds(18);

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            isGameOver = true;

            AdjustView();
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

            // set up the player image and the star image from the images folder
            playerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/player.png"));
            powerUpImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));

            // assign the player image to the player rectangle from the canvas
            player.Child = playerImage;

            // set the default background colour to gray
            myCanvas.Background = App.Current.Resources["RoadBackgroundColor"] as SolidColorBrush;

            // run a initial foreach loop to set up the cars and remove any star in the game
            foreach (var x in myCanvas.Children.OfType<GameObject>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    // if we find any rectangle with the car tag on it then we will
                    case Constants.CAR_TAG:
                        {
                            // set a random location to their top and left position
                            Canvas.SetTop(x, (rand.Next(100, (int)myCanvas.Height) * -1));
                            Canvas.SetLeft(x, rand.Next(0, (int)(myCanvas.Width - 55)));

                            // run the change cars function
                            RandomizeCar(x);
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            // set a random location to their top and left position
                            Canvas.SetTop(x, (rand.Next(100, (int)myCanvas.Height) * -1));
                            Canvas.SetLeft(x, rand.Next(0, (int)(myCanvas.Width - 55)));

                            // run the change cars function
                            RandomizeTruck(x);
                        }
                        break;
                    case Constants.POWERUP_TAG:
                        {
                            removableObjects.Add(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            removableObjects.Clear();
        }

        public async void RunGame()
        {
            gameTimer = new PeriodicTimer(frameTime);

            while (await gameTimer.WaitForNextTickAsync())
            {
                GameLoop();
            }
        }

        private void GameLoop()
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
            foreach (var gameObject in myCanvas.Children.OfType<GameObject>())
            {
                var tag = (string)gameObject.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            UpdateRoadMark(gameObject);
                        }
                        break;
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
                //playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/player.png"));
                myCanvas.Background = App.Current.Resources["RoadBackgroundColor"] as SolidColorBrush;
            }

            foreach (GameObject y in removableObjects)
            {
                myCanvas.Children.Remove(y);
            }

            // as you progress in the game you will score higher and game speed will go up
            ScaleDifficulty();
        }

        private void GameOver()
        {
            playerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/player-crashed.png"));
            player.Child = playerImage;

            gameTimer.Dispose();
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
            if (moveRight == true && Canvas.GetLeft(player) + 55 < myCanvas.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }
        }

        #endregion

        #region Road Marks

        private void UpdateRoadMark(GameObject roadMark)
        {
            Canvas.SetTop(roadMark, Canvas.GetTop(roadMark) + gameSpeed);

            if (Canvas.GetTop(roadMark) > myCanvas.Height)
            {
                RandomizeRoadMark(roadMark);
            }
        }

        private void RandomizeRoadMark(GameObject roadMark)
        {
            carNum = rand.Next(1, 4);

            Image carImage = new Image();

            switch (carNum)
            {
                case 1:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/road-dash1.png"));
                    break;
                case 2:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/road-dash2.png"));
                    break;
                case 3:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/road-dash3.png"));
                    break;
            }

            roadMark.Child = carImage;
            Canvas.SetTop(roadMark, -152);
        }

        #endregion

        #region Vehicles

        private void UpdateVehicle(GameObject vehicle)
        {
            // move down vehicle
            Canvas.SetTop(vehicle, Canvas.GetTop(vehicle) + vehicle.Speed);

            // if vechicle goes out of bounds
            if (Canvas.GetTop(vehicle) > myCanvas.Height)
            {
                if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                    RandomizeTruck(vehicle);
                else
                    RandomizeCar(vehicle);
            }

            // if vehicle collides with player
            if (playerHitBox.IntersectsWith(vehicle.GetHitBox()))
            {
                if (isPowerMode)
                {
                    if ((string)vehicle.Tag == Constants.TRUCK_TAG)
                        RandomizeTruck(vehicle);
                    else
                        RandomizeCar(vehicle);
                }
                else
                {
                    GameOver();
                }
            }

            if (isGameOver)
                return;

            // if vechicle will collide with another vehicle
            if (myCanvas.Children.OfType<GameObject>()
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

        private void RandomizeCar(GameObject car)
        {
            carNum = rand.Next(1, 6);

            Image carImage = new Image();

            switch (carNum)
            {
                case 1:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car1.png"));
                    break;
                case 2:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car2.png"));
                    break;
                case 3:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car3.png"));
                    break;
                case 4:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car4.png"));
                    break;
                case 5:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car5.png"));
                    break;
                case 6:
                    carImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/car6.png"));
                    break;
            }

            car.Child = carImage;
            car.Speed = gameSpeed - rand.Next(0, 7);

            SetRandomVehiclePostion(car);
        }

        private void RandomizeTruck(GameObject truck)
        {
            carNum = rand.Next(1, 5);

            Image truckImage = new Image();

            switch (carNum)
            {
                case 1:
                    truckImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/truck1.png"));
                    break;
                case 2:
                    truckImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/truck2.png"));
                    break;
                case 3:
                    truckImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/truck3.png"));
                    break;
                case 4:
                    truckImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/truck4.png"));
                    break;
            }

            truck.Child = truckImage;
            truck.Speed = gameSpeed - rand.Next(0, 5);

            SetRandomVehiclePostion(truck);
        }

        private void SetRandomVehiclePostion(GameObject vehicle)
        {
            // set a random top and left position for the traffic car
            Canvas.SetTop(vehicle, (rand.Next(100, (int)myCanvas.Height) * -1));
            Canvas.SetLeft(vehicle, rand.Next(0, (int)myCanvas.Width - 50));

            //if (myCanvas.Children.OfType<GameObject>().Where(x => x is Car or Truck).Any(y => y.GetDistantHitBox().IntersectsWith(hitBox)))
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
                removableObjects.Add(powerUp);
                isPowerMode = true;
                powerModeCounter = 200;
            }

            if (Canvas.GetTop(powerUp) > myCanvas.Height)
            {
                removableObjects.Add(powerUp);
            }
        }

        private void PowerUp()
        {
            myCanvas.Background = new SolidColorBrush(Colors.Goldenrod);
        }

        private void SpawnPowerUp()
        {
            PowerUp newStar = new PowerUp
            {
                Height = 50,
                Width = 50,
                Child = powerUpImage
            };

            Canvas.SetLeft(newStar, rand.Next(0, (int)(myCanvas.Width - 55)));
            Canvas.SetTop(newStar, (rand.Next(100, (int)myCanvas.Height) * -1));

            myCanvas.Children.Add(newStar);
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
            myCanvas.Width = Window.Current.Bounds.Width > 900 ? Window.Current.Bounds.Width / 1.5 : Window.Current.Bounds.Width;
            myCanvas.Height = Window.Current.Bounds.Height;

            columns = myCanvas.Width / 200;
            rows = myCanvas.Height / 240;

            Console.WriteLine($"ROAD SIZE {myCanvas.Width}x{myCanvas.Height}");
        }

        #endregion

        #endregion

        #region Events

        private void myCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (isGameOver)
            {
                myCanvas.Focus(FocusState.Programmatic);

                StartGame();
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
