// ICSDK2 - Library
// start.c - startup

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// Shutdown turns off all the peripherals, then HALTs the CPU
// This version never wakes up

/////////////////////////////////////////////////////////

void ADECL ic_shutdown()
{
    SFR_WDMOD  =  0;
    SFR_WDCR  =  0xB1;
	_ic_delay_sunplus(); // avoid jamming the sunplus chip
	_ic_send_to_daughter_sync(0x2F); // eyes off
	_ic_delay_sunplus(); // avoid jamming the sunplus chip
	_ic_send_to_daughter_sync(0x40); // reset

//REVIEW: what about shutting down RSC-300 ??

    // disable all interrupts
    __asm ( "EI 7" );

    // REVIEW_LATER: are all these necessary ?? (a pain to re-enable)
	SFR_INTE0AD = 0;
	// SFR_INTE12 = 0;
	SFR_INTE3ALM4 = 0;
	SFR_INTEALM01 = 0;
	SFR_INTEALM23 = 0;
	SFR_INTETA01 = 0;
	SFR_INTETA23 = 0;
	// SFR_INTRTC = 0;
	SFR_INTES0 = 0;
	// SFR_INTES1 = 0;
	// SFR_INTES2 = 0;
	// SFR_INTETC01 = 0;
	// SFR_INTETC23 = 0;
	// SFR_INTEP01 = 0;

	SFR_P2 &= 3; // turn off non-critical P2 bits
	SFR_P5 &= 0xC3;

    MIRROR_PB &= 0xFD;
    SFR_PB = MIRROR_PB;

    MIRROR_PC &= 0xF8;
    SFR_PC = MIRROR_PC;

    MIRROR_PD &= 0x1F;
    SFR_PD = MIRROR_PD;

	MIRROR_P6 &= 0xFD;
	SFR_P6 = MIRROR_P6;

	SFR_SYSCR2 |= (0x3D); // 2^16 warm up, HALT=IDLE2, pins stay
    SFR_SYSCR0 |= (1<<6); // low freq osc start
    SFR_SYSCR0 |= (1<<2); // write start timer
    // wait for warmup
    while (!(SFR_SYSCR0 & (1<<2)))
        ;
    SFR_SYSCR1 |= (1<<3); // use fs (low freq osc) system clock
    SFR_SYSCR0 &= ~(1<<7); // stop high freq oscillator

    MIRROR_PC |= 0x10;
    SFR_PC = MIRROR_PC;


    while (1)
    {
	    __asm ( "HALT" );
    }
}
    
//LATER: allow sleep mode and wakeup
#ifdef LATER
	LDL XIY,SFR__code_for_ramSubroutine0} ;; -- ADDRESS OF CODE
	LDL XIX,SFR_ramSubroutine0}
Lbl0097:
	LDIB_X
	CP XIY,SFR_d0096}
	JR NZ,SFR_Lbl0097}
Lbl0098:
	CALL SFR_ramSubroutine0} ; NEAR
	JR SFR_Lbl0098} ; reference {Lbl0069}

_code_for_ramSubroutine0:
	HALT
	BIT 4,SFR_P8
	JR Z,SFR__code_for_ramSubroutine0}
	A, = SFR_mirrorPC}
	AND A,0FBH // 1111 1011
	LD SFR_mirrorPCA
	LD SFR_PCA
	SFR_WDMOD = 0x86;
	RET ; reference SFR__code_for_ramSubroutine0}
#endif

