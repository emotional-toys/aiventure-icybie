// tool to generate sine wave lookup table

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <math.h>

////////////////////////////////////////////

#define PI 3.14159265358979

int main(int argc, char* argv[])
{
    const int NUMSAMP = 256; // for 2*PI radians (makes easy to lookup)
    
    for (int i = 0; i < NUMSAMP; i++)
    {
        if ((i & 15) == 0)
            printf("\t");
        else
            printf(",");
        double theta = (2*PI * i) / NUMSAMP;
        int samp = 127 * sin(theta);
        printf("%d", samp);
        if ((i & 15) == 15)
            printf("\n");
    }

    return 0;
}

