// ICSDK2 - Library

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// A very lame printf that prints to the serial port.
// Designed to be simple and small (in code size), not have
//  all the bells and whistles of a regular printf.

/////////////////////////////////////////////////////////
// lame printf

void printf(const char* format, ...)
{
    byte* argPtr = (byte*)&format + sizeof(format);
        // stack grows down, args go up

    char ch;
    while ((ch = *format++) != '\0')
    {
        if (ch == '%')
        {
            bool bLong = false;

            ch = *format++;
            if (ch == 'l')
            {
                bLong = true;
	            ch = *format++;
            }

            if (ch == 'c')
            {
                int arg = *argPtr; // lowbyte
                argPtr += sizeof(int); // chars are pushed as 2 byte ints
                sic_send_serial_byte((byte)arg);
            }
            else if (ch == 'd')
            {
                long arg;
                bool bEmit = false;
                int i;

                if (bLong)
                {
                    arg = *(long*)argPtr;
                    argPtr += sizeof(long);
                }
                else
                {
                    arg = *(int*)argPtr;
                    argPtr += sizeof(int);
                }

                if (arg < 0)
                {
			        sic_send_serial_byte('-');
                    arg = -arg;
                }

                for (i = 0; i < 10; i++)
                {
                    // long can go up to +-2billion
                    static const long factors[] =
	                    { 1E9, 1E8, 1E7, 1E6, 1E5, 1E4, 1E3, 100, 10, 1 };
                    long factor = factors[i];

					byte digit = 0;
                    // cheesy divide & remainder
                    while (arg >= factor)
                    {
                        digit++;
                        arg -= factor;
                    }
                    if (digit > 0)
                    {
		                sic_send_serial_byte((char)('0' + digit));
                        bEmit = true;
                    }
                    else if (bEmit || factor == 1)
                    {
		                sic_send_serial_byte('0');
                    }
                }
            }
            else if (ch == 'x')
            {
                // XXXX or XXXXXXXX only
                ulong arg; // unsigned
                int i;

                if (bLong)
                {
                    arg = *(ulong*)argPtr;
                    argPtr += sizeof(ulong);
                    i = 7;  // always 8 hex digits
                }
                else
                {
                    arg = *(word*)argPtr;
                    argPtr += sizeof(word);
                    i = 3;  // always 4 hex digits
                }

                for (; i >= 0; i--)
                {
                    ulong factor = 1L << (i*4);

					byte digit = 0;
                    // cheesy divide & remainder
                    while (arg >= factor)
                    {
                        digit++;
                        arg -= factor;
                    }
                    if (digit > 9)
		                sic_send_serial_byte((char)('A' + digit - 10));
                    else
		                sic_send_serial_byte((char)('0' + digit));
                }
            }
            else if (ch == 's')
            {
                const char* sz = *(const char**)argPtr;
                argPtr += sizeof(const char*);
                while ((ch = *sz++) != '\0')
			        sic_send_serial_byte(ch);
            }
            else if (ch == '%')
            {
		        sic_send_serial_byte(ch);
            }
            else
            {
                // pass it through
		        sic_send_serial_byte('%');
		        sic_send_serial_byte(ch);
            }
            
        }
        else if (ch == '\n')
        {
            // CR+LF
	        sic_send_serial_byte(13);
	        sic_send_serial_byte(10);
        }
        else
        {
	        sic_send_serial_byte(ch);
        }
    }
}

/////////////////////////////////////////////////////////
