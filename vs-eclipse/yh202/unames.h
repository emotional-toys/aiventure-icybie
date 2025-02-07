// Unified Name List

#ifndef UNAMES_DYNAMIC_ADDRESSES
    #error old unames not supported
#endif

struct UNAME
{
    int iStore;
        // zero based permanent number for load/store

    const char* szStdAction;    // standard action number
            // may end in:
                //  S (no sound substitute)
                //  T (sitting tendency)
                //  L (lie down tendency)
    const char* szSym;  // symbol name
    const char* szDesc; // description

// only dynamic
    addr_t addrDynamic;
};

#define UNAMEX(index, stdstring, sym, desc) \
    { index, stdstring, sym, desc, NULL }

#define UNAMEN(index, num, sym, desc) \
    { index, #num, sym, desc, NULL }

static UNAME g_uniNames[] =		// only dynamic supported
{
#include "unames2.h"
};

#define NUM_UNAME       (sizeof(g_uniNames)/sizeof(UNAME))


const UNAME* FindUniName(addr_t addrFind)
{
	// find the action from master list
	for (int iAction = 0; iAction < NUM_UNAME; iAction++)
		if (addrFind == g_uniNames[iAction].addrDynamic)
			return &g_uniNames[iAction];
	return NULL;
}
