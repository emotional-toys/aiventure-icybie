// ICSDK2
// leg/servo handler for the 12 legs

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
//REVIEW: work in progress
// really simple slow-down technique - fast but sloppy

/////////////////////////////////////////////////////////
// main data structure for servo move logic (common to all strategies)

static byte __near g_strategy; // = 0 = SVS_RAW3STAGE

typedef struct 
{
    IC_MOVE_RESULT state; // current state as well as result
    byte target;    // target position
    sbyte move_direction;   // 1 of positive, 0 if negative
} MOVE_JOB;

static MOVE_JOB __near g_mjobs[NUM_LEGS];

////////////////////////////////////////////////////////
// Customizable (all strategies)

#define CLOSE_ENOUGH_STATIC     4   /* close enough - don't bother moving */
#define CLOSE_ENOUGH_DYNAMIC    5   /* close enough when moving  - just stop */

#define KICK_SPEED              10
        // REVIEW: startup kick should use better strategy

/////////////////////////////////////////////////////////


void ADECL _ic_set_leg_strategy(byte svs)
{
    byte ileg;
    ic_assert(svs < SVS_MAX);
    for (ileg = 0; ileg < NUM_LEGS; ileg++)
		ic_stop_leg(ileg); // just in case
    g_strategy = svs;
}


void ADECL ic_stop_leg(byte ileg)
{
    MOVE_JOB* jobPtr = &g_mjobs[ileg];
    ic_assert(ileg < NUM_LEGS);

    // forced stop
    jobPtr->state = ICMOVE_DONE;
	ic_set_motor_speed(ileg, 0, 0);
}

void ADECL ic_move_leg(byte ileg, byte newPos)
{
    MOVE_JOB* jobPtr = &g_mjobs[ileg];
    ic_assert(ileg < NUM_LEGS);

    jobPtr->state = ICMOVE_PRESTART; // work done in timer proc
    jobPtr->target = newPos;
    // wait for timer to kick in to do the real work
}

IC_MOVE_RESULT ADECL ic_get_leg_status(byte ileg)
{
    ic_assert(ileg < NUM_LEGS);
    return g_mjobs[ileg].state;
}

// return 4 nibble counts
    // jammed:moving_fast:moving_slow:stopped
word ADECL ic_count_busy_legs()
{
    byte ileg;
    word busy_counts = 0;
    for (ileg = 0; ileg < NUM_LEGS; ileg++)
    {
        byte state = g_mjobs[ileg].state;
        if (state == ICMOVE_DONE)
            busy_counts += 0x0001;
        else if (state == ICMOVE_JAMMED)
            busy_counts += 0x1000;
        else if (state == ICMOVE_STOPPING)
            busy_counts += 0x0010;
        else
            busy_counts += 0x0100; // PRE-MOVE or MOVING
    }
    return busy_counts;
}

/////////////////////////////////////////////////////////
// re-evaluate the speed/direction of the leg movement

static void timerproc_servo_move(); // TIMER_PROC

void ADECL ic0_init_legs()
{
	_ic_settimerprocPWM10(timerproc_servo_move);
#ifdef LATER
    // 64Hz for old-style
#endif
}

/////////////////////////////////////////////////////////
// Specific moving & stopping techniques

#include "leg_raw.c_"

/////////////////////////////////////////////////////////

// common code for all techniques
static void timerproc_servo_move()
{
    byte ileg;

    for (ileg = 0; ileg < NUM_LEGS; ileg++)
    {
		byte legCur = _ic_legcur[ileg]; // we are here now
        MOVE_JOB* jobPtr = &g_mjobs[ileg];
		int error = (int)jobPtr->target - (int)legCur;    // signed
        int abs_error = error;
        if (abs_error < 0)
            abs_error = -abs_error;

        if (jobPtr->state == ICMOVE_PRESTART)
        {
            if (abs_error <= CLOSE_ENOUGH_STATIC)
            {
                // don't bother moving it
				ic_set_motor_speed(ileg, 0, 0); // just in case
                jobPtr->state = ICMOVE_DONE;
            }
            else
            {
                // initial kick
				ic_set_motor_speed(ileg,
                       (error > 0) ? KICK_SPEED : -KICK_SPEED, 0);
                jobPtr->state = ICMOVE_MOVING;
			    jobPtr->move_direction = (error > 0);
            }
        }
        else if (jobPtr->state == ICMOVE_MOVING || jobPtr->state == ICMOVE_STOPPING)
        {
            // 51.2 times per second

            if (abs_error <= CLOSE_ENOUGH_DYNAMIC)
            {
				ic_set_motor_speed(ileg, 0, 0);
                jobPtr->state = ICMOVE_DONE;
            }
            else if ((error > 0) != jobPtr->move_direction)
		    {
		        // flipped direction (overshoot)
				ic_set_motor_speed(ileg, 0, 0);
		        jobPtr->state = ICMOVE_DONE;
            }
            else
            {
                // still moving in original direction to target
                if (g_strategy == SVS_RAW3STAGE)
                    AdjustSpeedRaw3Stage(ileg, jobPtr, legCur, abs_error);
                else if (g_strategy == SVS_RAW1STAGE)
                    AdjustSpeedRaw1Stage(ileg, jobPtr, legCur);
                else
                    ic_assert(false);
            }
        }
        else
        {
            // idle
            ic_assert(jobPtr->state == ICMOVE_DONE ||
		            jobPtr->state == ICMOVE_JAMMED);
        }
    }
}

/////////////////////////////////////////////////////////
