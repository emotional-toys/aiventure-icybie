// ICSDK2
// sound, LEDs

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// We send data to the SunPlus chip (daugher)
// Used for LED, polling, SunPlus reset and sound playback
// Synchronous (blocking) playback only

/////////////////////////////////////////////////////////

void ADECL _ic_send_to_daughter_sync(byte data)
{
    byte ibit;
    for (ibit = 0; ibit < 4; ibit++)
    {
        byte bits = 0;
        if (data & (0x80 >> ibit))
            bits |= PD_UPPERDATA_BIT;
        if (data & (0x8 >> ibit))
            bits |= PD_LOWERDATA_BIT;
        MIRROR_PD = (MIRROR_PD & 0x3F) | PD_DATACLOCK_BIT | bits;
        SFR_PD = MIRROR_PD;
	    do {} while ((SFR_P9 & P9_DATAIN_BIT) == 0);
        MIRROR_PD &= ~PD_DATACLOCK_BIT; // lower clock
        SFR_PD = MIRROR_PD;
	    do {} while (SFR_P9 & P9_DATAIN_BIT);
    }
}

void ADECL ic_show_eyes(byte pattern)
{
	_ic_delay_sunplus(); // avoid jamming the sunplus chip
	_ic_send_to_daughter_sync(pattern);
}

void ADECL ic_play_sound(SOUND_ADPCM4 const* soundPtr)
{
    const byte* bytePtr = soundPtr->data;
    word byteCount = soundPtr->size;
    ic_assert(soundPtr->zero == 0);

	_ic_delay_sunplus(); // avoid jamming the sunplus chip

    __asm ( "EI 5" ); // disable most interrupts

	_ic_send_to_daughter_sync(0x10);
	_ic_send_to_daughter_sync(0); // under 64K
    _ic_send_to_daughter_sync(HIBYTE(soundPtr->size));
    _ic_send_to_daughter_sync(LOBYTE(soundPtr->size));

    while (byteCount--)
	    _ic_send_to_daughter_sync(*bytePtr++);
    __asm ( "EI 0" ); // enable!
}

