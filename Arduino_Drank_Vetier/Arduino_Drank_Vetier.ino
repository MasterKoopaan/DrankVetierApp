//Pin nummers:    0 = output  1 = input   A = analoge
#define ledPin    9    //0, connection indication

#define ultrasoneSensorenCount 2  //de aantal ultrasonice sensors
int triggerPins[3] = {2, 4, 6};   //0, trigger pins voor ultrasone sensors
int echoPins[3] = {3, 5, 7};      //1, echo pins voor ultrasone sensors

#define servoPin  6     //0, output voor servo motor
#define buzzerPin 7     //0, output voor buzzer

#define tempPin   1     //A, leest temperatuur waardes;
#define lightPin  0     //A, analoog voor lichtsensor

//objecten en libery:
#include <SPI.h>                  
#include <Ethernet.h>
#include <Servo.h>
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
int ethPort = 3300; 
IPAddress ip(192, 168, 1, 3);
EthernetServer server(ethPort);

//variabelen:

float vardistance;
int avg;                               //returnwaarde methode gemiddelde ultrasone
int hoogstewaarde = 0;                 //standaardwaarde voor methode gemiddelde ultrasone
int laagstewaarde = 999;               //standaardwaarde voor methode gemiddelde ultrasone
int totaal;                            //totaalwaarde voor methode gemiddelde ultrasone
int tempc = 0;                         //standaardwaarde voor returnwaarde uit temp functie
int samples [5];                       //array gebruikt om temperatuur te berekenen in 
int tijd;                              //var gebruikt voor distance functie
float afstand;                         //returnwaarde uit ultrasonesensor
unsigned long previousBlinkMillis = 0; //standaardwaarde voor customDelay methode
bool LedPinState = false;
String InMessage;             //incoming message
bool ConfigureSet = false;    //is there a configure
byte layers = 0;              //the amount of layers, max 99     
int width = 0;                //the width of the rack, max 999
byte span[ultrasoneSensorenCount];            //the length of space beteen a unit per layer, max 99
Servo servo1; 

void setup() {
  Serial.begin(9600); Serial.println("Domotica project: Drank rek\n");
  servo1.attach(servoPin);

  //init IO-pins:
  DDRB = 0x3F;                                      //sets all of PORTB to output
  for(int i = 0; i < ultrasoneSensorenCount; i++){  //init echo and trigger pins
     pinMode(triggerPins[i], OUTPUT);
     pinMode(echoPins[i], INPUT);
  }
  pinMode(buzzerPin, OUTPUT);                       //init buzzer
  pinMode(ledPin, OUTPUT);                          //init ledPin
  
  //default states:
  digitalWrite(ledPin, LOW);

  //get IP adress:
//  if (Ethernet.begin(mac) == 0) {
//    //Get IP failed
//    Serial.println("No DHCP. Get IP failed");
   Ethernet.begin(mac, ip);
//  }
  server.begin();
  Serial.print("Listening address server: "); Serial.print(Ethernet.localIP());
  Serial.print(" on port "); Serial.print(ethPort);
}

void loop() {
  //wait for client:
  EthernetClient UserClient = server.available();
  if (!UserClient) {
    blink(ledPin, 200, LedPinState);
    return;
  }
  Serial.println("User connected"); digitalWrite(ledPin, LOW); LedPinState = false;

  while (UserClient.connected()) {
    while (UserClient.available())
      {
         char inByte = UserClient.read();   // Get byte from the client.
         if (inByte == '\n') {
          Read(InMessage, UserClient);
          InMessage = "";
         } else {
          InMessage += inByte;
         }         
      }
  }
  ConfigureSet = false;
  InMessage = "";
}

void Read(String InMessage, EthernetClient &UserClient) {
  bool bigMessage = InMessage.length() > 1;
  switch (InMessage[0]) {
    case 'c':                                   //c for configure
      if(bigMessage){
        ConfigureSet = setConfigure(InMessage.substring(1));      //  vb. 03.120.07.04.12 - '.'  
        UserClient.println(ConfigureSet);
      }
      break;
    case 'a':                                   //a for amount
      UserClient.println(getCurrentAmount());                    // 03.003.005.012 - '.'
      break;
      
  }
}

//returns if the Configure is correctie set
bool setConfigure(String ConfigureString) {  //vb. 3120070412  -> layers(3)width(120cm)span1(7cm)span2(4cm)span3(12cm), maxlayers = 99, maxwidth = 999cm, maxspan = 99cm 
  if (ConfigureString.length() == 10){
      layers = ConfigureString.substring(0,1).toInt(); //sets layers
      if (layers > ultrasoneSensorenCount || layers == 0) {
        return false;
      }
      width = ConfigureString.substring(2,4).toInt(); //sets width
      for (int i = 0; i < layers ; i++){
        span[i] = ConfigureString.substring(5 + (2 * i), 5 + (2 * i + 1)).toInt(); //sets span
      }
      return true;
  }
  else{
    return false;
  }
}

//return the current amount in storage
String getCurrentAmount() {                  //vb. 03002004010   -> layers(3)value1(2)value2(4)value3(10), maxlayers = 99, maxvalue = 999
  String CurrentAmount = String(layers); 
  if (CurrentAmount.length() < 2) CurrentAmount = "0" + CurrentAmount;
  for (int layer = 0; layer < layers; layer++) {
    CurrentAmount += ReadLayer(layer);
  }
  return CurrentAmount;
}

//return the amount of units on the layer
String ReadLayer(int layer) {                 //vb. 002 (2), maxvalue = 999
  int counterval = width - distance(triggerPins[layer], echoPins[layer]);
  String Amount = GetAmountLayer(counterval, span[layer]);
  //make 3 char long
  if(Amount.length() < 3) {
    Amount = "0" + Amount;
  }
  if (Amount.length() < 3) {
    Amount = "0" + Amount;
  }
  Serial.println(Amount);
  return Amount;
}

String GetAmountLayer (int counterval, int span) {
  for (int i = 0; i < width/span; i++) {
    if (span * i > counterval) {
      return String(i);
    }
  }
}
//read ultrasonische sensor
float distance(int trigger, int echo){

  digitalWrite(trigger, LOW);
  delayMicroseconds(2);

  digitalWrite(trigger, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigger, LOW);
  tijd = pulseIn(echo, HIGH, 12371);
  afstand = tijd*0.034/2;

  return afstand;
}

void blink(int pn, int interval, bool &pnState)     //for waiting on client
{
  unsigned long currentMillis = millis();
  
  if (currentMillis - previousBlinkMillis >= interval) {
    digitalWrite(pn, pnState = !pnState); 
    previousBlinkMillis = currentMillis;
  }
}


//servo
void changeFlag(bool doRaise){  //true raises the flag and visa versa
  if(doRaise){
    servo1.write(179);
  }
  else if(doRaise == false){
    servo1.write(0);
  }
}

//light sensor
bool fridgeClosed(int acceptanceValue){ //acceptanceValue is the maximum accepted lightlevel to return "Closed"
  if(analogRead(lightPin) < acceptanceValue){
    return true;
  }
  else{
    return false;
  }
}

//buzzer
void activateBuzzer(int buzzerpin){
  digitalWrite(buzzerpin, HIGH);
  delay(1000);
  digitalWrite(buzzerpin, LOW);
  delay(1000);
  digitalWrite(buzzerpin, HIGH);
  delay(1000);
  digitalWrite(buzzerpin, LOW);
  delay(1000);
}

//temp sensor
int temp(int pinNo){
  for (int i= 0 ; i<=4;i++){
    samples[i] = (5.0 * analogRead(pinNo)*100.0)/1183.0;//1023
    tempc = tempc + samples[i];
    delay(50);
  }
  { 
    tempc= tempc/5.0;
    Serial.println ("celsious"); // celcious is what this temperature represent 
    Serial.println (tempc,DEC); //this is used so that the serial is show what to write in this case it is temperature
    delay(50); 
  }
}

//
//
//string calculate(int pin1, int pin2){
//
//    if(layers > 3 && layers <= 0){
//      Serial.println("Breek het partijkartel!");
//    }
//    else{
//      for(int i = 0; i >= layers; i++;){
//        span(layers) = distance(trigger, echo);
//    }

//    }
//}





