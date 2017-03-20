using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Samples.Kinect.KinectSecuritySystem.Common;

namespace Microsoft.Samples.Kinect.KinectSecuritySystem
{
    /// <summary>
    /// Holds the logic for controlling the robot arm using skeleton tracking
    /// </summary>
    /// 
    class RobotControl : BindableBase
    {
        /// <summary>
        /// Port for communicating over xBees
        /// </summary>
        static SerialPort port;

        /// <summary>
        /// Delay for sending information to the MeArm
        /// </summary>
        private Stopwatch detectionDelay = new Stopwatch();

        /// <summary> GestureResultView for displaying gesture results associated with the tracked person in the UI </summary>
        private GestureResultView gestureResultView = null;

        /// <summary>
        /// Booleans for arrow display on GUI
        /// </summary>
        private bool moveUp = false;
        private bool moveDown = false;
        private bool moveLeft = false;
        private bool moveRight = false;

        /// <summary>
        /// String to save the axis, information to send to the MeArm
        /// </summary>
        private string kinectAxis = "X";

        /// <summary>
        /// Floats for detecting previous wrist location
        /// </summary>
        private float previousX = 0.0f;
        private float previousY = 0.0f;


        /// <summary> 
        /// Gets or sets the value indicating which axis of the robot arm to control
        /// </summary>
        public string KinectAxis
        {
            get
            {
                return this.kinectAxis;
            }

            set
            {
                this.SetProperty(ref this.kinectAxis, value);
            }
        }


        /// <summary>
        /// Assigns the port based on the baud rate and name
        /// </summary>
        /// <param name="baud"></param>
        /// <param name="name"></param>
        static void beginSerial(int baud, string name)
        {
            port = new SerialPort(name, baud);
        }

        /// <summary>
        /// Opens the serial and connects to the Arduino
        /// </summary>
        public RobotControl(GestureResultView view, string kinectAxis)
        {
            this.gestureResultView = view;
            this.KinectAxis = kinectAxis;

            beginSerial(9600, KinectSecuritySystem.Properties.Settings.Default.COMPort);
            makeConnection();
        }

        /// <summary>
        /// Opens the port
        /// </summary>
        private void makeConnection()
        {
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
        }

        /// <summary>
        /// Update the position of the arm so we can move the servo arm
        /// </summary>
        public void updateArmData(Body body)
        {
            if (body != null)
            {
                Joint wrist;

                if (body.IsTracked)
                {
                    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                    foreach (JointType jointType in joints.Keys)
                    {
                        if (jointType == JointType.WristRight)
                        {
                            joints.TryGetValue(jointType, out wrist);

                            if (wrist != null && wrist.TrackingState == TrackingState.Tracked)
                            {
                                //port.WriteLine("X," + calculateDeg(wrist.Position.X) + "," + calculateDeg(wrist.Position.Y));

                                //Console.WriteLine("DEG X: " + calculateDeg(wrist.Position.X));
                                //Console.WriteLine("DEG Y: " + calculateDeg(wrist.Position.Y));
                                Console.WriteLine("DEGREES X: " + calculateDeg(wrist.Position.X) + ", Y: " + calculateDeg(wrist.Position.Y));

                                if (KinectAxis.Equals("X"))
                                {
                                    moveUp = false;
                                    moveDown = false;

                                    if (wrist.Position.X > previousX)
                                    {
                                        moveRight = true;
                                        moveLeft = false;
                                    }
                                    else if (wrist.Position.X < previousX)
                                    {
                                        moveRight = false;
                                        moveLeft = true;
                                    }
                                    else
                                    {
                                        moveRight = false;
                                        moveLeft = false;
                                    }
                                    port.WriteLine("X," + calculateDeg(wrist.Position.X));
                                }
                                else if (KinectAxis.Equals("Y"))
                                {
                                    moveLeft = false;
                                    moveRight = false;

                                    if (wrist.Position.Y > previousY)
                                    {
                                        moveDown = false;
                                        moveUp = true;
                                    }
                                    else if (wrist.Position.Y < previousY)
                                    {
                                        moveDown = true;
                                        moveUp = false;
                                    }
                                    else
                                    {
                                        moveDown = false;
                                        moveUp = false;
                                    }
                                    port.WriteLine("Y," + calculateDeg(wrist.Position.Y));
                                }

                                previousX = wrist.Position.X;
                                previousY = wrist.Position.Y;
                            }
                        }

                        gestureResultView.UpdateGestureResult(true, false, false, false, 0.0f, true, 0, false, moveUp, moveDown, moveRight, moveLeft);
                       // this.gestureResultView = new GestureResultView(false, false, false, false, -1.0f, null, false, 0, 3, false, false, true, true, false);

                    }
                }
            }
        }

        /// <summary>
        /// Converts the x/y variables to degrees ranging from 0 - 180
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private float calculateDeg(float f)
        {
            float deg;
            //Divides the variable detected by the Kinect (from -1 to 1) and divides it by 1/180 (0.011)
                //add 90 so the degrees are always positive and between 0 - 180
            deg = ((f / 0.011f) + 90);
            //Reverses the degrees detected to match the left/right positioning of the MeArm
            deg = 180 - deg;
            return deg;
        }

        /// <summary>
        /// Outputs data recieved by the Arduino
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.Write(port.ReadExisting());
            //Console.WriteLine("");
            //Console.WriteLine("> ");
        }
    }
}
