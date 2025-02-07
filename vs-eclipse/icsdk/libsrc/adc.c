// ICSDK2
// AtoD converter

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// The ADC converter is running all the time
// It will sample the 12 leg positions, battery voltage and light level
// The 12 legs are done four at a time
// See the Toshiba CPU specs for details on the ADC SFR registers

/////////////////////////////////////////////////////////
// data variables (for use in this module)

byte __near _ic_legcur[NUM_LEGS];  // last sampled leg position
                // also used by leg1.c

static byte __near g_last_light_level;
static byte __near g_last_batt_level;

/////////////////////////////////////////////////////////
// PUBLIC functions

byte ADECL ic_get_leg_position(byte ileg)
{
    ic_assert(ileg < NUM_LEGS);
    return _ic_legcur[ileg];
}

byte ADECL ic_get_light_level()
{
    return g_last_light_level;
}

byte ADECL ic_get_battery_level()
{
    return g_last_batt_level;
}

/////////////////////////////////////////////////////////
// IMPLEMENTATION
/////////////////////////////////////////////////////////

static byte g_adcMode;
    // 0 (leg), 1 (hip), 2 (knee) for leg servo positions (multiplexed)
        // sampled 4 values at a time
    // 3 for light, 4 for battery (sampled one at a time)

static void ADECL SetAdcServoSelect()
{
    if (g_adcMode == 0)
    {
        // leg
		SFR_P2 = (SFR_P2 & 0x3F) | 0x40;
	    SFR_P5 &= ~(1<<2);
    }
    else if (g_adcMode == 1)
    {
        // hip
	    SFR_P5 &= ~(1<<2);
		SFR_P2 = (SFR_P2 & 0x3F) | 0x80;
    }
    else if (g_adcMode == 2)
    {
        // knee
		SFR_P2 = (SFR_P2 & 0x3F);
	    SFR_P5 |= (1<<2);
    }
}

static void ADECL StartAdcConversion()
{
    if (g_adcMode < 3)
    {
	    SFR_ADMOD1 = 0xC3; // scan 0,1,2,3
	    SFR_ADMOD0 = 3; // start scan mode
    }
    else if (g_adcMode == 3)
    {
        // light level
	    SFR_ADMOD1 = 0xC6; // fixed AN6 input
	    SFR_ADMOD0 = 1; // start fixed mode
    }
    else if (g_adcMode == 4)
    {
        // battery level
	    SFR_ADMOD1 = 0xC7; // fixed AN7 input
	    SFR_ADMOD0 = 1; // start fixed mode
    }
    else
    {
        ic_assert(false);
    }
}

/////////////////////////////////////////////////////////
//INITIALIZATION

void ADECL ic0_init_adc()
{
	SFR_INTE0AD = 0x40;
    g_adcMode = 0;
    SetAdcServoSelect();
	StartAdcConversion();
}

/////////////////////////////////////////////////////////
// INTERRUPT handler for ADC conversion complete

// Int27 - ADC complete
#pragma section code f_cart_int27
void __interrupt interrupt_handler27()
{
    if (g_adcMode < 3)
    {
        // original ROM order
	    _ic_legcur[g_adcMode] = SFR_ADREG04H; // LF
	    _ic_legcur[g_adcMode+3] = SFR_ADREG15H; // RF
	    _ic_legcur[g_adcMode+9] = SFR_ADREG26H; // LH
	    _ic_legcur[g_adcMode+6] = SFR_ADREG37H; // RH
    }
    else if (g_adcMode == 3)
    {
        // light sample
		g_last_light_level = SFR_ADREG26H;
    }
    else if (g_adcMode == 4)
    {
		static byte g_batt_count;
		static word g_batt_sum;
	    byte batt = SFR_ADREG37H;

        g_batt_sum += batt;
        if (++g_batt_count >= 30)
        {
			g_last_batt_level = g_batt_sum / 30;
            g_batt_sum = 0;
            g_batt_count = 0;
        }
    }

	g_adcMode++;
    if (g_adcMode > 4)
        g_adcMode = 0;
	SetAdcServoSelect();
	StartAdcConversion();
}

/////////////////////////////////////////////////////////
