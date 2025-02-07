// ICSDK2
// action playback

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// This is a simple routine to decode the RAW_ACTION format
//  and play back a list of motions

// REVIEW: NOT COMPLETE - some fields ignored (not sure if important or not)

/////////////////////////////////////////////////////////


bool ADECL ic_perform_action(RAW_ACTION const* action)
{
    byte repeatCount = action->repeatCount;
    ic_assert(repeatCount >= 1 && repeatCount <= 10);
        // sanity check

    while (repeatCount--)
    {
	    byte partCount = action->partCount;
	    RAW_ACTION_PART const* part = action->parts;

        ic_assert(partCount > 0 && partCount < 100); // sanity check
        while (partCount--)
        {
            byte posture = (part->posture >> 5) - 1;
		    byte partRepeatCount = part->repeatCount;
		    ic_assert(partRepeatCount >= 1 && partRepeatCount <= 10);
            ic_assert(part->zero == 0);
            ic_assert(posture >= 0 && posture < NUM_POSTURE);

            if (!ic_move_to_posture(posture))
                return false;

            while (partRepeatCount--)
            {
	            if (!ic_perform_motion(part->motionIndex))
	                return false;
            }
#ifdef LATER
            // ? other ?
#endif

            part++;
        }
    }
    return true;
}

//////////////////////////////////////////////////////
