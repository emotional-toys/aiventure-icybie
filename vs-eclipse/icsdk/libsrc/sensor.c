// ICSDK2
// sensor polling

#include "icpriv.h"

////////////////////////////////////////////////////
//OVERVIEW:
// Talks to SunPlus chip to get 10 bits of sensor data

//LATER: timeout and polling logic could be better
////////////////////////////////////////////////////

#define TIMEOUT_VALUE   30000

word ADECL ic_get_sensors()
{
    register word sensbits = 0;
    byte ibit;

	_ic_delay_sunplus(); // avoid jamming the sunplus chip
poll_retry:
	_ic_send_to_daughter_sync(0x30);   // poll request to daughter

	MIRROR_PD &= ~PD_DATACLOCK_BIT;
	SFR_PD = MIRROR_PD;

    //BLOCK: setup with timeout
    {
	    register long timeout_loop = TIMEOUT_VALUE;
	    while ((SFR_P9 & P9_DATAIN_BIT) == 0)
	    {
	        if (--timeout_loop == 0)
	        {
                printf("SENSOR TIMEOUT - retrying\n");
                goto poll_retry;
	        }
	    }
    }

	    MIRROR_PD |= PD_LOWERDATA_BIT; // some form of reset
        SFR_PD = MIRROR_PD;
	    ic_sleep(10);
	    MIRROR_PD &= ~PD_LOWERDATA_BIT;
        SFR_PD = MIRROR_PD;
	    ic_sleep(20);


    // now get the data
    for (ibit = 0; ibit < 10; ibit++)
    {
	    __asm ( "EI 5" ); // disable most interrupts
	    MIRROR_PD |= PD_DATACLOCK_BIT; // get bit
        SFR_PD = MIRROR_PD;
	    if (SFR_P9 & P9_DATAIN_BIT)
	    {
            sensbits |= (1 << ibit);
	    }
	    MIRROR_PD &= ~PD_DATACLOCK_BIT; // got it
        SFR_PD = MIRROR_PD;
	
	    __asm ( "EI 0" ); // enable!
        ic_sleep(2);
	}

    // wrap up
	_ic_killtime(1000);
    MIRROR_PD |= PD_LOWERDATA_BIT;
	SFR_PD = MIRROR_PD;

    //BLOCK: timeout test
	{
	    register long timeout_loop = TIMEOUT_VALUE;
	    while ((SFR_P9 & P9_DATAIN_BIT) == 0)
	    {
	        if (--timeout_loop == 0)
	        {
                printf("SENSOR TIMEOUT - retrying\n");
                goto poll_retry;
	        }
	    }
	    MIRROR_PD &= ~PD_LOWERDATA_BIT;

	    while ((SFR_P9 & P9_DATAIN_BIT) == 0)
	    {
	        if (--timeout_loop == 0)
	        {
                printf("SENSOR TIMEOUT - retrying\n");
                goto poll_retry;
	        }
        }
    }

// poll_done:
	_ic_DiddleP5();

    return sensbits;
}
