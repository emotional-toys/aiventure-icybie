// very primitive personality
// DalekCybie

#include "ic.h"

/////////////////////////////////////////////////////


// lots of custom sounds
EXTERN_AUDIO(EMRGNCY);
EXTERN_AUDIO(EXTERM2);
EXTERN_AUDIO(EXTRMHUM);
EXTERN_AUDIO(FOLLOWU);
EXTERN_AUDIO(HALT);
EXTERN_AUDIO(IAMREADY);
EXTERN_AUDIO(IOBEY);
EXTERN_AUDIO(IOBEY2);
EXTERN_AUDIO(NOQUEST);
EXTERN_AUDIO(OBYORDRS);
EXTERN_AUDIO(RANT);
EXTERN_AUDIO(SERVANT);
EXTERN_AUDIO(SERVEU);
EXTERN_AUDIO(STAY);
EXTERN_AUDIO(WHYKILL);
EXTERN_AUDIO(YESMAST);
EXTERN_AUDIO(EXTNOW);
EXTERN_AUDIO(FUZZY);

void main()
{
    byte ileg;
    word sensorLast = 0;

    // move legs to center position
	for (ileg = 0; ileg < NUM_LEGS; ileg++)
		ic_move_leg(ileg, 128);

	ic_show_eyes_green();

	ic_sleep(1*1000);
	ic_play_my_sound(IAMREADY);
    ic_sleep(200);

    while (1)
    {
        // main loop

        // get sensor values
        word sensorNow = ic_get_sensors();
        word sensorChange = sensorNow & ~sensorLast;
             // low to high transitions
        word sensorChangeNot = ~sensorNow & sensorLast;
             // high to low transitions
        byte remoteButton = ic_get_last_remote_button();

        sensorLast = sensorNow; // for next time through the loop

        // now act on sensor inputs

        if (sensorChange & SENSOR_HEAD_PRESS)
        {
	        byte r;

            // daleks like to be petted
            printf("GOOD\n");
			ic_show_eyes_green();

            r = rand() % 6;
            if (r == 0)
	            ic_play_my_sound(IOBEY);
            else if (r == 1)
				ic_play_my_sound(IOBEY2);
            else if (r == 2)
				ic_play_my_sound(SERVANT);
            else if (r == 3)
				ic_play_my_sound(SERVEU);
            else if (r == 4)
				ic_play_my_sound(FOLLOWU);
            else if (r == 5)
				ic_play_my_sound(YESMAST);
        }

        if (sensorChange & SENSOR_BACK_PRESS)
        {
	        byte r;

            // pressing their back just gets them mad
            printf("BAD\n");
			ic_show_eyes_red();

            r = rand() % 2;
            if (r == 0)
				ic_play_my_sound(EXTERM2);
            else if (r == 1)
				ic_play_my_sound(EXTRMHUM);
            else if (r == 3)
				ic_play_my_sound(EXTNOW);
        }

        if (sensorNow & SENSOR_TILT_MASK)
        {
            // currently in tilt state
			ic_show_eyes_red();
			ic_play_my_sound(EMRGNCY);
			ic_sleep(3*1000); // and keep repeating it
        }

        if (sensorChangeNot & SENSOR_NOSE)
        {
            // NOSE bit goes low -> saw something
	        byte r;
			ic_show_eyes_yellow();
            r = rand() % 2;
            if (r == 0)
				ic_play_my_sound(HALT);
            else
				ic_play_my_sound(STAY);
        }

        if (remoteButton == 1)
        {
			ic_play_my_sound(RANT);
        }
        else if (remoteButton == 2)
        {
			ic_play_my_sound(WHYKILL); // dalek reflects on humanity
        }
        else if (remoteButton == 3)
        {
			ic_play_my_sound(FUZZY);
        }
        
    }
}

#if 0
    // not yet used
	ic_play_my_sound(NOQUEST);
	ic_play_my_sound(OBYORDRS);
#endif


/////////////////////////////////////////////////////
