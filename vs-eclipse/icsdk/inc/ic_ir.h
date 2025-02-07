// ICSDK2 - ICybie Library
// IR support

////////// ////////// ////////// ////////// //////////
// IR remote inputs

// using standard remote IR modulation

byte ADECL ic_get_remote_button();
    // debounced version of 'ic_get_last_remote_button()'

byte ADECL ic_get_last_remote_button();
    // 0 if none, 1->12 for numbers, 13 for small, 14 for large
    // erases last button (so next call will return 0 unless a new
        // remote button is pressed
    // NOTE: pressing a remote button will typically repeat the same code,
        // so program accordingly. If you want to let the user enter more
        // than one button, separate with the "Large" button like in
        // the standard ICybie commands (eg: 9/Large/1/Large = 91)

////////// ////////// ////////// ////////// //////////
// Voice commands (must be listened for)

bool ADECL ic_listen_for_voice_command(); // call for each reply

byte ADECL ic_get_last_voice_command();
    // 0 if none, 1->8 for standard voice commands
    // other value for error (usually 0xEF)

#define NUM_VOICE_CMDS 8

enum
{
    // standard names, but you can assign any 8 phrases you want
    VOICECMD_TAKE_A_BOW = 1,
    VOICECMD_SIT_DOWN = 2,
    VOICECMD_GOOD_BOY = 3,
    VOICECMD_TRICK = 4,
    VOICECMD_BAD_DOG = 5,
    VOICECMD_ICYBIE = 6, // dog's name
    VOICECMD_STAY = 7,
    VOICECMD_GUARD = 8
};

byte ADECL ic_check_voice_recorded(byte vcmd);
    // return 1 if recorded, 0 if not, 0xFF is timeout (not recorded)

////////// ////////// ////////// ////////// //////////
// direct access for 16 bit

enum // tx_direction
{
    TXIR_TOP  = 0x08,    // topmost LED (brightest)
	// you can OR in TXIR_TOP to any of the following
    TXIR_TL   = 0x30,    // top left [dog's left]
    TXIR_TR   = 0x10,    // top right
    TXIR_BL   = 0x24,    // bottom left
    TXIR_BR   = 0x04,    // bottom right
    TXIR_L    = 0x34,    // both left (top+bottom)
    TXIR_R    = 0x14,    // both right (top+bottom)

    TXIR_VOICE = 0xFF    // special case
}; /*CPU SFR COMPATIBLE DO NOT ALTER*/

bool ADECL ic_send_ir_code(byte tx_direction, word code);
    // will fail if IR logic is busy doing something else

bool ADECL ic_is_sending_ir_code();

word ADECL ic_get_last_ir_code(); // any 16 bit IR received

////////// ////////// ////////// ////////// //////////
////////// ////////// ////////// ////////// //////////
// IR chest array for obstacle detection

typedef enum
{
    // 5 individual directions
    CHESTDIR_TOP,
    CHESTDIR_TL,   // top left [the dog's left]
    CHESTDIR_TR,   // top right
    CHESTDIR_BL,   // bottom left
    CHESTDIR_BR,   // bottom right
    // combinations
    CHESTDIR_L,    // top+bottom left
    CHESTDIR_R,    // top+bottom right
    CHESTDIR_TOP_TL,    // top + top left
    CHESTDIR_TOP_TR,    // top + top right
    CHESTDIR_TOP_L,    // top + top left + bottom left
    CHESTDIR_TOP_R,    // top + top right + bottom right
        // TOP + bottom only don't make sense
    CHESTDIR_MAX
} CHEST_DIRECTION;

// NOTE: the original ICybie uses standard IR remote modulation
// This takes about 1/14 of a second to send (and hopefully receive) one signal
// You need to do this five times.

// Instead we use a much lower level algorithm:
// We check to see there is no other IR flooding the area
// Then we send out short pulses from all five transmitters
//  This is repeated several times, and the number of reflections
//  in each of the directions is returned
// The main program decides how to interpret this information

word ADECL ic_get_IR_quiet_level();
    // number of IR ticks (~280 us) since an IR pulse has been detected
    // if under 500, may be noisy

int ADECL ic_fast_obstacle_detect();
    // return < 0 if error
    // otherwise indicator bits for the different CHEST_DIRECTIONS

bool ADECL ic_count_obstacle_detect(int counts[CHESTDIR_MAX], int countMax);
    // returns false on error
    // otherwise delay count before modulated IR signal is detected
    //   (or -1 if not reflectiong in 'countMax' tries)
    // recommended countMax is 100
    // not an indication of the distance to the obstacle
    //  (light travels too fast for that), but is a very rough
    //  indication of reflection strength (only marginally better
    //  than the bit indicators returned by 'ic_fast_obstacle_detect'

////////// ////////// ////////// ////////// //////////
