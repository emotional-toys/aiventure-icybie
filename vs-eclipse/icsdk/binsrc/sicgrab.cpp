// SICGRAB - grab the Super-ICybie ROM or Cartridge ROM
// (get ROM image from ICybie to PC)
// formerly called 'sicdown'


#include "std.h"

#include "siclib.h"


static void PrUsage()
{
    printf("usage: sicgrab [flags] file.bin\n");
    printf("\tGrabs entire 256K from ICybie Flash ROM\n");
    printf("\tflags:\n");
    printf("\t\t-2 use COM2 instead of COM1\n");
    printf("\t\t-c use cartridge instead of system ROM\n");
    exit(-1);
}


int main(int argc, char* argv[])
{
    int iPort = 1;  // default COM1
    bool bCart = false;

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
	    else if (strcmp(szFlag, "-c") == 0)
            bCart = true;
	    else if (strcmp(szFlag, "-l") == 0)
			pfSicLog = fopen("siclog.out", "wt");
        else
            PrUsage();  // bad flag
    }
    
    if (argc != 2)
        PrUsage();
    const char* szOutFile = argv[1];


	HANDLE hSerial = OpenSerial(iPort);
    if (hSerial == (HANDLE)-1)
    {
        fprintf(stderr, "Failed to open serial port COM%d\n", iPort);
        return -1;
    }

    if (!SetCommDefaults(hSerial))
    {
        fprintf(stderr, "Failed to set COM%d to 9600 baud\n", iPort);
        CloseSerial(hSerial);
        return -1;
    }

    FILE* pfOut = fopen(szOutFile, "wb");
    if (pfOut == NULL)
    {
        fprintf(stderr, "ERROR: failed to create output file '%s'\n", szOutFile);
        exit(-1);
    }

    printf("Ready to connect to ICybie, please reset SuperICybie now\n");

    WaitForCromConnect(hSerial, true);

    printf("Setting baud rate to 38.4kbps");
	
    if (!SetFastCrom(hSerial))
    {
		printf(" FAILED!\n");
		CloseSerial(hSerial);
        return -1;
    }
    printf("\n");

    printf("grabbing 256K from %s to '%s'\n",
        bCart ? "Cartridge" : "SystemROM", szOutFile);

    long base_address = (bCart) ? 0x200000 : 0xFC0000;

    BYTE bLastErr = 0;
    for (int iBlock = 0; iBlock < NUM_BLOCKS_256K; iBlock++)
    {
	    BYTE rgbData[BLOCK_SIZE];
        long address = base_address + BLOCK_SIZE * iBlock;
        printf("BLOCK %2d $%06X\n", iBlock, address);
        BYTE bErr = GetErrorCount(hSerial);
        if (bErr != bLastErr)
        {
            fprintf(stderr, "CROM WARNING: serial %d errors so far\n", bErr);
            bLastErr = bErr;
        }
        
        SetAddress(hSerial, address);
        ReadFromICybie(hSerial, rgbData, BLOCK_SIZE);

	    if (fwrite(rgbData, sizeof(rgbData), 1, pfOut) != 1)
        {
            fprintf(stderr, "ERROR: Write data failed - aborting\n");
            exit(-1);
        }
        fflush(pfOut);
    }

	CloseSerial(hSerial);

    // now write the file
    if (fclose(pfOut) != 0)
    {
        fprintf(stderr, "ERROR: Write data failed - aborting\n");
        exit(-1);
    }

    printf("saved image to to %s\n", szOutFile);
    return 0;
}


