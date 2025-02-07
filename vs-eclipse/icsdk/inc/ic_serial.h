// ICSDK2 - ICybie Library
// Serial port functions (Super ICybie=SIC only)

////////// ////////// ////////// ////////// //////////
// serial port (Super ICybie only)

void ADECL sic_send_serial_byte(byte b);
byte ADECL sic_get_serial_byte(); // will block if not ready
bool ADECL sic_is_serial_byte(); // is one ready?

typedef enum
{
    BAUD_38400 = 0, // phiT0
    BAUD_9600  = 1, // phiT2
    BAUD_2400  = 2, // phiT8
    BAUD_600   = 3  // phiT32
} BAUD_RATE; /*CPU SFR VALUES - DO NOT ALTER*/

void ADECL sic_init_serial(BAUD_RATE baud);

////////// ////////// ////////// ////////// //////////
// formatted input/output

int sic_input(const char* prompt);
     // actually unsigned

void __cdecl printf(const char* format, ...); // very minimal printf
    // format supports only:
        // %c, %s, %d, %ld, %x, %lx, %%
        // %x is always 4 hex digits
        // %lx is always 8 hex digits
        // %d is 16 bits, %ld is 32 bits
        // (be sure you don't mix up the arguments, printf is not type safe)

////////// ////////// ////////// ////////// //////////
