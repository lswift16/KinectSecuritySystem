# Kinect Security System

The Kinect Security System is a biometric system that allows users to unlock their doors with simple physical movements. By selecting from a set of pre-generated gestures the user can set a "lock" consisting of any three. To unlock the door a user simply has to preform these gestures, in their decided order, in view of the Kinect. The system connects to an arduino via xBees which control a servo representing a physical door lock.

### Equipment Required

1x Computer(Requires: Intel Chipset, USB-3)

1x Kinect 2

1x Arduino Uno

1x Servo Motor

2x xBee Wireless Connectors

### Setup

1. Connect Kinect to computer and arduino to power source/servo motor
2. Via the xBees and using XCTU, connect the computer to the arduino
3. Run the code as a .exe
4. Choose a lock for the system via the lock builder
5. Unlock the system by performing the gestures *you* set in the order *you* set them.

### What can KinectSecuritySystem do?

#### Kinect Gesture Detection


##### Build a lock

Users can select a series of gestures from a pre-built database. By selecting these gestures they build a "lock" which forces anyone who wishes to open the door connected to the lock to perform said gestures.

The system for building a lock firstly assigns saved .gbd files (gestures generated via Visual Gesture Builder) to three varaibles (although more could be added). These gestures are then available for selection via buttons on the GUI. The users upon choosing to build a lock is able to press these buttons in an order to build said lock.

##### Unlock your door

Users perform the chosen set of gestures and upon succeeding a message sent from the computer system to the arduino, activating a servo motor. Upon failure an email is sent to the owner.

The system uses the above mentioned variables to compare the current skeleton wire frames positions to that of the saved .gbd file gestures. Upon a succesful performance of the gestures in the correct order, the program sends a message via xBees to the arduino. The arduino will then activate the servo motor to unlock the door. Upon a failure (which constitutes performing the wrong gesture) a message is sent from the program to the arduino to email the owner about the attempt to unlock the door.

#### Arduino Support


##### Servo control

Servos are activated by the arduino to turn a specific amount of degrees which allow the opening of the door.

Upon receiving the applicable message the arduino turns the servo motor in order to allow the opening of the door connected to it.

![alt tag](http://i.imgur.com/BBcRBLy.jpg)

##### Email upon failure

Upon failure an email is sent by the arduino to the owner of the system.

The arduino does this by...

### References

#### [Kinect Gesture Creation] https://channel9.msdn.com/Blogs/k4wdev/Custom-Gestures-End-to-End-with-Kinect-and-Visual-Gesture-Builder

Developers, developers, developers

### Software Used

#### Visual Studio

#### Kinect Gesture Builder:

This software allows the conversion of raw video footage (recorded via the Kinect 2) into gestures. This is done by importing video footage of a gesture being performed, and then giving each frame a mark of either TRUE (gesture is being performed), or FALSE (gesture isn't being performed). This is then processed using Kinect Gesture Builder to create a .gbd file that is used in our system.




