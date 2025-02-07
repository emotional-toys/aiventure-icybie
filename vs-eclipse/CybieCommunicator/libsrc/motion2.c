// ICSDK2
// motion from numbers

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// All system ROMs have the same 30 sounds
// BUT of the 60KB of motions, there are some differences.
//
// Instead of having a copy of all the motions (about 60KB),
//  we reuse those in the system ROM we can, and just store
//  the exceptions in the cartridge (using about 4KB instead of 60KB)
//
// The generated file "mote.c_" contains the exceptions
/////////////////////////////////////////////////////////

#include "mote.c_"  // exceptions


static RAW_MOTION_INFO const* ADECL CheckExceptions(int motionIndex,
	MOTION_EX const* exceptions, byte numExcept)
{
    while (numExcept--)
    {
        if (exceptions->motionIndex == motionIndex)
        {
#ifdef ICL_VERBOSE
		    printf("\t using exception for motion %d\n", motionIndex);
#endif
            return (RAW_MOTION_INFO const*)exceptions->pbMotion;
        }
        exceptions++;
    }
    return NULL;
}


/////////////////////////////////////////////////////////

bool ADECL ic_perform_motion(word motionIndex)
{
	RAW_MOTION_INFO const* motionPtr = NULL;

#ifdef ICL_VERBOSE
    printf("ic_perform_motion(%d)\n", motionIndex);
#endif
    if (motionIndex <= 230)
    {
        ic_assert(motionIndex >= 1 && motionIndex <= 230);
        ic_assert(ic_sysrominfo.motions_1_230 != NULL);
        motionPtr = CheckExceptions(motionIndex,
            g_motionExLower, NUM_LOWER_EXCEPT);
        if (motionPtr == NULL)
	        motionPtr = ic_sysrominfo.motions_1_230[motionIndex-1];
    }
    else if (motionIndex >= 1000 && motionIndex <= 1137)
    {
        ic_assert(ic_sysrominfo.motions_1000_1137 != NULL);
        motionPtr = CheckExceptions(motionIndex,
            g_motionExUpper, NUM_UPPER_EXCEPT);
        if (motionPtr == NULL)
	        motionPtr = ic_sysrominfo.motions_1000_1137[motionIndex-1000];
    }
    else if (motionIndex >= 1138 &&
        motionIndex < 1138 + NUM_EXTRA_EXCEPT)
    {
        motionPtr = (RAW_MOTION_INFO const*)g_motionExtra[motionIndex-1138];
    }
    else
    {
        ic_assert(false);   // NYI - custom motions
    }

    if (motionPtr == NULL)
        return false;
	return ic_perform_motion_raw(motionPtr);
}

////////// ////////// ////////// ////////// //////////
