#!/bin/bash

RETVAL=0
MYCONFDIR=/usr/local/etc/inotify.d
WORKINGDIR=/usr/local/sbin
SCRIPTNAME=inotify.sh

# Check if I am root
[ $(id -u) -eq 0 ] || RETVAL=1

# Check if pidof is installed
test -x /bin/pidof || RETVAL=1

# Check if inotifywait is installed
test -x /usr/bin/inotifywait || RETVAL=1

# Check if logger is installed
test -x /usr/bin/logger || RETVAL=1

# Check if chmod is working
test -x /bin/chmod || RETVAL=1

# Check if chown is working
test -x /bin/chown || RETVAL=1

# Check config filename format
[[ "$1" =~ ^([a-z0-9]*\.conf)$ ]] || exit 0

# Check if input is a valid file
test -f $MYCONFDIR/$1 || exit 1

# Include config
. $MYCONFDIR/$1

# Test if $MYDIR exists
test -z "$MYDIR" && RETVAL=1

# Test if directory exists
test -d $MYDIR || RETVAL=1

# Test if $MYOWNER exists
test -z "$MYOWNER" && RETVAL=1

# Split OWNER
MYUSER=$(echo $MYOWNER | cut -d ":" -f 1)
MYGROUP=$(echo $MYOWNER | cut -d ":" -f 2)

# Check if user and group exists
/bin/egrep "^${MYGROUP}:" /etc/group > /dev/null 2>&1 || RETVAL=1
/bin/egrep "^${MYUSER}:" /etc/passwd > /dev/null 2>&1 || RETVAL=1


# Check if inotifywait is not running for this config
ps -ef | grep "inotifywait" | grep "$MYDIR" > /dev/null 2>&1
[ $? -eq 0 ] && RETVAL=1

if [ $RETVAL -eq 1 ]; then
		/usr/bin/logger -p local5.notice -t "starsky" "ERROR can't start inotifywait monitor for config $1..."
		exit $RETVAL
fi

# Write log message
/usr/bin/logger -p local5.notice -t "starsky" "Starting inotifywait monitoring script for $MYDIR ..."

# Start inotify monitoring
/usr/bin/inotifywait -mrq --format '%w%f' -e close_write -e delete -e moved_to -e create $MYDIR | while read file; do

		# Change owner
		/bin/chown $MYOWNER -R $file > /dev/null 2>&1

		# Change mode
		[ "$MYCHMOD" != "" ] && /bin/chmod $MYCHMOD -R $file > /dev/null 2>&1

		# Start starsky
		$STARSKYPATH -p $file -i true -t true -o true
		## Check starskycli -h for the shortcut meanings
done


# Quit script
exit 0
