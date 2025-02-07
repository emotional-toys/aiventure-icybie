// ICSDK2 - ICybie Library
// E2PROM storage (and Flash ROM for advanced users)

////////// ////////// ////////// ////////// //////////

word ADECL ic_read_storage(byte index);
bool ADECL ic_write_storage(byte index, word value);

// 64 words of data
// NOTE: be very careful if you use these
//   most of the E2PROM data is used by the standard ICybie
// standard ICybie E2PROM usage
    // index:
    //  0 -> 17 - leg data (for last calibration)
    // 18 -> 27 - not used (FREE for your use)
    // 28       - used for walkup charger threshold
    // 29 ?
    // 30       - use voice commands (0=>false, 1=>true)
    // 31       - battery thresholds
    // 31       - LOBYTE = "lo" battery threshold (0 for disabled)
    // 31       - HIBYTE = "hi" battery threshold (255 for disabled)
    // 32       - used for copy of walkup charger threshold
    // NOTE: 33 -> 63 are erased in some cartridge formats (see crt0.asm)
    // 33       - some form of storage lock/recover for leg data
    // 34       - HIBYTE = reboot count, LOBYTE = 0x80 flag
    // 35       - not used
    // 36       - LOBYTE = last mood, HIBYTE = wake mood
    // 37       - LOBYTE = scramble value, HIBYTE = battery (was) low flag
    // 38 -> 63 - 50 bytes + 2 for storing mood information

// I recommend you just use 18->27 (10 words = 20 bytes)
#define STORAGE_USER0       18
#define STORAGE_USER1       19
#define STORAGE_USER2       20
#define STORAGE_USER3       21
#define STORAGE_USER4       22
#define STORAGE_USER5       23
#define STORAGE_USER6       24
#define STORAGE_USER7       25
#define STORAGE_USER8       26
#define STORAGE_USER9       27

// put your 16 bit program ID here to test for initializaton
#define STORAGE_USER_TYPE   32

////////// ////////// ////////// ////////// //////////
// Advanced - saving data to cartridge flash ROM

void ADECL ic_write_flash_word(word offset, word data);
    // for safety sake, can only write to first 64KB of cartridge
    // actually first and last words are off limits too

#ifdef LATER
// Flash read/write - dangerous - only use if you know what you are doing!
bool ADECL ic_erase_flash_page(long addr8K);
bool ADECL ic_write_flash_page(long addr, const word* save_data, int word_count);
#endif

////////// ////////// ////////// ////////// //////////
