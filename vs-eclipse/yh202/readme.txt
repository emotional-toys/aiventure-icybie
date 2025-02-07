Source code to HIMAGE, YICT202 and ICAUD

Freeware, but still copyrighted (c) 2002-2003 ICybiePet
Please keep the credit to ICybiePet intact

-----------
Brief notes:

Most tools requires Visual C++ to build. Version 6 recommended.

If changing the UI features, you can keep the version number unchanged (202).

If adding new HIMAGE firmware features, you must change 
the version number to avoid incompatibilities with YICT 202.

Most of the code is rather simple, designed at editing
the data structure defined in yictdata202.h

-----------
MAJOR WARNING:

YICT is tightly tied to the modified ICybie firmware
called "HIMAGE".

New data members should be added to yictdata202.h
and yictdata.a_ in the HIMAGE project.
This data structure must match exactly.

Other YICT data structures assume direct correlation with
in HIMAGE data structures and other ordering.
Unless you understand a feature thorougly, do not change it!

-----------

See also:

    yict202\readme.txt - source to YICT tool
    himage\readme.txt - source to Hacked Image (firmware)
    icaud\readme.txt - source to ICAUD tool

    

