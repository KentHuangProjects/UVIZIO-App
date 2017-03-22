/*
 * Usage: Send a binary array to the RFduino..
 * 
 * Array Definiton (Corresponds to the Data struct definiton below)
 * 
 * 0: mode value  -> See MODE_X options below
 * 1-2: period      -> time (in ms) between iterations. Rounded up to MIN_PERIOD if below. Currently doesn't do much since the max for a 2-digit hex (255) is < 500...
 * 3: num_pixels  -> number of pixels/frames to iterate through. Doesn't matter unless you're using MODE_FRAMES. Assumed <= MAX_FRAMES.
 * 4-6+           -> color intensity value. 4-> r (0-255 or FF), 5 -> g, 6 -> b. If num_pixels = 2, then 7-> r2, 8->g, 9->b for frame 2, etc. 
 * 
 */



#include <RFduinoBLE.h>

void setup () {
}

void loop() {
  delay(1000);
}
