#define BLOCK_SIZE      8192
#define NUM_BLOCKS_256K 32

HANDLE OpenSerial(int iPort);
void CloseSerial(HANDLE hSerial);
bool SetCommDefaults(HANDLE hSerial);
void PurgeSerial(HANDLE hSerial);

bool WaitForCromConnect(HANDLE hSerial, bool bVerbose);

BYTE GetErrorCount(HANDLE hSerial);

void SetAddress(HANDLE hSerial, long address);
void ReadFromICybie(HANDLE hSerial, BYTE* pb, int cb);
bool EraseBlock(HANDLE hSerial);
bool WriteToICybie(HANDLE hSerial, const BYTE* pb, int cb, bool bVerbose);

void RestartAfterLoad(HANDLE hSerial);

extern FILE* pfSicLog;

// advanced
BYTE GetSerialByte(HANDLE hSerial);
bool PeekSerialByte(HANDLE hSerial, BYTE& bRet); // no logging
void SendSerialBytes(HANDLE hSerial, const BYTE* pb, int cb);

// checksums
WORD GetChecksumFromICybie(HANDLE hSerial, int cb);
WORD CalcChecksum(const BYTE* pb, int cb);

bool SetFastCrom(HANDLE hSerial); // added in 1.1
