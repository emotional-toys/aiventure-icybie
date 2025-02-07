// Requires CROM 1.1

#include "std.h"

#include "siclib.h"

FILE* pfSicLog = NULL;

HANDLE OpenSerial(int iPort)
{
    char szPort[32];
    sprintf(szPort, "COM%d", iPort);

    HANDLE h = CreateFile(szPort,
        GENERIC_READ | GENERIC_WRITE,
		0,                    // exclusive access
        NULL,                 // no security attrs
        OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL,
        NULL);

    if (pfSicLog != NULL)
        fprintf(pfSicLog, "Open Serial %s returned $%x\n", szPort, h);

    return h;
}

void CloseSerial(HANDLE hSerial)
{
    CloseHandle(hSerial);
}

static bool SetCommDefaultsPart1(HANDLE hSerial, int baud)
{
	DCB        dcb;
    memset(&dcb, 0, sizeof(DCB));
	dcb.DCBlength = sizeof(DCB);

    dcb.fBinary = TRUE;
    dcb.fParity = TRUE;
	dcb.BaudRate = baud;
	dcb.ByteSize = 8;
    dcb.Parity = NOPARITY;
    dcb.StopBits = ONESTOPBIT;

	dcb.fOutxDsrFlow = FALSE;
	dcb.fDtrControl = DTR_CONTROL_ENABLE;
	dcb.fOutxCtsFlow = FALSE;
	dcb.fRtsControl = RTS_CONTROL_ENABLE;

    dcb.fInX = dcb.fOutX = FALSE; // no XON/XOFF

    if (!SetCommState(hSerial, &dcb))
        return false;
    return true;
}

bool SetCommDefaults(HANDLE hSerial)
{
    if (!SetCommDefaultsPart1(hSerial, CBR_9600))
        return false;

    // set no timeouts
    COMMTIMEOUTS to;
    memset(&to, 0, sizeof(to));
    to.ReadIntervalTimeout = MAXDWORD;  // return immediately if no data
    if (!SetCommTimeouts(hSerial, &to))
        return false;

    // purge all
    PurgeComm(hSerial,
      PURGE_TXABORT | PURGE_RXABORT | PURGE_TXCLEAR | PURGE_RXCLEAR);
    return true;
}


void PurgeSerial(HANDLE hSerial)
{
    while (1)
    {
	    BYTE b;
	    DWORD cbRead;
        if (!ReadFile(hSerial, &b, 1, &cbRead, NULL))
        {
	        fprintf(stderr, "FATAL: Purge failed for serial port\n");
            exit(-1);
        }
        if (cbRead == 0)
            break;  // empty
	    if (pfSicLog != NULL)
	        fprintf(pfSicLog, "PURGING byte $%02X\n", b);
    }
}

///////////////////////////////////////////////////////////
// Serial helpers

bool PeekSerialByte(HANDLE hSerial, BYTE& bRet) // no logging
{
	DWORD cbRead;
    if (!ReadFile(hSerial, &bRet, 1, &cbRead, NULL))
    {
	    fprintf(stderr, "FATAL: ReadFile failed for serial port\n");
        exit(-1);
    }
    if (cbRead == 1)
        return true;
    return false;   // no data
}

BYTE GetSerialByte(HANDLE hSerial)
{
    if (pfSicLog != NULL)
    {
        fprintf(pfSicLog, "\tGET BYTE ");
        fflush(pfSicLog);   // before blocking call
    }

	BYTE b;
    int nRetry = 0;
    while (!PeekSerialByte(hSerial, b))
    {
        nRetry++;
    }
    if (pfSicLog != NULL)
        fprintf(pfSicLog, "%02X %c [%d retries]\n",
            b, (b >= ' ' && b < 0x7F) ? b : ' ', nRetry);

    return b;
}

// code assumes same byte ordering (Intel = ICybie)
void SendSerialBytes(HANDLE hSerial, const BYTE* pb, int cb)
{
    DWORD cbWrite;
    if (!WriteFile(hSerial, pb, cb, &cbWrite, NULL) || cbWrite != cb)
    {
        fprintf(stderr, "FATAL: WriteFile failed for serial port\n");
        exit(-1);
    }
    if (pfSicLog != NULL)
    {
	    for (int ib = 0; ib < cb; ib++)
        {
            BYTE b = pb[ib];
	        fprintf(pfSicLog, "\tSEND BYTE %02X %c\n", b,
				(b >= ' ' && b < 0x7F) ? b : ' ');
        }
    }
}

static void SendSerialString(HANDLE hSerial, const char* sz)
{
    SendSerialBytes(hSerial, (const BYTE*)sz, strlen(sz));
}

static void SendSerialWord(HANDLE hSerial, WORD w)
{
    SendSerialBytes(hSerial, (const BYTE*)&w, 2);
}

static void DumpByte(BYTE b)
{
    if ((b >= ' ' && b < 0x7f) || b == '\r' || b == '\n')
        printf("%c", b);
    else
        printf("$%02X", b); // hex dump of non-printable ASCII
}


///////////////////////////////////////////////////////////

bool WaitForCromConnect(HANDLE hSerial, bool bVerbose)
{
    const char* szMatch = "#CROM11#"; // start sequence
    const char* pchWaitingFor = szMatch;
    while (*pchWaitingFor != '\0')
    {
        // get BYTE from serial port
        BYTE b = GetSerialByte(hSerial);

        if (b == *pchWaitingFor)
        {
            // getting ready to start
            pchWaitingFor++;
        }
        else if (bVerbose)
        {
            DumpByte(b);
		    pchWaitingFor = szMatch; // start over
        }
    }
    if (pfSicLog != NULL)
        fprintf(pfSicLog, "CROM CONNECTED\n");
    // got the start sequence, now reply
    SendSerialString(hSerial, "[UPL]");


    // CROM will send back a string (ending with '\0')
    while (1)
    {
        BYTE b = GetSerialByte(hSerial);
        if (bVerbose)
            DumpByte(b);
        if (b == '\n')
            break;  // after first line
    }

	if (pfSicLog != NULL)
		fprintf(pfSicLog, "CROM CONNECTED 2\n");
    return true; // connected
}


BYTE GetErrorCount(HANDLE hSerial)
{
	PurgeSerial(hSerial);
	if (pfSicLog != NULL)
		fprintf(pfSicLog, "GETERRORCOUNT\n");
    SendSerialString(hSerial, "!?");
    
    return GetSerialByte(hSerial);
}

void SetAddress(HANDLE hSerial, long address)
{
	if (pfSicLog != NULL)
		fprintf(pfSicLog, "SETADDRESS $%06X\n", address);
	PurgeSerial(hSerial);
    SendSerialString(hSerial, "!A");
    assert((address & 0xFFFFFE) == address);

    SendSerialBytes(hSerial, (const BYTE*)&address, 3);  // lower 3 bytes
}

void ReadFromICybie(HANDLE hSerial, BYTE* pb, int cb)
{
	if (pfSicLog != NULL)
		fprintf(pfSicLog, "READFROMICYBIE cb=%d\n", cb);
    assert(cb > 0);
    assert(cb <= BLOCK_SIZE); // can go up to 64K-2 actually
    assert((cb & 1) == 0); // even number of WORDs

	PurgeSerial(hSerial);
    SendSerialString(hSerial, "!R");
    SendSerialWord(hSerial, cb/2); // WORD count

    while (cb--)
        *pb++ = GetSerialByte(hSerial);
}

bool EraseBlock(HANDLE hSerial)
{
    SendSerialString(hSerial, "!E~");
        // check error count to see if it worked

    // wait for ack byte
    BYTE bAck = GetSerialByte(hSerial);
    return bAck == '$';
}

static bool IsBlockEmpty(const BYTE* pb)
{
    for (int i = 0; i < 256; i++)
        if (pb[i] != 0xFF)
            return false;
    return true;
}

bool WriteToICybie(HANDLE hSerial, const BYTE* pb, int cb, bool bVerbose)
{
	if (pfSicLog != NULL)
		fprintf(pfSicLog, "WRITETOICYBIE cb=%d\n", cb);
    assert(cb > 0);
    assert(cb <= BLOCK_SIZE); // can be larger
    assert((cb & 255) == 0); // even number of 256 byte blocks

    int cblockActual = cb/256;
    while (cblockActual > 0 &&
      IsBlockEmpty(&pb[(cblockActual-1)*256]))
        cblockActual--;

    if (cblockActual == 0)
    {
        // all blank
        return true;
    }

	PurgeSerial(hSerial);
    SendSerialString(hSerial, "!W");
    SendSerialWord(hSerial, cblockActual);
    SendSerialString(hSerial, "~");

    while (cblockActual--)
    {
	    BYTE bReady = GetSerialByte(hSerial);
	    if (bReady != '.')
        {
	        fprintf(stderr, "FATAL: write sequence failed - no ready\n");
	        exit(-1);
        }
        if (bVerbose)
			printf("+");
	    SendSerialBytes(hSerial, pb, 256);
        pb += 256;
    }

    BYTE bAck = GetSerialByte(hSerial);
    if (bAck != '$')
    {
        return false;
	        fprintf(stderr, "FATAL: write sequence failed - no final ack\n");
        exit(-1);
    }
	if (bVerbose)
	    printf("$");
    return true;
}

void RestartAfterLoad(HANDLE hSerial)
{
    SendSerialString(hSerial, "!X~");
}


WORD GetChecksumFromICybie(HANDLE hSerial, int cb)
{
    assert(cb == BLOCK_SIZE); // for our purposes

	PurgeSerial(hSerial);
    SendSerialString(hSerial, "!C");
    SendSerialWord(hSerial, cb/2); // WORD count
	BYTE b1 = GetSerialByte(hSerial);
	BYTE b2 = GetSerialByte(hSerial);
    return MAKEWORD(b1, b2);
}

WORD CalcChecksum(const BYTE* pb, int cb)
{
    assert((cb & 1) == 0);
    int cw = cb/2;
    const WORD* pw = (const WORD*)pb;
    WORD sum = 0;
    while (cw--)
        sum += *pw++;
    return sum;
}

bool SetFastCrom(HANDLE hSerial)
{
    byte rgbReq[2+2+1];
    memcpy(rgbReq, "!U??~", 5);

    // 38372 Kbps
    rgbReq[2] = 0x40 + 10;      // SFR_BR0CR value
    rgbReq[3] = 4;              // SFR_BR0ADD value
    SendSerialBytes(hSerial, rgbReq, sizeof(rgbReq));

    Sleep(50);

    if (!SetCommDefaultsPart1(hSerial, CBR_38400))
        return false;

    Sleep(50);
    // send '$' at new baud rate to ack
    SendSerialString(hSerial, "$");
    return true;
}
