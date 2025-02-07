// on-the-fly ADPCM4 encoder
// takes 8bit signed linear WAV samples and compresses to 4 bit ADPCM
// can not be used realtime - must store results in a buffer

void PrepareAdpcm();
byte ADECL EncodeAdpcm(sbyte sampNow);

void ReportAdpcmStatus();
