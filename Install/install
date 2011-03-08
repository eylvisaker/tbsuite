#!/bin/bash

echo

mono --version > /dev/null

if [ "$?" -ne "0" ]; then
    echo "You must have mono installed."
    exit 1
fi

if [ -e Install.exe ]; then
	mono Install.exe
else
	mono exe/Install.exe
fi

echo
