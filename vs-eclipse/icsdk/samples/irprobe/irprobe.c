// low level program to probe IR
// custom startup

#include <ic.h>
#include <ic_main0.h> // custom startup

#include <ic_sfr.h> // access to PB

/////////////////////////////////////////////////////////

extern void main(); // real main

void main0()
{
	ic0_init_interrupt_handlers();

	sic_init_serial(BAUD_38400);
    __asm ( "EI 0" ); // enable!

    printf("SicSDK2 - trimmed down startup\n");

	ic0_init_timers();
    ic0_init_peripherals();
	    // nothing else - don't use standard IR processing

    main();
    ic_shutdown();
}

/////////////////////////////////////////////////////////

#define CAPTURE_SIZE    1000

typedef struct
{
    bool type;
    int count;
} CAPTURE_INFO;

CAPTURE_INFO capture_info[CAPTURE_SIZE];
volatile int g_nCapture = CAPTURE_SIZE; // full

// Int22 - timer handler (about 3537Hz?)
#pragma section code f_cart_int22
void __interrupt interrupt_handler22() // must be first in section
{
    CAPTURE_INFO* pci;
    bool now;
    if (g_nCapture >= CAPTURE_SIZE)
        return; // full

    pci = &capture_info[g_nCapture];
    now = (SFR_PB & 4) ? 1 : 0;
    if (now == pci->type && pci->count < 1000)
    {
        pci->count++;
        return;
    }
    // new record
    if (++g_nCapture >= CAPTURE_SIZE)
        return; // full
    pci = &capture_info[g_nCapture];
    pci->type = now;
    pci->count = 1;
}
#pragma section code f_code

/////////////////////////////////////////////////////////
bool GetEncoded(int iStart, word* pwRet)
{
    word wRet = 0;
    byte i;

    for (i = 0; i < 16; i++)
    {
	    CAPTURE_INFO* pci0 = &capture_info[iStart + i*2];
	    CAPTURE_INFO* pci1 = &capture_info[iStart + i*2+1];
        bool bLast = (i == 15);
        if (pci0->type != 0)
            return false;
        if (pci1->type != 1)
            return false;
        if (pci0->count >= 3 && pci0->count <= 5 &&
            ((pci1->count >= 11 && pci1->count <= 13) || bLast))
        {
            // zero bit
        }
        else if (pci0->count >= 11 && pci0->count <= 13  &&
            ((pci1->count >= 3 && pci1->count <= 5) || bLast))
        {
            // one bit
            wRet |= (0x8000 >> i);
        }
        else
        {
            return false; // bogus timing
        }
    }

    *pwRet = wRet;
    return true;
}

void main()
{
    printf("IR Probe program V.001\n");

	SFR_INTETA23 |= 0x40; // TA3 enabled

    while (1)
    {
	    int i, iStart;
        printf("\nRe-Starting capture (initial values may be bogus)\n");

        capture_info[0].type = 0xFF;    // fake first record
        capture_info[0].count = 0;
        g_nCapture = 0; // start

        iStart = -1;
        for (i = 1; i < CAPTURE_SIZE; i++) // skip first one
        {
		    CAPTURE_INFO* pci = &capture_info[i];

            while (g_nCapture <= i)
                ; // wait until we get data

            if (pci->count > 16)
            {
                if (pci->type == 1 && iStart != -1 &&
                    i == iStart + 31)
                {
                    // possible encoded
                    word w;
                    if (GetEncoded(iStart, &w))
                        printf("\t-- ENCODED $%x\n", w);
                    else
                        printf("\t-- BAD ENCODING\n");
                }
                
	            printf("GAP(%c%d) ", (pci->type == 0) ? 'L' : 'H', pci->count);
                iStart = i+1; // potential zero bit
            }
            else
            {
	            printf("%c%d ", (pci->type == 0) ? 'L' : 'H', pci->count);
            }
        }
        printf("\n\n");

    }
}
