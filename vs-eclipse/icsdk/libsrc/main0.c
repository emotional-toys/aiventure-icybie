// ICSDK2 - Library
// main0.c - lowest level main0()

#include "icpriv.h"
#include "ic_main0.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// This does all the sub-system initialization for a 'typical' program
// The 'main0' routine can be replaced to customize the startup
//  process if not all the sub-systems are required

/////////////////////////////////////////////////////////

extern void main(); // user provided main

void main0()
{
    // standard initialization
    // assume we are going to walk, and talk, and use the system ROM sounds

	ic0_init_interrupt_handlers(); // do this before enabling interrupts

	sic_init_serial(BAUD_38400); // always enable
    __asm ( "EI 0" ); // enable!

#ifdef ICL_VERBOSE
    printf("SicSDK2\n");
    printf("\tserial port running at 38.4 kbps\n");
    printf("\truntime (c) 2002 ICybiePet\n");
#endif

	ic0_init_timers();
    ic0_init_peripherals();

    if (!ic0_init_sysrom())
    {
	    printf("FATAL: bad system ROM\n");
		ic_show_eyes_red();
        return;
    }

    ic0_init_ir();
    ic0_init_adc();
	ic0_init_motors();
	ic0_init_legs();
	ic0_load_legcalib();

	ic0_init_motion();

#ifdef ICL_VERBOSE
    printf("\tcalling main()\n");
#endif

    main();

#ifdef ICL_VERBOSE
    printf("main() returned - calling shutdown\n");
#endif

    ic_shutdown();
}

/////////////////////////////////////////////////////////
