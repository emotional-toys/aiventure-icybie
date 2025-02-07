#include "icpriv.h"

void ADECL ic_memset(byte* destPtr, byte val, int byteCount)
{
    while (byteCount--)
        *destPtr++ = val;
}

