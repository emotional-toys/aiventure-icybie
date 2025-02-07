// ICSDK2
// timer services

#include "icpriv.h"

///////////////////////////////////////////////////////////////////
//OVERVIEW:
// General timer services.
// Only those timers actually used are running and have interrupts enabled.
// Even with simple interrupt handlers, the overhead in handling interrupts
//  can slow down processing of the main (non interrupt) program.

///////////////////////////////////////////////////////////////////

void ADECL ic0_init_timers()
{
    // ALM1 = 512Hz (run low priority)
	SFR_INTEALM01 = 0x45;  // 512Hz prio 4, 8kHz prio 5
    SFR_INTEALM23 = 0x03;  // 2Hz off, 64Hz prio 3 (lower than others and ADC)
	SFR_INTE3ALM4 = 0x10;  // 1Hz prio 1
	SFR_ALMINT |= 0x17; // 1Hz | 64Hz | 512Hz | 8kHz
}

///////////////////////////////////////////////////////////////////
// Int14 - INTALM0 - 8kHz

static long __near s_clock8000;

#pragma section code f_cart_int14
void __interrupt interrupt_handler14()
{
    // keep it short
    s_clock8000++;
}

long ADECL ic_getclock8000()
{
    return s_clock8000;
}

#pragma section code f_libcode

///////////////////////////////////////////////////////////////////
// Int15 - INTALM1 - 512Hz

volatile int __near s_waitTimer512;
static TIMER_PROC __near g_timerProc512; // only one of them for Servo PWM

// Int15 - INTALM1 - 512Hz
#pragma section code f_cart_int15
void __interrupt interrupt_handler15()
{
    if (s_waitTimer512 > 0)
		s_waitTimer512--;
    if (g_timerProc512 != NULL)
        (*g_timerProc512)();
}

void ADECL _ic_settimerproc512(TIMER_PROC pfn)
{
    ic_assert(g_timerProc512 == NULL);
    g_timerProc512 = pfn;
}

void ADECL ic_sleep(int approxms)
{
    s_waitTimer512 = approxms / 2;    // approximate
    while (s_waitTimer512 > 0)
        ;
}

#pragma section code f_libcode

///////////////////////////////////////////////////////////////////
// Int16 - INTALM2 - 64Hz

// allow two hookable timer procs
static TIMER_PROC __near g_timerProc64a;
static TIMER_PROC __near g_timerProc64b;

#pragma section code f_cart_int16
void __interrupt interrupt_handler16()
{
    if (g_timerProc64a != NULL)
        (*g_timerProc64a)();
    if (g_timerProc64b != NULL)
        (*g_timerProc64b)();
}

void ADECL _ic_settimerproc64(TIMER_PROC pfn)
{
    if (g_timerProc64a == NULL)
	    g_timerProc64a = pfn;
    else if (g_timerProc64b == NULL)
	    g_timerProc64b = pfn;
    else
        ic_assert(false);   // too many
}

#pragma section code f_libcode

///////////////////////////////////////////////////////////////////
// Int17 - INTALM3 - 2Hz

// not currently used

///////////////////////////////////////////////////////////////////
// Int18 - INTALM4 - 1Hz

static long __near s_clock1sec;

#pragma section code f_cart_int18
void __interrupt interrupt_handler18()
{
    s_clock1sec++;
}

long ADECL ic_getclock1()
{
    return s_clock1sec;
}

///////////////////////////////////////////////////////////////////
