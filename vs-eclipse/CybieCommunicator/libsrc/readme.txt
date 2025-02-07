TO BUILD:
    "m" = m.bat = build
    "m clean" = remove temp files

-----------------------------------------------------------------
NOTES:
    Read the "OVERVIEW" at the top of each source file.
    Read any related "NOTES\*.TXT" files.

    Items marked with "REVIEW" are items needing fixing before
            a real ICSDK release is made.
    Items marked with "LATER" need work, but can wait until later.

    See ..\README.TXT for general list of project status

-----------------------------------------------------------------
FILES:

	icpriv.h - header for library components, general private stuff
    ircodes.h - some IR codes

SOURCE FILES:
    (features are layered)

Core runtime:
    crt3.asm - startup code forces linkage of start.c
	start.c - main startup
	init1.c - peripheral init
	assert.c - support for "ic_assert" macro
	shutdown.c - shut down

	main0.c - standard init (pulls in everything else)

Limited C runtime: (implementations are lame, but small in size)
	printf.c
	memcpy.c
	memset.c
	rand.c
    ldivl.asm, lmull.asm - compiler helpers for long math

Hardware Subsystems: (independent of most other things)
	serial.c - serial port (requires a "Super ICybie")
	storage.c - E2PROM storage
	flash.c - writing to flash ROM

	timer.c - general purpose timers
	adc.c - A->D converter (for legs, light and battery level)
	ir1.c - IR modulation (includes voice chip communication)

	sensor.c - sensor inputs
    sound.c - sound playback (and LED eyes)
	motor.c - PWM motor control (legs + head + neck + mouth + tail)

Higher Levels built on Hardware Subsystems

	ir2.c - IR command interpretation

	leg1.c - leg movement
	leg2.c - scaled leg movement (needed for motions)
	leg3.c - re-calibration of legs (NYI)

	motion.c - motion playback (needed for actions)
	posture.c - posture control (needed for actions)
	action.c - action playback

System ROM glue:
	sysrom.c - glue to system ROM

	motion2.c - standard numbered motions (most of data is in system ROM)
    action2*.c - generated files containing the 200+ standard actions

================================================================

REVIEW: Add block diagram
REVIEW: Add comparison to System ROM implementations

================================================================
