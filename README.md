
If you publish results using this code, it would be appreciated if you cite the following article:

> PHYSICAL REVIEW B 83, 014509 (2011)

# Installation

## Prerequisites

### CentOS

Install mono according to [these instructions](https://linuxize.com/post/how-to-install-mono-on-centos-8/)

Install Lapack with the following command
```
sudo yum install -y lapack
```

## Build and Install TBSuite

TBSuite can be built and installed with standard makefile instructions:
```
make clean
make
sudo make install
```
