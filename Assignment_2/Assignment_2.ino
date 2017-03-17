//Group Best
#include <Servo.h> 
 
Servo middle, claw, left, right;    //Declare servos

int incomingByte;
int middlePosition = 90;
int leftRightPosition= 90;
int clawPosition;

void setup() {
  Serial.begin(9600);               //Initialise serial 
  middle.attach(6);                 //Attach servos to pins
  claw.attach(9);
  left.attach(11);
  right.attach(10);
}

void loop() {  
    
    if (Serial.available() > 0) {                 //Checks if serial input is available
      incomingByte = Serial.read();               //Get letter from serial
      Serial.read();                              //Get comma from serial
      int incomingPosition = Serial.parseInt();   //Get int from serial for position
      //Serial.read();                            //Get comma
      //int incomingYPosition = Serial.parseInt();//Get int for Y position
      
      switch(incomingByte)
      {
        case 'X':                                              //Sets the middle servo position
            middle.write(incomingPosition);
            //left.write(incomingYPosition);
            //right.write(incomingYPosition);
            delay(15);
          break;
        case'Y':                                                //Move the right and left servos
            left.write(incomingPosition);
            right.write(incomingPosition);
          break;
        /*case 'R':                                             //Turn right from 180 -> 0
           for(int i = middlePosition; i>=incomingPosition; i-= 1)
           {
              middle.write(i);
              delay(15);
              middlePosition = incomingPosition;
           }
          break;
        case'U':                                                //Move arm up
           for(int i = leftRightPosition; i>=incomingPosition; i-= 1)
           {
              left.write(i);
              right.write(i);
              delay(15);
              leftRightPosition = incomingPosition;
           }
          break;*/

      }
    }
}





