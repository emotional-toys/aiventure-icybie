// ICSDK2
// IR logic (higher level)

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Higher level IR logic
// Called by lower level IR logic (see ir1.c) when an IR code is decoded

/////////////////////////////////////////////////////////

static word __near g_lastIRCode; // 16 bit code
// filtered interpretation
static byte __near g_lastButton;
static byte __near g_lastDebouncedButton;
static byte __near g_lastVoice;

/////////////////////////////////////////////////////////
// PUBLIC

byte ADECL ic_get_last_remote_button()
{
    byte bRet = g_lastButton;
    g_lastButton = 0;
    return bRet;
}

byte ADECL ic_get_remote_button()
{
	byte b = ic_get_last_remote_button();
    if (b == 0)
        return 0; // nothing ready

    if (b == g_lastDebouncedButton)
        return 0;   // no repeats

	g_lastDebouncedButton = b;
    return b;
}

word ADECL ic_get_last_ir_code()
{
    word wRet = g_lastIRCode;
    g_lastIRCode = 0;
    return wRet;
}

byte ADECL ic_get_last_voice_command()
{
    byte bRet = g_lastVoice;
    g_lastVoice = 0;
    return bRet;
}

/////////////////////////////////////////////////////////
// IMPLEMENTATION

void ADECL _ic_process_ircode(word ir_code)
{
#ifdef ICL_VERBOSE
    if (ir_code >= 0xA0 && ir_code <= 0xAF)
	    printf("{Remote IR Code %x} ", ir_code);
    else if (ir_code >= 0xB0 && ir_code <= 0xEF)
	    printf("{Voice Code %x} ", ir_code);
    else if (ir_code >= 0x3381 && ir_code <= 0x338F)
        ; // skip obstacle report
    else
	    printf("{IR %x} ", ir_code);
#endif
    g_lastIRCode = ir_code; // anything
    if (HIBYTE(ir_code) == 0)
    {
        byte code00 = (byte)ir_code;
        if ((code00 & 0xF0) == 0xA0)
        {
            // IR remote
            g_lastButton = (ir_code & 0xF)+1;
        }
	    else if (code00 >= 0xB0 && code00 <= 0xEF)
        {
            // Reply from Voice chip
            if (code00 >= 0xE0 && code00 <= 0xE7)
	            g_lastVoice = (code00 - 0xE0)+1;
            else
	            g_lastVoice = code00; // usually EF error
	    }
    }
}

/////////////////////////////////////////////////////////
