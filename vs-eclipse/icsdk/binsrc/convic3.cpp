// ic3 cartridge

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned long ulong;

////////////////////////////////////////////

#define ADDR_BASE   0x200000
#define MAX_SIZE    256*1024
#define MAX_SRECORD 100 /* no more than 100 bytes per line please */

////////////////////////////////////////////


byte GetNib(char ch)
{
    if (ch >= '0' && ch <= '9')
        return ch - '0';
    else if (ch >= 'A' && ch <= 'F')    // upper only
        return ch - 'A' + 10;
    assert(false);  // bogus hex digit
    return 0;
}

byte GetHex(const char* sz2)
{
    return GetNib(sz2[0]) * 16 + GetNib(sz2[1]);
}

int Count(const byte* pb, int cb)
{
    int n = 0;
    while (cb--)
    {
        assert(*pb == 0 || *pb == 1);
        n += *pb++;
    }
    return n;
}

int main(int argc, char* argv[])
{
    if (argc != 3)
    {
        printf("usage: convic0 infile.s24 outfile.ic0\n");
        return -1;
    }
    FILE* pfS = fopen(argv[1], "rt"); // S records
    if (pfS == NULL)
    {
        printf("FATAL: can't open input file '%s'\n", argv[1]);
        return -1;
    }

    byte rgbImage[MAX_SIZE];
    memset(rgbImage, 0xFF, MAX_SIZE);
    byte rgbUsed[MAX_SIZE];
    memset(rgbUsed, 0, MAX_SIZE);

    char szLine[256];
    int cbCodeTotal = 0; // code or code space data
    int cbDead = 0;
    int cbGaps[4];
    memset(cbGaps, 0, sizeof(cbGaps));

    while (fgets(szLine, sizeof(szLine), pfS) != NULL)
    {
        // lame parsing - just assert if bad
        char* pchEnd = strchr(szLine, '\n');
        assert(pchEnd != NULL);
        *pchEnd = '\0';

        assert(szLine[0] == 'S');
        if (szLine[1] == '2')
        {
            // load record
            const char* pch = szLine+2;
            byte cbToFollow = GetHex(pch);
            pch += 2;

            assert(cbToFollow > 1);
            assert(cbToFollow <= MAX_SRECORD);
            byte bytes[MAX_SRECORD];
                 // does not include size, but does include checksum

            int i;
            byte sum = cbToFollow;
            for (i = 0; i < cbToFollow; i++)
            {
                byte b = GetHex(pch);
                pch += 2;
                bytes[i] = b;
                sum += b;
            }
            assert(*pch == '\0');   // must use up entire line
            assert(sum == 255);

            assert(cbToFollow > 4);
            int cbCode = cbToFollow - 4;

            int addr = bytes[0] * 0x10000 + bytes[1] * 0x100 + bytes[2];

            if ((addr & 0xFF0000) == 0xD00000)
            {
                // dead code - remove it
                cbDead += cbCode;
                continue;
            }
            cbCodeTotal += cbCode;
            assert(addr >= ADDR_BASE);
            assert(addr + cbCode <= ADDR_BASE + MAX_SIZE);

            memcpy(&rgbImage[addr-ADDR_BASE], &bytes[3], cbCode);
            memset(&rgbUsed[addr-ADDR_BASE], 1, cbCode);
        }
        else if (strcmp(szLine, "S804000000FB") != 0)
        {
            assert(false); // unknown format
        }

    }
    assert(pfS);

    FILE* pfB = fopen(argv[2], "wb"); // binary
    if (pfB == NULL)
    {
        printf("FATAL: can't open output file '%s'\n", argv[2]);
        return -1;
    }
    bool bOK = fwrite(rgbImage, MAX_SIZE, 1, pfB) == 1;
    if (fclose(pfB) != 0)
        bOK = false;

    if (!bOK)
    {
        printf("FATAL: error writing to file '%s'\n", argv[2]);
        return -1;
    }
    if (cbDead > 0)
		printf("%d bytes dead code eliminated\n", cbDead);

    // Guess free memory - not exact because of 1 byte gaps
    printf("Free memory:");
    int cbGapTotal = 0;
    for (int iPool = 0; iPool < 4; iPool++)
    {
        int cbGap = 0x10000 - Count(&rgbUsed[iPool * 0x10000], 0x10000);
        printf(" %d %c", cbGap, (iPool != 3) ? '+' : '=');
        cbGapTotal += cbGap;
    }
    printf(" %d\n", cbGapTotal);
    printf("Used memory: %d bytes of ROM space\n", cbCodeTotal);

    return 0;
}

