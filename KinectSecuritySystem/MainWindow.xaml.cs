//-----------------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <Description>
// This program was based on the microsoft sample "KinectSecuritySystem"
// It allows a user to set a sequence of gestures to use as a 'pin' to unlock a door by communicating over
// Xbees to an arduino which controls a door.
// </Description>
//-------------------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectSecuritySystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for the MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;
        
        /// <summary> Array for the bodies (Kinect can track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        /// <summary>  Index of the active body (first tracked person in the body array) </summary>
        private int activeBodyIndex = 0;

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> Current kinect status text to display </summary>
        private string statusText = null;

        /// <summary> KinectBodyView object which handles drawing the active body to a view box in the UI </summary>
        private KinectBodyView kinectBodyView = null;
        
        /// <summary> Gesture detector which will be tied to the active body (closest skeleton to the sensor) </summary>
        private GestureDetector gestureDetector = null;

        /// <summary> GestureResultView for displaying gesture results associated with the tracked person in the UI </summary>
        private GestureResultView gestureResultView = null;

        /// <summary> SpaceView for displaying spaceship position and rotation, which are related to gesture detection results </summary>
        private SpaceView spaceView = null;

        /// <summary> Timer for updating Kinect frames and space images at 60 fps </summary>
        private DispatcherTimer dispatcherTimer = null;


        private Stopwatch updateTimer = new Stopwatch();

        private RobotControl robotControl = new RobotControl();
        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;


        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            // initialize the MainWindow
            this.InitializeComponent();

            // only one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);


            // open the sensor
            this.kinectSensor.Open();

            // set the initial status text
            this.UpdateKinectStatusText();

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);
            
            // initialize the SpaceView object
            //this.spaceView = new SpaceView(this.spaceGrid, this.spaceImage);

            // initialize the GestureDetector object
            this.gestureResultView = new GestureResultView(false, false, false, false, -1.0f, null, false, 0, 3, false);
            this.gestureDetector = new GestureDetector(this.kinectSensor, this.gestureResultView);

            // set data context objects for display in UI
            this.DataContext = this;
            this.kinectBodyViewbox.DataContext = this.kinectBodyView;
            this.gestureResultGrid.DataContext = this.gestureResultView;
            this.outputGrid.DataContext = this.gestureResultView;
            //this.spaceGrid.DataContext = this.spaceView;
            //this.collisionResultGrid.DataContext = this.spaceView;

            this.updateTimer.Start();
        }

        /// <summary>
        /// Handles taking a picture during 'security alert' for sending via email
        /// 
        /// </summary>
        private void screenShot()
        {
            if (this.colorBitmap != null)
            {
                //Png encoder to save as .png
                BitmapEncoder encoder = new PngBitmapEncoder();

                //create a frame
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string photosLoc = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(photosLoc, "KinectScreenshot" + ".png");

                //Write screenshot .png to disk
                try
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    Console.WriteLine("Screenshot saved in " + path);

                    sendEmail();
                }
                catch (IOException e)
                {
                    //TODO statusText ?
                    Console.WriteLine(e.Message);
                }
            }
            this.gestureDetector.isTakingScreenshot = false;
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the current Kinect sensor status text to display in UI
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            private set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the GestureDetector object
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.gestureDetector != null)
                {
                    this.gestureDetector.Dispose();
                    this.gestureDetector = null;
                }
            }
        }

        /// <summary>
        /// Polls for new Kinect frames and updates moving objects in the spaceView
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.UpdateKinectStatusText();
            this.UpdateKinectFrameData();
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        /// <summary>
        /// Starts the dispatcher timer to check for new Kinect frames and update objects in space @60fps
        /// Note: We are using a dispatcher timer to demonstrate usage of the VGB polling APIs,
        /// please see the 'DiscreteGestureBasics-WPF' sample for event notification.
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            // set the UI to render at 60fps
            CompositionTarget.Rendering += this.DispatcherTimer_Tick;

            // set the game timer to run at 60fps
            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Tick += this.DispatcherTimer_Tick;
            this.dispatcherTimer.Interval = TimeSpan.FromSeconds(1 / 60);
            this.dispatcherTimer.Start();
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            CompositionTarget.Rendering -= this.DispatcherTimer_Tick;

            if (this.dispatcherTimer != null)
            {
                this.dispatcherTimer.Stop();
                this.dispatcherTimer.Tick -= this.DispatcherTimer_Tick;
            }

            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetector != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                this.gestureDetector.Dispose();
                this.gestureDetector = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Gets the first body in the bodies array that is currently tracked by the Kinect sensor
        /// </summary>
        /// <returns>Index of first tracked body, or -1 if no body is tracked</returns>
        private int GetActiveBodyIndex()
        {
            int activeBodyIndex = -1;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                // find the first tracked body and verify it has hands tracking enabled (by default, Kinect will only track handstate for 2 people)
                if (this.bodies[i].IsTracked && (this.bodies[i].HandRightState != HandState.NotTracked || this.bodies[i].HandLeftState != HandState.NotTracked))
                {
                    activeBodyIndex = i;
                    break;
                }
            }

            return activeBodyIndex;
        }


        /// <summary>
        /// Retrieves the latest body frame data from the sensor and updates the associated gesture detector object
        /// </summary>
        private void UpdateKinectFrameData()
        {
            //FORCE A UI UPDATE TODO:
            //this.gestureDetector.forceUIUPdate();

            bool dataReceived = false;

            using (var bodyFrame = this.bodyFrameReader.AcquireLatestFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    if (!this.bodies[this.activeBodyIndex].IsTracked)
                    {
                        // we lost tracking of the active body, so update to the first tracked body in the array
                        int bodyIndex = this.GetActiveBodyIndex();
                        
                        if (bodyIndex > 0)
                        {
                            this.activeBodyIndex = bodyIndex;
                        }
                    }

                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                Body activeBody = this.bodies[this.activeBodyIndex];

                // visualize the new body data
                this.kinectBodyView.UpdateBodyData(activeBody);

                //
                //Console.WriteLine(this.updateTimer.ElapsedMilliseconds);
                if(this.updateTimer.ElapsedMilliseconds > 3000 && this.gestureResultView.DoorUnlockState)
                {
                    this.robotControl.updateArmData(activeBody);
                    this.updateTimer.Restart();
                }
                
                //this.gestureDetector.updateArmData(activeBody);

                // visualize the new gesture data
                if (activeBody.TrackingId != this.gestureDetector.TrackingId)
                {
                    // if the tracking ID changed, update the detector with the new value
                    this.gestureDetector.TrackingId = activeBody.TrackingId;
                }

                if (this.gestureDetector.TrackingId == 0)
                {
                    // the active body is not tracked, pause the detector and update the UI
                    this.gestureDetector.IsPaused = true;
                    this.gestureDetector.ClosedHandState = false;
                    this.gestureResultView.UpdateGestureResult(false, false, false, false, -1.0f, false, 0, false);
                }
                else
                {
                    // the active body is tracked, unpause the detector
                    this.gestureDetector.IsPaused = false;
                    
                    // steering gestures are only valid when the active body's hand state is 'closed'
                    // update the detector with the latest hand state
                    if (activeBody.HandLeftState == HandState.Closed || activeBody.HandRightState == HandState.Closed)
                    {
                        this.gestureDetector.ClosedHandState = true;
                    }
                    else
                    {
                        this.gestureDetector.ClosedHandState = false;
                    }
                    
                    // get the latest gesture frame from the sensor and updates the UI with the results
                    this.gestureDetector.UpdateGestureData();

                    //Console.WriteLine(this.gestureResultView.DoorUnlockState);
                    if(this.gestureResultView.DoorUnlockState)
                    {
                        this.tabControl.SelectedIndex = 2;
                    }
                    else if (this.gestureDetector.isTakingScreenshot)
                    {
                        screenShot();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the StatusText with the latest sensor state information
        /// </summary>
        private void UpdateKinectStatusText()
        {
            // reset the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Click event for the set gestures button
        /// </summary>
        /// <param name="sender">The button object</param>
        /// <param name="e">The event args</param>
        private void setGestureSequence(object sender, RoutedEventArgs e)
        {
            string firstGesture = this.cmbFirst.SelectedValue.ToString();
            string secondGesture = this.cmbSecond.SelectedValue.ToString();
            string thirdGesture = this.cmbThird.SelectedValue.ToString();

            //Reset the sequence
            this.gestureDetector.resetSequence();
            //Set the new gesture names
            this.gestureDetector.setGestures(firstGesture, secondGesture, thirdGesture);
            //Direct use to the main screen
            this.tabControl.SelectedIndex = 0;
        }

        /// <summary>
        /// Initializes the three comboboxes with the possible gestures names for selection
        /// </summary>
        /// <param name="sender">The sender (combobox) being initialized </param>
        /// <param name="e"> Data associated with the current event</param>
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Add all possible gesture names to a list:
            List<String> data = new List<String>();
            data.Add("Stop_Left");
            data.Add("Stop_Right");
            data.Add("ThumbUp_Left");
            data.Add("ThumbUp_Right");

            //Get a ref to the combobox
            var comboBox = sender as ComboBox;

            //Assign data to the comboBox
            comboBox.ItemsSource = data;

            //Select first item
            comboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            screenShot();
        }

        /// <summary>
        /// Sends an email with an attached screenshot
        /// </summary>
        private void sendEmail()
        {
            string currentTime = DateTime.Now.ToString("dd/MM/yy h:mm:ss tt");

            //Set the attachment path to that of the application
            string attachmentPath = "C:/Users/Luke/Pictures";
            //Get the attachment data.xml found at this path
            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachmentPath + "/KinectScreenshot.png");

            //Make a new email message
            MailMessage mail = new MailMessage();

            //Set the various variables required for an email
            mail.From = new MailAddress("programmingthings@gmail.com");
            mail.To.Add("programmingthings@gmail.com");
            mail.Subject = "KinectSecurity Alert";
            mail.Body = "A user has attempted to unlock your KinectSecurity System at: " + currentTime;
            mail.Attachments.Add(attachment);

            //Create a new emailServer and set its details including email & password
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("programmingthings", "Kangaroo") as ICredentialsByHost;
            smtpServer.EnableSsl = true;

            //Check the security certificates and return applicable errors (if any exist)
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
                { return true; };

            //Send the email
            smtpServer.Send(mail);

            //Dispose of that email
            mail.Dispose();
        }
    }
}
