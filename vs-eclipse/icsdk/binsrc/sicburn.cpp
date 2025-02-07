// SICBURN - the Super-ICybie Transfer utility (put ROM image from PC to ICybie)
// formerly called 'sicup'

#include "std.h"

#include "siclib.h"

#include <ctype.h>
#include <conio.h>  // for keyboard polling

#include <windows.h> // for Sleep

static void PrUsage()
{
    printf("usage: sicburn [flags] file.ext\n");
    printf("\t(version 2.01 - formerly 'sicup')\n");
    printf("\tSends image file to Super ICybie Flash ROM or cartridge\n");
    printf("\tSuper ICybie must be attached to the serial port of the PC\n");
    printf("\tCOM1 is the default\n");
    printf("  ext:\n");
    printf("\t.ic2 - 248K - system ROM replacement\n");
    printf("\t.ic3 - 256K - cartridge ROM\n");
    printf("\tnone - two file Downloader format 'file-l.bin', 'file-h.bin'\n");
    printf("  flags:\n");
    printf("\t\t-r run the program and keep listening to serial port\n");
    printf("\t\t-q quiet (not verbose)\n");
    printf("\t\t-s smart mode (don't bother if checksums the same)\n");
    printf("\t\t-w slow mode (9600 baud, 38400 is default)\n");
    printf("\t\t-2 use COM2\n");
    printf("\t\t-3 use COM3\n");
    printf("\t\t-4 use COM4\n");
    exit(-1);
}

bool LoadFile(const char* szInFile, byte* pbImage, int cbImage)
{
    FILE* pfIn = fopen(szInFile, "rb");
    if (pfIn == NULL)
    {
        fprintf(stderr, "FATAL: can't open input file '%s'\n", szInFile);
        return false;
    }
    fseek(pfIn, 0, SEEK_END);
    int cbFile = ftell(pfIn);
    fseek(pfIn, 0, SEEK_SET);

    if (cbFile != cbImage)
    {
        fclose(pfIn);
        fprintf(stderr, "FATAL: input file '%s' wrong size\n", szInFile);
        fprintf(stderr, "\t %d bytes (%d bytes expected)\n",
                cbFile, cbImage);
        return false;
    }

	if (fread(pbImage, cbImage, 1, pfIn) != 1)
	{
		fclose(pfIn);
		fprintf(stderr, "FATAL: read error on input file '%s'\n", szInFile);
        return false;
    }
    fclose(pfIn);
    return true;
}

BYTE g_rgbImage[256*1024];    // max size
BYTE g_rgbHalf[128*1024];    // for -l/-h loading

int main(int argc, char* argv[])
{
    int iPort = 1;  // default COM1
    bool bRunAfter = false;
    bool bSmart = false;
    bool bSlow = false;
    bool bVerbose = true;

    while (argc > 2)
    {
        // process flags
        const char* szFlag = argv[1];
        argv++;
        argc--;

	    if (strcmp(szFlag, "-1") == 0)
            iPort = 1;
	    else if (strcmp(szFlag, "-2") == 0)
            iPort = 2;
	    else if (strcmp(szFlag, "-3") == 0)
            iPort = 3;
	    else if (strcmp(szFlag, "-4") == 0)
            iPort = 4;
	    else if (strcmp(szFlag, "-r") == 0)
            bRunAfter = true;
	    else if (strcmp(szFlag, "-s") == 0)
            bSmart = true;
	    else if (strcmp(szFlag, "-w") == 0)
            bSlow = true;
	    else if (strcmp(szFlag, "-q") == 0)
            bVerbose = false;
	    else if (strcmp(szFlag, "-l") == 0)
			pfSicLog = fopen("siclog.out", "wt");
        else
            PrUsage();  // bad flag
    }
    
    if (argc != 2)
        PrUsage();
    const char* szInputSource = argv[1]; // file or file prefix

    int cbImage = -1;
    long baseAddr = 0;

    const char* szExt = strchr(szInputSource, '.');
    if (szExt != NULL && strcmpi(szExt, ".ic2") == 0)
    {
        // all of system ROM except last 8K (CROM)
        cbImage = 248*1024;
        baseAddr = 0xFC0000;
    }
    else if (szExt != NULL && strcmpi(szExt, ".ic3") == 0)
    {
        // all of cartridge ROM (from 1 file)
        cbImage = 256*1024;
        baseAddr = 0x200000;
    }
    else if (szExt == NULL)
    {
        // all of cartridge ROM (from 2 files)
        cbImage = 256*1024;
        baseAddr = 0x200000;
    }

    if (cbImage == -1)
    {
        fprintf(stderr, "FATAL: can't determine file type '%s'\n", szInputSource);
        exit(-1);
    }
    
    assert(cbImage > 0 && cbImage <= sizeof(g_rgbImage));

    //BLOCK: read in image
    if (szExt != NULL)
    {
        if (!LoadFile(szInputSource, g_rgbImage, cbImage))
            return -1;
            // will exit if errors detected
    }
    else
    {
        char szInFile[_MAX_PATH];
        sprintf(szInFile, "%s-l.bin", szInputSource);
        if (!LoadFile(szInFile, g_rgbHalf, cbImage/2))
            return -1;
        for (int i = 0; i < cbImage/2; i++)
            g_rgbImage[i*2] = g_rgbHalf[i];

        sprintf(szInFile, "%s-h.bin", szInputSource);
        if (!LoadFile(szInFile, g_rgbHalf, cbImage/2))
            return -1;
        for (i = 0; i < cbImage/2; i++)
            g_rgbImage[i*2+1] = g_rgbHalf[i];
    }

    // now connect to ICybie
	HANDLE hSerial = OpenSerial(iPort);
    if (hSerial == (HANDLE)-1)
    {
        fprintf(stderr, "Failed to open serial port COM%d\n", iPort);
        exit(-1);
    }

    if (!SetCommDefaults(hSerial))
    {
        fprintf(stderr, "Failed to set COM%d to 9600 baud\n", iPort);
        CloseSerial(hSerial);
        return -1;
    }

    if (bVerbose)
	    printf("Ready to connect to ICybie, please reset SuperICybie now\n");

    WaitForCromConnect(hSerial, bVerbose);

    if (!bSlow)
    {
	    if (bVerbose)
	    {
		    printf("\nConnected!\n");
		    printf("Setting baud rate to 38.4kbps");
	    }
	
	    if (!SetFastCrom(hSerial))
	    {
	        if (bVerbose)
		        printf(" FAILED!\n");
	        fprintf(stderr, "Failed to increase baud rate to 38400\n");
	        CloseSerial(hSerial);
	        return -1;
	    }
		if (bVerbose)
		    printf("\n");
	}


    if (bVerbose)
	    printf("sending from '%s' to ROM at $%06X (%dKB)%s\n",
	         szInputSource, baseAddr, cbImage/1024,
             bSmart ? " - SMART" : "");

    for (long addrNow = baseAddr;
      addrNow < baseAddr + cbImage;
       addrNow += BLOCK_SIZE)
    {

        if (bVerbose)
            printf("Block at $%06X : ", addrNow);

	    BYTE bErr;
	    if ((bErr = GetErrorCount(hSerial)) != 0)
	    {
	        fprintf(stderr,
                "\nFATAL: Error count = %d before block transfer\n", bErr);
	        CloseSerial(hSerial);
		    exit(-1);
	    }

	    BYTE* pbNow = &g_rgbImage[addrNow - baseAddr];
	    WORD wSumImage = CalcChecksum(pbNow, BLOCK_SIZE);
	
	    SetAddress(hSerial, addrNow);
        if (bSmart)
        {
			WORD wSumBefore = GetChecksumFromICybie(hSerial, BLOCK_SIZE);
            if (wSumBefore == wSumImage)
            {
                printf("skipped\n");
                continue;
            }
        }
	
        if (bVerbose)
		    printf("Erasing");
	    if (!EraseBlock(hSerial))
	    {
	        fprintf(stderr, "\nFATAL: erasing operation did not ACK\n");
	        CloseSerial(hSerial);
		    exit(-1);
	    }

		if (GetChecksumFromICybie(hSerial, BLOCK_SIZE) != 0xF000)
        {
	        if (bVerbose)
			    printf("\010\010\010e error");
	        fprintf(stderr, "\nFATAL: erase failed - cartridge may be missing\n");
	        CloseSerial(hSerial);
		    exit(-1);
        }
        if (bVerbose)
		    printf("\010\010\010ed. ");
	
        if (bVerbose)
		    printf("Writing...");
		if (!WriteToICybie(hSerial, pbNow, BLOCK_SIZE, bVerbose))
	    {
	        fprintf(stderr, "\nFATAL: writing operation did not ACK\n");
	        CloseSerial(hSerial);
		    exit(-1);
	    }
        if (bVerbose)
		    printf(". ");
	    
	    // just checksum the result
		WORD wSumNow = GetChecksumFromICybie(hSerial, BLOCK_SIZE);
	    if (wSumNow != wSumImage)
	    {
	        fprintf(stderr, "\nFATAL: Checksum after write bogus\n");
	        exit(-1);
	    }
	
        if (bVerbose)
		    printf("OK!\n");

    }

    //BLOCK: one final error check
    {
	    BYTE bErr;
	    if ((bErr = GetErrorCount(hSerial)) != 0)
	    {
	        fprintf(stderr, "\nFATAL: Error count = %d after writing\n", bErr);
	        CloseSerial(hSerial);
            exit(-1);
	    }
    }

    if (bVerbose)
    {
	    printf("Transfer complete\n");
    }
	
    if (bRunAfter)
    {
        if (bVerbose)
	        printf("Running program ");
	    RestartAfterLoad(hSerial);

        if (bVerbose)
	        printf("(hit Ctrl+C to stop serial dump)\n");
        bool bRun = true;
        while (bRun)
        {
            BYTE b;
            // data from ICybie ?
            while (PeekSerialByte(hSerial, b))
            {
                if (b == 0x1A)
                    bRun = false;  // Ctrl-Z to abort
		        printf("%c", b);
            }
            if (_kbhit())
            {
                b = _getch();
				SendSerialBytes(hSerial, &b, 1);
            }
            Sleep(50);  // don't eat up all the CPU
        }
    }
	CloseSerial(hSerial);
	
    return 0;
}


