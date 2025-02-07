======================================================================
ICSDK2: Implementation Notes (C) 2002 ICybiePet
======================================================================

Overview:
=========

This directory contains various technical notes on the hardware
and software of the ICybie and ICSDK Library (ICLib2).

Of particular interest is the Toshiba CPU (ICybie's main CPU),
and how it is used in ICybie.

For more details, see the source code to the ICLib2 (ICybie Library).

======================================================================
Hardware:
=========

The ICybie has 3 CPUs, the main Toshiba CPU, a SunSite daughter CPU
for sound playback (and other features) and finally the RSC-300 voice
recognition CPU (in ICybie's head).

The main features are in the Toshiba CPU, which has 8KB of RAM,
256KB of system ROM, 256KB of optional cartridge ROM, 128 bytes of E2PROM,
and many peripherals including interfaces to the other two CPUs.

The main CPU can controlled by writing programs that run from
the cartridge. The ICSDK provides the tools, library and samples
for these programs.

The other two CPUs (SunSite and RSC-300) are not user programmable,
and must be used as-is.

Hardware notes:
	ports.txt - SFR and I/O port usage
	e2prom.txt - access to 64 byte E2PROM
	timers.txt - CPU timer usage
	misc.txt - serial port, ADC, etc

    irlogic.txt - IR interface logic
    ircodes.txt - IR codes
    [NOT WRITTEN YET]

	rsc300.txt - interface to the RSC-300 voice recognition CPU
	sunplus.txt - interface to the SunPlus daughter CPU

======================================================================
Software/Firmware:
==================

The software/firmware for the Toshiba CPU comes from two locations,
the System ROM and the (optional) Cartridge ROM.

Both ROMs are 256KB in size (for a maximum of 512KB of ROM)

Compatibility with the System ROM raises many restrictions on
the layout of memory (ROM and RAM). These are discussed in the
following technical notes.

    memmap.txt - general memory map and usage restrictions

    most other documentation is in the "IcLib2" source code
        (see libsrc\readme.txt)

======================================================================
