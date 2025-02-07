// ICSDK2 - ICybie Library
// calibrarted legs and motion playback
// ADVANCED - not used directly (use action playback instead)

//////////////////////////////////////////////////////
// Leg calibration

// for now use the E2PROM values from last calibration

#ifdef LATER
bool ADECL ic_calibrate_legs();
#endif

//////////////////////////////////////////////////////
// Abstract coordinates
	// motPos is used in motion records
	// position is 0->63

byte ADECL ic_scale_motion_leg(byte ileg, byte motPos);

//////////////////////////////////////////////////////
// Motion playback (SYSROM compatible structure)

typedef byte RAW_MOTION_FRAME[19];
	 /*SYSROM COMPATIBLE DO NOT ALTER - 19 bytes */

typedef struct
{
    byte frameCount;
    byte frames;    // RAW_MOTION_FRAME frames[/*frameCount*/];
} RAW_MOTION_INFO; /*SYSROM COMPATIBLE DO NOT ALTER*/

//////////////////////////////////////////////////////
// play a motion part - can be dangerous

// low level
bool ADECL ic_perform_motion_raw(RAW_MOTION_INFO const* motionPtr);

// by index (requires SYSROM)
bool ADECL ic_perform_motion(word motionIndex);
    // 1->230, 1000->1137 are standard (somewhat sysrom dependent)
    // 1138 and up are very sysrom dependent
    // 500->999 are user definable

//////////////////////////////////////////////////////
// Posture control

enum 
{
    POSTURE_STAND = 0,
    POSTURE_SIT = 1,
    POSTURE_PLAYDEAD = 2,
    POSTURE_HEAD = 3,
    POSTURE_RECOVER1 = 4,
    POSTURE_RECOVER2 = 5,
    POSTURE_LIE = 6,
    NUM_POSTURE = 7
}; /*SYSROM COMPATIBLE DO NOT ALTER*/

byte ADECL ic_get_current_posture(byte posture);
bool ADECL ic_move_to_posture(byte posture);

//////////////////////////////////////////////////////
