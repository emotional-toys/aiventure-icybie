// ICSDK2 - ICybie Library
// Sensors

////////// ////////// ////////// ////////// //////////
// Common Sensors - bit inputs

word ADECL ic_get_sensors();
    // returns value you can '&' with a SENSOR_ value

// button presses (if currently pressed)
#define SENSOR_HEAD_PRESS	        0x008
#define SENSOR_BACK_PRESS           0x100

// others
#define SENSOR_NOSE	                0x002 // usually on when nothing in sight
#define SENSOR_BATTERY_DOOR_CLOSED	0x010 // set when door closed
#define SENSOR_MYSTERY              0x080 // ??? 'Mystery' sensor

// tilt indicators
#define SENSOR_TILT_BACK            0x001 // on his back
#define SENSOR_TILT_BUTT	        0x004 // on his butt
#define SENSOR_TILT_RIGHT	        0x020 // on right side
#define SENSOR_TILT_LEFT	        0x040 // on left side
#define SENSOR_TILT_FACE            0x200 // on his face

#define SENSOR_TILT_MASK            0x265 /* any of the tilt bits */

////////// ////////// ////////// ////////// //////////
// Analog sensor values

byte ADECL ic_get_battery_level();
    // result of a 30 sample average
    // in range 0->255

byte ADECL ic_get_light_level();
    // in range 0->255


////////// ////////// ////////// ////////// //////////
