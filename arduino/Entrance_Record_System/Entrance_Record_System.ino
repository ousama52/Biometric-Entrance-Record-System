#include <Adafruit_Fingerprint.h>
#if (defined(__AVR__) || defined(ESP8266)) && !defined(__AVR_ATmega2560__)
SoftwareSerial mySerial(2, 3);
#else
#define mySerial Serial1
#endif
Adafruit_Fingerprint finger = Adafruit_Fingerprint(&mySerial);
const int ledPin1 = 8;
const int ledPin2 = 9;
const int buzzerPin = 11;
void setup()
{
  pinMode(ledPin1, OUTPUT);
  pinMode(ledPin2, OUTPUT);
  pinMode(buzzerPin, OUTPUT);
  Serial.begin(9600);
  while (!Serial);
  delay(100);
  finger.begin(57600);
  delay(5);
  if (finger.verifyPassword()) {
  } else {
    while (1) { delay(1); }
  }
  finger.getParameters();
  finger.getTemplateCount();
  if (finger.templateCount == 0) {
  }
  else {
  }
}
void loop()
{
  getFingerprintID();
  delay(50);
}
uint8_t getFingerprintID() {
  uint8_t p = finger.getImage();
  switch (p) {
    case FINGERPRINT_OK:
      break;
    case FINGERPRINT_NOFINGER:
      return p;
    case FINGERPRINT_PACKETRECIEVEERR:
      return p;
    case FINGERPRINT_IMAGEFAIL:
    digitalWrite(ledPin1, HIGH);
      digitalWrite(ledPin2, LOW);
      tone(buzzerPin, 1000, 200);
  delay(1000);
    digitalWrite(ledPin1, LOW);
      return p;
    default:
      return p;
  }
  p = finger.image2Tz();
  switch (p) {
    case FINGERPRINT_OK:
      break;
    case FINGERPRINT_IMAGEMESS:
     digitalWrite(ledPin1, HIGH);
      digitalWrite(ledPin2, LOW);
      tone(buzzerPin, 1000, 200);
  delay(1000);
    digitalWrite(ledPin1, LOW);
      return p;
    case FINGERPRINT_PACKETRECIEVEERR:
      return p;
    case FINGERPRINT_FEATUREFAIL:
    digitalWrite(ledPin1, HIGH);
      digitalWrite(ledPin2, LOW);
      tone(buzzerPin, 1000, 200);
  delay(1000);
    digitalWrite(ledPin1, LOW);
      return p;
    case FINGERPRINT_INVALIDIMAGE:
    digitalWrite(ledPin1, HIGH);
      digitalWrite(ledPin2, LOW);
      tone(buzzerPin, 1000, 200);
  delay(1000);
    digitalWrite(ledPin1, LOW);
      return p;
    default:
      return p;
  }
  p = finger.fingerSearch();
  if (p == FINGERPRINT_OK) {
  } else if (p == FINGERPRINT_PACKETRECIEVEERR) {
    return p;
  } else if (p == FINGERPRINT_NOTFOUND) {
    digitalWrite(ledPin1, HIGH);
      digitalWrite(ledPin2, LOW);
      tone(buzzerPin, 1000, 200);
  delay(1000);
    digitalWrite(ledPin1, LOW);
    return p;
  } else {
    return p;
  }
  Serial.print("ID#"); Serial.print(finger.fingerID);
digitalWrite(ledPin1, LOW);
  digitalWrite(ledPin2, HIGH);
  delay(1000);
  digitalWrite(ledPin2, LOW);
  return finger.fingerID;
}
int getFingerprintIDez() {
  uint8_t p = finger.getImage();
  if (p != FINGERPRINT_OK)  return -1;
  p = finger.image2Tz();
  if (p != FINGERPRINT_OK)  return -1;
  p = finger.fingerFastSearch();
  if (p != FINGERPRINT_OK)  return -1;
  Serial.print("ID #"); Serial.print(finger.fingerID);
  return finger.fingerID;
}