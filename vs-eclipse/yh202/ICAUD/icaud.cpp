// audio mucking for YICT201 Cartridge format

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include <windows.h>    // for GetPrivateProfileString

#include "../yictdata202.h"

typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned long ulong;

typedef unsigned long addr_t;

////////////////////////////////////////////

byte g_rgbRom[CART_SIZE]; // intel format

byte* Lookup(addr_t addr)
{
    assert(addr >= CART_BASE && addr < CART_BASE + CART_SIZE);
    return &g_rgbRom[addr - CART_BASE];
}

#define NUM_AUDIO_TOTAL     60
    // 1->30 standard(but mappable), 31->60 custom
#define NUM_AUDIO_STD       30

enum AUDTYPE
{
    AT_DEFAULT = 0, // not specified
    AT_SUBST,       // system sound replaced (for 1->30)
    AT_LEAF,        // custom audio (for 31->60)
};

struct AUDINFO
{
    AUDTYPE at;
    int numAudioSubst;  // if AT_SUBST
    const char* szFileLeaf; // if AT_LEAF
};

// iAudio is in range 0->59, numAudio is in range 1->60

AUDINFO g_ai[NUM_AUDIO_TOTAL];


////////////////////////////////////////////

bool LoadADP(const char* szFile, int& cbAud, byte* &pData)
{
    FILE* pf = fopen(szFile, "rb");
    if (pf == NULL)
        return false;

    const int CB_HDR = 0x30;
    fseek(pf, 0, SEEK_END);
    cbAud = ftell(pf) - CB_HDR;
    fseek(pf, CB_HDR, SEEK_SET);

    pData = new byte[cbAud];
    assert(fread(pData, cbAud, 1, pf) == 1);
    fclose(pf);
    return true;
}

////////////////////////////////////////////

void ClearMemoryPools(YICT_DATA* pyd)
{
    for (int iPool = 0; iPool < NUM_AUDIO_POOLS; iPool++)
    {
		YICT_MEMPOOL& pool = pyd->pools[iPool];
        memset(Lookup(pool.start), 0xFF, pool.max_size);
        pyd->audio_sizes[iPool] = 0;
    }
}

addr_t AllocSpace(YICT_DATA* pyd, int cbNeeded, int& iPoolRet)
{
    for (int iPool = 0; iPool < NUM_AUDIO_POOLS; iPool++)
    {
		YICT_MEMPOOL& pool = pyd->pools[iPool];
        word& cbUsed = pyd->audio_sizes[iPool];
		if (cbNeeded <= pool.max_size - cbUsed)
        {
            addr_t addrRet = pool.start + cbUsed;
            cbUsed += cbNeeded;
            iPoolRet = iPool;
            return addrRet;
        }
    }
    return 0;   // no room
}


int main(int argc, char* argv[])
{
    if (argc != 3)
    {
        printf("usage: icaud <source_binary> <basename>\n");
        printf("  inputs:\n");
        printf("\t<source_binary> = source binary\n");
        printf("\t<basename>.ini = specification file\n");
        printf("\t<basename>\\*.adp = file names for audio (suggested location)\n");
        printf("  outputs:\n");
        printf("\t<basename>.ic3 = output binary\n");
        return -1;
    }
    const char* szBase = argv[2];

    char szIniFile[_MAX_PATH];
    sprintf(szIniFile, ".\\%s.INI", szBase);

    //BLOCK: load in original
    {
	    FILE* pf = fopen(argv[1], "rb");
	    assert(pf);
	    assert(fread(g_rgbRom, sizeof(g_rgbRom), 1, pf) == 1);
	    fclose(pf);

    }
	YICT_DATA* pyd = (YICT_DATA*)Lookup(YICT_CUSTOMIZATION_ADDRESS);
    if (memcmp(pyd->yict_id, YICT_ID, YICTTAG_SIZE) != 0 ||
        memcmp(pyd->end_tag, YICTEND_ID, YICTTAG_SIZE) != 0)
    {
        printf("FATAL: %s is not a YICT201 image\n", argv[1]);
    }

    //BLOCK: check input layout
    {
        int cbOld = 0;
        for (int iPool = 0; iPool < NUM_AUDIO_POOLS; iPool++)
            cbOld += pyd->audio_sizes[iPool];

        if (cbOld > 0)
        {
            printf("WARNING: old image has %d byte of audio - discarded\n",
                cbOld);
        }
    }
    // save the behavior data
    word cbYictBehavior = pyd->pool3_yictsize;
    assert(cbYictBehavior > 0);
    byte* pbYictBehavior = new byte[cbYictBehavior];
    addr_t addrYictBehaviorOld = pyd->pools[2].start + pyd->audio_sizes[2];
    memcpy(pbYictBehavior, Lookup(addrYictBehaviorOld), cbYictBehavior);

    printf("INFO: %d bytes of YICT editable behavior data\n",
	    cbYictBehavior);

    //BLOCK: load configuration information
    {
        for (int iA = 0; iA < NUM_AUDIO_TOTAL; iA++)
        {
            bool bStd = (iA < NUM_AUDIO_STD);
			AUDINFO& ai = g_ai[iA];

            char szNum[32];
            sprintf(szNum, "%d", 1+iA);
            char szValue[256];
            if (GetPrivateProfileString(bStd ? "STANDARD" : "CUSTOM",
                szNum, "", szValue, sizeof(szValue), szIniFile) > 0)
            {
                if (szValue[0] == '#')
                {
                    ai.at = AT_SUBST;
                    ai.numAudioSubst = atoi(szValue+1);
                }
                else
                {
                    ai.at = AT_LEAF;
                    ai.szFileLeaf = strdup(szValue);
                }
            }
            else
            {
                assert(ai.at == AT_DEFAULT);
            }
        }

        // now check configuration
        int nErr = 0;
        int nCustom = 0;
        for (iA = 0; iA < NUM_AUDIO_TOTAL; iA++)
        {
            bool bStd = (iA < NUM_AUDIO_STD);
			AUDINFO& ai = g_ai[iA];
            if (ai.at == AT_SUBST)
            {
                if (!bStd)
                {
                    printf("INVALID: #%d=subst -- not a system sound\n",
	                        1+iA);
					nErr++;
                }
                else if (ai.numAudioSubst >= 1 && ai.numAudioSubst <= 30)
                {
                    // system sounds are always there
	                if (ai.numAudioSubst != 1+iA)
	                    printf("WARNING: replacing system sound with system sound\n");
                }
                else if (ai.numAudioSubst >= 31 && ai.numAudioSubst <= 60)
                {
					AUDINFO& aiUse = g_ai[ai.numAudioSubst-1];
                    if (aiUse.at != AT_LEAF)
                    {
	                    printf("INVALID: #%d=%d -- custom is not defined\n",
	                        1+iA, ai.numAudioSubst);
                        nErr++;
                    }
                }
                else
                {
                    printf("INVALID: #%d=%d -- invalid value",
                        1+iA, ai.numAudioSubst);
					nErr++;
                }
            }
            else if (ai.at == AT_LEAF)
            {
                if (bStd)
                {
                    printf("INVALID: #%d=%s -- can't replace system sounds\n",
                        1+iA, ai.szFileLeaf);
                    nErr++;
                }
                else
                {
                    nCustom++;
                }
                // otherwise assume file is ok
            }
        }

        if (nCustom == 0)
        {
	        printf("ERROR: no custom audio defined\n");
	        nErr++;
        }
        
        if (nErr > 0)
        {
	        printf("FATAL: input errors for config file '%s'\n",
		        szIniFile);
            return -1;
        }
    
    }

    byte* pbSysAudioLookup = Lookup(pyd->addrSysAudioLookup);
    addr_t* paddrExtraAudioPointers = (addr_t*)
            Lookup(pyd->addrExtraAudioPointers);

    ClearMemoryPools(pyd);

    //BLOCK: load in .ADP, store in a memory pool
    {
        for (int iA = 0; iA < NUM_AUDIO_TOTAL; iA++)
        {
            bool bStd = (iA < NUM_AUDIO_STD);
			AUDINFO& ai = g_ai[iA];
            addr_t* pAddrExtra = NULL;
            if (!bStd)
            {
                pAddrExtra = paddrExtraAudioPointers + (iA - NUM_AUDIO_STD);
                *pAddrExtra = NULL; // not used by default
            }

            if (bStd)
	            pbSysAudioLookup[iA] = iA; // default (1-to-1)


            if (ai.at == AT_SUBST)
            {
                assert(bStd);
                assert(pAddrExtra == NULL);
                assert(iA < 30);
	            pbSysAudioLookup[iA] = ai.numAudioSubst - 1;
            }
            else if (ai.at == AT_LEAF)
            {
                assert(pAddrExtra != NULL);
	            int cbAud;
	            byte* pbData;

	            if (!LoadADP(ai.szFileLeaf, cbAud, pbData))
	            {
	                printf("FATAL: failed to load %s\n", ai.szFileLeaf);
	                return -1;
		        }
	
                int iPool;
	            addr_t addrD = AllocSpace(pyd, 4 + cbAud, iPool);
	            if (addrD == NULL)
	            {
	                printf("FATAL: failed to find space for %s\n", ai.szFileLeaf);
	                return -1;
	            }

	            byte* pb = Lookup(addrD);
	            long cbSize = cbAud;
	            memcpy(pb, &cbSize, 4);
	            pb += 4;
	            memcpy(pb, pbData, cbAud);

                *pAddrExtra = addrD;
	            printf("Saved #%d - %s into pool %d (4+%d bytes)\n",
	                    1+iA, ai.szFileLeaf, iPool, cbAud);
	            delete [] pbData;
            }
        }
    }

    //BLOCK: put YictBehaviorData at end of last pool
        // and adjust the PROBACTION table
    {
        int iPool;

		YICT_MEMPOOL& pool = pyd->pools[2];
        word cbAudio = pyd->audio_sizes[2];
		if (cbYictBehavior > pool.max_size - cbAudio)
        {
            printf("FATAL: no more room in last pool for YICT behavior\n");
            return -1;
        }
        addr_t addrYictBehaviorNew = pool.start + cbAudio;
        // yictsize already set

	    memcpy(Lookup(addrYictBehaviorNew), pbYictBehavior, cbYictBehavior);
		printf("Saved Behavior into last pool (%d bytes)\n",
	                    cbYictBehavior);

        delete [] pbYictBehavior;
        pbYictBehavior = NULL;

        // adjust PROBACTION pointers
        addr_t* paddrPA = (addr_t*)Lookup(pyd->addrProbActionTable);
        for (int iPA = 0; iPA < 64; iPA++)
        {
			addr_t& addrPA = paddrPA[iPA];
            addr_t addrOld = addrPA;
            if (addrPA != NULL)
            {
                assert(addrPA >= addrYictBehaviorOld);
                assert(addrPA < addrYictBehaviorOld + cbYictBehavior);
                addrPA += (addrYictBehaviorNew - addrYictBehaviorOld);
                assert(addrPA >= addrYictBehaviorNew);
                assert(addrPA < addrYictBehaviorNew + cbYictBehavior);
            }
        }
    }

    // print pool usage report
    for (int iPool = 0; iPool < NUM_AUDIO_POOLS; iPool++)
    {
		YICT_MEMPOOL& pool = pyd->pools[iPool];
        word cbUsed1 = pyd->audio_sizes[iPool];
        word cbUsed2 = 0;
        if (iPool == 2)
            cbUsed2 = pyd->pool3_yictsize;
        word cbUsed = cbUsed1 + cbUsed2;
        printf("pool %d : %d+%d / %d used (%d free)\n",
            iPool, cbUsed1, cbUsed2, pool.max_size, pool.max_size - cbUsed);
    }

    assert(strlen(szBase) < 16);
    // change base name
    memset(pyd->basename, 0, 16);
    strcpy(pyd->basename, szBase);

    //BLOCK: save out new one
    {
        char szOut[_MAX_PATH];
        sprintf(szOut, "%s.ic3", szBase);
	    FILE* pf = fopen(szOut, "wb");
	    assert(pf);
	    assert(fwrite(g_rgbRom, sizeof(g_rgbRom), 1, pf) == 1);
	    fclose(pf);
    }
    
#if 1
    //BLOCK
    {
        for (int i = 0; i < 30; i++)
            printf("%d ", pbSysAudioLookup[i]);
        printf("\n");
    }
#endif

    return 0;
}
