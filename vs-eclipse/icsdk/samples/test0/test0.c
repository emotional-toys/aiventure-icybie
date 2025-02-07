// low level tests

#include <ic.h>

// eat up some RAM - zero initialized
byte data1[3000];

// 'const' data is stored in ROM
const byte ro_data1[1024] = { 1, 2, 3 };

// non-const data is stored in RAM
//  variables can be initialized to non-zero values
byte test1 = 123;
int test2 = 32000;

/////////////////////////////////////////////////////

// forward declarations
bool DoMoveLegs(bool bAbstract);
void DoIRObstacleProbeSlow();
void DoIRObstacleProbeFast();
void DoIRObstacleProbeDetail();
void DoOvershoot();


/////////////////////////////////////////////////////
void main()
{
    printf("test0: low level test program\n");

    //be sure global variables are initialized correctly
    {
        // RAM data
	    ic_assert(test1 == 123);
	    test1--;    // must be in RAM
	    ic_assert(test1 == 122);
		ic_assert(test2 == 32000);

        // ROM data
        ic_assert(ro_data1[0] == 1);
        ic_assert(ro_data1[2] == 3);
        ic_assert(ro_data1[1023] == 0); // end of array
        *(byte*)&ro_data1[0] = 55; // store to ROM, does nothing
        ic_assert(ro_data1[0] == 1); // not changed
    }

    printf("SysRom version %d\n", ic_sysrominfo.ver);

    // check for CARTRIDGE present
    {
	    word w1 = *(word*)0x200000;
	    word w2 = *(word*)0x20FFFE;

	    if (w1 == 0xBF0A || w1 == 0xA05F || w2 == 0xABCD)
	    {
	        printf("Bootable Cartridge Detected!\n");
	        // printf("\t$%x $%x\n", w1, w2);
	    }
	    else
	    {
	        printf("ERROR: No cartridge detected\n");
            // this should be impossible for the current version
	    }
    }

    // stack info - very implementation specific
    {
        const byte* pb = (const byte*)0x1501;
        ic_assert(*(const byte*)0x1EFF == 0x55); // marker
        while (*pb == 0)
            pb++;
        printf("Stack: %d of %d bytes used so far\n",
            0x1EFF-(int)(long)pb, 0x1EFF-0x1501);
    }

    // terminal input
    while (1)
    {
        byte chCmd;
        printf("(press a key)");
		chCmd = sic_get_serial_byte();
        printf(" -- %c\n", chCmd);
        if (chCmd == '?')
        {
            printf("\ttest0 commands:\n");
            printf("\t\t? = this help screen\n");
            printf("\tIR commands\n");
            printf("\t\ti = IR obstacle mode (slow)\n");
            printf("\t\tI = IR obstacle mode (fast)\n");
            printf("\t\tj = IR obstacle counts\n");
            printf("\tLeg commands\n");
            printf("\t\tl = dump change in leg position\n");
            printf("\t\tL = dump absolute leg positions\n");
            printf("\t\tc = stop leg motors\n");
            printf("\t\tC = center leg motors\n");
            printf("\tLeg tests\n");
            printf("\t\tm = move leg to a specific position\n");
            printf("\t\tM = move leg to an abstract position\n");
            printf("\t\tH = oversHoot test\n");
            printf("\tExit commands\n");
            printf("\t\tQ = Quit and stop\n");
        }
        else if (chCmd == 'i')
        {
            DoIRObstacleProbeSlow();
        }
        else if (chCmd == 'I')
        {
            DoIRObstacleProbeFast();
        }
        else if (chCmd == 'j')
        {
            DoIRObstacleProbeDetail();
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
        else if (chCmd == 'm')
        {
            while (DoMoveLegs(false))
                ;
        }
        else if (chCmd == 'M')
        {
            while (DoMoveLegs(true))
                ;
        }
        else if (chCmd == 'H')
        {
            DoOvershoot();
        }
        else if (chCmd == 'Q')
        {
            printf("Goodbye\n\032"); // CTRL-Z
            return;
        }
        else
        {
            printf("\tUNKNOWN COMMAND '%c'\n", chCmd);
        }
    }
}

/////////////////////////////////////////////////////
// Leg walk

bool DoMoveLegs(bool bAbstract)
{
    byte count;
    byte ileg;
    printf("%s leg movement", bAbstract ? "ABSTRACT" : "RAW");
    ileg = (byte)sic_input(" - enter ileg index (0->15, or 99 to exit)");

    if (ileg > 12)
    {
        printf("move leg tests completed\n");
        return false;
    }

    while (1)
    {
	    int target;
	
		printf("leg %d = now at %d\n", ileg, ic_get_leg_position(ileg));
	    target = sic_input("Enter new position (or 999 to exit)");
        if (target > 255)
            return true;
	
        if (bAbstract)
        {
            printf("\tcalc: %d ->", target);
            target = ic_scale_motion_leg(ileg, (byte)target);
            printf(" %d\n", target);
            ic_assert(target >= 0 && target <= 255);
        }
	    printf("ic_move_leg(%d, %d, %d)\n", ileg, target);
		ic_move_leg(ileg, (byte)target);
	
	    count = 20; // no more than 2 seconds
	    while (1)
	    {
	        IC_MOVE_RESULT result;
	        if (--count == 0)
	        {
	            printf("ABORTING!\n");
				ic_stop_leg(ileg);
	            break;
	        }
	        ic_sleep(100);
			printf("leg %d = now at %d -- ", ileg, ic_get_leg_position(ileg));
			result = ic_get_leg_status(ileg);
	        if (result == ICMOVE_DONE)
            {
	            printf("ICMOVE_DONE\n");
                break;
            }
#ifdef LATER
	        else if (result == ICMOVE_JAMMED)
            {
	            printf("ICMOVE_JAMMED\n");
                break;
            }
#endif
	        else
	            printf("ICMOVE_....(in progress)\n");
	    }
	    printf("\n");
    }
}


///////////////////////////////////////////////////
// Overshoot test

void MoveTest(byte ileg, byte target)
{
    byte old = ic_get_leg_position(ileg);
    byte new;
    byte stat;
    ic_move_leg(ileg, target);
	while (1)
    {
        byte stat = ic_get_leg_status(ileg);
        if (stat == ICMOVE_DONE)
            break;
        if (stat == ICMOVE_JAMMED)
        {
            printf("JAMMED ! : ");
            break;
        }
        // printf(".");
    }
	ic_sleep(500);  // landing time
    new = ic_get_leg_position(ileg);
    printf("moved from %d to %d - hit %d (%d)\n",
        old, target, new, (int)new-(int)target);
}

void DoOvershoot()
{
	_ic_set_leg_strategy((byte)sic_input("enter strategy"));

    printf("OVERSHOOT TESTS: front left leg\n");

    MoveTest(2, 128);
    MoveTest(2, 128+40);
    MoveTest(2, 128-40);
    MoveTest(2, 128+60);
    MoveTest(2, 128);

    printf("overshoot test completed\n");
}


///////////////////////////////////////////////////
// IR tests

#if 0
#define CHESTDIR_REPORT CHESTDIR_MAX
#else
#define CHESTDIR_REPORT 5 // just the 5 main directions
#endif

static const char g_letters[CHESTDIR_MAX] = "TLRlrxyXYvw";

// Slow works like system ROM

void DoIRObstacleProbeSlow()
{
    typedef struct
    {
        char chReport;  // direction to report
        byte txir;  // direction
        word code;  // code to use
    } IRPROBE;
    static const IRPROBE probes[5] =
    {
        { 'T', TXIR_TOP, 0x3381 },
        { 'L', TXIR_TL, 0x3385 }, // 85 <-> 87 ???
        { 'R', TXIR_TR, 0x3387 },
        { 'l', TXIR_BL, 0x3389 },
        { 'r', TXIR_BR, 0x3389 },
    };
    printf("Starting slow IR obstacle probing (press any key to stop)\n");
	printf("\tprints characters showing direction of obstacle\n");
	printf("\t ('T'op, 'L'eft, 'R'ight, 'l' left bottom, 'r' right bottom)\n");
    while (!sic_is_serial_byte())
    {
	    byte idir;
	    for (idir = 0; idir < 5; idir++)
	    {
		    IRPROBE const* p = &probes[idir];
            word last;
	        ic_sleep(50); // wait a long while
		    if (!ic_send_ir_code(p->txir, p->code))
		    {
	            printf("ERROR sending IR $%x\n", p->code);
	            return;
		    }
		    while (ic_is_sending_ir_code())
		        ;
	        ic_sleep(4); // need not be long
		    last = ic_get_last_ir_code();
            if (last == p->code)
	            printf("%c", p->chReport);
            else if (last != 0)
                printf(" ?%d? ", last);
		}
        printf("; ");
	}
    sic_get_serial_byte();
    printf("\n");
    printf("Done slow IR obstacle probing\n");
}

void DoIRObstacleProbeFast()
{
    printf("Starting fast IR obstacle probing (press any key to stop)\n");
	printf("\tprints characters showing direction of obstacle\n");
	printf("\t ('T'op, 'L'eft, 'R'ight, 'l' left bottom, 'r' right bottom)\n");
    while (!sic_is_serial_byte())
    {
        int bits;
	    byte idir;

        ic_sleep(5);
        if (ic_get_IR_quiet_level() < 10)
        {
            printf("?? "); // general noise
            continue;
        }

        bits = ic_fast_obstacle_detect();
        if (bits < 0)
        {
            // either IR is already busy or noise on the line
            printf("? ");
            continue;
        }
	    for (idir = 0; idir < CHESTDIR_REPORT; idir++)
	    {
            if (bits & (1<<idir))
                printf("%c", g_letters[idir]);
		}
        printf("; ");
	}
    sic_get_serial_byte();
    printf("\n");
    printf("Done fast IR obstacle probing\n");
}

///////////////////////////////////////////////////
// Dumps counts for obstacle reflection sampling 100 times
    // still a little flakey - because of cross-talk

void DoIRObstacleProbeDetail()
{
	int counts[CHESTDIR_MAX];
    byte idir;

    printf("Counts: ");
    if (!ic_count_obstacle_detect(counts, 100))
    {
        printf("NOISY!\n");
        return;
    }
    for (idir = 0; idir < CHESTDIR_REPORT; idir++)
    {
        if (counts[idir] != -1)
	        printf("%c=%d%% ", g_letters[idir], counts[idir]);
    }
    printf("\n");
}

///////////////////////////////////////////////////
