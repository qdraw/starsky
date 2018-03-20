#! /bin/sh
### BEGIN INIT INFO
# Provides:          inotifywait
# Required-Start:    $remote_fs $syslog
# Required-Stop:     $remote_fs $syslog
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Starts inotifywait monitoring scripts
# Description:       The inofifywait script is located at
#                    /usr/local/sbin/inotify and the config file
#                    at /usr/local/etc/inotidy.d
### END INIT INFO

# Author: docs.homelinux.org <admin@docs.homelinux.org>

# PATH should only include /usr/* if it runs after the mountnfs.sh script
DESC="inotifywait monitoring scripts"
NAME=inotify.sh
DAEMON=/usr/local/sbin/$NAME
DAEMON_ARGS=""
PIDFILE=/var/run/$NAME.pid
SCRIPTNAME=/etc/init.d/${NAME}d
CONFDIR=/usr/local/etc/inotify.d

# Exit if the package is not installed
[ -x "$DAEMON" ] || exit 0

# Exit if pidof is not installed
[ -x /bin/pidof ] || exit 0

# Exit if inotifywait is not installed
[ -x /usr/bin/inotifywait ] || exit 0

# Create config dir if it doesn't exist
[ -d $CONFDIR ] || mkdir $CONFDIR

# Read configuration variable file if it is present
[ -r /etc/default/$NAME ] && . /etc/default/$NAME

# Load the VERBOSE setting and other rcS variables
. /lib/init/vars.sh

# Define LSB log_* functions.
# Depend on lsb-base (>= 3.2-14) to ensure that this file is present
# and status_of_proc is working.
. /lib/lsb/init-functions

VERBOSE="yes"

#
# Function that starts the daemon/service
#
do_start()
{
		# Return
		#   0 if daemon has been started
		#   1 if daemon was already running
		#   2 if daemon could not be started
		pidof inotifywait > /dev/null && return 1

		for CONFIG in $(ls $CONFDIR); do

				start-stop-daemon --start --quiet --background --exec $DAEMON --test \
						$CONFIG > /dev/null \
						|| return 1

				start-stop-daemon --start --quiet --background --exec $DAEMON -- \
						$CONFIG \
						|| return 2
		done;

		return 0
		# Add code here, if necessary, that waits for the process to be ready
		# to handle requests from services started subsequently which depend
		# on this one.  As a last resort, sleep for some time.
}

#
# Function that stops the daemon/service
#
do_stop()
{
		# Return
		#   0 if daemon has been stopped
		#   1 if daemon was already stopped
		#   2 if daemon could not be stopped
		#   other if a failure occurred
		#start-stop-daemon --stop --quiet --retry=TERM/30/KILL/5 --pidfile $PIDFILE --name $NAME
		#RETVAL="$?"
		#[ "$RETVAL" = 2 ] && return 2
		# Wait for children to finish too if this is a daemon that forks
		# and if the daemon is only ever run from this initscript.
		# If the above conditions are not satisfied then add some other code
		# that waits for the process to drop all resources that could be
		# needed by services started subsequently.  A last resort is to
		# sleep for some time.
		#start-stop-daemon --stop --quiet --oknodo --retry=0/30/KILL/5 --exec $DAEMON
		#[ "$?" = 2 ] && return 2
		# Many daemons don't delete their pidfiles when they exit.
		#rm -f $PIDFILE
		kill $(pidof inotifywait) > /dev/null 2>&1
		return $?
}

case "$1" in
  start)
		[ "$VERBOSE" != no ] && log_daemon_msg "Starting $DESC" "$NAME"
		do_start
		case "$?" in
				0) [ "$VERBOSE" != no ] && log_end_msg 0 ;;
				1) [ "$VERBOSE" != no ] && log_end_msg 1 ;;
				2) [ "$VERBOSE" != no ] && log_end_msg 1 ;;
		esac
		;;
  stop)
		[ "$VERBOSE" != no ] && log_daemon_msg "Stopping $DESC" "$NAME"
		do_stop
		case "$?" in
				0|1) [ "$VERBOSE" != no ] && log_end_msg 0 ;;
				2) [ "$VERBOSE" != no ] && log_end_msg 1 ;;
		esac
		;;
  status)
	   status_of_proc "$DAEMON" "$NAME" && exit 0 || exit $?
	   ;;
  restart)
		log_daemon_msg "Restarting $DESC" "$NAME"
		do_stop
		case "$?" in
		  0|1)
				sleep 1
				do_start
				case "$?" in
						0) log_end_msg 0 ;;
						1) log_end_msg 1 ;; # Old process is still running
						*) log_end_msg 1 ;; # Failed to start
				esac
				;;
		  *)
				# Failed to stop
				log_end_msg 1
				;;
		esac
		;;
  *)
		echo "Usage: $SCRIPTNAME {start|stop|status|restart}" >&2
		exit 3
		;;
esac
