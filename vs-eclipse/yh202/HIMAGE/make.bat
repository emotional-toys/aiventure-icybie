rem - make new HIMAGE

call clean.bat

rem - delete all release binaries
del ..\rel\*.bin

rem - NEWFEAT,SERIAL -- but not SERIAL_VERBOSE

bin\asw -cpu 93C141 -L -A -D NEWFEAT,SERIAL newcart.a >  2
if ERRORLEVEL 1 goto bad_assemble
bin\p2bin -r 0x200000-0x23FFFF newcart.p newcart.ic3
if ERRORLEVEL 1 goto bad_assemble
bin\splitic3 newcart.ic3 ..\rel\generic202-l.bin ..\rel\generic202-h.bin 

del *.p

