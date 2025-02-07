// ICSDK2 - ICybie Library
// main0 initialization routines (advanced)

////////// ////////// ////////// ////////// //////////

extern void main0(); // user provided low-level main

////////// ////////// ////////// ////////// //////////
// low level initialization for cartridge personalities
// only call if you are implementing your own "main0"

void ADECL ic0_init_interrupt_handlers();
    // use cartridge provided

void ADECL ic0_init_timers();
void ADECL ic0_init_peripherals();
    // call after interrupts enabled and timers running

// start subsystems
void ADECL ic0_init_ir();
void ADECL ic0_init_adc();
void ADECL ic0_init_motors();   // PWM init
void ADECL ic0_init_legs();     // leg movement
void ADECL ic0_load_legcalib(); // load leg calibration

void ADECL ic0_init_motion();   // motion playback

////////// ////////// ////////// ////////// //////////
