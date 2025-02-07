// aud2c.cpp

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

typedef unsigned char byte;

void GetName(char* szOut, const char* szFile)
{
    _splitpath(szFile, NULL, NULL, szOut, NULL);
}


void Process(FILE* pfOut, const char* szFile, int& cbFirstSeg)
{
    FILE* pf = fopen(szFile, "rb");
    assert(pf != NULL);

    fseek(pf, 0, SEEK_END);
    const int IGNORE_AT_START = 0x30;   // ADP format
    long cbAud = ftell(pf) - IGNORE_AT_START;
    fseek(pf, IGNORE_AT_START, SEEK_SET);

    assert(cbAud > 0);
    assert(cbAud < 0xFFFF);
    char szFname[_MAX_PATH];
    GetName(szFname, szFile);

    int cbNeeded = 4 + cbAud;
    if (cbFirstSeg > 0)
    {
        if (cbNeeded >= cbFirstSeg)
        {
            // not big enough
	        fprintf(pfOut, "#pragma section const f_audio1\n"); // normal seg
            cbFirstSeg = 0;
        }
        else
        {
            cbFirstSeg -= cbNeeded;
            assert(cbFirstSeg > 0); // can't use it all up
        }
    }

    fprintf(pfOut, "const unsigned char audio_%s[] =\n{\n", szFname);
    //BLOCK: length (low-endian format)
    {
        union
        {
            byte rgb[4];
            long l;
        } u;
        u.l = cbAud;
        assert(u.rgb[2] == 0);
        assert(u.rgb[3] == 0);
        fprintf(pfOut, "\t0x%02x, 0x%02x, 0, 0, // %d bytes\n",
            u.rgb[0], u.rgb[1], cbAud);
    }
    
    int ib = 0;
    byte b;
    while (fread(&b, 1, 1, pf) == 1)
    {
        if ((ib & 7) == 0)
            fprintf(pfOut, "\t");
        fprintf(pfOut, "0x%02x, ", b);
        if ((ib & 7) == 7)
            fprintf(pfOut, "\n");
        ib++;
    }
	if ((ib & 7) != 7)
		fprintf(pfOut, "\n");

    assert(ib == cbAud);

    fprintf(pfOut, "};\n\n");

    fclose(pf);
}


int main(int argc, char* argv[])
{
    assert(argc > 2);
    int cbFirstSegment = atoi(argv[1]);

	printf("#pragma section const %s\n",
         (cbFirstSegment > 0) ? "f_audio0" : "f_audio1");
	for (int iarg = 2; iarg < argc; iarg++)
    {
        const char* szFile = argv[iarg];
        Process(stdout, szFile, cbFirstSegment);
    }

    return 0;
}

