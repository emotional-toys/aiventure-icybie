// ICSDK2
// Serial port features (Super ICybie, SIC only)

#include "icpriv.h"

////////////////////////////////////////////////////
//OVERVIEW
//  Serial port functions
//  There is no buffering for send (it will block until serial port free)
//  There is a single byte buffer for receive

////////////////////////////////////////////////////
// DATA

static bool __near isSendingFlag;
static bool __near didReceiveFlag;
static byte __near dataReceived;

//////////////////////////////////////////////////////
// PUBLIC functions

bool ADECL sic_is_serial_byte()
{
    return didReceiveFlag;
}

byte ADECL sic_get_serial_byte()
{
    // do ... while generate tigher code than while() {}
    do { } while (!didReceiveFlag);

    // single byte buffer
    didReceiveFlag = false;
    return dataReceived;
}

void ADECL sic_send_serial_byte(byte b)
{
	if (SFR_INTES0 == 0)
        return; // if not initialized, don't print anything
    do { } while (isSendingFlag);
    isSendingFlag = true;
    SFR_SC0BUF = b;
}

//////////////////////////////////////////////////////
// INITIALIZE

void ADECL sic_init_serial(BAUD_RATE baud)
{
    byte ignored;

    // ic_lowlevel_assert(baud >= 0 && baud < 4);

    SFR_SC0MOD0 = 0x29;
	SFR_SC0CR = 0x40;

    // 38.4kbps based values (or /4, /16 or /64)
	SFR_BR0CR = 0x40 + (baud << 4) + 10;
    SFR_BR0ADD = 4;
                        
	SFR_SC0MOD1 = 0x40;
    SFR_PCFC = 5; // TxD0 and RxD0 enabled
    SFR_PCCR = 0x3D; // RxD0 in

    SFR_INTES0 = 0x66; // priority 6 - higher than other interrupts
            // so we can 'printf' in interrupt handlers
            // (but not recommended)

    // globals are zero inited
    //// isSendingFlag = false;
    //// didReceiveFlag = false;
    ignored = SFR_SC0BUF;  // flush old
}

////////////////////////////////////////////////////
// INTERRUPT handlers

// Int23 - RX0 handler
#pragma section code f_cart_int23
void __interrupt interrupt_handler23()
{
    dataReceived = SFR_SC0BUF;
    didReceiveFlag = true;
}

// Int24 - TX0 handler
#pragma section code f_cart_int24
void __interrupt interrupt_handler24()
{
    isSendingFlag = false;
}

////////////////////////////////////////////////////
