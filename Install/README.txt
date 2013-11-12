You can install the tbsuite with the following command

sh install.sh

The installer will ask you for a directory to install to.
If you wish to install to the default directory (/usr/local),
you must run with elevated privileges using the sudo command.

For examples on how to use the tightbinding and rpa programs,
see the tests directory.

fplobweights can be run to produce fat bands plot files which 
are compatible with xmgrace.

fplowannierconverter will read the opendx files produced by
fplo's Wannier function routines and convert them into a format
that xcrysden can use. xcrysden is much more suited to the 
task of plotting densities, etc. within a crystal structure than
opendx.