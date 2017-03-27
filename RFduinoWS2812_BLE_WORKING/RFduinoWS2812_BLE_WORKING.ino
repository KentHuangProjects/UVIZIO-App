// WS2812_RFduino_Test
// By Thomas Olson
// teo20140220.01
// teo20140719.01 Modified for Arduino 1.5.7
// 20141022.. verified works with Arduino 1.5.8
// No complicated Pixel Library needed.
// Tested with WS2812B 4 pin versions.

#include <RFduinoBLE.h> //RFduino support

const int ws2812pin = 3;

const int nPIXELS = 60;

const int nLEDs = nPIXELS * 3;
uint8_t ledBar[nLEDs];


void setup() {

  RFduinoBLE.deviceName = "PixelPusher"; //Sets the device name to RFduino
  RFduinoBLE.advertisementInterval = 5000; 
  RFduinoBLE.begin();

  pinMode(ws2812pin, OUTPUT);
  digitalWrite(ws2812pin, LOW);
  // Initialize the ledBar array - all LEDs OFF.
  for(int wsOut = 0; wsOut < nLEDs; wsOut++){
    ledBar[wsOut] = 0x00;
  }
  loadWS2812();
  
  delay(1);
}

void loop() {
    
// for(int wsOut = 0; wsOut < nLEDs; wsOut+=3){ // green
//    ledBar[wsOut] = 0x3c;
//    loadWS2812();
//    
//    delay(10);
//    
//    ledBar[wsOut] = 0x00;
//    loadWS2812();
//  }
//
// for(int wsOut = 1; wsOut < nLEDs; wsOut+=3){ // red
//    ledBar[wsOut] = 0x3c;
//    loadWS2812();
//    
//    delay(10);
//    
//    ledBar[wsOut] = 0x00;
//    loadWS2812();
//  }
//  
//   for(int wsOut = 2; wsOut < nLEDs; wsOut+=3){ // blue
//    ledBar[wsOut] = 0x3c;
//    loadWS2812();
//    
//    delay(10);
//    
//    ledBar[wsOut] = 0x00;
//    loadWS2812();
//  }

  rainbow(50);


}

void rainbow(uint8_t wait) {

  uint16_t i, j;

  for(j=0; j<256; j++) {
    while (RFduinoBLE.radioActive)
;
      //RFduinoBLE.end();

    for(i=0; i<nLEDs; i+=3) {
//      (RFduinoBLE.radioActive);
 //   uint32_t aColor = Wheel((i+j) & 255);

  int WheelPos = ((i+j)/1)%255;

  if(WheelPos < 85) {
    ledBar[i] = WheelPos * 3;
    ledBar[i+1] = 255 - WheelPos * 3;
    ledBar[i+2] = 0;
  } else if(WheelPos < 170) {
    WheelPos -= 85;
    ledBar[i] = 255 - WheelPos * 3;
    ledBar[i+1] = 0;
    ledBar[i+2] = WheelPos * 3;
  } else {
   WheelPos -= 170;
    ledBar[i] = 0;
    ledBar[i+1] = WheelPos * 3;
    ledBar[i+2] = 255 - WheelPos * 3;
  }

    
//    ledBar[i] = 0x3c;
//    ledBar[i+1] = 0x3c;
//    ledBar[i+2] = 0x3c;
    
      //strip.setPixelColor(i, Wheel((i+j) & 255));
    }

//    while(!RFduinoBLE.radioActive);
//    while(RFduinoBLE.radioActive);
    loadWS2812();
    delay(wait);
     // RFduinoBLE.begin();
  }
}

void loadWS2812(){


  noInterrupts();

  for(int wsOut = 0; wsOut < nLEDs; wsOut++){
    for(int x=7; x>=0; x--){
      NRF_GPIO->OUTSET = (1UL << ws2812pin);
      if(ledBar[wsOut] & (0x01 << x)) {
        __ASM ( \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              );
        NRF_GPIO->OUTCLR = (1UL << ws2812pin);
      
      }else{
        NRF_GPIO->OUTCLR = (1UL << ws2812pin);
        __ASM ( \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              " NOP\n\t" \
              );      
      }
    }
  }
  delayMicroseconds(2); // latch and reset WS2812.
  interrupts();  
}

// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Wheel(byte WheelPos) {
  if(WheelPos < 85) {
   return color(WheelPos * 3, 255 - WheelPos * 3, 0);
  } else if(WheelPos < 170) {
   WheelPos -= 85;
   return color(255 - WheelPos * 3, 0, WheelPos * 3);
  } else {
   WheelPos -= 170;
   return color(0, WheelPos * 3, 255 - WheelPos * 3);
  }
}

uint32_t color(uint8_t r, uint8_t g, uint8_t b) {
  return ((uint32_t)r << 16) | ((uint32_t)g <<  8) | b;
}
