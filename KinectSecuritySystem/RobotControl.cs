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

        private Stopwatch detectionDelay = new Stopwatch();

        static void beginSerial(int baud, string name)
        {
            port = new SerialPort(name, baud);
        }

        public RobotControl()
        {
            beginSerial(9600, "COM4");
            makeConnection();
        
        }

        /// <summary>
        /// Update the position of the arm so we can move the servo arm
        /// </summary>
        public void updateArmData(Body body)
        {
            float previousPositionX = 0.0f;
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

        private float calculateDeg(float f)
        {
            float deg;
            deg = ((f / 0.011f) + 90);
            deg = 180 - deg;
            return deg;
        }
        private void makeConnection()
        {
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
        }

        static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.Write(port.ReadExisting());
            //Console.WriteLine("");
            //Console.WriteLine("> ");
        }
    }
}
