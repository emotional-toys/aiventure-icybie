// ICSDK2 - Library
// init1.c - low level device init

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// Some SFR ports are mirrored in RAM.
// misc low level routines based on System ROM logic

/////////////////////////////////////////////////////////
// DATA

IC_PORT_MIRRORS __near _ic_ipm;
    // RAM copy of SFR port registers used in motor PWM output

/////////////////////////////////////////////////////////

void _ic_DiddleP5()
{
    SFR_P5 &= ~(1<<6);
    ic_sleep(4);
    SFR_P5 |= (1<<6);
    ic_sleep(4);
}


void ADECL _ic_killtime(int fasttick)
{
	// done in assembly to have a known execution time
	__asm("wait_loop: DJNZ WA,wait_loop");
}
void ADECL _ic_killtime2(int fasttick)
{
    while (fasttick--)
        ;
}

// interrupts and timers must be running
void ADECL ic0_init_peripherals()
{
    MIRROR_P6 = MIRROR_P7 = MIRROR_PA = 0xFF;
    MIRROR_PB = 0xFD; // leave TA1OUT bit alone
    MIRROR_PC = 0x2F;
    MIRROR_PD = 0x1F;
	_ic_DiddleP5();
}

/////////////////////////////////////////////////////////
