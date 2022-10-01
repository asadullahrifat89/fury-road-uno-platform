using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing.PrintTicket;
using Windows.System;
using Windows.UI.Core;

namespace FuryRoad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        PeriodicTimer gameTimer;
        List<GameObject> removableObjects = new List<GameObject>();

        Random rand = new Random();

        ImageBrush playerImage = new ImageBrush();
        ImageBrush powerUpImage = new ImageBrush();

        Rect playerHitBox;

        int gameSpeed = 15;
        int playerSpeed = 8;
        int carNum;
        int powerUpCounter = 30;
        int powerModeCounter = 1000;

        double score;
        //double i;

        bool moveLeft, moveRight, isGameOver, isPowerMode;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            myCanvas.Width = Window.Current.Bounds.Width > 500 ? Window.Current.Bounds.Width / 1.5 : Window.Current.Bounds.Width;
            myCanvas.Height = Window.Current.Bounds.Height;

            isGameOver = true;

            this.SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            myCanvas.Width = Window.Current.Bounds.Width > 500 ? Window.Current.Bounds.Width / 1.5 : Window.Current.Bounds.Width;
            myCanvas.Height = Window.Current.Bounds.Height;
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
            playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/player.png"));
            powerUpImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));

            // assign the player image to the player rectangle from the canvas
            player.Fill = playerImage;

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
                            ChangeCars(x);
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            // set a random location to their top and left position
                            Canvas.SetTop(x, (rand.Next(100, (int)myCanvas.Height) * -1));
                            Canvas.SetLeft(x, rand.Next(0, (int)(myCanvas.Width - 55)));

                            // run the change cars function
                            ChangeTrucks(x);
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
            gameTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(18));

            while (await gameTimer.WaitForNextTickAsync())
            {
                GameLoop();
            }
        }

        private void GameLoop()
        {
            score += .05; // increase the score by .5 each tick of the timer

            powerUpCounter -= 1; // reduce 1 from the star counter each tick

            scoreText.Text = "Survived " + score.ToString("#.#") + " Seconds"; // this line will show the seconds passed in decimal numbers in the score text label

            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height); // assign the player hit box to the player

            // below are two if statements that are checking the player can move or right in the scene. 
            if (moveLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight == true && Canvas.GetLeft(player) + 55 < myCanvas.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }

            // if the star counter integer goes below 1 then we run the make star function and also generate a random number inside of the star counter integer
            if (powerUpCounter < 1)
            {
                SpawnPowerUp();
                powerUpCounter = rand.Next(600, 900);
            }

            // below is the main game loop, inside of this loop we will go through all of the rectangles available in this game
            foreach (var gameObject in myCanvas.Children.OfType<GameObject>())
            {
                // first we search through all of the rectangles in this game
                var tag = (string)gameObject.Tag;

                switch (tag)
                {
                    case Constants.ROADMARK_TAG:
                        {
                            UpdateRoadMark(gameObject);
                        }
                        break;
                    case Constants.CAR_TAG:
                        {
                            UpdateCar(gameObject);
                        }
                        break;
                    case Constants.TRUCK_TAG:
                        {
                            UpdateTruck(gameObject);
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

            // if the power mode is true
            if (isPowerMode == true)
            {
                powerModeCounter -= 1; // reduce 1 from the power mode counter 

                // run the power up function
                PowerUp();

                // if the power mode counter goes below 1 
                if (powerModeCounter < 1)
                {
                    // set power mode to false
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

            // below are the score and speed configurations for the game
            // as you progress in the game you will score higher and traffic speed will go up

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
        }

        private void GameOver()
        {
            playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/player-crashed.png"));
            player.Fill = playerImage;

            // if the power is OFF and car and the player collide then
            gameTimer.Dispose(); // stop the game timer
            scoreText.Text += " Press Enter to replay"; // add this text to the existing text on the label
            isGameOver = true; // set game over boolean to true           
        } 

        #endregion

        #region Update Game Objects

        private void UpdatePowerUp(GameObject gameObject)
        {
            // move it down the screen 5 pixels at a time
            Canvas.SetTop(gameObject, Canvas.GetTop(gameObject) + 5);

            // create a new rect with for the star and pass in the star X values inside of it
            Rect starHitBox = gameObject.GetHitBox();

            // if the player and the star collide then
            if (playerHitBox.IntersectsWith(starHitBox))
            {
                // add the star to the item remover list
                removableObjects.Add(gameObject);

                // set power mode to true
                isPowerMode = true;

                // set power mode counter to 200
                powerModeCounter = 200;
            }
            // if the star goes beyon (int)myCanvas.Height pixels then add it to the item remover list
            if (Canvas.GetTop(gameObject) > myCanvas.Height)
            {
                removableObjects.Add(gameObject);
            }
        }

        private void UpdateTruck(GameObject gameObject)
        {
            Canvas.SetTop(gameObject, Canvas.GetTop(gameObject) + gameObject.Speed); // move the rectangle down using the speed variable

            // if the car has left the scene then run then run the change cars function with the current x rectangle inside of it
            if (Canvas.GetTop(gameObject) > myCanvas.Height)
            {
                ChangeTrucks(gameObject);
            }

            // create a new rect called car hit box and assign it to the x which is the cars rectangle
            Rect tructHitBox = gameObject.GetHitBox();

            if (playerHitBox.IntersectsWith(tructHitBox))
            {
                // if the player hit box and the car hit box collide and the power mode is ON
                if (isPowerMode)
                {
                    ChangeTrucks(gameObject); // run the change cars function with the cars rectangle X inside of it
                }
                else
                {
                    GameOver();
                }
            }
        }

        private void UpdateCar(GameObject gameObject)
        {
            Canvas.SetTop(gameObject, Canvas.GetTop(gameObject) + gameObject.Speed); // move the rectangle down using the speed variable

            // if the car has left the scene then run then run the change cars function with the current x rectangle inside of it
            if (Canvas.GetTop(gameObject) > myCanvas.Height)
            {
                ChangeCars(gameObject);
            }

            // create a new rect called car hit box and assign it to the x which is the cars rectangle
            Rect carHitBox = gameObject.GetHitBox();

            if (playerHitBox.IntersectsWith(carHitBox))
            {
                // if the player hit box and the car hit box collide and the power mode is ON
                if (isPowerMode)
                {
                    // run the change cars function with the cars rectangle X inside of it
                    ChangeCars(gameObject);
                }
                else
                {
                    GameOver();
                }
            }
        }

        private void UpdateRoadMark(GameObject gameObject)
        {
            // if we find any of the rectangles with the road marks tag on it then 
            Canvas.SetTop(gameObject, Canvas.GetTop(gameObject) + gameSpeed); // move it down using the speed variable

            // if the road marks goes below the screen then move it back up top of the screen
            if (Canvas.GetTop(gameObject) > myCanvas.Height)
            {
                ChangeRoadMark(gameObject);
            }
        }

        #endregion

        #region Change Game Objects

        private void ChangeCars(GameObject car)
        {
            // we want the game to change the traffic car images as they leave the scene and come back to it again

            carNum = rand.Next(1, 6); // to start lets generate a random number between 1 and 6

            ImageBrush carImage = new ImageBrush(); // create a new image brush for the car image 

            // the switch statement below will see what number have generated for the car num integer and 
            // based on that number it will assign a different image to the car rectangle
            switch (carNum)
            {
                case 1:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car1.png"));
                    break;
                case 2:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car2.png"));
                    break;
                case 3:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car3.png"));
                    break;
                case 4:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car4.png"));
                    break;
                case 5:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car5.png"));
                    break;
                case 6:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/car6.png"));
                    break;
            }

            car.Fill = carImage; // assign the chosen car image to the car rectangle
            car.Speed = gameSpeed - rand.Next(0, 7);
            SetRandomVehiclePostion(car);
        }

        private void ChangeRoadMark(GameObject roadMark)
        {
            // we want the game to change the traffic car images as they leave the scene and come back to it again

            carNum = rand.Next(1, 4); // to start lets generate a random number between 1 and 6

            ImageBrush carImage = new ImageBrush(); // create a new image brush for the car image 

            // the switch statement below will see what number have generated for the car num integer and 
            // based on that number it will assign a different image to the car rectangle
            switch (carNum)
            {
                case 1:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/road-dash1.png"));
                    break;
                case 2:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/road-dash2.png"));
                    break;
                case 3:
                    carImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/road-dash3.png"));
                    break;
            }

            roadMark.Fill = carImage; // assign the chosen car image to the car rectangle
            Canvas.SetTop(roadMark, -152);
        }

        private void ChangeTrucks(GameObject truck)
        {
            // we want the game to change the traffic car images as they leave the scene and come back to it again

            carNum = rand.Next(1, 5); // to start lets generate a random number between 1 and 6

            ImageBrush truckImage = new ImageBrush(); // create a new image brush for the car image 

            // the switch statement below will see what number have generated for the car num integer and 
            // based on that number it will assign a different image to the car rectangle
            switch (carNum)
            {
                case 1:
                    truckImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/truck1.png"));
                    break;
                case 2:
                    truckImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/truck2.png"));
                    break;
                case 3:
                    truckImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/truck3.png"));
                    break;
                case 4:
                    truckImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/truck4.png"));
                    break;
            }

            truck.Fill = truckImage; // assign the chosen car image to the car rectangle
            truck.Speed = gameSpeed - rand.Next(0, 5);
            SetRandomVehiclePostion(truck);
        }

        #endregion

        private void SetRandomVehiclePostion(GameObject car)
        {
            var top = (rand.Next(100, (int)myCanvas.Height) * -1);
            Canvas.SetTop(car, top);

            // set a random top and left position for the traffic car
            var left = rand.Next(0, (int)(myCanvas.Width - 55));
            Canvas.SetLeft(car, left);
        }

        private void PowerUp()
        {
            // this is the power up function, this function will run when the player collects the star in the game

            //i += .5; // increase i by .5 

            // if i is greater than 4 then reset i back to 1
            //if (i > 4)
            //{
            //    i = 1;
            //}

            // with each increment of the i we will change the player image to one of the 4 images below

            //switch (i)
            //{
            //    case 1:
            //        playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/powermode1.png"));
            //        break;
            //    case 2:
            //        playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/powermode2.png"));
            //        break;
            //    case 3:
            //        playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/powermode3.png"));
            //        break;
            //    case 4:
            //        playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/powermode4.png"));
            //        break;
            //}

            // change the background to light coral colour
            myCanvas.Background = new SolidColorBrush(Colors.Goldenrod);
        }

        private void SpawnPowerUp()
        {
            // this is the make star function
            // this function will create a rectangle, assign the star image to and place it on the canvas

            // creating a new star rectangle with its own properties inside of it
            PowerUp newStar = new PowerUp
            {
                Height = 50,
                Width = 50,                
                Fill = powerUpImage
            };

            // set a random left and top position for the star
            Canvas.SetLeft(newStar, rand.Next(0, (int)(myCanvas.Width - 55)));
            Canvas.SetTop(newStar, (rand.Next(100, (int)myCanvas.Height) * -1));

            // finally add the new star to the canvas to be animated and to interact with the player
            myCanvas.Children.Add(newStar);
        }

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
            // key down function will listen for you the user to press the left or right key and it will change the designated boolean to true

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
                // if both of these conditions are true then we will run the start game function
                StartGame();
            }
        }

        #endregion
    }
}
