// sine wave generator using on-the-fly ADPCM compressor/encoder

#include <ic.h>

#include "encoder.h"

/////////////////////////////////////////////////////

// my sounds
EXTERN_AUDIO(GOODFUN);

// forward declarations
void DoToneTest();

void main()
{
    printf("sine: Test program running\n");

    printf("oh what jolly good fun");
    ic_play_my_sound(GOODFUN);
    printf("!\n");
	ic_sleep(100); // short gap

    // Super ICybie input
    while (1)
    {
        byte chCmd;
        printf("(press a key)");
		chCmd = sic_get_serial_byte();
        printf(" -- %c\n", chCmd);
        if (chCmd == '?')
        {
            printf("\tCommands:\n");
            printf("\t\tT,t = tone test\n");
            printf("\t\tQ = Quit and stop\n");
        }
        else if (chCmd == 'T' || chCmd == 't')
        {
            DoToneTest();
        }
        else if (chCmd == 'Q' || chCmd == 'q')
        {
            printf("Goodbye\n\032"); // CTRL-Z
            return;
        }
        else
        {
            printf("\tUNKNOWN COMMAND '%c' - press '?' for help\n", chCmd);
        }
    }
}


//////////////////////////////////////////////////////////////////////////
// sine wave table (2*PI radians in 256 samples)

static sbyte const sine_wave_table[256] =
{
    // 360degrees for easy lookup
	0,3,6,9,12,15,18,21,24,27,30,33,36,39,42,45,
	48,51,54,57,59,62,65,67,70,73,75,78,80,82,85,87,
	89,91,94,96,98,100,102,103,105,107,108,110,112,113,114,116,
	117,118,119,120,121,122,123,123,124,125,125,126,126,126,126,126,
	127,126,126,126,126,126,125,125,124,123,123,122,121,120,119,118,
	117,116,114,113,112,110,108,107,105,103,102,100,98,96,94,91,
	89,87,85,82,80,78,75,73,70,67,65,62,59,57,54,51,
	48,45,42,39,36,33,30,27,24,21,18,15,12,9,6,3,
	0,-3,-6,-9,-12,-15,-18,-21,-24,-27,-30,-33,-36,-39,-42,-45,
	-48,-51,-54,-57,-59,-62,-65,-67,-70,-73,-75,-78,-80,-82,-85,-87,
	-89,-91,-94,-96,-98,-100,-102,-103,-105,-107,-108,-110,-112,-113,-114,-116,
	-117,-118,-119,-120,-121,-122,-123,-123,-124,-125,-125,-126,-126,-126,-126,-126,
	-127,-126,-126,-126,-126,-126,-125,-125,-124,-123,-123,-122,-121,-120,-119,-118,
	-117,-116,-114,-113,-112,-110,-108,-107,-105,-103,-102,-100,-98,-96,-94,-91,
	-89,-87,-85,-82,-80,-78,-75,-73,-70,-67,-65,-62,-59,-57,-54,-51,
	-48,-45,-42,-39,-36,-33,-30,-27,-24,-21,-18,-15,-12,-9,-6,-3,
};

//////////////////////////////////////////////////////////////////////////

void FillBufferWithSineWave(byte* adpcmBufferPtr, int bufferSize, int freq)
    // bufferSize = size in bytes (2 samples / byte)
    // freq = frequency in Hz
{
	int iSample2; // count of pair of samples
    long time = 0;

	PrepareAdpcm(); // prepare for encoding

	for (iSample2 = 0; iSample2 < bufferSize; iSample2++)
	{
        long theta1 = time / 8000;
	    long theta2 = (time + freq * 256) / 8000;
        sbyte samp1 = sine_wave_table[theta1 & 255]; // sine wave lookup
        sbyte samp2 = sine_wave_table[theta2 & 255]; // sine wave lookup
        byte adpcm1 = EncodeAdpcm(samp1);
        byte adpcm2 = EncodeAdpcm(samp2);
        adpcmBufferPtr[iSample2] = (adpcm2 << 4) | adpcm1;
				// low nibble played first

        time += 2*256*freq;
        if (time >= 2*256*8000)
            time -= 2*256*8000;
    }
}

//////////////////////////////////////////////////////////////////////////

void PlayBufferToAudioChip(const byte* adpcmBufferPtr,
    int bufferSize, int byteCount)
	// sizes in bytes (=2 samples) = 1/4000 sec
{
	int index = 0;

    __asm ( "EI 5" ); // disable interrupts
	_ic_send_to_daughter_sync(0x10);
	_ic_send_to_daughter_sync((byte)(byteCount >> 16));
	_ic_send_to_daughter_sync((byte)(byteCount >> 8));
	_ic_send_to_daughter_sync((byte)byteCount);
	
    while (byteCount--)
    {
		_ic_send_to_daughter_sync(adpcmBufferPtr[index]);
		if (++index >= bufferSize)
			index = 0; // repeat buffer from start
	}
	__asm ( "EI 0" ); // enable!
	
}


/////////////////////////////////////////////////////

#define MAX_BUFF_SIZE   1000
static byte my_buffer[MAX_BUFF_SIZE];
    // we can't calculate anything on the fly since the SunPlus protocol
    //  is such a CPU hog, so we create a small buffer of repeatable WAV data

void DoToneTest()
{
	int freq = sic_input("input freq ");
    int duration = 2; // 2 seconds

    // this is a bogus implementation
    // a proper implementation has to worry about the end of the buffer
        // the sample value (and ADPCM slope) must be zero

    printf("generating sine wave and compressing for %d samples\n",
            MAX_BUFF_SIZE*2);
    FillBufferWithSineWave(my_buffer, MAX_BUFF_SIZE, freq);

	ReportAdpcmStatus();
        // if 0, 0 - then there is a chance it will sound good
    
    printf("Playing tone from buffer\n");
    PlayBufferToAudioChip(my_buffer, MAX_BUFF_SIZE, duration*4000);
	printf("Tone complete\n");
}

