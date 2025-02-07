// ICSDK2 - ICybie Library
// basic types, helper macros, common routines

// For the Toshiba CPU:
//  'int' is 16 bits, use byte/sbyte if you can
//  'long' is 32 bits, use only if necessary
//  pointers (to RAM data, ROM data or code) are 32 bits (24 bits significant)

typedef unsigned char byte; // 8 bit byte
typedef signed char sbyte;  // 8 bit signed byte
typedef unsigned short word;
typedef unsigned long ulong;

#define HIBYTE(v)       ((byte)((v)>>8))
#define LOBYTE(v)       ((byte)(v))

typedef unsigned char bool; // it fits into a byte
typedef enum { false, true } bool_enum;

#define NULL        ((void*)0)

#define ADECL __adecl
        // '__adecl' for register calling convention

////////// ////////// ////////// ////////// //////////
// use 'ic_assert' frequently

#ifdef ICL_DEBUG
void ADECL ic_assert_failed(const char* file, int line);
#define ic_assert(cond) { if (!(cond)) ic_assert_failed(__FILE__, __LINE__); }
#else
#define ic_assert(cond) { }
#endif

////////// ////////// ////////// ////////// //////////
