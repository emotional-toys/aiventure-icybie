# At first the file to read must be specified.
# This must be done as first step because GDB uses the endianness
# defined in the ELF file for reading the data.
# Specifying the file might also be done on the command line
# (this is done by most tools (e.g., Eclipse)).
# The specified file is used for execution in system mode.
#
# Current directory is DF_EV2\FireOS\config
# Uncomment/comment (integration or recompiled version)
#########################
file ../GOP_HAL/SystemModeOS/bin/t508/P40_SystemMode_t508

# Set the simulator as a target
target sim

# Load a specific dll as simulator.
# The point in thime where this is done is crucial. It can not be
# done before setting the target. However, it must be done before
# loading the debug information.
sim load_dll %DEVENV%\t508\simulator\P40_GDBSIM.dll

# Load the system mode debug information from the specified files.
load
sim load_app1   ../bin/t508/DESFireEV2

#############
# After a file has been read, instruct GDB to use the instruction
# set simulator. As the instruction set simulator runs on the host,
# it uses little endian byte order. As this is different to the
# ELF file, GDB must be forced to use little endian.
##############
set endian little

####################
# the "./" before the name is essential
# Uncomment the *.ini file used (integration or recompiled HAL)
####################
sim load_config ./P40_MMU.xml

###################
# Set breakpoints as desired (optional, can also be set by tools).
# break phhalP40_LifeCycle_PreRelease_Release.c:55
# break *0x152
# break phhalP40_Bootflow_Main.c:182
# break phBootflow.c:123
# Finally start executing the program on the instruction set
# simulator (optional).
