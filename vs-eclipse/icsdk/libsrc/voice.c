// ICSDK2
// voice interface

#include "icpriv.h"

#include "ircodes.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// The RSC-300 voice recognition chip in ICybie's head is interfaced
//  to the main CPU using the IR protocol and special codes
// See 'ir2.c' for handling of voice command notifications

/////////////////////////////////////////////////////////
// PUBLIC

bool ADECL ic_listen_for_voice_command()
{
    if (!ic_send_ir_code(TXIR_VOICE, IRCODE_VOICEOP_CLEAR))
        return false;
	while (ic_is_sending_ir_code())
	    ic_sleep(2);
    if (!ic_send_ir_code(TXIR_VOICE, IRCODE_VOICEOP_LISTEN))
        return false;
    return true;
}

byte ADECL ic_check_voice_recorded(byte vcmd)
{
	int timeout = 15000; //LATER: timeout could be smarter

    ic_assert(vcmd < NUM_VOICE_CMDS);
	ic_get_last_ir_code();  // purge

    if (!ic_send_ir_code(TXIR_VOICE, IRCODE_VOICEOP_ISTRAINED(vcmd)))
        return 0xFF;    // transmit error
	while (timeout--)
	{
		word reply = ic_get_last_voice_command();
        if (reply == IRCODE_VOICE_TRAIN)
            return 1; // Yes
        else if (reply == IRCODE_VOICE_NOTRAIN)
            return 0; // No
		else if (reply != 0)
            return 0xFF;    // receive error
    }
    return 0xFF; // timed out
}


/////////////////////////////////////////////////////////
