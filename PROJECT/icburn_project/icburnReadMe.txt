December 2013
How to use ICBurn with the Downloader to burn the bootloader (CROMINST)

In general the serial interface to a Super-iCybie is more reliable than the parallel port Downloader. It lets you read the contents of a cartridge (using "SICGRAB.EXE") as well as program it (using "SICBURN.EXE"). It is also more universal since it doesn't require a true parallel port on the computer (which are getting hard to find) USB->Parallel port adapters do not work well in this scenario.
--
However if you have your heart set on the parallel port approach, the source code to "ICBURN" is included--foldername icburn_src. The source to SICBURN is in the SDK http://www.aibohack.com/icybie/icsdk.htm. 

("ICBURN" for parallel port, "SICBURN" for serial port).

ICBURN is derived from source code SilverLit gave me a long time ago. It uses the "WinIO" library to get raw access to the parallel port bits. The one included here is pretty old, it is version 2.0.0.0. Check the Web for something more up-to-date. NOTE: This is from 10 years ago. Direct bit-banging on the parallel port was old even back then.

For “icburn”:

usage: icburn [flags] base_filename
     version 0.01
     Sends image file to cartridge via Downloader
     Downloader with cartridge must be attached to parallel port
     input files:
           base_filename-l.bin = lower half
           base_filename-h.bin = upper half
           (or) filename.ic3 = all in one file
     flags:
           -t trust-me mode (don't verify)
 
So use “icburn crominst” where the two BIN files are in the current folder. ICBURN is more reliable than the SilverLit downloader, but it depends on your parallel port hardware. Do you have an official downloader (with parallel port) from SilverLit? If yes, then you need to install the CROMINST program on the working cartridge. This will replace the walkup charger personality for a short period of time. You can do this with the official SilverLit downloader software or my “icburn” program. You need a computer with a real parallel port (not one with a USB adapter). You use the CROMINST program *ONCE* to patch the system ROM. Then you put the Walkup personality back on the cartridge (or even better use the ZCybie personality)
