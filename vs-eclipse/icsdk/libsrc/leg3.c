// ICSDK2
// leg calibration

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
//  NOT YET IMPLEMENTED - just a placeholder

/////////////////////////////////////////////////////////

#ifdef LATER
bool ADECL ic_calibrate_legs(IC_CALIBRATE_METHOD method)
{
    ic_assert(method == ICCALIBRATE_RAW);   // only one supported now

    if (method == ICCALIBRATE_STANDARD)
    {
        ic_play_action(&action_RolloverRight);

        // check for tilt left 3x
        if ((ic_get_sensors() & SENSOR_TILT_LEFT) == 0)
            return false;
        // do this for all legs...
    }

    if (!CalibrateLeg(...)

    return true;
}

#endif
