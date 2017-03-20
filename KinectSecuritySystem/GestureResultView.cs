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

        // <summary> Each movement will be true when a detecting movement for controlling the robot arm </summary>
        private bool movementUp = false;
        private bool movementDown = false;
        private bool movementLeft = false;
        private bool movementRight = false;

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="firstGesture">True, if the first gesture is currently detected</param>
        /// <param name="secondGesture">True, if the second gesture is currently detected</param>
        /// <param name="thirdGesture">True, if the third gesture is currently detected</param>
        public GestureResultView(bool isTracked, bool firstGesture, bool secondGesture, bool thirdGesture, float progress, bool doorUnlockState,
                                 int numberOfTries, int numberOfTriesLeft, bool isTakingScreenshot, bool movementUp, bool movementDown, bool movementRight, bool movementLeft)
        {
            
            this.IsTracked = isTracked;
            this.FirstGesture = firstGesture;
            this.SecondGesture = secondGesture;
            this.ThirdGesture = thirdGesture;
            this.IsTakingScreenshot = isTakingScreenshot;
            this.DoorUnlockState = doorUnlockState;
            this.NumberOfTries = numberOfTries;
            this.NumberOfTriesLeft = numberOfTriesLeft;

            this.movementDown = movementDown;
            this.movementUp = movementUp;
            this.movementRight = movementRight;
            this.movementLeft = movementLeft;
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
        /// Gets or sets the current state of the 'MovementUp' when controlling the roboot arm
        /// </summary>
        public bool MovementUp
        {
            get
            {
                return this.movementUp;
            }

            private set
            {
                this.SetProperty(ref this.movementUp, value);

            }
        }


        /// <summary>
        /// Gets or sets the current state of the 'MovementDown' when controlling the roboot arm
        /// </summary>
        public bool MovementDown
        {
            get
            {
                return this.movementDown;
            }

            private set
            {
                this.SetProperty(ref this.movementDown, value);

            }
        }

        /// <summary>
        /// Gets or sets the current state of the 'MovementLeft' when controlling the roboot arm
        /// </summary>
        public bool MovementLeft
        {
            get
            {
                return this.movementLeft;
            }

            private set
            {
                this.SetProperty(ref this.movementLeft, value);

            }
        }

        /// <summary>
        /// Gets or sets the current state of the 'Movementp' when controlling the roboot arm
        /// </summary>
        public bool MovementRight
        {
            get
            {
                return this.movementRight;
            }

            private set
            {
                this.SetProperty(ref this.movementRight, value);

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

        /// <summary>
        /// Gets or sets the current state of the 'IsTakingScreenshot' for use in the security alert
        /// </summary>
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
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool firstGesture, bool secondGesture, bool  thirdGesture, float progress, bool doorState,
                                        int numberOfTries, bool isTakingScreenshot, bool moveUp, bool moveDown, bool moveRight, bool moveLeft)
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
                this.MovementUp = false;
                this.MovementDown = false;
                this.MovementRight = false;
                this.MovementLeft = false;
            }
            else
            {
                this.FirstGesture = firstGesture;
                this.SecondGesture = secondGesture;
                this.ThirdGesture = thirdGesture;
                this.DoorUnlockState = doorState;
                this.NumberOfTries = numberOfTries;
                this.IsTakingScreenshot = isTakingScreenshot;
                this.MovementUp = moveUp;
                this.MovementDown = moveDown;
                this.MovementRight = moveRight;
                this.MovementLeft = moveLeft;
            }
        }
    }
}
