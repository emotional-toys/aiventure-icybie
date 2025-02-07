// ICSDK2
// motion playback

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
    //work in progress!!!
	//REVIEW: motion playback is very primitive, and just for legs & sounds

/////////////////////////////////////////////////////////
// DATA STRUCTURES

//REVIEW: clean this up

// NOTE: motion frame data comes in two formats:
//  RAW_MOTION_FRAME is the format used in the system ROM (19 bytes)
//  MOTION_FRAME is the the same structure (20 bytes after padding)
//  EXPANDED_MOTION_FRAME is what we use internally
//  If you are planning on synthesizing your own data, use EXPANDED_MOTION_FRAME

typedef struct
{
    // strange packed structure used in system rom
    byte legmot[NUM_LEGS];
        // bits0->5 are "motPos" (for hips scale by 2)
        //      0 => don't move leg
        //      otherwise subtract 1, scale hips, then use as motPos
        // bit6=> movement speed (1=>fast, 0=>not as fast)
        // bit7=> ???sign???
        // if bEnd.bit? => legmot[11] is special for sound index
    byte bNeck;
    byte bHead;
    byte bMouth;
    byte bTail;
    byte bLED;

    byte bOther;
    
    byte bEnd;
        // bit6 => legmot[11] is really sound index (1->30)
        // bits0->4 = frame duration
} MOTION_FRAME; /*SYSROM COMPATIBLE DO NOT ALTER*/

typedef struct
{
    byte duration;
    byte iSound; // or 0 if no sound, 1->30 standard, 50->? custom
    byte leg_position[NUM_LEGS]; // 0 if no change
    byte leg_speed[NUM_LEGS]; // 0 if no change
#ifdef REVIEW_MISSING // HEAD/NECK ETC
    head/neck/mouth/tail...
    LED playback...
#endif
} EXPANDED_MOTION_FRAME;

/////////////////////////////////////////////////////////
// IMPLEMENTATION: simple 1/64sec timer

static volatile int __near s_waitTimer64; // SLEAZY FOR NOW
static void timerproc_motion() // TIMER_PROC
{
    if (s_waitTimer64 > 0)
		s_waitTimer64--;
}

void ADECL ic0_init_motion()
{
    // frame duration is 1/64 sec
	_ic_settimerproc64(timerproc_motion);
}


static void ADECL _ic_sleep64(int ticks) // SLEAZY FOR NOW
{
    s_waitTimer64 = ticks;
    while (s_waitTimer64 > 0)
        ;
}

/////////////////////////////////////////////////////////
// Convert from RAW to EXPANDED motion frame format

static void ADECL _ic_expand_motion_format(EXPANDED_MOTION_FRAME* pxmf,
    MOTION_FRAME const* prmf)
{
	byte ileg;

	pxmf->iSound = (prmf->bEnd & 0x40) ? prmf->legmot[11] : 0;
    pxmf->duration = (prmf->bEnd & 0xF) * 10;

	ic_assert(pxmf->iSound >= 0 && pxmf->iSound <= 30);
        // REVIEW: expand for custom sounds

    for (ileg = 0; ileg < NUM_LEGS; ileg++)
    {
        byte legmot = prmf->legmot[ileg];
        byte motpos = legmot & 0x3F;

        if (motpos == 0 || (ileg == 11 && pxmf->iSound != 0))
        {
            // no movement/change
            pxmf->leg_position[ileg] = 0;
            pxmf->leg_speed[ileg] = 0;
        }
        else
        {
	        motpos--;
	        if ((ileg % 3) == 1)
	            motpos = motpos/2; // reduce hips
	        pxmf->leg_position[ileg] = ic_scale_motion_leg(ileg, motpos);
	        pxmf->leg_speed[ileg] = (legmot & 0x40) ? 10 : 8;
        }
    }
}

static bool ADECL _ic_perform_motion_frame(EXPANDED_MOTION_FRAME const* pxmf,
    bool bWaitDuration)
{
    byte ileg;

#ifdef ICL_VERBOSE
    printf("_ic_perform_motion_frame: (dur=%d,snd=%d)\n", pxmf->duration,
            pxmf->iSound);
#endif

    for (ileg = 0; ileg < NUM_LEGS; ileg++)
    {
        byte pos = pxmf->leg_position[ileg];
        if (pos != 0)
			ic_move_leg(ileg, pos);
//REVIEW: pxmf->leg_speed[ileg] not used!
    }
#ifdef REVIEW_MISSING
    // do something with head/tail
#endif
    if (pxmf->iSound != 0)
		ic_play_sysrom_sound(pxmf->iSound);  // start async playback

    if (bWaitDuration)
    {
        if (pxmf->duration == 0)
        {
            // wait until
            while (1)
            {
                word counts = ic_count_busy_legs();
#ifdef REVIEW_LATER
                if (counts & 0xF000)
                    do something about jammed limbs
#endif
                if ((counts & 0x0F00) == 0)
                    break;  // not moving fast anymore
            }
        }
        else
        {
			_ic_sleep64(pxmf->duration);
        }
    }

#ifdef REVIEW_LATER
    // jam detect?
#else
    return true;
#endif
}

/////////////////////////////////////////////////////////
// PUBLIC: do the real work

bool ADECL ic_perform_motion_raw(RAW_MOTION_INFO const* motionPtr)
{
    byte frameCount = motionPtr->frameCount;
    RAW_MOTION_FRAME const* prmf = (RAW_MOTION_FRAME const*)&motionPtr->frames;
    ic_assert(sizeof(RAW_MOTION_FRAME) == 19);

    while (frameCount--)
    {
        EXPANDED_MOTION_FRAME xmf;
        _ic_expand_motion_format(&xmf, (MOTION_FRAME*)prmf);

#if 1
        //BLOCK: report
        {
            byte ileg;
	        MOTION_FRAME const* prmf2 = (MOTION_FRAME*)prmf;
            printf("MOT ");
            for (ileg = 0; ileg < 12; ileg++)
                printf("%d ", prmf2->legmot[ileg] & 0x3F);
            printf("=> ");
            for (ileg = 0; ileg < 12; ileg++)
                printf("%d ", xmf.leg_position[ileg]);
            printf("\n");
        }
#endif

        // now play the expanded motion
        if (!_ic_perform_motion_frame(&xmf, true))
            return false;
        prmf++;
    }
    return true;
}

/////////////////////////////////////////////////////////
