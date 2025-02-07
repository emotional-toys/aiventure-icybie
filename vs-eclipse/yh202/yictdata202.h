// must track the yictdata.a_ file in NEWSRC

    // YICT202 - final

#define YICT_CUSTOMIZATION_ADDRESS 0x23FE00
#define CART_BASE 0x200000
#define CART_SIZE (2*128*1024)

typedef unsigned long addr_t; // cartridge ROM address
typedef unsigned short word;
typedef unsigned char byte;

#define YICT_ID      "YICT202"
#define YICTEND_ID      "YICTEND"
#define YICTTAG_SIZE        8

#include <pshpack2.h>

struct YICT_MEMPOOL
{
    addr_t start;
    word max_size;
};
#define NUM_AUDIO_POOLS     3

struct YICT_ACTIONCHUNK
{
    addr_t start; // ACTIONDAT structure stored back-to-back
    word count;
};
#define NUM_ACTION_CHUNK    3

struct YICT_DATA
{
    char yict_id[8];
    char copyright[24];
    word generation;


// Pointers to structures you can muck with
    // memory pools
    YICT_MEMPOOL pools[NUM_AUDIO_POOLS];
            // 3 allowed for audio, end of last one for YICT probaction data

    addr_t addrSysAudioLookup; // 30*byte
    addr_t addrExtraAudioPointers;  // 30*addr_t

    addr_t addrProbActionTable; // 64*addr_t for PROBACTIONs
            // points to data at end of pool3
	addr_t addrExtraActionTable;	// num*addr_t for ACTIONs
    word numExtraActionTable;

    addr_t addrSitStandNewData; // 14 bytes in compact order

	YICT_ACTIONCHUNK availableActions[NUM_ACTION_CHUNK];

    char name[16]; // personality name
    char basename[16]; // base personality name
    word audio_sizes[NUM_AUDIO_POOLS];
    word pool3_yictsize;
    
    // simple data (saved in .ICB file)
    byte bStretchCount;
    byte bWagTime;
    word wSslSkipCount; // or 0xFFFF for posture-aware

    byte mood_data[10];
        // byte force; byte reserved; word threshold; word factors[3];

///////////////////////
// YICT202 added fields follow:
// NOTE: not added to .ICB file
//   Defaults are the safest
///////////////////////

    byte boolDisableCharger;
    byte boolMute;
    byte bIdleDelay;

    byte reserved;

    char end_tag[8];
};

#include <poppack.h>
