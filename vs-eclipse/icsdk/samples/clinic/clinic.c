// simple diagnostics (mostly editing E2PROM)

#include "ic.h"

///////////////////////////////////////////////////////////////
// forward declarations

void DoDumpStorage(bool bRaw);
void DoEditStorage();
void DoBatteryAdjust();
void DoLegAdjust();

///////////////////////////////////////////////////////////////

void main()
{
	ic_show_eyes_green();

	ic_show_eyes_yellow();
	ic_sleep(1*1000);

    printf("\nWelcome to the ICybie Clinic\n\n");
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
            printf("\t\tt = dump E2PROM storage (raw)\n");
            printf("\t\tT = dump E2PROM storage (formatted)\n");
            printf("\t\tB = battery adjustment mode\n");
            printf("\t\tL = leg adjustment mode\n");
            printf("\t\tW = edit E2PROM storage (raw)\n");
        }
        else if (chCmd == 't')
        {
			DoDumpStorage(true);
        }
        else if (chCmd == 'T')
        {
			DoDumpStorage(false);
        }
        else if (chCmd == 'B' || chCmd == 'b')
        {
			printf("Current battery voltage is %d\n", ic_get_battery_level());
			DoBatteryAdjust();
        }
        else if (chCmd == 'L')
        {
            printf("Force leg recalibration\n");
            if (sic_input("are you sure? (if yes, enter 1)") == 1)
            {
                DoLegAdjust();
                printf("\n\tnow take out the cartridge\n");
                printf("\tforce a leg calibration, helping ICybie roll over\n");
                printf("\tthen repeat\n");
                printf("\t\t10 + 9 + Large + 7 + Large\n\n");
            }
        }
        else if (chCmd == 'W')
        {
			DoEditStorage();
        }
        else if (chCmd == 'q' || chCmd == 'Q')
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

//////////////////////////////////////////////////////////////////////
// Storage dump

// compensate for lack of 'float' and lame 'printf'
static void print_div50(int val)
{
    int cents = (val % 50) * 2;
    printf("%d", val / 50);
    printf(".");
    if (cents < 10)
        printf("0");
    printf("%d", cents);
}

void DoDumpStorage(bool bRaw)
{
    word storage[64];

    //BLOCK: read from E2PROM into local variables
    {
	    byte i;
	    for (i = 0; i < 64; i++)
	        storage[i] = ic_read_storage(i);
    }

    if (bRaw)
    {
	    byte i, j;
        printf("E2PROM storage: Raw hex dump (64 words = 128 bytes)\n");
	    for (i = 0; i < 64; i += 8)
        {
            printf("\t");
		    for (j = 0; j < 8; j++)
                printf("%x ", storage[i+j]);
            printf("\n");
        }
    }
    else
    {
		printf("E2PROM storage: Interpreted contents\n");
	    //BLOCK: pretty print leg info
	    {
	        struct leginfo
	        {
	            int offsets[12];
	            byte gains[12];
	        } * pleginfo;
	        byte ileg;
	        pleginfo = (struct leginfo*)storage;
	        printf("\tLeg calibration:\n");
	        for (ileg = 0; ileg < 12; ileg++)
	        {
	            int offset = pleginfo->offsets[ileg];
	            int gain = pleginfo->gains[ileg];
	
	            if (ileg % 3 == 0)
	                printf("\t\t");
	            printf("leg%d*", ileg);
	            print_div50(gain);
	            printf("+");
	            print_div50(offset);
	            printf(" ");
	            if (ileg % 3 == 2)
	                printf("\n");
	        }
	    }
	
	    //BLOCK: pretty print misc info
	    {
	        printf("\tuse voice commands = %d\n", storage[0x1E]);
	        printf("\tbattery thresholds = %d (lo), %d (hi)\n",
	            LOBYTE(storage[0x1F]), HIBYTE(storage[0x1F]));
	        printf("\treboot count = %d, flag = %d\n",
	            HIBYTE(storage[0x22]), LOBYTE(storage[0x22]));
	        printf("\tmood = %d, wakemode = %d\n",
	            LOBYTE(storage[0x24]), HIBYTE(storage[0x24]));
	        printf("\tscramble = %d, battlow flag = %d\n",
	            LOBYTE(storage[0x25]), HIBYTE(storage[0x25]));
	    }
	
	    //BLOCK: pretty mood info
	    {
		    byte influence;
			const byte* data = (const byte*)&storage[0x26]; // 50 bytes
	        printf("\tMood influences:\n");
	        for (influence = 0; influence < 10; influence++)
	        {
	            static const char* const names[] =
	            {
	                "\t  Interaction:\n\t\tGeneral   :",
	                "\t\tPet back  :",
	                "\t\tHead/Butt :",
	                "\t\tNose      :",
	                "\t  Environment:\n\t\tDark 1    :",
	                "\t\tDark 2    :",
	                "\t  Terrain:\n\t\t??????    :",
	                "\t\tWall1     :",
	                "\t\tWall2     :",
	                "\t\tWall3     :",
	            };
		        byte history;
	
	            printf("%s", names[influence]);
	            // 5 samples for each
		        for (history = 0; history < 5; history++)
	                printf(" %d", data[influence*1 + history*10]);
	            printf("\n");
	        }
	    }
    }
}
//////////////////////////////////////////////////////////////////////

void DoEditStorage()
{
    byte index = (byte)sic_input("RAW EDIT: enter index (99 to cancel)");
    word value;

    if (index < 0 || index > 63)
    {
        printf("EDIT CANCELLED\n");
        return;
    }
	value = ic_read_storage(index);
    printf("Storage %d was %d -- ", index, value);
    value = sic_input("enter new value");
    ic_write_storage(index, value);
    printf("Storage %d is now %d\n", index, ic_read_storage(index));
}

void DoLegAdjust()
{
    // if the E2PROM leg values are screwed up,
    // we set them to zero gain and guestimate center position
    // if the user then forces a leg recalibration, the legs should
    //  come close (a second recalibration should fix all problems)
	byte gain = 0;
	word gain2 = ((word)gain << 8) | gain;
	int offset = 50*128;
    byte index;

    for (index = 0; index < 12; index++)
	    ic_write_storage(index, offset);
    for (index = 12; index < 18; index++)
	    ic_write_storage(index, gain2);
    printf("E2PROM leg gains erased. Leg offsets set to midpoint\n");
}

void DoBatteryAdjust()
{
    word wStored;
    byte thres1, thres2;
	printf("Battery adjust mode:\n");

    wStored = ic_read_storage(0x1F);
    thres1 = LOBYTE(wStored);
    thres2 = HIBYTE(wStored);

    printf("Current thresholds = %d %d\n", thres1, thres2);

    thres1 = (byte)sic_input("Enter new lower threshold");
    thres2 = (byte)sic_input("Enter new upper threshold");

    if (thres1 >= thres2)
    {
        printf("illegal thresholds - not changed\n");
    }
    else
    {
	    wStored = ((word)thres2 << 8) | thres1;
        ic_write_storage(0x1F, wStored);
	    wStored = ic_read_storage(0x1F);
	    thres1 = LOBYTE(wStored);
	    thres2 = HIBYTE(wStored);
    }
    wStored = ic_read_storage(0x1F);
    thres1 = LOBYTE(wStored);
    thres2 = HIBYTE(wStored);
    printf("New thresholds = %d %d\n", thres1, thres2);
}
