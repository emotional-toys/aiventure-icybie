// ICSDK2
// hook into system ROM

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// The system ROM has 256KB of code+motions+actions+sounds
// We steal the sounds and many of the motions from the system ROM
//  for use in our cartridge program.
// There are 3 known versions of the system ROM with slightly
//  different contents. This module detects which version of the system ROM
//  is present, and compensates accordingly.
// The 30 system sounds are all the same.
// The motions are all slighly different. 'motion2.c' handles
//  these exceptions, so that only a small amount of cartridge ROM is
//  needed to support the hundreds of standard motions.

/////////////////////////////////////////////////////////
// DATA

SYSROMINFO __near ic_sysrominfo;

/////////////////////////////////////////////////////////
//INITIALIZATION

bool ADECL ic0_init_sysrom()
{
    // figure out which version of the system ROM we have
    const long *pl = (const long*)0xFCFFF5;

	ic_sysrominfo.ver = SYSROMVER_ERR;
	ic_sysrominfo.sounds = (SOUND_ADPCM4 const* const*)0xFED975;
                // all know versions have sounds in same location

    if (pl[0] == 0x03CE0D00)
    {
        ic_sysrominfo.ver = SYSROMVER_ORIG;
		ic_sysrominfo.motions_1_230 = (_RAWMOT_ARRAY)0xFC4DA3;
		ic_sysrominfo.motions_1000_1137 = (_RAWMOT_ARRAY)0xFF0B0F;
    }
    else if (pl[0] == 0x0463EF00)
    {
        if (pl[1] == 0x000AAF01)
	        ic_sysrominfo.ver = SYSROMVER_NEWER;
        else if (pl[1] == 0x0009B001)
	        ic_sysrominfo.ver = SYSROMVER_REVK;

		ic_sysrominfo.motions_1_230 = (_RAWMOT_ARRAY)0xFC4DB5;
		ic_sysrominfo.motions_1000_1137 = (_RAWMOT_ARRAY)0xFF10DE;
    }

	return (ic_sysrominfo.ver != SYSROMVER_ERR);
}

////////// ////////// ////////// ////////// //////////
// PUBLIC functions

void ADECL ic_play_sysrom_sound(byte sound_index)
{
    ic_assert(sound_index >= 1 && sound_index <= NUM_SYSROM_SOUNDS);
    ic_assert(ic_sysrominfo.sounds != NULL);

    ic_play_sound(ic_sysrominfo.sounds[sound_index-1]);
}

////////// ////////// ////////// ////////// //////////
