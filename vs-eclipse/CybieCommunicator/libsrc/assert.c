// ICSDK2 - Library
// simple debug assert

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW
// Routine called when an assert ('ic_assert') fails.
// A message is printed and ICybie shuts down

/////////////////////////////////////////////////////////

void ADECL ic_assert_failed(const char* file, int line)
{
	__asm ( "EI 6" ); // enable interrupts for serial
    printf("ASSERT FAILED in file %s, line %d\n", file, line);
    ic_shutdown();
}

/////////////////////////////////////////////////////////
