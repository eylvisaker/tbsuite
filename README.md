
If you publish results using this code, it would be appreciated if you cite the following article:

> PHYSICAL REVIEW B 83, 014509 (2011)

# Installation

## Prerequisites

### CentOS

Install mono. The following commands are adapted from [these instructions](https://linuxize.com/post/how-to-install-mono-on-centos-8/):
```
sudo rpm --import 'http://pool.sks-keyservers.net/pks/lookup?op=get&search=0x3fa7e0328081bff6a14da29aa6a19b38d3d831ef'
sudo dnf config-manager --add-repo https://download.mono-project.com/repo/centos8-stable.repo
sudo dnf install -y mono-complete 
```

Install Lapack.
```
sudo dnf install -y lapack
```

## Build and Install TBSuite

First, clone the tbsuite repository:
```
git clone https://github.com/eylvisaker/tbsuite
cd tbsuite
```

TBSuite can be built and installed with standard makefile instructions:
```
make clean
make
make install
```
The installation will ask for a target directory to install to. It will try to install to `/usr/local/bin` by default. If you wish to install in this directory, you must run it with elevated privileges:
```
sudo make install
```
