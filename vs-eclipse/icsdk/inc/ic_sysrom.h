// ICSDK2 - ICybie Library
// sysrom - interface to the system ROM from a cartridge
// ADVANCED - used by startup code
	// used for stealing 150KB+ of audio samples,
	// and most of the motions

////////// ////////// ////////// ////////// //////////

typedef enum
{
    SYSROMVER_ERR = 0,
    SYSROMVER_ORIG, // with the original ICybies
    SYSROMVER_NEWER, // later release
    SYSROMVER_REVK // Jetta cost reduction release

    // add more with later releases
} SYSROMVER;


typedef RAW_MOTION_INFO const* const* _RAWMOT_ARRAY;

typedef struct
{
    enum SYSROMVER ver;
	SOUND_ADPCM4 const* const* sounds;
	_RAWMOT_ARRAY motions_1_230; // 1->230
	_RAWMOT_ARRAY motions_1000_1137; // 1000 -> 1137
                            // may be more, but this is all we use
} SYSROMINFO;

#define NUM_SYSROM_SOUNDS 30

////////// ////////// ////////// ////////// //////////

bool ADECL ic0_init_sysrom();
    // if returns false, can not use sysrom features

extern SYSROMINFO __near ic_sysrominfo; // be careful

////////// ////////// ////////// ////////// //////////
