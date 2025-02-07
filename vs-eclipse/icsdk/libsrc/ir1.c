// ICSDK2
// lower level IR logic

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:

// We have out own IR handler
// Supports several modes. Usually backward compatible for
//  receiving IR Remote compatible signals and talk to the voice chip
// Faster IR obstacle probing supported for obstacle detect

// NOTE: not well documented. Be sure you know what you are doing
//  before changing. The public API provides most functionality you will need.
/////////////////////////////////////////////////////////
// DATA STRUCTURES

enum IRS_STATE
{
    IRS_IDLE,       // not transmitting, will be receiving
    IRS_SENDING,    // sending the old fashioned way (16 bits)
    IRS_PROBING     // sending short blips for obstacle detect
};

typedef struct
{
    byte state; // enum IRS_STATE

    // all the time (listening)

    // low level receiving
    byte bWasLow;
    byte nLow;
    word nHigh; // max out at 10000

    // high level receiving
    byte rxBitCount;
    word rxIrCode;

	// if state == IRS_SENDING
	word txSendCode;
	byte txDirection;
	byte txBitCount;
	byte txTickCount, txTick2Count;

} IR_LOGIC_STATE;

static IR_LOGIC_STATE __near g_irs;

///////////////////////////////////////////////////////////////////
// Timing for standard IR codes

// receive timing based on low transition time
// high time ignored except for very long start case
#define MIN_SHORTGAP 2
#define MAX_SHORTGAP 5
#define MIN_LONGGAP 10
#define MAX_LONGGAP 14

// for sending
#define IDEAL_SHORTGAP 4
#define IDEAL_LONGGAP 12

///////////////////////////////////////////////////////////////////

void ADECL ic0_init_ir()
{
	SFR_INTETA23 |= 0x40; // TA3 enabled

    // initialize - timer 0 not properly initialized
    SFR_TA01RUN = 0;    // stop both
    SFR_TA01MOD = 5;    // both phi_ti
    SFR_TA1REG = 0x2C;  // count value
    SFR_TA1FFCR = 0xB;
    SFR_TA01RUN = 5;    // run timer1 prescaler and timer0
                            // but not actual timer1
}

/////////////////////////////////////////////////////////
// Sending, using one of 5 IR LEDs

// Port2 (bits 2->5) select the IR LEDs (strange encoding)
// PortB (bit 1) TA1OUT is used for IR modulation ???

static void ADECL Modulate(bool bOn)
{
	if (g_irs.txDirection == TXIR_VOICE)
    {
        // special non-IR path to talk to voice chip
        if (bOn)
	        SFR_P2 |= 4;
        else
		    SFR_P2 &= 0xC3; // turn off other bits
    }
    else
    {
        // real IR modulation
		ic_assert((g_irs.txDirection & 0xC3) == 0);
		ic_assert((MIRROR_PB & 2) == 0); // never set this bit
        if (bOn)
        {
		    SFR_P2 |= g_irs.txDirection;
	        SFR_TA01RUN |= 2; // run timer1
        }
        else
        {
		    SFR_TA01RUN &= ~2;    // stop
		    SFR_TA1REG = 0x2C; // reload count value
		    SFR_P2 &= 0xC3; // turn off other bits
        }
    }
}

bool ADECL ic_send_ir_code(byte tx_direction, word code)
{
    if (g_irs.state != IRS_IDLE)
        return false;

    g_irs.txDirection = tx_direction;
    g_irs.txSendCode = code;
    g_irs.txBitCount = 0xFF;  // startup
    g_irs.state = IRS_SENDING;
    // interrupt handler will do the rest

    return true;
}

bool ADECL ic_is_sending_ir_code()
{
    return (g_irs.state == IRS_SENDING);
}

word ADECL ic_get_IR_quiet_level()
{
    if (g_irs.bWasLow)
        return 0;   // just got something
    return g_irs.nHigh;
}


/////////////////////////////////////////////////////////
// INTERRUPT handler

static void ADECL Handle_IR_Receiving();
static void ADECL Handle_IR_Sending();

// Int22 - timer handler (about 3537Hz?)
#pragma section code f_cart_int22
void __interrupt interrupt_handler22() // must be first in section
{
    if (g_irs.state == IRS_PROBING)
        return; // do nothing while in probe mode

	Handle_IR_Receiving(); // RX
    if (g_irs.state == IRS_SENDING)
	    Handle_IR_Sending(); // TX
}


static void ADECL Handle_IR_Receiving()
{
    ic_assert(MIRROR_PB & 4);

    if (SFR_PB & 4)
    {
        // now high
        if (g_irs.bWasLow)
        {
            // low-> high transition - do all the work
            byte new_bit = 0xFF;

            if (g_irs.nLow >= MIN_SHORTGAP && g_irs.nLow <= MAX_SHORTGAP)
            {
                // looks like a zero bit
                new_bit = 0;
            }
            else if (g_irs.nLow >= MIN_LONGGAP && g_irs.nLow <= MAX_LONGGAP)
            {
                // looks like a one bit
                new_bit = 1;
            }
            
            if (new_bit != 0xFF)
            {
	            // low was valid, see if high is ok
	            if (g_irs.nHigh > MAX_LONGGAP)
	            {
	                // start bit
	                g_irs.rxIrCode = new_bit;
	                g_irs.rxBitCount = 1;
	            }
	            else if (g_irs.rxBitCount > 0)
	            {
	                // another data bit
	                g_irs.rxIrCode = (g_irs.rxIrCode << 1) | new_bit;
	                g_irs.rxBitCount++;
	                if (g_irs.rxBitCount == 16)
	                {
	                    _ic_process_ircode(g_irs.rxIrCode);
	                    g_irs.rxBitCount = 0;
	                }
	            }
	            else
	            {
	                // ignore
	            }
            }
            else
            {
                // looks like noise
                g_irs.rxBitCount = 0;
            }
	
            g_irs.nHigh = 1;
            g_irs.bWasLow = false;
        }
        else
        {
            // still high
            if (g_irs.nHigh < 10000)
	            g_irs.nHigh++;
        }
    }
    else
	{
        // now low
        if (g_irs.bWasLow)
        {
            // still low
            if (g_irs.nLow != 255)
	            g_irs.nLow++;
        }
        else 
        {
            // high to low transition
            g_irs.nLow = 1;
            g_irs.bWasLow = true;
        }
    }


}

/////////////////////////////////////////////////////////
// High level sending

static void ADECL Handle_IR_Sending()
{
    ic_assert(g_irs.state == IRS_SENDING);

    if (g_irs.txBitCount == 0xFF)
    {
        // initialize - start with signal high for a while
		// IR modulation is used in special case to send to RSC-300
	    Modulate(false);

        g_irs.txTickCount = 50;  // long startup time
//REVIEW_LATER: tweek startup and cutoff times
        g_irs.txTick2Count = 0; // then start sending data
	    g_irs.txBitCount = 16;
    }
    else if (g_irs.txTickCount > 0)
    {
        // nothing to do this time
        g_irs.txTickCount--;
    }
    else if (g_irs.txTick2Count > 0)
    {
        // toggle from low to high, then wait some more
        Modulate(false);
        g_irs.txTickCount = g_irs.txTick2Count;
        g_irs.txTick2Count = 0;
    }
    else if (g_irs.txBitCount > 0)
    {
        // send another bit
        byte new_bit;
		Modulate(true);

        g_irs.txBitCount--;
        new_bit = (g_irs.txSendCode & (1 << g_irs.txBitCount)) != 0;
        if (new_bit)
        {
            // send "1" bit
            g_irs.txTickCount = IDEAL_LONGGAP;
            g_irs.txTick2Count = IDEAL_SHORTGAP;
        }
        else
        {
            // send "0" bit
            g_irs.txTickCount = IDEAL_SHORTGAP;
            g_irs.txTick2Count = IDEAL_LONGGAP;
        }
    }
    else
    {
        // all 16 bits sent
        Modulate(false); // just in case
	    g_irs.state = IRS_IDLE;  // sent
    }
}

/////////////////////////////////////////////////////////
// fast obstacle detection happens immediately
// (does not require interrupts)

static const byte g_txir_values[CHESTDIR_MAX] = // same order as enum CHEST_DIRECTION
{
    // 5 normal
    TXIR_TOP, TXIR_TL, TXIR_TR, TXIR_BL, TXIR_BR,
    // 2 simple combinations (2 LEDs)
    TXIR_L, TXIR_R,
    // 4 top combinations (2 LEDs)
    TXIR_TOP | TXIR_TL, TXIR_TOP | TXIR_TR,
    // 4 top + side combinations (3 LEDs)
    TXIR_TOP | TXIR_L, TXIR_TOP | TXIR_R
};

#define IR_PROBE_SLEEP_TIME 4

int ADECL ic_fast_obstacle_detect()
{
    word result = 0;
    byte idir;

    if (g_irs.state != IRS_IDLE)
    {
#ifdef ICL_VERBOSE
		printf("IRPROBE: busy\n");
#endif
        return -1;
    }

    g_irs.state = IRS_PROBING;

    // light travels pretty darn fast - so do it now (no interrupts)
    // since IR is modulated at 37.5kHz, we still need to wait a short
    // while for modulated IR to be detected (or undetected)

    for (idir = 0; idir < CHESTDIR_MAX; idir++)
    {
	    g_irs.txDirection = g_txir_values[idir];
        Modulate(false);

	    ic_sleep(IR_PROBE_SLEEP_TIME);
		if ((SFR_PB & 4) == 0)
        {
            // to noise to probe
#ifdef ICL_VERBOSE
			printf("IRPROBE: noisy\n");
#endif
		    g_irs.state = IRS_IDLE;
            return -2;
        }

        Modulate(true);
	    ic_sleep(IR_PROBE_SLEEP_TIME);

        if ((SFR_PB & 4) == 0)
            result |= (1 << idir);
        Modulate(false);
    }
    g_irs.state = IRS_IDLE;
    return result; // >= 0
}

bool ADECL ic_count_obstacle_detect(int counts[CHESTDIR_MAX], int countMax)
{
    word result = 0;
    byte idir;

    if (g_irs.state != IRS_IDLE)
    {
#ifdef ICL_VERBOSE
		printf("IRPROBE: busy\n");
#endif
        return false;
    }

    g_irs.state = IRS_PROBING;
    for (idir = 0; idir < CHESTDIR_MAX; idir++)
    {
        int count;
	    g_irs.txDirection = g_txir_values[idir];
        Modulate(false);
	    ic_sleep(IR_PROBE_SLEEP_TIME);

		if ((SFR_PB & 4) == 0)
        {
            // to noise to probe
#ifdef ICL_VERBOSE
			printf("IRPROBE: noisy\n");
#endif
		    g_irs.state = IRS_IDLE;
            return -2;
        }

		counts[idir] = -1;  // if not detected

        Modulate(true);
        for (count = 0; count < countMax; count++)
        {
			if ((SFR_PB & 4) == 0)
	        {
                counts[idir] = count;
                break;
            }
        }

        Modulate(false);
    }
    g_irs.state = IRS_IDLE;
    return true;
}

/////////////////////////////////////////////////////////
