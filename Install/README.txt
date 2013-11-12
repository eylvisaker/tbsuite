==============
 Installation
==============

- Prerequisites -

You must have mono installed in order to use these programs on
Linux, BSD or Mac OS X. If you have root access on your machine
then use your local package management software to install it.
On OpenSuse, the command "zypper install mono-core" should be 
sufficient.

If you have lapack libraries installed the installer will detect
this and automatically set up the programs to use it. This will
provide a moderate speed up for the matrix diagonalization 
routines over the built-in functions for doing so.

- Installation -

You can install the tbsuite with one of the following commands

sh install.sh
sudo sh install.sh

The installer will ask you for a directory to install to.
If you wish to install to the default directory (/usr/local),
you must run with elevated privileges using the sudo command.

==============
    Usage
==============

For examples on how to use the tightbinding and rpa programs,
see the tests directory.

fplobweights can be run to produce fat bands plot files which 
are compatible with xmgrace.

fplowannierconverter will read the opendx files produced by
fplo's Wannier function routines and convert them into a format
that xcrysden can use. xcrysden is much more suited to the 
task of plotting densities, etc. within a crystal structure than
opendx.


Copyright 2010-2013, Erik Ylvisaker
eylvisaker@gmail.com