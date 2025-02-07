
#include <ic.h>

#include "encoder.h"

/////////////////////////////////////////////////////
// ADPCM decoding tables for SunPlus chip

#define QTABLE_SIZE 64
#define QTABLE_MASK 63

const byte slope_table[QTABLE_SIZE] =
{
	0,1,2,3,4,5,7,9,
	1,2,3,4,5,7,9,13,
	2,3,5,8,12,17,23,30,
	3,6,11,18,27,38,51,66,
	4,9,14,24,34,44,64,84,
	5,11,17,29,41,53,77,101,
	6,13,20,34,48,62,90,118,
	7,15,23,39,55,71,103,125,
};

const byte next_step[QTABLE_SIZE] =
{
	/* 00 */ 0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x08,
	/* 08 */ 0x00,0x00,0x08,0x08,0x08,0x08,0x10,0x10,
	/* 10 */ 0x08,0x08,0x10,0x10,0x10,0x10,0x18,0x18,
	/* 18 */ 0x10,0x10,0x10,0x18,0x18,0x20,0x20,0x28,
	/* 20 */ 0x18,0x18,0x18,0x20,0x20,0x28,0x28,0x30,
	/* 28 */ 0x20,0x20,0x28,0x28,0x28,0x28,0x30,0x38,
	/* 30 */ 0x28,0x28,0x30,0x30,0x30,0x30,0x38,0x38,
	/* 38 */ 0x30,0x30,0x38,0x38,0x38,0x38,0x38,0x38,
};

/////////////////////////////////////////////////////
// DATA: state of encoder

static struct
{
	byte qindex;
	signed char sampPrev;
} g_adpcmc;

void PrepareAdpcm()
{
    g_adpcmc.qindex = 0;
    g_adpcmc.sampPrev = 0;
}

void ReportAdpcmStatus()
{
    printf("ADPCM encoder : qindex = %d, last sample = %d\n",
            g_adpcmc.qindex, g_adpcmc.sampPrev);
}

byte ADECL EncodeAdpcm(sbyte sampNow)
{
    byte sign = (sampNow < g_adpcmc.sampPrev) ? 1 : 0; // 1 for negative
    byte valBest = 0xFF; // best match
    int deltaBest = 256;
    sbyte sampBest;
    byte val;

    ic_assert((g_adpcmc.qindex & 0x38) == g_adpcmc.qindex);

    // dumb version of slope matching
    // test all 8 possible values, closest match wins
    // this may not be optimal since the future slope curve is not considered
    for (val = 0; val < 8; val++)
    {
		byte slope = slope_table[(g_adpcmc.qindex + val) & QTABLE_MASK];
        sbyte sampTest = (sign) ? (g_adpcmc.sampPrev-slope) : (g_adpcmc.sampPrev+slope);
        int delta = (int)sampTest - (int)sampNow;
        if (delta < 0)
            delta = -delta;

        if (delta < deltaBest)
        {
            deltaBest = delta;
            sampBest = sampTest;
            valBest = val;
            // ic_assert(g_adpcmc.qindex + val < QTABLE_SIZE); // no wrap
        }
    }
    // ic_assert(valBest >= 0 && valBest <= 7);
    g_adpcmc.sampPrev = sampBest;
    g_adpcmc.qindex = next_step[g_adpcmc.qindex + valBest]; // no wrap
    return (valBest << 1) | sign;
}

//////////////////////////////////////////////////////
