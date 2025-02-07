# Standard Make for ICSDK cartridge programs

# imported
#       "SDKROOT" = relative path to SIC SDK
#       "APPNAME" = name of program
#       "OBJS" = list of all objects

goal: $(APPNAME).ic3

SICBURN_FLAGS=-1 -s -r
    # "-1" COM1 (default), "-2" COM2 etc
    # "-r" run program after uploading
        
LIST_FLAGS=-l
    # "-l" to general .lst files

######################################################
# Directories

BIN=$(SDKROOT)\bin
LIBPATH=$(SDKROOT)\lib
INC_FLAGS=-I$(SDKROOT)\inc
LIBS=$(LIBPATH)\crt3.o $(LIBPATH)\iclib2.lib
LCFFILE=$(LIBPATH)\cartic3.lcf

######################################################
# Build Rules

.SUFFIXES: .c .asm .o .ic3

.c.o:
    SET THOME=$(SDKROOT)
	$(BIN)\cc900 -c -O3 $(INC_FLAGS) -o $*.o $(LIST_FLAGS) $*.c

.asm.o:
	$(BIN)\asm900 -o $*.o $(LIST_FLAGS) $*.asm

# Final link
$(APPNAME).ic3: makefile $(OBJS) $(LIBS) makefile
	$(BIN)\tulink -o $(APPNAME).abs $(LCFFILE) $(OBJS) $(LIBS)
	$(BIN)\tuconv -Fs24 $(APPNAME).abs
    del $(APPNAME).abs
    $(BIN)\convic3 $(APPNAME).s24 $(APPNAME).ic3
    del $(APPNAME).s24
    $(BIN)\splitic3 $(APPNAME).ic3 $(APPNAME)-l.bin $(APPNAME)-h.bin
    del $(APPNAME).ic3

######################################################
# special build for audio/sounds (audio_data.c is a generated file)

audio_data.o: audio_data.c

audio_data.c: audio\*.wav
    cd audio
    -..\$(BIN)\adpcm66 *.wav
    cd ..
    $(BIN)\adp2c 0 audio\*.adp > audio_data.c

######################################################

test_serial:
    $(BIN)\sicburn $(SICBURN_FLAGS) $(APPNAME)

clean:
	del /s /q *.o *.abs *.map *.lst *.fal *.s24 *.ic3 *.bin
    IF EXIST audio_data.c del audio_data.c
    IF EXIST audio\*.adp del audio\*.adp

######################################################
