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
        private readonly string gestureDatabase = @"Database\Security.gbd";

        //Gesture definitions:
        /// <summary> The first gesture to be detected - can be changed by user</summary>
        private string first_Gesture = "Stop_Left"; //null;
        private string second_Gesture = "Stop_Right"; //null;
        private string third_Gesture = "ThumbUp_Left"; //null;
        private string fourth_Gesture = "HandsUp"; //null;

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum left turn position </summary>
        //private readonly string maxTurnLeftGestureName = "MaxTurn_Left";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is holding the maximum right turn position </summary>
        //private readonly string maxTurnRightGestureName = "MaxTurn_Right";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is holding the wheel straight </summary>
        //private readonly string steerStraightGestureName = "SteerStraight";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is actively turning the wheel to the left </summary>
        //private readonly string steerLeftGestureName = "Steer_Left";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is actively turning the wheel to the right </summary>
        //private readonly string steerRightGestureName = "Steer_Right";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is returning the wheel to the straight position after turning left </summary>
        //private readonly string returnRightGestureName = "Return_Left";

        ///// <summary> Name of the discrete gesture in the database for detecting when the user is returning the wheel to the straight position after turning right </summary>
        //private readonly string returnLeftGestureName = "Return_Right";

        ///// <summary> Name of the continuous gesture in the database which tracks the steering progress </summary>
        //private readonly string steerProgressGestureName = "SteerProgress";

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

            // disable the set of gestures which determine the 'keep straight' behavior, we will use hand state instead
            //foreach (var gesture in this.vgbFrameSource.Gestures)
            //{
            //    if (gesture.Name.Equals(this.steerStraightGestureName) || gesture.Name.Equals(this.returnLeftGestureName) || gesture.Name.Equals(this.returnRightGestureName))
            //    {
            //        this.vgbFrameSource.SetIsEnabled(gesture, false);
            //    }
            //}
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

                        foreach (var gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    if (gesture.Name.Equals(this.fourth_Gesture) && (result.Confidence == 1)) 
                                    {

                                        Console.WriteLine("Fourth detected");
                                    }
                                    if (gesture.Name.Equals(this.first_Gesture))
                                    {
                                        firstGestureDetected = result.Detected;
                                        Console.WriteLine("First detected");
                                    }
                                    else if (gesture.Name.Equals(this.second_Gesture))
                                    {
                                        secondGestureDetected = result.Detected;
                                        Console.WriteLine("Second detected");
                                    }
                                    else if (gesture.Name.Equals(this.third_Gesture))
                                    {
                                        thirdGestureDetected = result.Detected;
                                        Console.WriteLine("Third detected");
                                    }
                                }
                            }

                            //if (continuousResults != null)
                            //{
                            //    if (gesture.Name.Equals(this.steerProgressGestureName) && gesture.GestureType == GestureType.Continuous)
                            //    {
                            //        ContinuousGestureResult result = null;
                            //        continuousResults.TryGetValue(gesture, out result);

                            //        if (result != null)
                            //        {
                            //            steerProgress = result.Progress;
                            //        }
                            //    }
                            //}
                        }

                        // update the UI with the latest gesture detection results
                        this.GestureResultView.UpdateGestureResult(true, firstGestureDetected, secondGestureDetected, thirdGestureDetected, 0.0f);
                    }
                }
            }
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
