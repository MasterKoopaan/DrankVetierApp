//Pin nummers:    0 = output  1 = input   A = analoge
#define ledPin    9    //0, connection indication

#define ultrasoneSensorenCount 2  //de aantal ultrasonice sensors
int triggerPins[3] = {2, 4, 6};   //0, trigger pins voor ultrasone sensors
int echoPins[3] = {3, 5, 7};      //1, echo pins voor ultrasone sensors

#define servoPin  6     //0, output voor servo motor
#define buzzerPin 7     //0, output voor buzzer

#define tempPin   1     //A, leest temperatuur waardes;
#define lightPin  0     //A, analoog voor lichtsensor

#define buzzerInterval 1000 // interval voor buzzer

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
unsigned long previousBlinkMillis = 0, previousBuzzerMillis = 0; //standaardwaarde voor customDelay methode
bool LedPinState = false, buzzerState = false;
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
  for (int i = 0; i < ultrasoneSensorenCount; i++) { //init echo and trigger pins
    pinMode(triggerPins[i], OUTPUT);
    pinMode(echoPins[i], INPUT);
  }
  pinMode(buzzerPin, OUTPUT);                       //init buzzer
  pinMode(ledPin, OUTPUT);                          //init ledPin

  //default states:
  digitalWrite(ledPin, LOW);

  //get IP adress:
  if (Ethernet.begin(mac) == 0) {
    //Get IP failed
    Serial.println("No DHCP. Get IP failed");
    Ethernet.begin(mac, ip);
  }
  server.begin();
  Serial.print("Listening address server: "); Serial.print(Ethernet.localIP());
  Serial.print(" on port "); Serial.println(ethPort);
}

void loop() {
  //wait for client:
  EthernetClient UserClient = server.available();
  if (!UserClient) {
    blink(ledPin, 200, LedPinState);
    return;
  }
  Serial.println("User connected"); digitalWrite(ledPin, HIGH); LedPinState = true;

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
  Serial.println("User disconnected");
  ConfigureSet = false;
  InMessage = "";
}

// Read the received message and respont
void Read(String InMessage, EthernetClient &UserClient) {
  String result = "";
  bool bigMessage = InMessage.length() > 1;
  switch (InMessage[0]) {
    case 'c':                                   //c for configure
      if (bigMessage) {
        result = setConfigure(InMessage.substring(1));      //  vb. 03.120.07.04.12 - '.'
        Serial.print("Message out: "); Serial.println(result);
        char buf[4];
        intToCharBuf(result, 3, buf);
        UserClient.println(buf);
      }
      break;
    case 'a':                                   //a for amount
      result = getCurrentAmount();
      char buf[result.length() + 1];
      intToCharBuf(result, result.length(), buf);
      UserClient.println(buf);                                    //  vb. 03.003.005.012 - '.'
      break;
  }
}

// Format the string message that will be send to the user in a char array
void intToCharBuf(String s, int len, char buf[])                 //s = messeage to send, len = the length of messege to send excluding the '\n', buf = referes to the char array where the messege will in be stored
{
  while (s.length() < len) {              // prefix redundant "0" if not long
    s = "0" + s;
  }
  s = s + '\n';                           // add newline
  s.toCharArray(buf, len + 1);            // convert string to char-buffer
}

//returns if the Configure is correctie set
String setConfigure(String ConfigureString) {  //vb. 03120070412  -> layers(3)width(120cm)span1(7cm)span2(4cm)span3(12cm), maxlayers = 99, maxwidth = 999cm, maxspan = 99cm
  Serial.print("c) Message in: "); Serial.println(ConfigureString);
  if (ConfigureString.length() >= 7) {
    layers = ConfigureString.substring(0, 2).toInt(); //sets layers
    Serial.print("layerscount: "); Serial.print(layers);
    if (layers > ultrasoneSensorenCount || layers == 0) {
      return "out";
    }
    width = ConfigureString.substring(2, 5).toInt(); //sets width
    Serial.print(" width: "); Serial.print(width);
    for (int i = 0; i < layers ; i++) {
      span[i] = ConfigureString.substring(5 + (2 * i), 7 + (2 * i)).toInt(); //sets span
      Serial.print(" "); Serial.print(span[i]);
    }
    Serial.println();
    ConfigureSet = true;
    return "suc";
  }
  else {
    return "err";
  }
}

//return the current amount in storage
String getCurrentAmount() {                  //vb. 03002004010   -> layers(3)value1(2)value2(4)value3(10), maxlayers = 99, maxvalue = 999
  Serial.print("a) ");
  String CurrentAmount = String(layers);
  if (CurrentAmount.length() < 2) CurrentAmount = "0" + CurrentAmount;
  for (int layer = 0; layer < layers; layer++) {
    CurrentAmount += ReadLayer(layer);
  }
  Serial.print("Message out: "); Serial.println(CurrentAmount);
  return CurrentAmount;
}

//return the amount of units on the layer
String ReadLayer(int layer) {                 //vb. 002 (2), maxvalue = 999
  Serial.print("Measurement layer: "); Serial.print(layer);
  int counterval = width - Highdistance(triggerPins[layer], echoPins[layer]);
  String Amount = GetAmountLayer(counterval, span[layer]);
  //make 3 char long
  if (Amount.length() < 3) {
    Amount = "0" + Amount;
  }
  if (Amount.length() < 3) {
    Amount = "0" + Amount;
  }
  Serial.print("Amount: "); Serial.println(Amount);
  return Amount;
}

String GetAmountLayer (int counterval, int span) {
  for (int i = 0; i < width / span; i++) {
    if (span * i > counterval) {
      return String(i);
    }
  }
  return "000";
}

//read the avarage of 5 ultrasonische sensor readings
float Avgdistance(int trigger, int echoer) {
  int i = 0, minimum = 9999, maximum = -2, afstand, totaal;
  int j;
  while (i < 6) {
    afstand = distance(trigger, echoer);
    if (afstand != -1) {
      minimum = min(minimum , afstand);
      maximum = max(maximum , afstand);
      totaal += afstand;
      i++;
      j = 0;
      return (int) totaal / 4.00;
    }
    else if (j < 3)
    {
      j++;
    }
    else
    {
      return -1.00;
    }
  }
}

//read the avarage of 5 ultrasonische sensor readings
float Highdistance(int trigger, int echo) {
  float high = 0;
  float dis = 0;
  for (int i = 0; i < 8; i++) {
    dis = distance(trigger, echo);
    if (dis > high) {
      high = dis;
    }
    delayMicroseconds(10);
  }
  Serial.println();
  return high;
}

//read ultrasonische sensor
float distance(int trigger, int echo) {
  digitalWrite(trigger, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigger, LOW);
  tijd = pulseIn(echo, HIGH, 12371);
  afstand = tijd * 0.034 / 2;
  Serial.print(" "); Serial.print(afstand);
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
void changeFlag(bool doRaise) { //true raises the flag and visa versa
  if (doRaise) {
    servo1.write(179);
  }
  else if (doRaise == false) {
    servo1.write(0);
  }
}

//light sensor
bool fridgeClosed(int acceptanceValue) { //acceptanceValue is the maximum accepted lightlevel to return "Closed"
  if (analogRead(lightPin) < acceptanceValue) {
    return true;
  }
  else {
    return false;
  }
}

//buzzer
void activateBuzzer(int buzzerpin) {
  unsigned long currentMillis = millis();
  if (currentMillis - previousBuzzerMillis >= buzzerInterval) {
    tone(buzzerpin, 750, 1000);
    previousBuzzerMillis = currentMillis;
  }
}

//temp sensor
int temp(int pinNo) {
  for (int i = 0 ; i <= 4; i++) {
    samples[i] = (5.0 * analogRead(pinNo) * 100.0) / 1183.0; //1023
    tempc = tempc + samples[i];
    delay(50);
  }
  {
    tempc = tempc / 5.0;
    Serial.println ("celsius"); // celcious is what this temperature represent
    Serial.println (tempc, DEC); //this is used so that the serial is show what to write in this case it is temperature
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





