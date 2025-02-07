Source code to HIMAGE

Derived from originally copyrighted material
Modifications are copyrighted (c) 2002-2003 ICybiePet

-----------
Brief notes:

Requires the "ASW" assembler to build
(provided in the BIN subdirectory)

See make.bat for build operation.
See check.bat for base comparison

-----------
NOTE: code is arranged like the original binary image.
When built using 'check.bat' it should produce the
exact same binary as the original binary from SilverLit.

-----------
Interesting files:

newcart.a - main assembly file
    (includes all the rest)
    *.i - includes
	*.a_ - continuation of code
	*.dat - data structures

bin\*.* - tools needed to build

walkup8a.car - original cartridge for comparison (see check.bat)
