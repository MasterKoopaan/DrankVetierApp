// Arduino Domotica server with Klik-Aan-Klik-Uit-controller
//
// By Sibbele Oosterhaven, Computer Science NHL, Leeuwarden
// V1.2, 16/12/2016, published on BB. Works with Xamarin (App: Domotica)
//
// Hardware: Arduino Uno, Ethernet shield W5100; RF transmitter on RFpin; debug LED for serverconnection on ledPin
// The Ethernet shield uses pin 10, 11, 12 and 13
// Use Ethernet2.h libary with the (new) Ethernet board, model 2
// IP address of server is based on DHCP. No fallback to static IP; use a wireless router
// Arduino server and smartphone should be in the same network segment (192.168.1.x)
//
// Supported kaku-devices
// https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model left)
// kaku Action device, old model (with dipswitches); system code = 31, device = 'A'
// system code = 31, device = 'A' true/false
// system code = 31, device = 'B' true/false
//
// // https://eeo.tweakblogs.net/blog/11058/action-klik-aan-klik-uit-modulen (model right)
// Based on https://github.com/evothings/evothings-examples/blob/master/resources/arduino/arduinoethernet/arduinoethernet.ino.
// kaku, Action, new model, codes based on Arduino -> Voorbeelden -> RCsw-2-> ReceiveDemo_Simple
//   on      off
// 1 2210415 2210414   replace with your own codes
// 2 2210413 2210412
// 3 2210411 2210410
// 4 2210407 2210406
//
// https://github.com/hjgode/homewatch/blob/master/arduino/libraries/NewRemoteSwitch/README.TXT
// kaku, Gamma, APA3, codes based on Arduino -> Voorbeelden -> NewRemoteSwitch -> ShowReceivedCode
// 1 Addr 21177114 unit 0 on/off, period: 270us   replace with your own code
// 2 Addr 21177114 unit 1 on/off, period: 270us
// 3 Addr 21177114 unit 2 on/off, period: 270us

// Include files.
#include <Servo.h>
#include <SPI.h>                  // Ethernet shield uses SPI-interface
#include <Ethernet.h>             // Ethernet library (use Ethernet2.h for new ethernet shield v2)

// Set Ethernet Shield MAC address  (check yours)
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
int ethPort = 3300;                                  // Take a free port (check your router)

#define ledPin       8  // output, led used for "connect state": blinking = searching; continuously = connected
#define infoPin      9  // output, more information
#define echoPin      6
#define trigPin      5
#define servoPin     4

EthernetServer server(ethPort);              // EthernetServer instance (listening on port <ethPort>).

char actionDevice = 'A';                 // Variable to store Action Device id ('A', 'B', 'C')
bool toggled = false;
float threshold;
int result;
int minD, maxD;
byte counter, counter2;
unsigned long previousmillis = 0;
const long interval = 10000;


Servo servo1;

void setup()
{
  Serial.begin(9600);

  Serial.println("Domotica project, Arduino Domotica Server\n");

  //init servo and ultrasonic sensor pins
  DDRB = B111111;
  pinMode(trigPin, OUTPUT);
  digitalWrite(trigPin, LOW);
  pinMode(echoPin, INPUT);

  servo1.attach(servoPin);
  servo1.write(179);

  //Init I/O-pins
  pinMode(ledPin, OUTPUT);
  pinMode(infoPin, OUTPUT);

  //Default states
  digitalWrite(ledPin, LOW);
  digitalWrite(infoPin, LOW);

  while(counter < 5){
    result = Distance(trigPin, echoPin);
    if(result != -1){
      threshold += result;
      Serial.println(threshold);
      counter++;
    }
  }
  threshold /= 5.00;
  counter = 0;
  minD = round(threshold) - 5;
  maxD = round(threshold) + 5;

  //Try to get an IP address from the DHCP server.
  if (Ethernet.begin(mac) == 0)
  {
    Serial.println("Could not obtain IP-address from DHCP -> do nothing");
    while (true) {    // no point in carrying on, so do nothing forevermore; check your router
    }
  }

  Serial.print("LED (for connect-state and pin-state) on pin "); Serial.println(ledPin);
  Serial.println("Ethernetboard connected (pins 10, 11, 12, 13 and SPI)");
  Serial.println("Connect to DHCP source in local network (blinking led -> waiting for connection)");

  //Start the ethernet server.
  server.begin();

  // Print IP-address and led indication of server state
  Serial.print("Listening address: ");
  Serial.print(Ethernet.localIP());

  // for hardware debug: LED indication of server state: blinking = waiting for connection
  int IPnr = getIPComputerNumber(Ethernet.localIP());   // Get computernumber in local network 192.168.1.3 -> 3)
  Serial.print(" ["); Serial.print(IPnr); Serial.print("] ");
  Serial.print("  [Testcase: telnet "); Serial.print(Ethernet.localIP()); Serial.print(" "); Serial.print(ethPort); Serial.println("]");
  signalNumber(ledPin, IPnr);
}

void loop()
{
  // Listen for incomming connection (app)
  EthernetClient ethernetClient = server.available();
  if (!ethernetClient) {
    blink(ledPin);
    return; // wait for connection and blink LED
  }

  Serial.println("Application connected");
  digitalWrite(ledPin, LOW);

  // Do what needs to be done while the socket is connected.
  while (ethernetClient.connected())
  {
    if (!toggled) {
      result = DistanceAv(trigPin, echoPin);
      //Serial.print("Distance: ");
      //Serial.print(round(result));
      //Serial.println("cm.");
      //Serial.print("Servo:");
      //Serial.println(servo1.read());
      Run(result, minD, maxD);
      delay(100);
    }

    // Execute when byte is received.
    while (ethernetClient.available())
    {
      char inByte = ethernetClient.read();   // Get byte from the client.
      executeCommand(inByte);                // Wait for command to execute
      inByte = NULL;                         // Reset the read byte.
    }
  }
  Serial.println("Application disonnected");
}

// Implementation of (simple) protocol between app and Arduino
// Request (from app) is single char ('a', 's', 't', 'i' etc.)
// Response (to app) is 4 chars  (not all commands demand a response)
void executeCommand(char cmd)
{
  char buf[4] = {'\0', '\0', '\0', '\0'};
  char buf2[4] = {'\0', '\0', '\0', '\0'};

  // Command protocol
  Serial.print("["); Serial.print(cmd); Serial.print("] -> ");
  switch (cmd) {
    case 't': // toggle gate position
      //unsigned long currentMillis = millis();

      //if (currentMillis - previousMillis >= interval) {
      // save the last time you blinked the LED
      //previousMillis = currentMillis;
      servo1.write(0);
      delay(10000);
      servo1.write(179);

    case 'u':
      intToCharBuf(round(result), buf, 4);
      server.write(buf, 4);
      break;

    case 's':
      intToCharBuf(servo1.read(), buf, 4);
      server.write(buf, 4);
      break;
    default:
      digitalWrite(infoPin, LOW);
  }
}

// Visual feedback on pin, based on IP number, used for debug only
// Blink ledpin for a short burst, then blink N times, where N is (related to) IP-number
void signalNumber(int pin, int n)
{
  int i;
  for (i = 0; i < 30; i++)
  {
    digitalWrite(pin, HIGH);
    delay(20);
    digitalWrite(pin, LOW);
    delay(20);
  }
  delay(1000);
  for (i = 0; i < n; i++)
  {
    digitalWrite(pin, HIGH);
    delay(300);
    digitalWrite(pin, LOW);
    delay(300);
  }
  delay(1000);
}

// Convert IPAddress tot String (e.g. "192.168.1.105")
String IPAddressToString(IPAddress address)
{
  return String(address[0]) + "." +
         String(address[1]) + "." +
         String(address[2]) + "." +
         String(address[3]);
}

// Returns B-class network-id: 192.168.1.3 -> 1)
int getIPClassB(IPAddress address)
{
  return address[2];
}

// Returns computernumber in local network: 192.168.1.3 -> 3)
int getIPComputerNumber(IPAddress address)
{
  return address[3];
}

// Returns computernumber in local network: 192.168.1.105 -> 5)
int getIPComputerNumberOffset(IPAddress address, int offset)
{
  return getIPComputerNumber(address) - offset;
}

// Convert int <val> char buffer with length <len>
void intToCharBuf(int val, char buf[], int len)
{
  String s;
  s = String(val);                        // convert tot string
  if (s.length() == 1) s = "0" + s;       // prefix redundant "0"
  if (s.length() == 2) s = "0" + s;
  s = s + "\n";                           // add newline
  s.toCharArray(buf, len);                // convert string to char-buffer
}


int Distance(int trigger, int echoer) {
  digitalWrite(trigger, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigger, LOW);
  int distance = (int)(pulseIn(echoer, HIGH, 6200) / 2.0) * 0.03435;
  if (distance != 0) {
    return distance;
  }
  else {
    return -1;
  }
}

int DistanceAv(int trigger, int echoer){
  int i = 0, minimum = 999999, maximum = -999999, afstand, totaal;
  int j;
  while(i < 6){
     afstand = Distance(trigger, echoer);
     if(afstand != -1){
           minimum = min(minimum , afstand);
           maximum = max(maximum , afstand);
           totaal += afstand;
           i++;
           j = 0;
           return (int) totaal / 4.0;
     }
     else if(j < 3)
     {
      j++;
     }
     else
     {
      return -1;
     }
  }
}


void Run(int distance, int MinAcceptance, int MaxAcceptance) {
  //Serial.print(MinAcceptance);
  //Serial.print("<");
  //Serial.print(distance);
  //Serial.print("&&");
  //Serial.print(distance);
  //Serial.print("<");
  //Serial.println(MaxAcceptance);
  if (MinAcceptance < distance && distance < MaxAcceptance) {
    if (counter == 4) {
      servo1.write(0);
      Serial.println("dicht");
      counter = 0;
      counter2 = 0;
    }
    else{
      Serial.println(counter);
      counter++;
      counter2 = 0;
    }
  }
  else {
    if (counter2 == 4) {
      servo1.write(179);
      Serial.println("open");
      counter = 0;
      counter2 = 0;
    }
    else{
      Serial.println(counter2);
      counter = 0;
      counter2++;
    }
  }
}

void blink(int pin) {
  digitalWrite(pin, HIGH);
  delay(500);
  digitalWrite(pin, LOW);
  delay(500);
}

