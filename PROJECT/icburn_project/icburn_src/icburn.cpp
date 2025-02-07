// my downloader.exe program
// not open source since SilverLit has not publicly published interface

#include "std.h"

#include "downloader.h"

static void PrUsage()
{
    printf("usage: icburn [flags] base_filename\n");
    printf("\t(version 0.01\n");
    printf("\tSends image file to cartridge via Downloader\n");
    printf("\tDownloader with cartridge must be attached to parallel port\n");
    printf("\tinput files:\n");
    printf("\t\tbase_filename-l.bin = lower half\n");
    printf("\t\tbase_filename-h.bin = upper half\n");
    printf("\t\t(or) filename.ic3 = all in one file\n");
    printf("\tflags:\n");
    printf("\t\t-t trust-me mode (don't verify)\n");
    exit(-1);
}

bool ReInstall()
{
    RemoveWinIoDriver();    // remove old one

    char szHere[_MAX_PATH];
    //BLOCK: find directory where .EXE resides
    {
	    char szMod[_MAX_PATH];
	    GetModuleFileName(NULL, szMod, sizeof(szMod));
	    char szDrive[_MAX_DRIVE];
	    char szDir[_MAX_DIR];
        _splitpath(szMod, szDrive, szDir, NULL, NULL);
        _makepath(szHere, szDrive, szDir, NULL, NULL);
    }

    if (!InstallWinIoDriver(szHere, false))
    {
        printf("Failed to install WINIO\n");
        printf(" must be logged in as Administrator\n");
        return false;
    }
    printf("WINIO installed\n");
    return true;
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

void ProgressProc(int progress)
{
	printf(".");
}

int main(int argc, char* argv[])
{
    bool doVerify = true;

    while (argc > 2)
    {
        // process flags
        const char* szFlag = argv[1];
        argv++;
        argc--;

	    if (strcmp(szFlag, "-t") == 0)
            doVerify = false;
        else
            PrUsage();  // bad flag
    }
    
    if (argc != 2)
        PrUsage();
    const char* szInputSource = argv[1]; // file or file prefix

    const int cbImage = 256*1024;

    const char* szExt = strchr(szInputSource, '.');
    if (szExt != NULL && strcmpi(szExt, ".ic3") == 0)
    {
        if (!LoadFile(szInputSource, g_rgbImage, cbImage))
            return -1;
    }
    else if (szExt == NULL)
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
    else
    {
        fprintf(stderr, "FATAL: can't determine file type '%s'\n", szInputSource);
        exit(-1);
    }
    

    // WINIO
    if (!InitializeWinIo())
    {
        printf("Failed to initialize WINIO - trying to [re-]install\n");
        if (!ReInstall())
        {
            printf("Re-install failed\n");
            return -1;
        }
	    if (!InitializeWinIo())
        {
	        printf("Still failed to initialize - can't run WINIO\n");
            return -1;
        }
    }

    printf("Downloader:");
    if (!DOWNLOADER::Init())
    {
        printf(" -- ERROR\n");
        printf("Downloader init failed\n");
        printf(" (is it plugged in and turned on?)\n");
        Sleep(1000);
	    ShutdownWinIo();
        return -1;
    }
    printf(" initialized");

    // do it twice (lower then upper)
    if (!DOWNLOADER::EraseChip() || !DOWNLOADER::EraseChip())
    {
        printf(" -- ERROR\n");
	    printf("Erase failed (timeout)\n");
	    ShutdownWinIo();
	    return -1;
    }
    printf(", erased, writing: ");

    if (!DOWNLOADER::WriteChip(g_rgbImage, ProgressProc, doVerify))
    {
        printf(" -- ERROR\n");
		printf("WriteChip failed\n");
        if (doVerify)
            printf("Parallel port many not work bidirectionally. Try '-t' switch\n");
	    ShutdownWinIo();
        return -1;
    }

    printf("-- WRITTEN!\n");

    ShutdownWinIo();
	
    return 0;
}


