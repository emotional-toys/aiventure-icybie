// display order
	// first arg is storage index (do not change)

///////////////////////////////////////////////

// storage numbers: (do not renumber!)
// YICT 201:
    // 0 -> 313
// YICT 202:
	// 314 -> 343 - "S" no sound substitute versions of std 127->156
	//  (iStorage = 126->155)

#ifdef LATER_YICT203_OR_NEVER // sit/lie tendency
    // 344 -> 374 - "T"
    // 375 -> ??? - "L"
#endif

///////////////////////////////////////////////
// "T" - sit tendency, "L" - lie tendency

#ifdef LATER_YICT203_OR_NEVER

#error - not supported yet (maybe never)

#define T_BASE 344

#define UNAMEt(num, dindex, desc) \
    { T_BASE+dindex, #num "T", "sit_" #num "T", "SIT: " desc, NULL }


#define UNAMET(num, dindex, symNotUsed, desc) \
    UNAMEN(num-1, num, "stand_" #num, desc), \
    UNAMEt(num, dindex, desc) \

#define L_BASE 375

#define UNAMEL(num, dindex, desc) \
    UNAMEN(num-1, num, "stand_" #num, desc), \
    { L_BASE+dindex, #num "L", "lie_" #num "L", "LIE:" desc, NULL }

#endif

///////////////////////////////////////////////


#define UNAMEDUAL(num, desc)  \
    UNAMEX(num-1, #num, "stand_" #num, desc), \
    UNAMEX(num+187, #num "S", "stand_" #num "S", "SSND: " desc)

#ifdef LATER_YICT203_OR_NEVER

#error - not supported yet (maybe never)

// DUAL and T (separate, no DUAL+T)
#define UNAMEDUAL_T(num, dindex, desc)  \
    UNAMEDUAL(num, desc), \
	UNAMEt(num, dindex, desc)

#else
// no "T" yet
#define UNAMET(num, dindex, symNotUsed, desc) \
    UNAMEN(num-1, num, "stand_" #num, desc)
#define UNAMEL(num, dindex, desc) \
    UNAMEN(num-1, num, "stand_" #num, desc)
#define UNAMEDUAL_T(num, dindex, desc)  \
    UNAMEDUAL(num, desc)

#endif

///////////////////////////////////////////////

UNAMEN(  0,   1, "action_001", "Tail wag 1"),
UNAMEN(  1,   2, "action_002", "Crouch"),
UNAMEN(  2,   3, "action_003", "Sniffing ground1"),

UNAMEN(  3,   4, "action_004", "Guard 1"),
UNAMEN(  4,   5, "action_005", "Dig it"),
UNAMEN(  5,   6, "action_006", "Sniffing air"),
UNAMEN(  6,   7, "action_007", "Sitting, flea scratch 1"),
UNAMEN(  7,   8, "action_008", "Pee right 1"),
UNAMEN(  8,   9, "action_009", "Pee right 2"),
UNAMEN(  9,  10, "action_010", "Pee left 1"),
UNAMEN( 10,  11, "action_011", "Pee left 2"),
UNAMEN( 11,  12, "action_012", "Sniffing ground 2"),
UNAMEN( 12,  13, "action_013", "Sniffing ground 3"),
UNAMEN( 13,  14, "action_014", "Guard 2"),
UNAMEN( 14,  15, "action_015", "Sniffing and raising hand"),
UNAMET(16, 30, "action_016", "Sit 1"), // sit and stay sitting

UNAMEN( 16,  17, "action_017", "Play"),
UNAMEN( 17,  18, "action_018", "Standing neutral 1"),
UNAMEN( 18,  19, "action_019", "Stretch yawn"),
UNAMEN( 19,  20, "action_020", "Rocking 1"),
UNAMEN( 20,  21, "action_021", "Rocking 2"),
UNAMEN( 21,  22, "action_022", "Right hand shake"),
UNAMEN( 22,  23, "action_023", "Eat"),

// very few LIE tendency
UNAMEL(24, 0, "Push ups 1"),

// SIT tendency
UNAMET(25, 0, "action_025", "Sit rocking 1"),
UNAMEN( 25,  26, "action_026", "Tail wag, butt"),
    // not sit
UNAMET(27, 1, "action_027", "Happy"),
UNAMET(28, 2, "action_028", "Sitting, flea scratch 2"),
UNAMET(29, 3, "action_029", "Sit, butt rub side to side"),
UNAMET(30, 4, "action_030", "Sit guard"),
UNAMET(31, 5, "action_031", "Sit yawn"),
UNAMET(32, 6, "action_032", "Sit beg"),
UNAMET(33, 7, "action_033", "Sit, shake right hand"),
UNAMET(34, 8, "action_034", "Sit, hurt right paw"),
UNAMET(35, 9, "action_035", "Sit, worry"),
UNAMET(36, 10, "action_036", "Sit, hey over here"),
UNAMET(37, 11, "action_037", "Sit rocking 2"),
UNAMET(38, 12, "action_038", "Sitting hunger"),

UNAMEN( 38,  39, "action_039", "Charge me, before battery change"),
UNAMEN( 39,  40, "action_040", "Charge me, after battery change"),
UNAMEN( 40,  41, "action_041", "Swim"),
UNAMEN( 41,  42, "action_042", "Belly sections stretch"),

UNAMET(43, 13, "action_043", "Sitting, wave hello"),

UNAMEN( 43,  44, "action_044", "Belly, flea scratch"),
UNAMEN( 44,  45, "action_045", "Belly, lying still"),
UNAMEN( 45,  46, "action_046", "Belly, full body stretch"),
UNAMEN( 46,  47, "action_047", "Belly sleep"),
UNAMEN( 47,  48, "action_048", "Lying right side, rub belly"),
UNAMEN( 48,  49, "action_049", "Lying right side,sick puppy"),
UNAMEN( 49,  50, "action_050", "Nervous forward walk"),
UNAMEN( 50,  51, "action_051", "Forward walk"),
UNAMEN( 51,  52, "action_052", "Forward walking, dancing, play ful-attack"),
UNAMEN( 52,  53, "action_053", "Head stand 1"),
UNAMEN( 53,  54, "action_054", "Playful attack"),
UNAMEN( 54,  55, "action_055", "Happy dancing"),
UNAMEN( 55,  56, "action_056", "Lick"),
UNAMEN( 56,  57, "action_057", "Attention short pre"),
UNAMEN( 57,  58, "action_058", "Attention short post"),

UNAMET(59, 14, "action_059", "Sit give hug"),
UNAMET(60, 15, "action_060", "Sit give paw bent"),
UNAMET(61, 16, "action_061", "Sit give paw, give hug arms"),


UNAMEN( 61,  62, "action_062", "Looking around up"),
UNAMEN( 62,  63, "action_063", "Looking around down"),
UNAMEN( 63,  64, "action_064", "Shaking head to say 'no' 1"),
UNAMEN( 64,  65, "action_065", "Nod to say 'yes' 1"),
UNAMEN( 65,  66, "action_066", "Nod to say 'yes' 2"),
UNAMEN( 66,  67, "action_067", "Nod to say 'yes' 3"),
UNAMEN( 67,  68, "action_068", "Nod to say 'yes' 4"),
UNAMEN( 68,  69, "action_069", "Standing howl"),
UNAMEN( 69,  70, "action_070", "Belly turn on axis left"),
UNAMEN( 70,  71, "action_071", "Belly turn on axis right"),


UNAMET(72, 17, "action_072", "Sitting turn on axis left"),
UNAMET(73, 18, "action_073", "Sitting turn on axis right"),
UNAMET(74, 19, "action_074", "Sitting butt walk forward"),
UNAMET(75, 20, "action_075", "Sitting butt walk left"),
UNAMET(76, 21, "action_076", "Sitting butt walk right"),

UNAMEN( 76,  77, "action_077", "Slow forward walk"),
UNAMEN( 77,  78, "action_078", "Fast forward walk 1"),
UNAMEN( 78,  79, "action_079", "Fast forward walk 2"),
UNAMEN( 79,  80, "action_080", "Central axis turn left"),
UNAMEN( 80,  81, "action_081", "Central axis turn right"),
UNAMEN( 81,  82, "action_082", "Fast reverse walk"),
UNAMEN( 82,  83, "action_083", "Forward, then turn left 1"),
UNAMEN( 83,  84, "action_084", "Forward, then turn left 2"),
UNAMEN( 84,  85, "action_085", "Look around, turn left"),
UNAMEN( 85,  86, "action_086", "Forward, then turn right 1"),
UNAMEN( 86,  87, "action_087", "Look around, turn right"),
UNAMEN( 87,  88, "action_088", "Forward, then turn left 3"),
UNAMEN( 88,  89, "action_089", "Forward, then turn right 2"),
UNAMEN( 89,  90, "action_090", "Get up from butt 1"),
UNAMEN( 90,  91, "action_091", "Get up from butt 2"),
UNAMEN( 91,  92, "action_092", "Head stand to belly stretch"),
UNAMEN( 92,  93, "action_093", "Head stand 2"),
UNAMEN( 93,  94, "action_094", "Side left to belly, left scratch"),
UNAMEN( 94,  95, "action_095", "Side right to belly, right scratch"),
UNAMEN( 95,  96, "action_096", "Right side to standing"),
UNAMEN( 96,  97, "action_097", "Left side to standing"),
UNAMEN( 97,  98, "action_098", "Sit and get up"),
UNAMEN( 98,  99, "action_099", "To side, to belly"),

UNAMET(100, 22, "action_100", "Sit up"),

UNAMEN(100, 101, "action_101", "Play dead on left side"),
UNAMEN(101, 102, "action_102", "Play dead on right side"),
UNAMEN(102, 103, "action_103", "Play dead side & bow"),
UNAMEN(103, 104, "action_104", "Fast robo walk forward"),
UNAMEN(104, 105, "action_105", "Fast robo turn left"),
UNAMEN(105, 106, "action_106", "Fast robo turn right"),
UNAMEN(106, 107, "action_107", "Fast robo walk, stand, walk"),
UNAMEN(107, 108, "action_108", "Fast robo turn right, left, ..."),
UNAMEN(108, 109, "action_109", "Takes a bow 1"),
UNAMEN(109, 110, "action_110", "Happy tail wag & bark"),

UNAMET(111, 23, "action_111", "Cybie meets chirpychi"),

UNAMEN(111, 112, "action_112", "Cybie meets poochi"),
UNAMEN(112, 113, "action_113", "Cybie meets superpoochi"),
UNAMEN(113, 114, "action_114", "Cybie meets dinochi"),
UNAMEN(114, 115, "action_115", "Cybie meets meowchi"),

UNAMET(116, 24, "action_116", "Cybie meets chis"),
UNAMEN(116, 117, "action_117", "Pet my head"),
UNAMET(118, 25, "action_118", "New get up from seated"),

UNAMEN(118, 119, "action_119", "Get up from head"),
UNAMEN(119, 120, "action_120", "Cybie meets raptor"),
UNAMEN(120, 121, "action_121", "Side sleep left, twitch"),
UNAMEN(121, 122, "action_122", "Shaking head to say 'no' short"),
UNAMEN(122, 123, "action_123", "Nod to say 'yes' 5"),
UNAMEN(123, 124, "action_124", "Nod to say 'yes' 6"),
UNAMEN(124, 125, "action_125", "Shaking head to say 'no' 2"),
UNAMEN(125, 126, "action_126", "Nod"),



// 314 -> 343 - DUAL versions of std 127->156
//           (iStorage = 126->155)
UNAMEDUAL(127, "Belly sleep with sound"),
UNAMEDUAL(128, "Pee left 1 with sound"),
UNAMEDUAL(129, "Pee right 1 with sound"),
UNAMEDUAL(130, "Sick puppy, side right with sound"),
UNAMEDUAL(131, "Standing howl with sound"),
UNAMEDUAL(132, "Shaking head to say 'no' with sound"),
UNAMEDUAL(133, "Shaking head to say 'no' short with sound"),
UNAMEDUAL(134, "Sniffing air with sound"),
UNAMEDUAL(135, "Sniffing ground with sound 1"),
UNAMEDUAL(136, "Nod to say 'yes' 3 with sound"),
UNAMEDUAL(137, "Nod to say 'yes' 4 with sound"),
UNAMEDUAL(138, "Nod to say 'yes' 5 with sound"),
UNAMEDUAL(139, "Nod to say 'yes' 6 with sound"),
UNAMEDUAL(140, "Stretch yawn with sound"),
UNAMEDUAL(141, "Pee left 2 with sound"),
UNAMEDUAL(142, "Pee right 2 with sound"),
UNAMEDUAL(143, "Happy hang out sound"),
UNAMEDUAL(144, "Happy tail wag and bark sound"),
UNAMEDUAL(145, "Hyper hang out sound"),
UNAMEDUAL(146, "Nod to say 'yes' 1 with sound"),
UNAMEDUAL(147, "Nod to say 'yes' 2 with sound"),
UNAMEDUAL(148, "Sleepy hang out sound"),
UNAMEDUAL(149, "Sniffing ground with sound 2"),
UNAMEDUAL(150, "Sad hang out sound"),

UNAMEDUAL_T(151, 26, "Sit yawn with sound 1"),
UNAMEDUAL_T(152, 27, "Sit yawn with sound 2"),

UNAMEDUAL(153, "Eat with sound 1"),
UNAMEDUAL(154, "Eat with sound 2"),
UNAMEDUAL(155, "Sniffing ground with sound 3"),
UNAMEDUAL(156, "Side sleep left, twitch with sound"),

/////////

UNAMEN(156, 157, "action_157", "Walk left"),
UNAMEN(157, 158, "action_158", "Walk front"),
UNAMEN(158, 159, "action_159", "Walk right"),
UNAMEN(159, 160, "action_160", "Move head left only"),
UNAMEN(160, 161, "action_161", "Move head middle only"),
UNAMEN(161, 162, "action_162", "Move head right only"),
UNAMEN(162, 163, "action_163", "Head Home"),
UNAMEN(163, 164, "action_164", "Mood hyper act"),
UNAMEN(164, 165, "action_165", "Mood happy act 1"),
UNAMEN(165, 166, "action_166", "Mood happy act 2"),
UNAMEN(166, 167, "action_167", "Mood sick act"),
UNAMEN(167, 168, "action_168", "Mood sad act"),
UNAMEN(168, 169, "action_169", "Mood sleepy act"),
UNAMEN(169, 170, "action_170", "Mood guard act"),
UNAMEN(170, 171, "action_171", "Head Move to say Yes"),
UNAMEN(171, 172, "action_172", "Head Move to say No"),
UNAMEN(172, 173, "action_173", "Short Pee Right"),
UNAMEN(173, 174, "action_174", "Nod head and head stand"),

UNAMET(175, 28, "action_175", "Nod head and sit 1"),
UNAMEN(175, 176, "action_176", "Dance"),
UNAMET(177, 29, "action_177", "Give paw"),

// now start going out of kilter
UNAMEN(178, 178, "action_178", "Pee left 3"),
UNAMEN(180, 179, "action_179", "Pee right 3"),
UNAMEN(181, 180, "action_180", "Neck down & up 1"),
UNAMEN(183, 181, "action_181", "Nod to say 'yes' 7"),
UNAMEN(184, 182, "action_182", "Nod head and sit 2"),
UNAMEN(185, 183, "action_183", "Nod and standing neutral"),
UNAMEN(186, 184, "action_184", "Nod and forward"),
UNAMEN(187, 185, "action_185", "Nod and guard"),
UNAMEN(188, 186, "action_186", "Nod and tail wag"),
UNAMEN(189, 187, "action_187", "Nod and push up"),
UNAMEN(190, 188, "action_188", "Neck down & up 2"),
UNAMEN(191, 189, "action_189", "Head shake"),
UNAMEN(192, 190, "action_190", "Sit 2"),
UNAMEN(193, 191, "action_191", "Standing neutral 2"),
UNAMEN(194, 192, "action_192", "Fast forward walk 3"),
UNAMEN(195, 193, "action_193", "Guard 3"),
UNAMEN(196, 194, "action_194", "Tail wag 2"),
UNAMEN(197, 195, "action_195", "Push ups 2"),
UNAMEN(198, 196, "action_196", "Attention End"),
UNAMEN(199, 197, "action_197", "Demo 1 (Life of i-Cybie)"),
UNAMEN(200, 198, "action_198", "Demo 2 (i-Cybie the GREAT)"),
UNAMEN(201, 199, "action_199", "Demo 3 Move"),
UNAMEN(202, 200, "action_200", "Standby"),
UNAMEN(203, 201, "action_201", "Takes a bow 2"),
UNAMEN(204, 202, "action_202", "Self auto calibration - left side"),
UNAMEN(205, 203, "action_203", "Self auto calibration - right side"),
UNAMEN(206, 204, "action_204", "Self auto calibration - start"),
UNAMEN(216, 205, "action_205", "Communicate with poochi"),
UNAMEN(217, 206, "action_206", "Communicate with meowchi"),
UNAMEN(218, 207, "action_207", "Communicate with chis"),
UNAMEN(219, 208, "action_208", "Communicate with chirpychi"),
UNAMEN(220, 209, "action_209", "Communicate with superpoochi"),

// default sounds
UNAMEX(254, "S01", "playsound_01", "Play Sound 01 - BEEP"),
UNAMEX(255, "S02", "playsound_02", "Play Sound 02 - YIP"),
UNAMEX(256, "S03", "playsound_03", "Play Sound 03 - YUP"),
UNAMEX(257, "S04", "playsound_04", "Play Sound 04 - BARK"),
UNAMEX(258, "S05", "playsound_05", "Play Sound 05 - BARK2"),
UNAMEX(259, "S06", "playsound_06", "Play Sound 06 - BARK3"),
UNAMEX(260, "S07", "playsound_07", "Play Sound 07 - BURP"),
UNAMEX(261, "S08", "playsound_08", "Play Sound 08 - LICK"),
UNAMEX(262, "S09", "playsound_09", "Play Sound 09 - IN_PAIN"),
UNAMEX(263, "S10", "playsound_10", "Play Sound 10 - PANT"),
UNAMEX(264, "S11", "playsound_11", "Play Sound 11 - DRINK"),
UNAMEX(265, "S12", "playsound_12", "Play Sound 12 - GROWL"),
UNAMEX(266, "S13", "playsound_13", "Play Sound 13 - YOU_ASS_HOLE"),
UNAMEX(267, "S14", "playsound_14", "Play Sound 14 - HOWL"),
UNAMEX(268, "S15", "playsound_15", "Play Sound 15 - RURUFF"),
UNAMEX(269, "S16", "playsound_16", "Play Sound 16 - PANT2"),
UNAMEX(270, "S17", "playsound_17", "Play Sound 17 - PANT3"),
UNAMEX(271, "S18", "playsound_18", "Play Sound 18 - WATER_SFX"),
UNAMEX(272, "S19", "playsound_19", "Play Sound 19 - RURUFF2"),
UNAMEX(273, "S20", "playsound_20", "Play Sound 20 - MELODY"),
UNAMEX(274, "S21", "playsound_21", "Play Sound 21 - LICK_ONCE"),
UNAMEX(275, "S22", "playsound_22", "Play Sound 22 - SNEEZE"),
UNAMEX(276, "S23", "playsound_23", "Play Sound 23 - SNIFF"),
UNAMEX(277, "S24", "playsound_24", "Play Sound 24 - SNORE"),
UNAMEX(278, "S25", "playsound_25", "Play Sound 25 - PEE_SFX"),
UNAMEX(279, "S26", "playsound_26", "Play Sound 26 - WHINE"),
UNAMEX(280, "S27", "playsound_27", "Play Sound 27 - WHINE2"),
UNAMEX(281, "S28", "playsound_28", "Play Sound 28 - RURUFF3"),
UNAMEX(282, "S29", "playsound_29", "Play Sound 29 - YAWN_RUFF"),
UNAMEX(283, "S30", "playsound_30", "Play Sound 30 - SCREECH"),

// custom sounds
UNAMEX(284, "S31", "playsound_31", "Play Custom Sound 31"),
UNAMEX(285, "S32", "playsound_32", "Play Custom Sound 32"),
UNAMEX(286, "S33", "playsound_33", "Play Custom Sound 33"),
UNAMEX(287, "S34", "playsound_34", "Play Custom Sound 34"),
UNAMEX(288, "S35", "playsound_35", "Play Custom Sound 35"),
UNAMEX(289, "S36", "playsound_36", "Play Custom Sound 36"),
UNAMEX(290, "S37", "playsound_37", "Play Custom Sound 37"),
UNAMEX(291, "S38", "playsound_38", "Play Custom Sound 38"),
UNAMEX(292, "S39", "playsound_39", "Play Custom Sound 39"),
UNAMEX(293, "S40", "playsound_40", "Play Custom Sound 40"),
UNAMEX(294, "S41", "playsound_41", "Play Custom Sound 41"),
UNAMEX(295, "S42", "playsound_42", "Play Custom Sound 42"),
UNAMEX(296, "S43", "playsound_43", "Play Custom Sound 43"),
UNAMEX(297, "S44", "playsound_44", "Play Custom Sound 44"),
UNAMEX(298, "S45", "playsound_45", "Play Custom Sound 45"),
UNAMEX(299, "S46", "playsound_46", "Play Custom Sound 46"),
UNAMEX(300, "S47", "playsound_47", "Play Custom Sound 47"),
UNAMEX(301, "S48", "playsound_48", "Play Custom Sound 48"),
UNAMEX(302, "S49", "playsound_49", "Play Custom Sound 49"),
UNAMEX(303, "S50", "playsound_50", "Play Custom Sound 50"),
UNAMEX(304, "S51", "playsound_51", "Play Custom Sound 51"),
UNAMEX(305, "S52", "playsound_52", "Play Custom Sound 52"),
UNAMEX(306, "S53", "playsound_53", "Play Custom Sound 53"),
UNAMEX(307, "S54", "playsound_54", "Play Custom Sound 54"),
UNAMEX(308, "S55", "playsound_55", "Play Custom Sound 55"),
UNAMEX(309, "S56", "playsound_56", "Play Custom Sound 56"),
UNAMEX(310, "S57", "playsound_57", "Play Custom Sound 57"),
UNAMEX(311, "S58", "playsound_58", "Play Custom Sound 58"),
UNAMEX(312, "S59", "playsound_59", "Play Custom Sound 59"),
UNAMEX(313, "S60", "playsound_60", "Play Custom Sound 60"),

// old actions

UNAMEX(177, "adv", "actionold_176", "actionold_176"),
UNAMEX(179, "adv", "actionold_178", "actionold_178 - more peeing/bark"),
UNAMEX(182, "adv", "actionold_181", "actionold_181"),
UNAMEX(207, "adv", "actionold_206", "actionold_206 - Sleep1"),
UNAMEX(208, "adv", "actionold_207", "actionold_207 - Wakeup/Yawn"),
UNAMEX(209, "adv", "actionold_208", "actionold_208 - Sleep2"),
UNAMEX(210, "adv", "actionold_209", "actionold_209 - Wakeup/No Yawn"),
UNAMEX(211, "adv", "actionold_210", "actionold_210 - end talking1"),
UNAMEX(212, "adv", "actionold_211", "actionold_211 - end talking2"),
UNAMEX(213, "adv", "actionold_212", "actionold_212 - nose detect1"),
UNAMEX(214, "adv", "actionold_213", "actionold_213 - nose detect2"),
UNAMEX(215, "adv", "actionold_214", "actionold_214 - nap, get up funny"),


// new stuff

UNAMEX(221, "adv", "actionnew_01", "actionnew_01"),
UNAMEX(222, "adv", "actionnew_02", "actionnew_02"),
UNAMEX(223, "adv", "actionnew_03", "actionnew_03"),
UNAMEX(224, "adv", "actionnew_04", "actionnew_04"),
UNAMEX(225, "adv", "actionnew_05", "actionnew_05"),
UNAMEX(226, "adv", "actionnew_06", "actionnew_06"),
UNAMEX(227, "adv", "actionnew_07", "actionnew_07"),
UNAMEX(228, "adv", "actionnew_08", "actionnew_08"),
UNAMEX(229, "adv", "actionnew_09", "actionnew_09"),
UNAMEX(230, "adv", "actionnew_10", "actionnew_10"),
UNAMEX(231, "adv", "actionnew_11", "actionnew_11"),
UNAMEX(232, "adv", "actionnew_12", "actionnew_12"),
UNAMEX(233, "adv", "actionnew_13", "actionnew_13"),
UNAMEX(234, "adv", "actionnew_14", "actionnew_14"),
UNAMEX(235, "adv", "actionnew_15", "actionnew_15"),
UNAMEX(236, "adv", "actionnew_16", "actionnew_16"),
UNAMEX(237, "adv", "actionnew_17", "actionnew_17"),
UNAMEX(238, "adv", "actionnew_18", "actionnew_18"),
UNAMEX(239, "adv", "actionnew_19", "actionnew_19"),
UNAMEX(240, "adv", "actionnew_20", "actionnew_20"),
UNAMEX(241, "adv", "actionnew_21", "actionnew_21"),
UNAMEX(242, "adv", "actionnew_22", "actionnew_22"),
UNAMEX(243, "adv", "actionnew_23", "actionnew_23"),
UNAMEX(244, "adv", "actionnew_24", "actionnew_24"),
UNAMEX(245, "adv", "actionnew_25", "actionnew_25"),
UNAMEX(246, "adv", "actionnew_26", "actionnew_26"),
UNAMEX(247, "adv", "actionnew_27", "actionnew_27"),
UNAMEX(248, "adv", "actionnew_28", "actionnew_28"),
UNAMEX(249, "adv", "actionnew_29", "actionnew_29"),
UNAMEX(250, "adv", "actionnew_30", "actionnew_30"),
UNAMEX(251, "adv", "actionnew_31", "actionnew_31"),
UNAMEX(252, "adv", "actionnew_32", "actionnew_32"),
UNAMEX(253, "adv", "actionnew_33", "actionnew_33"),

///////////////////////////////////////////////

