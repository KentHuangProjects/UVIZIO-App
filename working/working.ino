// WS2812_RFduino_Test
// By Thomas Olson
// teo20140220.01
// teo20140719.01 Modified for Arduino 1.5.7
// 20141022.. verified works with Arduino 1.5.8
// No complicated Pixel Library needed.
// Tested with WS2812B 4 pin versions.

#include <RFduinoBLE.h> //RFduino support

#define PIN_GREEN 0
#define PIN_RED   1
#define PIN_BLUE  2

#define MODE_OFF    0
#define MODE_STATIC 1   // Display a static color.
#define MODE_BLINK  2   // Blink a color on/off at the specified period (ms)
#define MODE_FADE   3   // Like blink, but fade instead. Not yet working. MODE_FRAMES can handle this by defining fade "between" frames.
#define MODE_FRAMES 4   // Ability to define frames of pixels to loop between. 
                        // Loops the whole pattern by the period, and frames are equally divided between the period's time.

#define MODE_RAINBOW 5
#define MODE_COLOR_STROBE 6
#define MODE_COLOR_WALK 7

#define MIN_SPEED  10  // Min period (ms) to cycle on
#define MAX_FRAMES 10   // Max # of Frames we can handle



// RFDuino output settings
const int ws2812pin = 3;

const int nPIXELS = 60;

const int nLEDs = nPIXELS * 3;
uint8_t ledBar[nLEDs];

bool ADVERTISING = false;

// Defines
struct Pixel {
  uint8_t r;
  uint8_t g;
  uint8_t b;
  
};

struct Data {
  uint8_t mode;
  uint8_t speed;
  uint8_t brightness;
  int num_pixels;
  Pixel pixels[MAX_FRAMES];
};

const Pixel PIXEL_WHITE = {255, 255, 255};
const Pixel PIXEL_BLACK = {0, 0, 0};
const Pixel PIXEL_OFF = PIXEL_BLACK;
const Pixel PIXEL_ON  = PIXEL_WHITE;

// Defaults
//uint8_t MODE = MODE_RAINBOW;
const Data DEFAULT_DATA = {MODE_RAINBOW, MIN_SPEED, 255 };

Data DATA = {MODE_OFF}; 




void setup() {

  RFduinoBLE.deviceName = "PixelPusher"; //Sets the device name to RFduino
  RFduinoBLE.advertisementInterval = 100; 
  RFduinoBLE.advertisementData = "leds";

  RFduinoBLE.customUUID = "aba8a706-f28c-11e6-bc64-92361f002671";
  
//  RFduinoBLE.advertisementInterval = 5000; 
  RFduinoBLE.begin();

  Serial.begin(9600);

  pinMode(ws2812pin, OUTPUT);
  
  digitalWrite(ws2812pin, LOW);
  // Initialize the ledBar array - all LEDs OFF.
  for(int wsOut = 0; wsOut < nLEDs; wsOut++){
    ledBar[wsOut] = 0x00;
  }
  loadWS2812();
  
  delay(1);
}




void RFduinoBLE_onConnect() {
  Serial.println("Connected ");
  DATA = DEFAULT_DATA;
}

void RFduinoBLE_onReceive(char *data, int len) {

  Serial.println(data);
 

  // get the speed values
  DATA.mode = data[0];
  //DATA.period = (data[1] * 256) + data[2];
  //DATA.period = (DATA.period < MIN_PERIOD) ? MIN_PERIOD : DATA.period;
  
  //DATA.num_pixels = (data[3] < 1 ? 1 : data[3]);

  DATA.speed = data[1];
  DATA.brightness = data[2];

  //Pixel pixels[MAX_FRAMES];

  int base = 4;
  for(int i=0; i<DATA.num_pixels; i++)
  {
    // get the RGB values

    int color_start = base + (i*3);
    
    uint8_t r = data[color_start];
    uint8_t g = data[color_start+1];
    uint8_t b = data[color_start+2];

    DATA.pixels[i] = {r,g,b};
  }
}



void loop() {


  switch(DATA.mode) {
    case MODE_OFF           : off(1000); break;
    case MODE_STATIC        : displayStatic(DATA.pixels[0], 1000); break;
    case MODE_BLINK         : displayBlink(DATA.pixels[0], ((255-DATA.speed)/2)); break;
    case MODE_RAINBOW       : rainbow(255-DATA.speed); break;
    case MODE_COLOR_STROBE  : colorStrobe(255-DATA.speed); break;
    case MODE_COLOR_WALK    : colorWalk(255-DATA.speed); break;
    default                 : off(5000); break;
  }
  // rainbow(50);
  //delay(50);

  
}

void displayStatic(Pixel p, uint8_t wait) {
  for(int wsOut = 0; wsOut < nLEDs; ++wsOut){
    if(wsOut % 3 == PIN_RED)        ledBar[wsOut] = p.r;
    else if(wsOut % 3 == PIN_GREEN) ledBar[wsOut] = p.g;
    else if(wsOut % 3 == PIN_BLUE)  ledBar[wsOut] = p.b;
  }

  if(wait > -1) {
    loadWS2812();
    delay(wait);
  }
}


void displayBlink(Pixel p, uint8_t wait) {

  displayStatic(p, wait/2);
  displayStatic(PIXEL_OFF, wait/2);
  
}


void off(uint8_t wait) {
  for(int i=0; i<nLEDs; ++i) {
    ledBar[i] = 0; 
  }
  loadWS2812();
  delay(wait);
}

void staticRainbow(uint8_t j) {

  for(uint8_t i = 0; i < nLEDs; i+=3) {
  
    int WheelPos = ((i+j)/1)%256;
  
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

  }

}

void rainbow(uint8_t wait) {

  String waitString = String(wait);
  Serial.println("rainbow("+waitString+") begin.");
  

 // RFduinoBLE.end();
  uint16_t i, j;

  for(j=0; j<256; j+=2) {  // for each color?
    if(DATA.mode != MODE_RAINBOW) {
      Serial.println("exiting early! mode has changed.");
      return;
    }
    wait = 255-DATA.speed;
    
    staticRainbow(j);      
    
    loadWS2812(); // update LEDs
    
    delay(wait);
  } // end loop for each color

  //RFduinoBLE.begin();
}


void colorStrobe(uint8_t wait) {

    uint16_t i, j;

    for(j=0; j<256; j += 8) {  // for each color?
      if(DATA.mode != MODE_COLOR_STROBE) {
        Serial.println("exiting early! mode has changed.");
        return;
      }
      wait = 255-DATA.speed;

      if( (j/25) % 2 == 0 ) {
        off(wait*2);
      } else {
        staticRainbow(j);      
        loadWS2812(); // update LEDs
        delay(wait*2);  
      }
      
    } // end loop for each color
}

void colorWalk(uint8_t wait) {

    uint16_t i, j;

    int spacer = 16;

    for(j=0; j<256; j += spacer) {  // for each color?
      if(DATA.mode != MODE_COLOR_WALK) {
        Serial.println("exiting early! mode has changed.");
        return;
      }
      wait = 255-DATA.speed;

      if( (j/spacer) % 2 == 0 ) {
        // continue;
        //off(wait*2);
      } else {
        
          int WheelPos = j;
          
          if(WheelPos < 85) {
            displayStatic({WheelPos * 3, 255 - WheelPos * 3, 0}, -1);
          } else if(WheelPos < 170) {
            WheelPos -= 85;
            displayStatic({255 - WheelPos * 3, 0, WheelPos * 3}, -1);
          } else {
           WheelPos -= 170;
            displayStatic({0, WheelPos * 3, 255 - WheelPos * 3}, -1);
          }
      
        

        
      }
      loadWS2812(); // update LEDs
      delay(wait*(spacer/2));  
      
    } // end loop for each color
}


void loadWS2812(){

  //Serial.println("loadWS2812() begin.");


  //RFduinoBLE.end();
  while(!RFduinoBLE.radioActive);
  while(RFduinoBLE.radioActive);
  
  //noInterrupts();
  //RFduino_ULPDelay(INFINITE);

  for(int wsOut = 0; wsOut < nLEDs; wsOut++){
    for(int x=7; x>=0; x--){
      if(RFduinoBLE_radioActive) {
     //   delay(10);
      //  Serial.println("skipping LED "+String(wsOut)+" for advertisement");
      //  x = 0;
       // continue;
      }
     // Serial.println("display LED"+String(wsOut));
      
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
  //RFduinoBLE.begin();
  //interrupts();
  //delayMicroseconds(2); // latch and reset WS2812.
  //interrupts();  
  //RFduinoBLE.begin();
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
