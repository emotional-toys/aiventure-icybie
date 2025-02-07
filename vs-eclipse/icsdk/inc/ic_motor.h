// ICSDK2 - ICybie Library
// low level motor and leg servo control
// ADVANCED - not used directly (use action playback instead)

//////////////////////////////////////////////////////
// low level servo control

#define NUM_LEGS        12 // first 12 are real 'servos'
#define NUM_MOTORS      (NUM_LEGS+4) // last 4 are fake servos

enum // byte value used for 'imotor' (0->15) or 'ileg' (0->12)
{
    // first 12 are leg servos - in original ROM order
                    // + direction limb moves with more positive value
    ICMOTOR_LF_LEG,   // + forward
    ICMOTOR_LF_HIP,   // + inward
    ICMOTOR_LF_KNEE,  // + backward

    ICMOTOR_RF_LEG,   // + backward
    ICMOTOR_RF_HIP,   // + outward
    ICMOTOR_RF_KNEE,  // + forward

    ICMOTOR_RH_LEG,   // + backward
    ICMOTOR_RH_HIP,   // + outward
    ICMOTOR_RH_KNEE,  // + forward

    ICMOTOR_LH_LEG,   // + forward
    ICMOTOR_LH_HIP,   // + inward
    ICMOTOR_LH_KNEE,  // + backward

    // next 4 are motors (but not full servos)
    ICMOTOR_NECK,      // + up
    ICMOTOR_HEADTURN,  // + Left
    ICMOTOR_MOUTH,     // + up
    ICMOTOR_TAIL       // + up

}; /*SYSROM COMPATIBLE DO NOT ALTER*/

// low level motor movement (any of the 16 motors)

void ADECL ic_set_motor_speed(byte imotor, sbyte speed, byte pulse_len);
    // PWM speed from -10 to +10
    // for legs, specify pulse_len of 0 (max timeout of 5 seconds)
    // for others, specify length in pulses (1/51.2 seconds)

//////////////////////////////////////////////////////
// raw leg movement (not scaled)

byte ADECL ic_get_leg_position(byte ileg);
        // value from ADC for each of 12 legs

// simple servo movement
void ADECL ic_move_leg(byte ileg, byte newPos);
    // newPos = raw position
void ADECL ic_stop_leg(byte ileg);

typedef enum
{
    // end states
    ICMOVE_DONE,        // move completed
    ICMOVE_JAMMED,      // move jammed

    // other are internal status
    ICMOVE_PRESTART,    // hasn't started moving yet
    ICMOVE_MOVING,      // moving
    ICMOVE_STOPPING     // slowing down to target
};
typedef byte IC_MOVE_RESULT;

IC_MOVE_RESULT ADECL ic_get_leg_status(byte ileg);

//////////////////////////////////////////////////////
// more advanced features

// check status of all legs at once
word ADECL ic_count_busy_legs();
	// return 4 nibble counts
	    // jammed:moving_fast:moving_slow:stopped

// change servo loop strategy
enum
{
    // servo strategy
    SVS_RAW3STAGE, // simple
    SVS_RAW1STAGE, // good for fast sloppy walking
#ifdef LATER
    SVS_ORIGCLONE, // patterned after system ROM (64Hz/10)
    SVS_NEWCLONE,  // similar but (51.2Hz/5)
    SVS_SIMPLEPID, // if someone writes a real PID algorithm
#endif
    SVS_MAX
};

void ADECL _ic_set_leg_strategy(byte svs); // ADVANCED

//////////////////////////////////////////////////////
