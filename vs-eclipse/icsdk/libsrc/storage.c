// ICSDK2 - Library
// E2PROM storage

#include "icpriv.h"

////////////////////////////////////////////////////
//OVERVIEW:
// Access to the E2PROM storage.

////////////////////////////////////////////////////


static void ADECL ToggleP5Clock()
{
    SFR_P5 |= (1<<4);   // clock
    _ic_killtime(20);
    SFR_P5 &= ~(1<<4);   // un-clock
    _ic_killtime(20);
}

#define StartP5Send()  { SFR_P5 |= (1 << 3); }
#define EndP5Send()  { SFR_P5 &= ~(1 << 3); }

#define SendP5_1() { SFR_P5 |= (1<<5); ToggleP5Clock(); }
#define SendP5_0() { SFR_P5 &= ~(1<<5); ToggleP5Clock(); }

////////////////////////////////////////////////////

static void ADECL SendByteP5(byte val)
{
    register byte mask;
    for (mask = 0x80; mask != 0; mask = mask >> 1)
    {
        if (val & mask)
			SFR_P5 |= (1<<5);
        else
			SFR_P5 &= ~(1<<5);
        ToggleP5Clock();
    }
}

word ADECL ic_read_storage(byte index)
{
    word reply;
    byte count;
    ic_assert(index < 64);

    StartP5Send();
    SendP5_1();
    SendByteP5(0x80 | index); // opcode 2

    // wait for reply
    reply = 0;
    count = 16;
    while (count--)
    {
        reply = reply << 1;
		ToggleP5Clock();
        if (SFR_P7 & 1)
            reply |= 1;
    }
    EndP5Send();
    return reply;
}

bool ADECL ic_write_storage(byte index, word value)
{
    byte mask;
    word mask2;
    word count;
    ic_assert(index < 64);

    StartP5Send();
    SendP5_1();
    SendByteP5(0x3F);   // write enable on
    EndP5Send();

    StartP5Send();
    SendP5_1();
    SendByteP5(0x40 | index); // opcode 5
    // now 16 bits of data (hi->lo) for data
    SendByteP5(HIBYTE(value));
    SendByteP5(LOBYTE(value));
    EndP5Send();

    SendP5_0(); // special ending
    _ic_killtime(20);
    StartP5Send();

    // wait for ack
    count = 10000;
    while (--count)
    {
        if (SFR_P7 & 1)
            break;
    }
	EndP5Send();
	StartP5Send();
    SendP5_1();
    SendByteP5(0x00);   // write enable off
	EndP5Send();
    return (count > 0);
}

