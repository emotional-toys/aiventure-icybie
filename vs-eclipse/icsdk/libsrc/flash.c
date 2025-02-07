// ICSDK2
// flash ROM access - DANGEROUS!!

#include "icpriv.h"

/////////////////////////////////////////////////////////
//OVERVIEW:
// Very dangerous routine that lets you write over the first 64KB of
//  cartridge ROM. Useful for some debugging purposes, but not generally
//  recommended.
//LATER: can be made more general - but still dangerous

/////////////////////////////////////////////////////////
// data variables

static byte __near g_code_buffer[60];
	//LATER: move to shared buffer - for use in shutdown/restart too

/////////////////////////////////////////////////////////
// IMPLEMENTATION: low level cart writer

// must be copied and run from RAM
typedef void ADECL (*WRITE_PROC)(long addr, word data);

void ADECL _Write_Cart_Word_Routine(long addr, word data)
{
    // __asm("._Write_Cart_Word_Routine:");
    __asm("  EI 7 ; CRITICAL!\n");
	__asm("  LDL XIX,0x20AAAA ; 5555*2");
	__asm("  LDL XIY,0x205554 ; 2AAA*2");
	__asm("  LDW (XIX),0xAAAA ; 5555 <- AA");
	__asm("  LDW (XIY),0x5555 ; 2AAA <- 55");
	__asm("  LDW (XIX),0xA0A0 ; 5555 <- A0 ; byte program");
	__asm("  LDW (XWA),BC ; data");
	__asm("__loop_flash2: ; chip busy while burning");
    __asm("  LDL XIX,0x200000");
    __asm("  LDW WA,(XIX)");
    __asm("  NOP");
    __asm("  LDL XIX,0x200000");
	__asm("  LDW HL,(XIX)");
	__asm("  XOR WA,HL");
	__asm("  AND WA,0x4040");
	__asm("  CP WA,0");
	__asm("  JR NZ,__loop_flash2");
    __asm("  EI 0 ; done\n");
}

ADECL _Write_Cart_Word_Routine_END() // must immediately follow
{
}

/////////////////////////////////////////////////////////
// PUBLIC

void ADECL ic_write_flash_word(word offset, word data)
{
    long byteCount;

    if ((offset & 1) || offset == 0 || offset == 0xFFFE)
    {
	    ic_assert(false);   // bogus request
        return; // don't continue if non-debug version
    }

    byteCount = (byte*)_Write_Cart_Word_Routine_END -
        (byte*)_Write_Cart_Word_Routine;

    ic_assert(byteCount <= sizeof(g_code_buffer));
    ic_memcpy(g_code_buffer, (byte*)_Write_Cart_Word_Routine, (int)byteCount);

    // copy routine to RAM (can be slow if repeated)
    (*(WRITE_PROC)g_code_buffer)(0x200000 + offset, data);
}

/////////////////////////////////////////////////////////

