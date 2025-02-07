# build for SICSDK2 SicL2

goal: crt3.o iclib2.lib
    copy crt3.o ..\lib
    copy iclib2.lib ..\lib

OBJS=start.o main0.o shutdown.o \
    timer.o init1.o \
	serial.o printf.o input.o \
    motor.o adc.o \
    leg1.o leg2.o \
    motion.o motion2.o posture.o \
    action.o action2a.o action2b.o \
    sound.o \
    sensor.o storage.o \
    ir1.o ir2.o voice.o \
    sysrom.o \
    flash.o \
    assert.o rand.o memcpy.o memset.o \
    ldivl.o lmull.o

# LATER:
# leg3.o \


leg1.o: leg_*.c_

motion2.o: mote.c_

#############
# Directories
SDKROOT=..
BIN=$(SDKROOT)\bin

iclib2.lib: $(OBJS) makefile
    -del iclib2.lib
	$(BIN)\tulib -r iclib2.lib $(OBJS)
    copy iclib2.lib ..\lib


#############

INC_FLAGS=-I$(SDKROOT)\inc
OPT_FLAGS=-O3

.SUFFIXES: .c .asm .o .abs .ic0

.c.o:
    SET THOME=$(SDKROOT)
	$(BIN)\cc900 -c $(OPT_FLAGS) $(INC_FLAGS) -l -o $*.o $*.c

.asm.o:
	$(BIN)\asm900 -l -o $*.o $*.asm

clean:
	del /s /q *.o *.lst *.lib 

#############
