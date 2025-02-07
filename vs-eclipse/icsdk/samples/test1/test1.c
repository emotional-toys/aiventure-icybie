// simple test for new library structure

#include <ic.h>

/////////////////////////////////////////////////////

// my sounds
EXTERN_AUDIO(BARK);
EXTERN_AUDIO(MEOW);
EXTERN_AUDIO(BEEP);
EXTERN_AUDIO(WHINE);
EXTERN_AUDIO(GOODFUN);

// forward declarations
void DoPollSensors();
void DoDumpVoiceCommands();
void DoVoiceCommands();
bool DoStdAction();
bool DoExtraAction();



/////////////////////////////////////////////////////
// REVIEW: multi-tiered initialization
// REVIEW: clean up naming convention

typedef struct
{
    // loaded from E2PROM
	word legoffset[NUM_LEGS];
	byte leggain[NUM_LEGS];
} LEG_CALIBRATION_INFO;

extern LEG_CALIBRATION_INFO _ic_lci;

void main()
{
    printf("main: Test program running\n");

    printf("oh what jolly good fun");
    ic_play_my_sound(GOODFUN);
	ic_sleep(100); // short gap
    printf("!\n");

    // Super ICybie input
    while (1)
    {
        byte chCmd;
        printf("(press a key)");
		chCmd = sic_get_serial_byte();
        printf(" -- %c\n", chCmd);
        if (chCmd == '?')
        {
            printf("\tCommands:\n");
            printf("\t\t? = this help screen\n");
            printf("\t\tP,p = poll sensors\n");
            printf("\t\tv = listen for (one) voice command\n");
            printf("\tDump and bulk perform commands:\n");
            printf("\t\tE = play all eyes\n");
            printf("\t\tS,s = play sounds ('S'=>System, 's'=>custom)\n");
            printf("\t\tL,l = dump current leg positions\n");
            printf("\t\tV = dump voice command info\n");
            printf("\tSpecific perform commands:\n");
            printf("\t\tC = center legs\n");
            printf("\t\tc = stop legs\n");
            printf("\t\ta = play standard action\n");
            printf("\t\tA = play extra action\n");
            printf("\tExit commands\n");
            printf("\t\tQ = Quit and stop\n");
            printf("\t------------\n");
            printf("\tVoice last= %d\n",  ic_get_last_voice_command());
            printf("\tIR remote last= %d\n",  ic_get_last_remote_button());
            printf("\tIR last = $%x\n",  ic_get_last_ir_code());
            printf("\tIR quiet (now) = %d\n", ic_get_IR_quiet_level());
            printf("\tbatt = %d, light = %d\n",
				ic_get_battery_level(), ic_get_light_level());
        }
        else if (chCmd == 'E')
        {
            byte i;
            for (i = 0x20; i <= 0x2F; i++)
            {
	            printf("setting eyes $%x\n", i);
	            ic_show_eyes(i);
                ic_sleep(1000); // 1 second gap
            }
        }
        else if (chCmd == 'S')
        {
            byte i;
            for (i = 1; i <= NUM_SYSROM_SOUNDS; i++)
            {
                printf("playing system sound %d...", i);
                ic_show_eyes_green();
                ic_play_sysrom_sound(i);
                ic_sleep(100);  // short gap
                ic_show_eyes_off();
                printf("done\n");
                ic_sleep(500);  // short gap
            }
        }
        else if (chCmd == 's')
        {
            printf("playing my sounds\n");
		    ic_play_my_sound(BARK);
			ic_sleep(1000); // 1 second gap
		    ic_play_my_sound(MEOW);
			ic_sleep(1000); // 1 second gap
		    ic_play_my_sound(BEEP);
			ic_sleep(1000); // 1 second gap
		    ic_play_my_sound(WHINE);
			ic_sleep(1000); // 1 second gap
        }
        else if (chCmd == 'l' || chCmd == 'L')
        {
            static byte oldlegs[12];
            byte i;
            printf("LEGS\n");
            for (i = 0; i < 12; i++)
            {
                byte now = ic_get_leg_position(i);
                int diff = (int)now - (int)oldlegs[i];
                if (chCmd == 'l')
					printf("%d ", diff);
                else
					printf("%d ", now);
                oldlegs[i] = now;
                if ((i % 3) == 2)
                    printf("\n");
            }
	    }
        else if (chCmd == 'V')
        {
            DoDumpVoiceCommands();
        }
        else if (chCmd == 'P' || chCmd == 'p')
        {
			DoPollSensors();
        }
        else if (chCmd == 'v')
        {
            DoVoiceCommands();
        }
        else if (chCmd == 'C')
        {
            byte ileg;
	        printf("Centering Legs\n");
	        for (ileg = 0; ileg < NUM_LEGS; ileg++)
				ic_move_leg(ileg, 128);
        }
        else if (chCmd == 'c')
        {
            byte ileg;
	        printf("Stopping Legs\n");
	        for (ileg = 0; ileg < NUM_LEGS; ileg++)
				ic_stop_leg(ileg);
	    }
        else if (chCmd == 'a')
        {
            while (DoStdAction())
                ;
        }
        else if (chCmd == 'A')
        {
            while (DoExtraAction())
                ;
        }
        else if (chCmd == 'Q')
        {
            printf("Goodbye\n\032"); // CTRL-Z
            return;
        }
        else
        {
            printf("\tUNKNOWN COMMAND '%c' - press '?' for help\n", chCmd);
        }
    }
}

/////////////////////////////////////////////////////
// Dump routines

void DoPollSensors()
{
    printf("Polling ");
    //BLOCK: common sensors
    {
		word sensors = ic_get_sensors();
	    printf("Sensors = $%x\n", sensors);
	    if (sensors & SENSOR_TILT_MASK)
	    {
	        printf("\tFallen on his");
	        if (sensors & SENSOR_TILT_BACK)
	            printf(" back(upside-down)");
	        if (sensors & SENSOR_TILT_FACE)
	            printf(" face");
	        if (sensors & SENSOR_TILT_BUTT)
	            printf(" butt");
	        if (sensors & SENSOR_TILT_RIGHT)
	            printf(" right side");
	        if (sensors & SENSOR_TILT_LEFT)
	            printf(" left side");
	        printf("\n");
	    }
	    if (!(sensors & SENSOR_NOSE))
	        printf("\tNose detect\n"); // negative logic
	    if (sensors & SENSOR_HEAD_PRESS)
	        printf("\tHead press\n");
	    if (sensors & SENSOR_BACK_PRESS)
	        printf("\tBack press\n");
	    if (!(sensors & SENSOR_BATTERY_DOOR_CLOSED))
	        printf("\tBattery door open\n"); // negative logic
	    if (sensors & SENSOR_MYSTERY)
	        printf("\tUNKNOWN MYSTERY SENSOR\n");
    }
}



void DoDumpVoiceCommands()
{
    byte vcmd;

    printf("Voice command info:\n");
    for (vcmd = 0; vcmd < NUM_VOICE_CMDS; vcmd++)
    {
        byte result;
        printf("\tCmd#%d is ", 1+vcmd); // 1->8 like in manual

        result = ic_check_voice_recorded(vcmd);
        if (result != 0 && result != 1)
        {
            // just one retry
            ic_sleep(50);
	        result = ic_check_voice_recorded(vcmd);
        }
        if (result == 1)
            printf("trained\n");
        else if (result == 0)
            printf("not trained\n");
        else
            printf("unknown because of error (%d)\n", result);

    }
}


void DoVoiceCommands()
{
    byte reply;
    if (ic_listen_for_voice_command())
	    printf("Listening for voice commands\n");
    else
        printf("send error - listen\n");
}



/////////////////////////////////////////////////////

#ifdef LATER
// provide IR transmit feature

bool DoTest3()
{
    int val = sic_input("Enter IR value");
    if (val == 666)
        return false;
    printf("Sending $%x\n", val);

    if (val >= 0xB0 && val <= 0xFF)
    {
        printf("SENDING TO VOICE CHIP!\n");
	    if (!ic_send_ir_code(TXIR_VOICE, val))
	        printf("TX not ready\n");
    }
    else
    {
	    if (!ic_send_ir_code(TXIR_TOP, val))
	        printf("TX not ready\n");
    }

    return true;
}
#endif



///////////////////////////////////////////////////

static RAW_ACTION const* const std_actions[] =
{
	&action_001,
	&action_002,
	&action_003,
	&action_004,
	&action_005,
	&action_006,
	&action_007,
	&action_008,
	&action_009,
	&action_010,
	&action_011,
	&action_012,
	&action_013,
	&action_014,
	&action_015,
	&action_016,
	&action_017,
	&action_018,
	&action_019,
	&action_020,
	&action_021,
	&action_022,
	&action_023,
	&action_024,
	&action_025,
	&action_026,
	&action_027,
	&action_028,
	&action_029,
	&action_030,
	&action_031,
	&action_032,
	&action_033,
	&action_034,
	&action_035,
	&action_036,
	&action_037,
	&action_038,
	&action_039,
	&action_040,
	&action_041,
	&action_042,
	&action_043,
	&action_044,
	&action_045,
	&action_046,
	&action_047,
	&action_048,
	&action_049,
	&action_050,
	&action_051,
	&action_052,
	&action_053,
	&action_054,
	&action_055,
	&action_056,
	&action_057,
	&action_058,
	&action_059,
	&action_060,
	&action_061,
	&action_062,
	&action_063,
	&action_064,
	&action_065,
	&action_066,
	&action_067,
	&action_068,
	&action_069,
	&action_070,
	&action_071,
	&action_072,
	&action_073,
	&action_074,
	&action_075,
	&action_076,
	&action_077,
	&action_078,
	&action_079,
	&action_080,
	&action_081,
	&action_082,
	&action_083,
	&action_084,
	&action_085,
	&action_086,
	&action_087,
	&action_088,
	&action_089,
	&action_090,
	&action_091,
	&action_092,
	&action_093,
	&action_094,
	&action_095,
	&action_096,
	&action_097,
	&action_098,
	&action_099,
	&action_100,
	&action_101,
	&action_102,
	&action_103,
	&action_104,
	&action_105,
	&action_106,
	&action_107,
	&action_108,
	&action_109,
	&action_110,
	&action_111,
	&action_112,
	&action_113,
	&action_114,
	&action_115,
	&action_116,
	&action_117,
	&action_118,
	&action_119,
	&action_120,
	&action_121,
	&action_122,
	&action_123,
	&action_124,
	&action_125,
	&action_126,
	&action_127,
	&action_128,
	&action_129,
	&action_130,
	&action_131,
	&action_132,
	&action_133,
	&action_134,
	&action_135,
	&action_136,
	&action_137,
	&action_138,
	&action_139,
	&action_140,
	&action_141,
	&action_142,
	&action_143,
	&action_144,
	&action_145,
	&action_146,
	&action_147,
	&action_148,
	&action_149,
	&action_150,
	&action_151,
	&action_152,
	&action_153,
	&action_154,
	&action_155,
	&action_156,
	&action_157,
	&action_158,
	&action_159,
	&action_160,
	&action_161,
	&action_162,
	&action_163,
	&action_164,
	&action_165,
	&action_166,
	&action_167,
	&action_168,
	&action_169,
	&action_170,
	&action_171,
	&action_172,
	&action_173,
	&action_174,
	&action_175,
	&action_176,
	&action_177,
	&action_178,
	&action_179,
	&action_180,
	&action_181,
	&action_182,
	&action_183,
	&action_184,
	&action_185,
	&action_186,
	&action_187,
	&action_188,
	&action_189,
	&action_190,
	&action_191,
	&action_192,
	&action_193,
	&action_194,
	&action_195,
	&action_196,
	&action_197,
	&action_198,
	&action_199,
	&action_200,
	NULL, // &action_201, - weird format
	&action_202,
	&action_203,
	&action_204,
	&action_205,
	&action_206,
	&action_207,
	&action_208,
	&action_209,
};


bool DoStdAction()
{
    int value = sic_input("Enter standard action number (1->209)");
    if (value < 1 || value > 209)
    {
        printf("ABORT!\n");
        return false;
    }

    if (std_actions[value-1] == NULL)
    {
        printf("ACTION_201 is bogus\n");
        return true;
    }
    if (!ic_perform_action(std_actions[value-1]))
        printf("ic_perform_action returned false!!!\n");

    return true;
}

static RAW_ACTION const* const extra_actions[] =
{
&actionold_176,
&actionold_178,
&actionold_181,
&actionold_202,
&actionold_206,
&actionold_207,
&actionold_208,
&actionold_209,
&actionold_210,
&actionold_211,
&actionold_212,
&actionold_213,
&actionold_214,
&actionnew_01,
&actionnew_02,
&actionnew_03,
&actionnew_04,
&actionnew_05,
&actionnew_06,
&actionnew_07,
&actionnew_08,
&actionnew_09,
&actionnew_10,
&actionnew_11,
&actionnew_12,
&actionnew_13,
&actionnew_14,
&actionnew_15,
&actionnew_16,
&actionnew_17,
&actionnew_18,
&actionnew_19,
&actionnew_20,
&actionnew_21,
&actionnew_22,
&actionnew_23,
&actionnew_24,
&actionnew_25,
&actionnew_26,
&actionnew_27,
&actionnew_28,
&actionnew_29,
&actionnew_30,
&actionnew_31,
&actionnew_32,
&actionnew_33,
};

bool DoExtraAction()
{
    int value = sic_input("Enter extra action number (1->46)");
    if (value < 1 || value > 46)
    {
        printf("ABORT!\n");
        return false;
    }

    if (!ic_perform_action(extra_actions[value-1]))
        printf("ic_perform_action returned false!!!\n");

    return true;
}

///////////////////////////////////////////////////




