//Pin nummers:    0 = output  1 = input   A = analoge
#define ledPin    9   //0, connection indication 

//objecten en libery:
#include <SPI.h>                  
#include <Ethernet.h>
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
int ethPort = 3300; 
IPAddress ip(192, 168, 1, 3);
EthernetServer server(ethPort);

//variabelen:
String InMessage;             //incoming message
bool ConfigureSet = false;    //is there a configure
byte Layers = 0;              //the amount of layers, max 99     
int width = 0;                //the width of the rack, max 999
byte span[0];            //the length of space beteen a unit per layer 


void setup() {
  Serial.begin(9600); Serial.println("Domotica project: Drank rek\n");

  //init IO-pins:
  pinMode(ledPin, OUTPUT);
  
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
  Serial.print(" on port "); Serial.print(ethPort);
}

void loop() {
  //wait for client:
  EthernetClient UserClient = server.available();
  if (!UserClient) {
    blink(ledPin);
    return;
  }
  Serial.println("User connected"); digitalWrite(ledPin, LOW);

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
      
      ConfigureSet = setConfigure(InMessage.substring(1), bigMessage);      //  vb. 3.120.07.04.12 - '.'  
      UserClient.println(ConfigureSet);
      break;
    case 'a':                                   //a for amount
      UserClient.println(getCurrentAmount());                    // 03.003.005.012 - '.'
      break;
      
  }
}

//returns if the Configure is correctie set
bool setConfigure(String ConfigureString, bool bigmessage) {  //vb. 3120070412  -> layers(3)width(120cm)span1(7cm)span2(4)span3(12cm), maxlayers = 99, maxwidth = 999cm, maxspan = 99cm 
  if (bigmessage) {
    return true;
  } else {
    return false;
  } 
}

//return the current amount in storage
String getCurrentAmount() {                  //vb. 03002004010   -> layers(3)value1(2)value2(4)value3(10), maxlayers = 99, maxvalue = 999
  String CurrentAmount = String(Layers); 
  if (CurrentAmount.length() < 2) CurrentAmount = "0" + CurrentAmount;
  for (int layer = 0; layer < Layers; layer++) {
    CurrentAmount += ReadLayer(layer);
  }
  return CurrentAmount;
}

//return the amount of units on the layer
String ReadLayer(int layer) {                 //vb. 002 (2), maxvalue = 999
  String Amount = "000";
  return Amount;
}

//read ultrasoniche sensor

void blink(int pn)     //for waiting on client
{
  digitalWrite(pn, HIGH); 
  delay(100); 
  digitalWrite(pn, LOW); 
  delay(100);
}
