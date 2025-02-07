// ICSDK2 - ICybie Library
// Action playback

//////////////////////////////////////////////////////
// Action playback (SYSROM compatible structures)

typedef struct
{
    word motionIndex;   // motion_index
    byte repeatCount;   // motion repeat
    byte zero;          // always zero
    byte other;         // used for ?
    byte posture;       // upper 3 bits for posture+1 (1->7)
} RAW_ACTION_PART; /*SEMI-SYSROM COMPATIBLE DO NOT ALTER*/

#define ACTION_PART(mot, rep, other, posture) \
    { mot, rep, 0, other, (posture+1)<<5 }

typedef struct
{
    byte partCount;
    byte repeatCount;
    RAW_ACTION_PART parts[/*partCount*/];
} RAW_ACTION; /*SYSROM COMPATIBLE DO NOT ALTER*/

//////////////////////////////////////////////////////
// play a whole action (some with sound/LEDs)

bool ADECL ic_perform_action(RAW_ACTION const* actionPtr);
	// NOTE: most actions start and end in the STANDing position

//////////////////////////////////////////////////////
// see "ic_action2.h" for standard actions
//////////////////////////////////////////////////////
