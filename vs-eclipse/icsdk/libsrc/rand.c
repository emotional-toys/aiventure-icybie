// ICSDK2 - Library

#include "icpriv.h"

static word __near g_seed;

void srand(unsigned int seed)
{
	g_seed = seed;
}

int rand(void)
{
    g_seed = g_seed * 31415 + 27181; // very lame numbers
	return (int)(g_seed % RAND_MAX);
}

