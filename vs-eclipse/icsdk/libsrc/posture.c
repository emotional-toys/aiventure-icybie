// ICSDK2
// posture control (rarely used directly)

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Handles switching the AIBO from one posture to another

/////////////////////////////////////////////////////////
// DATA

static byte __near g_currentPosture; // (= 0 = POSTURE_STAND)

#ifdef ICL_VERBOSE
static const char* const g_postureNames[NUM_POSTURE] =
{
    "STAND",
    "SIT",
    "PLAYDEAD",
    "HEAD",
    "RECOVER1",
    "RECOVER2",
    "LIE"
};
#endif


typedef struct
{
    word motions[3];    // up to 3 different motions (0=>n/a)
} POSTURE_CHG_INFO;

static const POSTURE_CHG_INFO g_postureChgInfo[NUM_POSTURE*NUM_POSTURE] =
{
        // indexed by: fromPostureIndex*7+toPostureIndex
	// from posture 0 (standing)
    { 0 },
	{ 116 },
	{ 1080, 1081 },
	{ 1082 },
	{ 1080 },
	{ 1083 },
	{ 1084 },
    // from posture 1 (sitting)
	{ 1000 },
	{ 0 },
	{ 1000, 1080, 1081 },
	{ 1000, 1082 },
	{ 1000, 1080 },
	{ 1000, 1083 },
	{ 1000, 1084 },
    // from posture 2
	{ 1085 },
	{ 1085, 116 },
	{ 0 },
	{ 1085, 1082 },
	{ 1085, 1080 },
	{ 1085, 1083 },
	{ 1085, 1084 },
    // from posture 3
	{ 1078 },
	{ 1086, 1087 },
	{ 1081 },
	{ 0 },
	{ 1088 },
	{ 1088 },
	{ 96 },
    // from posture 4
	{ 114 },
	{ 1086, 1087 },
	{ 1081 },
	{ 1086, 1089 },
	{ 0 },
	{ 1086 },
	{ 1086 },
    // from posture 5
	{ 115 },
	{ 1090, 1087 },
	{ 1091 },
	{ 1090, 1089 },
	{ 1090, 1092 },
	{ 0 },
	{ 1090 },
    // from posture 6
	{ 1093 },
	{ 1087 },
	{ 1092, 1081 },
	{ 1089 },
	{ 1092 },
	{ 1079 },
	{ 0 }
};

/////////////////////////////////////////////////////////
// PUBLIC

byte ADECL ic_get_current_posture(byte posture)
{
    return g_currentPosture;
}

bool ADECL ic_move_to_posture(byte newPosture)
{
	POSTURE_CHG_INFO const* pChgInfo;
    byte i;

    ic_assert(newPosture < NUM_POSTURE);
    if (newPosture == g_currentPosture)
        return true;    // already there (or so we think)
#ifdef ICL_VERBOSE
    printf("ic_move_to_posture: from %s to %s\n",
        g_postureNames[g_currentPosture], g_postureNames[newPosture]);
#endif

    // lookup in table what we need to do
	pChgInfo = &g_postureChgInfo[g_currentPosture*NUM_POSTURE + newPosture];
    for (i = 0; i < 3; i++)
    {
        word motion = pChgInfo->motions[i];
        if (motion == 0)
            break;
        if (!ic_perform_motion(motion))
            return false;
    }
    return true;
}

/////////////////////////////////////////////////////////



