#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

typedef unsigned char byte;
typedef unsigned short word;
typedef unsigned long ulong;

////////////////////////////////////////////

#define CB_CART (256*1024)

byte g_image[CB_CART];
byte g_low[CB_CART/2];
byte g_high[CB_CART/2];

////////////////////////////////////////////

int main(int argc, char* argv[])
{

    assert(argc == 4);

    printf("Splitting %s -> %s %s\n", argv[1], argv[2], argv[3]);
    FILE* pf = fopen(argv[1], "rb");
    assert(pf);
    assert(fread(g_image, sizeof(g_image), 1, pf) == 1);
    fclose(pf);

    for (int i = 0; i < CB_CART/2; i++)
    {
        g_low[i] = g_image[i*2];
        g_high[i] = g_image[i*2+1];
    }

    // lobyte
    pf = fopen(argv[2], "wb");
    assert(pf);
    assert(fwrite(g_low, sizeof(g_low), 1, pf) == 1);
    fclose(pf);

    // hibyte
    pf = fopen(argv[3], "wb");
    assert(pf);
    assert(fwrite(g_high, sizeof(g_high), 1, pf) == 1);
    fclose(pf);

    return 0;
}

