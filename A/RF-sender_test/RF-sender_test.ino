#define REPin 3
#define unitCodeA 24895810

#include <NewRemoteTransmitter.h>

NewRemoteTransmitter apa3Transmitter(unitCodeA, REPin, 260, 3);

void setup() {
  pinMode(REPin, OUTPUT);
}

void loop() {
  apa3Transmitter.sendUnit(0, true);
  delay(2000);
  apa3Transmitter.sendUnit(0, false);
  delay(2000);
}
