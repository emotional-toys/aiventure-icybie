rem - build original "Walkup8A" binary image from reverse
rem - engineered source code (HIMAGE)
rem - and check that it is exactly the same

bin\asw -cpu 93C141 -L -A -D EXACT newcart.a >  2
if ERRORLEVEL 1 goto bad_assemble
fgrep REPORT 2 | sort | uniq
bin\p2bin -r 0x200000-0x23FFFF newcart.p exact.ic3
cmp walkup8a.car exact.ic3

if ERRORLEVEL 1 goto bad_exact
goto end

:bad_exact
ECHOLIT "ERROR - bad exact build!"
bcmp walkup8a.car exact.ic3

:end
del *.p

