#!/bin/bash

LOCKFILE="/tmp/starskyhourly.lock"

# using perl ready for linux
if [ -f $LOCKFILE ]; then
	# Control will enter here if file exists.
	if [ "$(( $(date +"%s") - $(perl -le 'print((stat shift)[9])' "$LOCKFILE") ))" -gt "14400" ]; then
		echo "$LOCKFILE is older then 4 hours"
		rm $LOCKFILE
	fi

	if [ -f $LOCKFILE ]; then
		if [ "$(( $(date +"%s") - $(perl -le 'print((stat shift)[9])' "$LOCKFILE") ))" -lt "14400" ]; then
			echo "$LOCKFILE is younger than 4 hours"
			exit 1
		fi
	fi
fi

if [ ! -f $LOCKFILE ]; then
	touch $LOCKFILE
fi

echo "script started"

/mnt/juno/storage/www/starsky/starskycli -s "/2018" -t true


echo "script ended"

if [ -f $LOCKFILE ]; then
 	echo "Lockfile removed"
 	rm $LOCKFILE
fi
