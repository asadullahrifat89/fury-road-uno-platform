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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FuryRoad
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        PeriodicTimer gameTimer;
        List<Rectangle> itemRemover = new List<Rectangle>();

        Random rand = new Random(); // make a new instance of the random class called rand

        ImageBrush playerImage = new ImageBrush(); // create a new image brush for the player
        ImageBrush starImage = new ImageBrush(); // create a new image brush for the star

        Rect playerHitBox; // this rect object will be used to calculate the player hit area with other objects

        // set the game integers including, speed for the traffic and road markings, player speed, car numbers, star counter and power mode counter
        int speed = 15;
        int playerSpeed = 10;
        int carNum;
        int starCounter = 30;
        int powerModeCounter = 200;

        // create two doubles one for score and other called i, this one will be used to animate the player car when we reach the power mode
        double score;
        double i;

        // we will need 4 boolean altogether for this game, since all of them will be false at the start we are defining them in one line. 
        bool moveLeft, moveRight, gameOver, powerMode;

        #endregion

        #region Ctor

        public MainPage()
        {
            this.InitializeComponent();

            myCanvas.Width = Window.Current.Bounds.Width / 1.5;
            myCanvas.Height = Window.Current.Bounds.Height;
        }


        #endregion

        #region Methods

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

            starCounter -= 1; // reduce 1 from the star counter each tick

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
            if (starCounter < 1)
            {
                MakeStar();
                starCounter = rand.Next(600, 900);
            }

            // below is the main game loop, inside of this loop we will go through all of the rectangles available in this game
            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                // first we search through all of the rectangles in this game

                var tag = (string)x.Tag;

                switch (tag)
                {
                    case "roadMarks":
                        {
                            // if we find any of the rectangles with the road marks tag on it then 

                            Canvas.SetTop(x, Canvas.GetTop(x) + speed); // move it down using the speed variable

                            // if the road marks goes below the screen then move it back up top of the screen
                            if (Canvas.GetTop(x) > myCanvas.Height)
                            {
                                Canvas.SetTop(x, -152);
                            }
                        }
                        break;
                    case "Car":
                        {
                            Canvas.SetTop(x, Canvas.GetTop(x) + speed); // move the rectangle down using the speed variable

                            // if the car has left the scene then run then run the change cars function with the current x rectangle inside of it
                            if (Canvas.GetTop(x) > myCanvas.Height)
                            {
                                ChangeCars(x);
                            }

                            // create a new rect called car hit box and assign it to the x which is the cars rectangle
                            Rect carHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                          
                            if (playerHitBox.IntersectsWith(carHitBox))
                            {  
                                // if the player hit box and the car hit box collide and the power mode is ON
                                if (powerMode)
                                {
                                    ChangeCars(x); // run the change cars function with the cars rectangle X inside of it
                                }                                    
                                else
                                {
                                    // if the power is OFF and car and the player collide then
                                    gameTimer.Dispose(); // stop the game timer
                                    scoreText.Text += " Press Enter to replay"; // add this text to the existing text on the label
                                    gameOver = true; // set game over boolean to true
                                }
                            }
                        }
                        break;
                    case "star":
                        {
                            // move it down the screen 5 pixels at a time
                            Canvas.SetTop(x, Canvas.GetTop(x) + 5);

                            // create a new rect with for the star and pass in the star X values inside of it
                            Rect starHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                            // if the player and the star collide then
                            if (playerHitBox.IntersectsWith(starHitBox))
                            {
                                // add the star to the item remover list
                                itemRemover.Add(x);

                                // set power mode to true
                                powerMode = true;

                                // set power mode counter to 200
                                powerModeCounter = 200;
                            }
                            // if the star goes beyon (int)myCanvas.Height pixels then add it to the item remover list
                            if (Canvas.GetTop(x) > myCanvas.Height)
                            {
                                itemRemover.Add(x);
                            }
                        }
                        break;
                    default:
                        break;
                }
            } // end of for each loop

            // if the power mode is true
            if (powerMode == true)
            {
                powerModeCounter -= 1; // reduce 1 from the power mode counter 

                // run the power up function
                PowerUp();

                // if the power mode counter goes below 1 
                if (powerModeCounter < 1)
                {
                    // set power mode to false
                    powerMode = false;
                }
            }
            else
            {
                // if the mode is false then change the player car back to default and also set the background to gray
                playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/player.png"));
                myCanvas.Background = new SolidColorBrush(Colors.Gray);
            }

            // each item we find inside of the item remove we will remove it from the canvas
            foreach (Rectangle y in itemRemover)
            {
                myCanvas.Children.Remove(y);
            }

            // below are the score and speed configurations for the game
            // as you progress in the game you will score higher and traffic speed will go up

            if (score >= 10 && score < 20)
            {
                speed = 12;
            }

            if (score >= 20 && score < 30)
            {
                speed = 14;
            }
            if (score >= 30 && score < 40)
            {
                speed = 16;
            }
            if (score >= 40 && score < 50)
            {
                speed = 18;
            }
            if (score >= 50 && score < 80)
            {
                speed = 22;
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
            if (e.Key == VirtualKey.Enter && gameOver == true)
            {
                // if both of these conditions are true then we will run the start game function
                StartGame();
            }
        }

        private void StartGame()
        {
            Console.WriteLine("GAME STARTED");

            // thi sis the start game function, this function to reset all of the values back to their default state and start the game

            speed = 8; // set speed to 8
            RunGame();

            // set all of the boolean to false
            moveLeft = false;
            moveRight = false;
            gameOver = false;
            powerMode = false;

            // set score to 0
            score = 0;

            // set the score text to its default content
            scoreText.Text = "Survived: 0 Seconds";

            // set up the player image and the star image from the images folder
            playerImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/player.png"));
            starImage.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/star.png"));

            // assign the player image to the player rectangle from the canvas
            player.Fill = playerImage;
            // set the default background colour to gray
            myCanvas.Background = new SolidColorBrush(Colors.Gray);

            // run a initial foreach loop to set up the cars and remove any star in the game

            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                var tag = (string)x.Tag;

                switch (tag)
                {
                    // if we find any rectangle with the car tag on it then we will
                    case "Car":
                        {
                            // set a random location to their top and left position
                            Canvas.SetTop(x, (rand.Next(100, (int)myCanvas.Height) * -1));
                            Canvas.SetLeft(x, rand.Next(0, (int)(myCanvas.Width - 55)));

                            // run the change cars function
                            ChangeCars(x);
                        }
                        break;
                    // if we find a star in the beginning of the game then we will add it to the item remove list
                    case "star":
                        {
                            itemRemover.Add(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            // clear any items inside of the item remover list at the start
            itemRemover.Clear();
        }

        private void myCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            myCanvas.Focus(FocusState.Programmatic);

            StartGame();
        }

        private void ChangeCars(Rectangle car)
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

            // set a random top and left position for the traffic car
            Canvas.SetTop(car, (rand.Next(100, (int)myCanvas.Height) * -1));
            Canvas.SetLeft(car, rand.Next(0, (int)(myCanvas.Width - 55)));
        }

        private void PowerUp()
        {
            // this is the power up function, this function will run when the player collects the star in the game

            i += .5; // increase i by .5 

            // if i is greater than 4 then reset i back to 1
            if (i > 4)
            {
                i = 1;
            }

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
            myCanvas.Background = new SolidColorBrush(Colors.LightCoral);
        }

        private void MakeStar()
        {
            // this is the make star function
            // this function will create a rectangle, assign the star image to and place it on the canvas

            // creating a new star rectangle with its own properties inside of it
            Rectangle newStar = new Rectangle
            {
                Height = 50,
                Width = 50,
                Tag = "star",
                Fill = starImage
            };

            // set a random left and top position for the star
            Canvas.SetLeft(newStar, rand.Next(0, (int)(myCanvas.Width - 55)));
            Canvas.SetTop(newStar, (rand.Next(100, (int)myCanvas.Height) * -1));

            // finally add the new star to the canvas to be animated and to interact with the player
            myCanvas.Children.Add(newStar);

        }

        #endregion
    }
}
