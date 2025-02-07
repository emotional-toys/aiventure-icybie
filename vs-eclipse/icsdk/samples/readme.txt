TO BUILD:
    "m" = m.bat = build
    "m clean" = remove temp files
    "t" = t.bat = test (using sicup)

--------------------------------------------------------------
ICSDK Samples:

Trivial Examples:

hello - simple hello world - really boring

base - base program (copy this to start your own project)


Useful Examples:

test1 - main test program
    test driver for high level features (like action playback)

sine - sine wave generation - WORK IN PROGRESS!
    on the fly ADPCM encoding and tone generation
	NOTE: encoder is slow, and sine wave generation does not generate
		a repeatable waveform

dalek - Dalek mini-personality - WORK IN PROGRESS!
    start of personality code
    example of having a lot of audio (>180K) - and tricky packing


clinic - diagnostic tool (for battery / E2PROM settings)



Low Level Examples:

test0 - lower level test program for lower level things
         (like higher level IR polling and motor control)

irprobe - low level IR probe (great for turning your ICybie into an IR sniffer)

-------------------------------------------------------------------
