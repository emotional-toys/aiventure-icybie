// simple program with custom audio
// 'base' your own program off of this one instead of 'hello'

#include "ic.h"

/////////////////////////////////////////////////////


// simple custom sound
EXTERN_AUDIO(MEOW);

void main()
{
    byte ileg;

    // move legs to center position
	for (ileg = 0; ileg < NUM_LEGS; ileg++)
		ic_move_leg(ileg, 128);

	ic_show_eyes_green();

	ic_sleep(1*1000);

	ic_play_my_sound(MEOW);
	ic_show_eyes_red();
	ic_sleep(10*1000);
}

/////////////////////////////////////////////////////
