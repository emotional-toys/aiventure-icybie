// ICSDK2
// low level PWM motor handler for 16 motors

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Each of the 16 motors (12 leg servos, and 4 non-servos) can have
//  a PWM speed of -10 to +10 (0 = off)
// The timerproc sends a pulse-width modulated (PWM) signal to the
//  12 motors depending on the motor speed (g_motorspeeds)
// There is an additional timeout (g_pulselengths) that can be
//  used for guestimating movement for the non-servos, but more
//  importantly is a timeout to prevent the motors from burning out
//  during software development.

/////////////////////////////////////////////////////////
// global variables (for use in this module only)

static sbyte __near g_motorspeeds[NUM_MOTORS]; // 0 => stopped, -10 or +10 max
static byte __near g_pulselengths[NUM_MOTORS]; // 1/51.2 sec

/////////////////////////////////////////////////////////
// PUBLIC functions

void ADECL ic_set_motor_speed(byte imotor, sbyte speed, byte pulse_len)
{
    ic_assert(imotor < NUM_MOTORS);
	g_motorspeeds[imotor] = speed;
    g_pulselengths[imotor] = pulse_len; // 0=>256 = 5 seconds
}

// adjust already moving motor
bool ADECL _ic_adjust_motor_speed(byte imotor, sbyte speedNew)
{
	if (g_motorspeeds[imotor] == 0)
        return false;   // timed out

	g_motorspeeds[imotor] = speedNew;
    // leave pulselengths alone
    return true;
}

/////////////////////////////////////////////////////////
//INITIALIZATION

static void timerproc_motor_pwm(); // TIMER_PROC

void ADECL ic0_init_motors()
{
	_ic_settimerproc512(timerproc_motor_pwm);
        // our routine is called 512 times per second
}

// hookable - set by standard initialization
static TIMER_PROC __near g_timerProcPWM10; // only one of them for leg movement

void ADECL _ic_settimerprocPWM10(TIMER_PROC pfn)
{
    ic_assert(g_timerProcPWM10 == NULL);
    g_timerProcPWM10 = pfn;
}

/////////////////////////////////////////////////////////
// IMPLEMENTATION
/////////////////////////////////////////////////////////

typedef struct
{
    byte* pPortFwd; // could be just a byte
    byte opFwd;
    byte* pPortBwd;
    byte opBwd;
    // both on for stop
} MOTORINFO;

    // forward for leg and knee towards front of dog
    // forward for hip, toward body of dog
    // forward for head/mouth/tail = up
    // forward for head turn = left

static const MOTORINFO g_motorinfo[NUM_MOTORS] =
{
    // LF (leg, hip, knee)
    // polarity aligned to move in same direction as pot/ADC values
    { &MIRROR_PA, 0x01, &MIRROR_PA, 0x02 },
    { &MIRROR_PA, 0x04, &MIRROR_PA, 0x08 },
    { &MIRROR_PA, 0x20, &MIRROR_PA, 0x10 },
    // RF
    { &MIRROR_PA, 0x80, &MIRROR_PA, 0x40 },
    { &MIRROR_PB, 0x08, &MIRROR_PB, 0x01 },
    { &MIRROR_PB, 0x10, &MIRROR_PB, 0x20 },
    // RH
    { &MIRROR_PB, 0x40, &MIRROR_PC, 0x08 },
    { &MIRROR_PC, 0x20, &MIRROR_PD, 0x01 },
    { &MIRROR_PD, 0x04, &MIRROR_PD, 0x02 },
    // LH
    { &MIRROR_PD, 0x08, &MIRROR_PD, 0x10 },
    { &MIRROR_P6, 0x08, &MIRROR_P6, 0x10 },
    { &MIRROR_P6, 0x40, &MIRROR_P6, 0x20 },

    // neck, head R/L, mouth, tail (no pots)
    { &MIRROR_P6, 0x80, &MIRROR_P7, 0x02 },
    { &MIRROR_P7, 0x04, &MIRROR_P7, 0x08 },
    { &MIRROR_P7, 0x20, &MIRROR_P7, 0x10 },
    { &MIRROR_P7, 0x80, &MIRROR_P7, 0x40 },
};

/////////////////////////////////////////////////////////
//TIMER_PROC

static void timerproc_motor_pwm()
{
	static byte g_pwmCount; // Pulse Width Modulation count
    register byte imotor;

    if (++g_pwmCount >= 10)
    {
        // 51.2 times per second
        g_pwmCount = 0;
        if (g_timerProcPWM10 != NULL)
	        (*g_timerProcPWM10)();

        // any pulse over
	    for (imotor = 0; imotor < NUM_MOTORS; imotor++)
        {
	        if (g_motorspeeds[imotor] != 0)
            {
			    if (--g_pulselengths[imotor] == 0)
                {
#if 1
                    // servo mechanism and jam detect will stop this earlier
                    if (imotor < NUM_LEGS)
                        printf("LEG SERVO %d PULSE TIMEOUT\n", imotor);
#endif
			        g_motorspeeds[imotor] = 0; // stop
                }
            }
        }
    }

    for (imotor = 0; imotor < NUM_MOTORS; imotor++)
    {
        sbyte speed = g_motorspeeds[imotor];
        MOTORINFO const* pInfo = &g_motorinfo[imotor];

        if (speed > 0 && g_pwmCount < speed)
        {
            // forward
            *(pInfo->pPortFwd) |= pInfo->opFwd;
            *(pInfo->pPortBwd) &= ~pInfo->opBwd;
        }
        else if (speed < 0 && g_pwmCount < -speed)
        {
            // backward
            *(pInfo->pPortFwd) &= ~pInfo->opFwd;
            *(pInfo->pPortBwd) |= pInfo->opBwd;
        }
        else
        {
            // stop both
            *(pInfo->pPortFwd) |= pInfo->opFwd;
            *(pInfo->pPortBwd) |= pInfo->opBwd;
        }
    }

    // output new values
    SFR_PA = MIRROR_PA;
    SFR_PB = MIRROR_PB;
    SFR_PC = MIRROR_PC;
    SFR_PD = MIRROR_PD;
    SFR_P6 = MIRROR_P6;
    SFR_P7 = MIRROR_P7;
}

/////////////////////////////////////////////////////////
