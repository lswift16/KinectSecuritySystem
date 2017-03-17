//------------------------------------------------------------------------------
// <copyright file="GestureResultView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.KinectSecuritySystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using Microsoft.Samples.Kinect.KinectSecuritySystem.Common;

    /// <summary>
    /// Tracks gesture results coming from the GestureDetector and displays them in the UI.
    /// Updates the SpaceView object with the latest gesture result data from the sensor.
    /// </summary>
    public sealed class GestureResultView : BindableBase
    {
        /// <summary>
        /// The current number of tries/attempts to unlock the door
        /// </summary>
        private int numberOfTries = 0;

        /// <summary>
        /// The current number of attempts left until the security alert is activated
        /// </summary>
        private int numberOfTriesLeft = 3;

        ///<summary>
        /// The current state of the door (locked/Unlocked)
        /// </summary>
        private bool doorUnlockState = false;

        /// <summary> True, if the user is doing first gesture </summary>
        private bool firstGesture = false;

        /// <summary> True, if the user is second gesture </summary>
        private bool secondGesture = false;

        /// <summary> True, if the user is doing third gesture  </summary>
        private bool thirdGesture = false;

        private bool isTakingScreenshot = false;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        /// <summary> SpaceView object in UI which has a spaceship that needs to be updated when we get new gesture results from the sensor </summary>
        private SpaceView spaceView = null;

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="firstGesture">True, if the first gesture is currently detected</param>
        /// <param name="secondGesture">True, if the second gesture is currently detected</param>
        /// <param name="thirdGesture">True, if the third gesture is currently detected</param>
        /// <param name="progress">Progress value of the 'SteerProgress' gesture</param>
        /// <param name="space">SpaceView object in UI which should be updated with latest gesture result data</param>
        public GestureResultView(bool isTracked, bool firstGesture, bool secondGesture, bool thirdGesture, float progress, SpaceView space, bool doorUnlockState,
                                 int numberOfTries, int numberOfTriesLeft, bool isTakingScreenshot)
        {
            
            this.IsTracked = isTracked;
            this.FirstGesture = firstGesture;
            this.SecondGesture = secondGesture;
            this.ThirdGesture = thirdGesture;
            this.IsTakingScreenshot = isTakingScreenshot;
            //this.SteerProgress = progress;
            this.spaceView = space;
            this.DoorUnlockState = doorUnlockState;
            this.NumberOfTries = numberOfTries;
            this.NumberOfTriesLeft = numberOfTriesLeft;
        }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                this.SetProperty(ref this.isTracked, value);
            }
        }

        /// <summary>
        /// Gets or sets the current state of the 'Door' for use when unlocking
        /// </summary>
        public bool DoorUnlockState
        {
            get
            {
                return this.doorUnlockState;
            }

            private set
            {
                this.SetProperty(ref this.doorUnlockState, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of attempts to unlock the door
        /// </summary>
        public int NumberOfTries
        {
            get
            {
                return this.numberOfTries;
            }

            private set
            {
                this.SetProperty(ref this.numberOfTries, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of tries left before activating the security alert
        /// </summary>
        public int NumberOfTriesLeft
        {
            get
            {
                return this.numberOfTriesLeft;
            }

            private set
            {
                this.SetProperty(ref this.numberOfTriesLeft, value);
            }
        }

        public bool IsTakingScreenshot
        {
            get
            {
                return this.isTakingScreenshot;
            }
            
            private set
            {
                this.SetProperty(ref this.isTakingScreenshot, value);
            }
        }
        /// <summary> 
        /// Gets a value indicating whether the user is doing the first gesture in the unlock sequence
        /// </summary>
        public bool FirstGesture
        {
            get
            {
                return this.firstGesture;
            }

            private set
            {
                this.SetProperty(ref this.firstGesture, value);
            }
        }

        /// <summary> 
        /// Gets a value indicating whether the user is doing the second gesture in the unlock sequence
        /// </summary>
        public bool SecondGesture
        {
            get
            {
                return this.secondGesture;
            }

            private set
            {
                this.SetProperty(ref this.secondGesture, value);
            }
        }

        /// <summary> 
        /// Gets a value indicating whether the user is doing the second gesture in the unlock sequence
        /// </summary>
        public bool ThirdGesture
        {
            get
            {
                return this.thirdGesture;
            }

            private set
            {
                this.SetProperty(ref this.thirdGesture, value);
            }
        }

        /// <summary>
        /// Updates gesture detection result values for display in the UI
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="left">True, if detection results indicate that the user is attempting to turn the ship left</param>
        /// <param name="right">True, if detection results indicate that the user is attempting to turn the ship right</param>
        /// <param name="straight">True, if detection results indicate that the user is attempting to keep the ship straight</param>
        /// <param name="progress">The current progress value of the 'SteerProgress' continuous gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool firstGesture, bool secondGesture, bool  thirdGesture, float progress, bool doorState,
                                        int numberOfTries, bool isTakingScreenshot)
        {
            this.IsTracked = isBodyTrackingIdValid;

            if (!this.isTracked)
            {
                this.FirstGesture = false;
                this.SecondGesture = false;
                this.ThirdGesture = false;
                this.DoorUnlockState = false;
                this.NumberOfTries = 0;
                this.IsTakingScreenshot = false;
            }
            else
            {
                this.FirstGesture = firstGesture;
                this.SecondGesture = secondGesture;
                this.ThirdGesture = thirdGesture;
                this.DoorUnlockState = doorState;
                this.NumberOfTries = numberOfTries;
                this.IsTakingScreenshot = isTakingScreenshot;
            }
        }
    }
}
