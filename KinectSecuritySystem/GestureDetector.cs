//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectSecuritySystem
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using System.Windows.Media.Imaging;
    using System.Threading.Tasks;
    using System.IO.Ports;              //Library for serial stuff
    using System.Windows.Media;
    using System.Threading;
    using System.Diagnostics;



    /// <summary>
    /// Gesture Detector class which polls for VisualGestureBuilderFrames from the Kinect sensor
    /// Updates the associated GestureResultView object with the latest gesture results
    /// </summary>
    public sealed class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"Database\Gestures.gbd";

        //Gesture definitions:
        /// <summary> The first gesture to be detected - can be changed by user</summary>
        private string first_Gesture = null;
        private string second_Gesture = null;
        private string third_Gesture = null;

        /// <summary>
        /// Toggle each gesture after it's recognized by kinect 
        /// </summary>
        private bool bFirstGesture = false;
        private bool bSecondGesture = false;
        private bool bThirdGesture = false;
        private bool doorUnlocked = false;

        /// <summary>
        /// The current number of attempts to open the door
        /// </summary>
        private int numberOfAttempts = 0;

        /// <summary>
        /// Boolean for checking whether a screenshot needs to be taken for the security alert
        /// </summary>
        public bool isTakingScreenshot = false;

        /// <summary> 
        /// Holds the target gesture for use in sequence logic 
        /// </summary>
        private string target_Gesture = null;

        /// <summary>
        /// Used for adding a delay to the gesture detection, required to prevent instant detection
        /// of false gestures immediatly after correct gestures
        /// </summary>
        private Stopwatch detectionDelay = new Stopwatch();  

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Sets the three gestures used to lock/unlock the system to the parameters
        /// </summary>
        /// <param name="gesture1"></param>
        /// <param name="gesture2"></param>
        /// <param name="gesture3"></param>
        public void setGestures(string gesture1, string gesture2, string gesture3)
        {
            this.first_Gesture = gesture1;
            this.second_Gesture = gesture2;
            this.third_Gesture = gesture3;

            target_Gesture = gesture1;

            Console.WriteLine("Target: " + target_Gesture);
        }

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }
          
            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            this.GestureResultView = gestureResultView;
            this.ClosedHandState = false;

            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
            }

            // load all gestures from the gesture database
            using (var database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }


            //Initialize the gestures to a set sequence: TODO set from the UI
            setGestures("Stop_Left", "Stop_Right", "ThumbUp_Left");
        }

        /// <summary> 
        /// Gets the GestureResultView object which stores the detector results for display in the UI 
        /// </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the body associated with the detector has at least one hand closed
        /// </summary>
        public bool ClosedHandState { get; set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Forces the UI to update it's required parameters
        /// </summary>
        public void forceUIUPdate()
        {
            //Force an update without the kinect plugged in to test the UI:
            this.GestureResultView.UpdateGestureResult(true, true, true, true, 0.0f, true, 1, false, false, false, false, false);
        }

        /// <summary>
        /// Retrieves the latest gesture detection results from the sensor
        /// </summary>
        public void UpdateGestureData()
        {
            this.detectionDelay.Start();
            isTakingScreenshot = false;
            using (var frame = this.vgbFrameReader.CalculateAndAcquireLatestFrame())
            {
                if (frame != null)
                {
                    //Get all discrete and continuous gesture results that arrived with the latest frame
                    var discreteResults = frame.DiscreteGestureResults;
                    var continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null && !doorUnlocked)
                    {
                        foreach (var gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (result.Confidence > 0.8 && detectionDelay.ElapsedMilliseconds > 2000)
                                    {
                                        if (gesture.Name.Equals(this.first_Gesture))
                                        {
                                            if (target_Gesture.Equals(gesture.Name))
                                            {
                                                bFirstGesture = true;
                                                target_Gesture = this.second_Gesture;
                                            }
                                            else
                                            {
                                                Console.WriteLine("First gesture detected but not target gesture");
                                                bFirstGesture = false;
                                                resetSequence();
                                                
                                            }
                                            detectionDelay.Restart();
                                        }
                                        else if (gesture.Name.Equals(this.second_Gesture))
                                        {
                                            if (target_Gesture.Equals(gesture.Name))
                                            {
                                                bSecondGesture = true;
                                                target_Gesture = this.third_Gesture;
                                            }
                                            else
                                            {
                                                Console.WriteLine("Second gesture detected but not target gesture");
                                                bSecondGesture = false;
                                                resetSequence();
                                               
                                            }
                                            detectionDelay.Restart();
                                        }
                                        else if (gesture.Name.Equals(this.third_Gesture))
                                        {
                                            if (target_Gesture.Equals(gesture.Name))
                                            {
                                                bThirdGesture = true;
                                                target_Gesture = "";
                                                doorUnlocked = true;
                                            }
                                            else
                                            {
                                                Console.WriteLine("Third gesture detected but not target gesture");
                                                bThirdGesture = false;
                                                resetSequence();
                                               
                                            }
                                            detectionDelay.Restart();
                                        }
                                    }
                                }
                            }
                            //Break out of loop if gesture sequence complete
                            if (doorUnlocked)
                            {
                                break;
                            }
                        }

                        if (!doorUnlocked && numberOfAttempts >= 3)
                        {
                            isTakingScreenshot = true;
                            numberOfAttempts = 0;
                        }

                        this.GestureResultView.UpdateGestureResult(true, bFirstGesture, bSecondGesture,
                           bThirdGesture, 0.0f, doorUnlocked, numberOfAttempts, isTakingScreenshot, false, false, false, false);

                        //Console.WriteLine("1st Gesture: " + bFirstGesture);
                        //Console.WriteLine("2nd Gesture:" + bSecondGesture);
                        //Console.WriteLine("3rd Gesture:" + bThirdGesture);
                        //Console.WriteLine("target:" + target_Gesture);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the current sequence of gestures after a failed attempt, 
        /// or when changing the unlock sequence
        /// </summary>
        public void resetSequence()
        {
            //Reset variables
            bFirstGesture = false;
            bSecondGesture = false;
            bThirdGesture = false;
            bool bDoorUnlockState = false;
            target_Gesture = first_Gesture;
            //Increment attempts made
            numberOfAttempts++;

            // update the UI with the latest gesture detection results
            this.GestureResultView.UpdateGestureResult(false, bFirstGesture, bSecondGesture, bThirdGesture, 0.0f, bDoorUnlockState, numberOfAttempts, isTakingScreenshot, false, false, false, false);

            //Console.WriteLine("Number of attempts after reset: " + numberOfAttempts);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        public void Dispose()
        {
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.Dispose();
                this.vgbFrameReader = null;
            }

            if (this.vgbFrameSource != null)
            {
                this.vgbFrameSource.Dispose();
                this.vgbFrameSource = null;
            }
        }
    }
}
