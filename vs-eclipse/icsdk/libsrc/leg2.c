// ICSDK2
// high level leg control, using calibration info (rarely used directly)

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// We use the leg calibration factors stored in E2PROM
// MOTION format stores leg positions in 6 bit values.
// These are scaled and offset adjusted to cover the full
//  range of the servos.

/////////////////////////////////////////////////////////
// DATA

LEG_CALIBRATION_INFO __near _ic_lci; // semi-public

/////////////////////////////////////////////////////////
// PUBLIC

// Calculate real servo leg position based on motPos

byte ADECL ic_scale_motion_leg(byte ileg, byte motPos)
{
    int val;
    ic_assert(ileg < NUM_LEGS);
    ic_assert(motPos < 64); // 6 bits from MOT format
    val = (motPos * _ic_lci.leggain[ileg] + _ic_lci.legoffset[ileg]) / 50;
    if (val <= 0 || val > 255)
    {
        printf("WARNING: ic_scale_motion_leg(%d, %d) = %d - returning byte\n",
            ileg, motPos, val);
    }
    return (byte)val;
}

/////////////////////////////////////////////////////////
// INITIALIZE

void ADECL ic0_load_legcalib()
{
    // first 18 words (36 bytes) of E2PROM are leg servo calibration info
    // we use the same leg ordering as the old ROM so we can use these directly
    word* pw = (word*)&_ic_lci;
    byte i;

	ic_read_storage(0); // first use is bogus

    ic_assert(sizeof(_ic_lci) == 36); // same servo order
    for (i = 0; i < sizeof(_ic_lci)/2; i++)
        pw[i] = ic_read_storage(i);
}

/////////////////////////////////////////////////////////
