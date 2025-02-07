// ICSDK2 - Library
// start.c - startup
// CROM11 compatible version

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Low level startup
// a lot of initialization is done by system ROM before calling cartridge

/////////////////////////////////////////////////////////

extern void main0();

#pragma section area f_data
#pragma section data f_data
extern byte _DataRAM;	// for f_data initialize

void ADECL _ic_startup()
{
    // upon entry, interrupts are globally disabled,
    // but some interrupt handlers are individually enabled

    // disable watchdog
	SFR_WDMOD = 0;
	SFR_WDCR = 0xB1;

    // disable all interrupts
	    // individual sub-systems will re-enable them when needed
	SFR_INTEALM01 = 0;
	SFR_INTEALM23 = 0;
	SFR_INTE3ALM4 = 0;
	SFR_ALMINT = 0;
	SFR_INTE0AD = 0;
	SFR_INTETA01 = 0;
	SFR_INTETA23 = 0;

    SFR_TA01RUN = 0;    // stop timer0 and timer1
    SFR_P2 &= 0xC3; // all IR diodes off

    // now initialize RAM

    //BLOCK: clear all of RAM (except CROM area)
    {
	    byte* pb = (byte*)0x1000;
	    while (pb < (byte*)0x1F00)
	        *pb++ = '\0';
	    pb = (byte*)0x2000;
	    while (pb < (byte*)0x3000)
	        *pb++ = '\0';
    }

    // copy f_data init values to real RAM
        // code stolen from NeoGeo runtime
	{
		// init f_data
		__asm("	ld	xde,__DataRAM");
		__asm("	ld	xhl,startof(f_data)");	
		__asm("	ld	xbc,sizeof(f_data)");	
		__asm(" or	xbc,xbc");		
		__asm("	j	z,FD1");		
		__asm("	ldirb	(XDE+),(XHL+)");	
		__asm("	cp	qbc,0");		
		__asm("	j	eq,FD1");		
		__asm("	ld	wa,qbc");		
		__asm("FD2:");				
		__asm("	ldirb	(xde+),(xhl+)");	
		__asm("	djnz	wa,FD2");		
		__asm("FD1:");			
	}

    // setup stack - right below CROM data
    {
        __asm(" ld xsp,0x1F00");
        __asm(" pushb 0x55"); // marker
    }

    // call low level main0()
    main0();
    while (1)
        ;
}

/////////////////////////////////////////////////////////

void ADECL ic0_init_interrupt_handlers()
{
    *((unsigned char*)0x1500) = 0xA5;
        // this will redirect interrupt handlers to cartridge vectors
        // all interrupt handlers should be in place
}

/////////////////////////////////////////////////////////
