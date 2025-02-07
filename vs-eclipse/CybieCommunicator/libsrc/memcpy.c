#include "icpriv.h"

void ADECL ic_memcpy(byte* destPtr, const byte* srcPtr, int byteCount)
{
    while (byteCount--)
        *destPtr++ = *srcPtr++;
}

