//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.ContinuousGestureBasics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /// <summary>
    /// Gesture Detector class which polls for VisualGestureBuilderFrames from the Kinect sensor
    /// Updates the associated GestureResultView object with the latest gesture results
    /// </summary>
    public sealed class GestureDetector : IDisposable
    {


        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"Database\Stop.gbd";

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

        /// <summary> 
        /// The current state of the door (Unlocked or locked)
        /// </summary>
            //private bool bDoorLockState = false;

        /// <summary> 
        /// Holds the last performed gesture for use in sequence logic 
        /// </summary>
        private string last_Gesture = null;

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
        public void SetGestures(string gesture1, string gesture2, string gesture3)
        {
            this.first_Gesture = gesture1;
            this.second_Gesture = gesture2;
            this.third_Gesture = gesture3;
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


            //Initialize the gestures to a set sequence
            SetGestures("Stop_Left", "Stop_Right", "ThumbUp_Left");
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

        public void forceUIUPdate()
        {
            //Force an update without the kinect plugged in to test the UI:
            this.GestureResultView.UpdateGestureResult(true, true, true, true, 0.0f, true);
        }

        /// <summary>
        /// Retrieves the latest gesture detection results from the sensor
        /// </summary>
        public void UpdateGestureData()
        {
            

            using (var frame = this.vgbFrameReader.CalculateAndAcquireLatestFrame())
            {
                if (frame != null)
                {
                    // get all discrete and continuous gesture results that arrived with the latest frame
                    var discreteResults = frame.DiscreteGestureResults;
                    var continuousResults = frame.ContinuousGestureResults;

                    if (discreteResults != null)
                    {
                        bool firstGestureDetected = this.GestureResultView.FirstGesture;
                        bool secondGestureDetected = this.GestureResultView.SecondGesture;
                        bool thirdGestureDetected = this.GestureResultView.ThirdGesture;
                        bool fourthGestureDetected = this.GestureResultView.FirstGesture;
                        bool bDoorUnlockState = this.GestureResultView.DoorUnlockState;

                        foreach (var gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (!firstGestureDetected && gesture.Name.Equals(this.first_Gesture) && (result.Confidence == 1))
                                    {
                                        firstGestureDetected = result.Detected;
                                        bFirstGesture = true;
                                        last_Gesture = gesture.Name;
                                    }
                                    else if (!secondGestureDetected && gesture.Name.Equals(this.second_Gesture) && (result.Confidence == 1))
                                    {
                                        
                                        secondGestureDetected = result.Detected;
                                        if (last_Gesture == first_Gesture)
                                        {
                                            bSecondGesture = true;
                                            last_Gesture = gesture.Name;
                                        }
                                        else
                                        {
                                            //Gesture sequence failed, Start from beginning
                                            //resetSequence();
                                        }
                                    }
                                    else if (!thirdGestureDetected && gesture.Name.Equals(this.third_Gesture) && (result.Confidence == 1))
                                    {
                                        thirdGestureDetected = result.Detected;
                                        

                                        if (bFirstGesture && last_Gesture == second_Gesture)
                                        {
                                            bThirdGesture = true;
                                            bDoorUnlockState = true;
                                            last_Gesture = gesture.Name;
                                        }
                                        else
                                        {
                                            //Gesture sequence failed, Start from beginning
                                            //resetSequence();
                                        }
                                    }
                                }
                            }

                            //Break out of loop if gesture sequence complete
                            if (bDoorUnlockState)
                            {
                                break;
                            }
                        }

                        // update the UI with the latest gesture detection results
                        this.GestureResultView.UpdateGestureResult(true, firstGestureDetected, secondGestureDetected, thirdGestureDetected, 0.0f, bDoorUnlockState);
                    }
                }
            }
        }

        public void resetSequence()
        {
            last_Gesture = null;
            bFirstGesture = false;
            bSecondGesture = false;
            bThirdGesture = false;

            bool bDoorUnlockState = false;

            // update the UI with the latest gesture detection results
            this.GestureResultView.UpdateGestureResult(false, bFirstGesture, bSecondGesture, bThirdGesture, 0.0f, bDoorUnlockState);
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
