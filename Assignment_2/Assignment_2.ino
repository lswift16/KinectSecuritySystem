//Group Best
#include <ZumoMotors.h>
#include <NewPing.h>
#include <Servo.h> 

#define SPEED 100             //Sets motor speed
#define STOP 0
#define REVERSE_DURATION  200 // ms
#define TURN_DURATION     100 // ms - 300 before
#define TRIGGER_PIN 2
#define ECHO_PIN 12
#define MAX_DISTANCE 200
 
Servo middle;  // creates 4 "servo objects"
ZumoMotors motors;

const int ledPin = 13; // the pin that the LED is attached to
int incomingByte;      // a variable to read incoming serial data into

void setup() {
  Serial.begin(9600);               //Initialise serial 
  middle.attach(6);  // attaches the servo on pin 11 to the middle object
  pinMode(9, OUTPUT);
  beep(50);
  beep(50);
  beep(50);
  delay(1000);
  
  //pinMode(ledPin, OUTPUT);          //Initialise the LED pin as an output
  //sensors.init();                   //Initialise sensors

}

void loop() {  
    //controlServo();
    //middle.write(90); // sets the servo position according to the value(degrees)
    //delay(300); // doesn't constantly update the servos which can fry them

    if (Serial.available() > 0) {     //Checks if serial input is available
      incomingByte = Serial.read();   //Reads input
      switch(incomingByte)            //Checks value of incomingByte
      {
        case 'U':
           middle.write(90); // sets the servo position according to the value(degrees)
           delay(50); // doesn't constantly update the servos which can fry them
           beep(200);
          break;
        case'L':
           middle.write(-90); // sets the servo position according to the value(degrees)
           delay(300); // doesn't constantly update the servos which can fry them
          break;
      }  
      incomingByte = Serial.read();
      incomingByte = 0;
    }
}

void controlServo() {                //For controlling zumo
  if (Serial.available() > 0) {     //Checks if serial input is available
    incomingByte = Serial.read();   //Reads input
    switch(incomingByte)            //Checks value of incomingByte
    {
      case 'U':
         middle.write(90); // sets the servo position according to the value(degrees)
         delay(50); // doesn't constantly update the servos which can fry them
        break;
      case'L':
         middle.write(-90); // sets the servo position according to the value(degrees)
         delay(300); // doesn't constantly update the servos which can fry them
        break;
    }  
    incomingByte = Serial.read();
    incomingByte = 0;
  }
}
void beep(unsigned char delayms){
  analogWrite(9, 20);      // Almost any value can be used except 0 and 255
                           // experiment to get the best tone
  delay(delayms);          // wait for a delayms ms
  analogWrite(9, 0);       // 0 turns it off
  delay(delayms);          // wait for a delayms ms   
} 





