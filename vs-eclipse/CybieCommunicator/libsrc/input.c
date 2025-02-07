// ICSDK2 - Library

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Lame input for a decimal number with limited editing (backspace)

/////////////////////////////////////////////////////////
// lame input

int sic_input(const char* prompt)
     // actually unsigned
{
    // very simple number input (no editing)
    int val = 0;
    printf("%s: ", prompt);
    while (1)
    {
        char ch = sic_get_serial_byte();
        if (ch == '\r')
        {
            break;
        }
        else if (ch == '\010')
        {
            // backspace
            val = val / 10;
            printf("\010 \010"); // delete character
        }
        else if (ch >= '0' && ch <= '9')
        {
	        val = val * 10 + ch - '0';
	        printf("%c", ch);   // feedback only on valid characters
        }
        else
        {
            printf("\007"); // beep (no echo)
        }
    }
    printf("\n");
	return val;
}

