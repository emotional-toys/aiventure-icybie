// ICSDK2 - ICybie Library
// Main header (ic.h)

// RELEASE VERSION 2.0 - Cartridge Personalities
// REVIEW: ALPHA RELEASE work in progress

////////// ////////// ////////// ////////// //////////
// customizable

#define ICL_DEBUG // debug version
#define ICL_VERBOSE // verbose library calls

////////// ////////// ////////// ////////// //////////
// all official functions start with "ic_"
// all Super ICybie only functions start with "sic_"
// private functions/data start with "_ic_"
// all use ADECL (register) calling convention

////////// ////////// ////////// ////////// //////////

#include "ic_types.h" // basic types, macros

#include "ic_sound.h" // sound and LED playback
#include "ic_sensor.h" // sensor features

#include "ic_ir.h" // IR features
#include "ic_storage.h" // E2PROM routines

#include "ic_motor.h" // low level motors and servos
#include "ic_motion.h" // mid level legs and motion features
#include "ic_action.h" // high level actions
#include "ic_action2.h" // 220 standard actions

#include "ic_sysrom.h" // defaults (requires known system ROM)
#include "ic_serial.h" // serial out/in for SIC

////////// ////////// ////////// ////////// //////////
// other general purpose library calls

void ADECL ic_sleep(int approxms);
void ADECL ic_shutdown();

long ADECL ic_getclock8000();   // 8000 counts per second
long ADECL ic_getclock1();      // 1 count per second

#define	RAND_MAX	0x7fffU
void srand(unsigned int seed);
int rand(void);

void ADECL ic_memcpy(byte* destPtr, const byte* srcPtr, int byteCount);
void ADECL ic_memset(byte* destPtr, byte val, int byteCount);


////////// ////////// ////////// ////////// //////////
// other features are incomplete - see README.TXT
// other headers not included by "ic.h"
// for ADVANCED programmers only

// ic_main0.h - for custom startup - replace main0()
// ic_sfr.h - direct access to hardware

////////// ////////// ////////// ////////// //////////
