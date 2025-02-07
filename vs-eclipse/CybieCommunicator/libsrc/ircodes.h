// IR Codes /*DO NOT ALTER*/

////////// //////////
// IR Remote ($Ax)
    // 0xA0 = 1, 0xAB = 12, 0xAC = Small, 0xAD = Large

////////// //////////
// Standard ICybie chest array obstacle detect ($338x)

    // 0x3381 = top
    // 0x3385, 0x3387 = left and right
    // 0x3389 = bottom

////////// //////////
// Other ICybie communication ($8x)
    // in the range $81->$84 etc

////////// //////////
// RoboChi codes ($4x->$7x, $10?)
	// LATER: haven't bothered to figure these out

////////// //////////
// Walkup charger detection ($8x)

	//REVIEW: fill these in
    // $8B->8F

////////// //////////
// IR Codes to voice chip ($Bx, $Dx, $Ex)
	// Communication to the RSC-300 voice chip is through special IR codes

#define IRCODE_VOICEOP_CLEAR    0xB0    // clear or reset or ?
#define IRCODE_VOICEOP_LISTEN   0xB2    // will reply one
#ifdef LATER
#define IRCODE_VOICEOP_ERASEALL 0xB3    // reply IRCODE_VOICE_NOTRAIN
#define IRCODE_VOICEOP_POWERDOWN 0xB4   // power down
#endif

#ifdef LATER
#define IRCODE_VOICEOP_TRAIN1(n) (0xC0 + (n)) // train voice command (n=0..7)
#define IRCODE_VOICEOP_TRAIN2(n) (0xB8 + (n)) // train voice command (n=0..7)
#endif

#define IRCODE_VOICEOP_ISTRAINED(n) (0xC8 + (n)) // test if trained
                                        // reply _TRAIN or _NOTRAIN

// specific reply codes (in response to poll)
#define IRCODE_VOICE_TRAIN      0xB5    // is trained
#define IRCODE_VOICE_NOTRAIN    0xB6    // is trained

// general poll results
// #define IRCODE_VOICE_HEARD(n)   (0xE0 + (n))
// also D9, DA (record failed), DB (recorded ok), E8->EA (error)

#define IRCODE_BAD_VOICE_HEARD  0xEF


