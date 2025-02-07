// ICSDK2 - ICybie Library
// LED/Sound playback support

// NOTE: only synchronous (blocking) playback supported
//  asynchronous playback is possible, but is very invasive because
//   of the 2 bit hardware protocol to send data to the SunPlus chip

////////// ////////// ////////// ////////// //////////
// LED control

void ADECL ic_show_eyes(byte pattern);

#define MAKE_EYES(left, right)   (0x20 + (left)*4 + (right))
#define EYE_YELLOW 0
#define EYE_RED    1
#define EYE_GREEN  2
#define EYE_OFF    3

#define ic_show_eyes_yellow()  ic_show_eyes(MAKE_EYES(EYE_YELLOW, EYE_YELLOW))
#define ic_show_eyes_red()     ic_show_eyes(MAKE_EYES(EYE_RED, EYE_RED))
#define ic_show_eyes_green()   ic_show_eyes(MAKE_EYES(EYE_GREEN, EYE_GREEN))
#define ic_show_eyes_off()     ic_show_eyes(MAKE_EYES(EYE_OFF, EYE_OFF))

////////// ////////// ////////// ////////// //////////
// PCM4 audio playback

typedef struct
{
    word size;    // bytes to follow (less than 64K)
    word zero;  // actually upper part of size
    byte data[];
} SOUND_ADPCM4; /*SYSROM COMPATIBLE DO NOT ALTER*/

void ADECL ic_play_sound(SOUND_ADPCM4 const* soundPtr);
    // synchronous playback only

////////// ////////// ////////// ////////// //////////
// Custom audio (helper macros, see usage in samples)

#define EXTERN_AUDIO(name) \
    extern const SOUND_ADPCM4 audio_##name;

#define ic_play_my_sound(name) \
    ic_play_sound(&audio_##name)

////////// ////////// ////////// ////////// //////////
// built-in sounds (requires SYSROM)

void ADECL ic_play_sysrom_sound(byte sound);
    // 'sound' must be in range of 1->30

enum
{
    SOUND_BEEP = 1,
    SOUND_YIP = 2, // very short
    SOUND_YUP = 3, // very short not recommended
    SOUND_BARK = 4,
    SOUND_BARK2 = 5,
    SOUND_BARK3 = 6,
    SOUND_BURP = 7,
    SOUND_LICK = 8, // lick 4 times
    SOUND_IN_PAIN = 9,
    SOUND_PANT = 10,
    SOUND_DRINK = 11,
    SOUND_GROWL = 12,
    SOUND_YOU_ASS_HOLE = 13,
    SOUND_HOWL = 14,
    SOUND_RURUFF = 15,
    SOUND_PANT2 = 16,
    SOUND_PANT3 = 17,
    SOUND_WATER_SFX = 18,
    SOUND_RURUFF2 = 19,
    SOUND_MELODY = 20,
    SOUND_LICK_ONCE = 21,
    SOUND_SNEEZE = 22,
    SOUND_SNIFF = 23,
    SOUND_SNORE = 24,
    SOUND_PEE_SFX = 25, // long
    SOUND_WHINE = 26,
    SOUND_WHINE2 = 27,
    SOUND_RURUFF3 = 28,
    SOUND_YAWN_RUFF = 29,
    SOUND_SCREECH = 30
}; /*SYSROM COMPATIBLE DO NOT ALTER*/

////////// ////////// ////////// ////////// //////////
// low level access to sunplus chip - ADVANCED

#define _ic_delay_sunplus() ic_sleep(15)
	// put this delay in before using sunplus chip

void ADECL _ic_send_to_daughter_sync(byte data);
    // direct output to sunplus chip

////////// ////////// ////////// ////////// //////////
