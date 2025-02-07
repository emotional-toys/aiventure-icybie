APPNAME=dalek
OBJS=$(APPNAME).o myaudio.o

SDKROOT=..\..
INCLUDE ..\..\INC\ICSTD.MAK


################################################################
# special rules

# we have lots of audio, put some in first area of cartridge
## REVIEW: make adp2c utility smarter so this won't be necessary
SOUND_THRESHOLD=60000

myaudio.o: myaudio.c

myaudio.c: audio\*.wav
    cd audio
    -..\$(BIN)\adpcm66 *.wav
    cd ..
    $(BIN)\adp2c $(SOUND_THRESHOLD) audio\*.adp > myaudio.c

clean2: clean
    del myaudio.c

################################################################
