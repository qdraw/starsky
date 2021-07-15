#!/bin/bash

# Mac OS only:
# launchctl load -w ~/Library/LaunchAgents/starsky.importer.plist
# launchctl unload ~/Library/LaunchAgents/starsky.importer.plist
# chmod +x /opt/starsky/import.sh
# replace data_disk with your disk name

BACKUP_DIR="/Volumes/data_disk/storage/fotobiebbak"


LOCKFILE="/tmp/importd.lock"

# lock file
if [ -f $LOCKFILE ]; then
	# Control will enter here if file exists.
	if [ "$(( $(date +"%s") - $(stat -f "%m" "$LOCKFILE") ))" -gt "3600" ]; then
		echo "$LOCKFILE is older then 1 hour"
		rm $LOCKFILE
	fi

	if [ -f $LOCKFILE ]; then
		if [ "$(( $(date +"%s") - $(stat -f "%m" "$LOCKFILE") ))" -lt "3600" ]; then
			echo "$LOCKFILE is younger than 1 hour"
			exit 1
		fi
	fi
fi

if [ ! -f $LOCKFILE ]; then
	touch $LOCKFILE
fi

echo "script started"
# end lock file

find /Volumes -type d -maxdepth 2 -name "DCIM" -print0 |
  while IFS= read -r -d '' line;
  do
        echo "$line"
        # just copy
        /opt/starsky/starsky/starskyimportercli --recursive true -v true -i false --path $line --structure "/yyyyMMdd_HHmmss_{filenamebase}.ext" --basepath $BACKUP_DIR -x false

        # import
        /opt/starsky/starsky/starskyimportercli --recursive true -v true --move true -i true --path $line
  done

# lock file
echo "script ended"

if [ -f $LOCKFILE ]; then
	echo "Lockfile removed"
	rm $LOCKFILE
fi
# end lock file
