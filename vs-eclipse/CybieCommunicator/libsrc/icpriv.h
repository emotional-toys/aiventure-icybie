///////////////////////////////////////////////////////////////
// Common include for IC Library implementation

#include <ic.h> // public interfaces
#include <ic_sfr.h> // CPU Special Function Registers

///////////////////////////////////////////////////////////////
// private low-level functions

extern void _ic_DiddleP5();
extern void ADECL _ic_killtime(int fasttick);


///////////////////////////////////////////////////////////////
// customizable timer and other timer related services

typedef void (*TIMER_PROC)(void);
extern void ADECL _ic_settimerproc512(TIMER_PROC pfn);
extern void ADECL _ic_settimerproc64(TIMER_PROC pfn);

///////////////////////////////////////////////////////////////
// private data and functions - shared between ICLib2 sub-systems


extern byte __near _ic_legcur[NUM_LEGS];  // last sampled leg position
	// positions are straight from ADC (see adc.c, leg1.c)

bool ADECL _ic_adjust_motor_speed(byte imotor, sbyte speedNew);
    // adjusting motor speed with servo feedback loop

extern void ADECL _ic_settimerprocPWM10(TIMER_PROC proc);
    // PWM timer controlling servo loop

void ADECL _ic_process_ircode(word ir_code);
    // low level IR -> high level IR code

///////////////////////////////////////////////////////////////
// abstract leg positions are calibrated (see leg2.c)

typedef struct
{
    // loaded from E2PROM
	word legoffset[NUM_LEGS];
	byte leggain[NUM_LEGS];
} LEG_CALIBRATION_INFO;

extern LEG_CALIBRATION_INFO __near _ic_lci;

///////////////////////////////////////////////////////////////
// interface to the SunPlus daughter chip

// the sound and LED playback is handled by the SunPlus chip
// also 10 bits of polling information is returned from the SunPlus chip
// from the main CPU to the sound chip is 2 bits of data + clock
//  from the sound chip to the CPU is a single data/ack bit

#define PD_UPPERDATA_BIT 0x80
#define PD_LOWERDATA_BIT 0x40
#define PD_DATACLOCK_BIT 0x20

#define P9_DATAIN_BIT 0x10

void ADECL _ic_wait_daughter_idle();
void ADECL _ic_daughter_timer_proc(); // from sound.c - not pluggable

//////////////////////////////////////////////

///////////////////////////////////////////////////////////////


// default sections for library data/code

#pragma section area f_libarea
          // try not to non-zero inited stuff in library

#pragma section code f_libcode
#pragma section const f_libconst

//////////////////////////////////////////////
