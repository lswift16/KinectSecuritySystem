using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.KinectSecuritySystem
{
    class RobotControl
    {
        /// <summary>
        /// Port for communicating over xBees
        /// </summary>
        static SerialPort port;

        /// <summary>
        /// Delay for sending information to the MeArm
        /// </summary>
        private Stopwatch detectionDelay = new Stopwatch();

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
        public RobotControl()
        {
            beginSerial(9600, "COM4");
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
                                port.WriteLine("X," + calculateDeg(wrist.Position.X));
                                //port.WriteLine("Y," + calculateDeg(wrist.Position.Y));
                                //port.WriteLine("X," + calculateDeg(wrist.Position.X) + "," + calculateDeg(wrist.Position.Y));

                                //Console.WriteLine("DEG X: " + calculateDeg(wrist.Position.X));
                                //Console.WriteLine("DEG Y: " + calculateDeg(wrist.Position.Y));
                                Console.WriteLine("DEGREES X: " + calculateDeg(wrist.Position.X) + ", Y: " + calculateDeg(wrist.Position.Y));
                            }
                        }

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
