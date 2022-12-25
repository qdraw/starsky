#!/bin/bash

SERVICE_NAME=starsky
EXE_NAME=starsky


if ! command -v launchctl &> /dev/null
then
    echo "launchctl could not be found"
    exit 1
fi

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}
OUTPUT_DIR="$(dirname "$(realpath "$0")")"

ANYWHERE=false
PORT=5000
# Port 4823 an example port number

# command line args
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
    
  CURRENT=$(($i-1))
  
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "(optional) --port 4823"
      echo "(optional) --output "$OUTPUT_DIR
      echo "(optional) --anywhere (to allow access from anywhere, defaults to false)"
      exit 0
  fi
      
  # When true, allow access from anywhere not only localhost
  # defaults to false
  # only used on creation, when enabled you need to manual remove a systemd instance
  if [[ ${ARGUMENTS[CURRENT]} == "--anywhere" ]];
  then
      ANYWHERE=true
  fi  
  
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))

    if [[ ${ARGUMENTS[PREV]} == "--port" ]];
    then
        PORT="${ARGUMENTS[CURRENT]}"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--output" ]];
    then
        OUTPUT_DIR="${ARGUMENTS[CURRENT]}"
    fi
  fi
done


# add slash if not exists
LAST_CHAR_OUTPUT_DIR=${OUTPUT_DIR:length-1:1}
[[ $LAST_CHAR_OUTPUT_DIR != "/" ]] && OUTPUT_DIR="$OUTPUT_DIR/"; :

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi

cd $OUTPUT_DIR

if [ -f $EXE_NAME ]; then
    chmod +rwx $EXE_NAME
else 
    echo "FAIL: " $EXE_NAME" is missing"
    echo "do nothing"
    exit 1
fi


# settings
echo "run with the following parameters "

if [ "$ANYWHERE" = true ] ; then
    ANYWHERESTATUSTEXT="--anywhere $ANYWHERE"
fi
echo "--port" $PORT $ANYWHERESTATUSTEXT


mkdir -p $HOME"/Library/LaunchAgents"
mkdir -p $HOME"/Library/Application Support/starsky/logs"



LAUNCHD_SERVICE_PATH=$HOME"/Library/LaunchAgents/"$SERVICE_NAME".plist"

if launchctl list com.$SERVICE_NAME | grep -q "$SERVICE_NAME";then
    echo "kill service"
    launchctl kill 9 "gui/"$UID"/com."$SERVICE_NAME
    
    echo "bootout service:"
    launchctl bootout "gui/"$UID $LAUNCHD_SERVICE_PATH

    echo "list should be none:"    
    launchctl list | grep -i $SERVICE_NAME || echo none
fi

if [ ! -f $LAUNCHD_SERVICE_PATH ]
then
    echo "next: create "$LAUNCHD_SERVICE_PATH
    touch $LAUNCHD_SERVICE_PATH
else 
    echo "next: going to overwrite "$LAUNCHD_SERVICE_PATH
fi

# anywhere port
HOSTNAME="localhost"
if [ "$ANYWHERE" = true ] ; then
    HOSTNAME="*"
fi

echo '<?xml version="1.0" encoding="UTF-8"?>' > $LAUNCHD_SERVICE_PATH # overwrite!
echo -e '<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">' >> $LAUNCHD_SERVICE_PATH
echo -e '<plist version="1.0">' >> $LAUNCHD_SERVICE_PATH
echo -e '  <dict>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <key>Label</key>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <string>com.'$SERVICE_NAME'</string>' >> $LAUNCHD_SERVICE_PATH
echo -e '   <key>ProgramArguments</key>' >> $LAUNCHD_SERVICE_PATH
echo -e '   <array>' >> $LAUNCHD_SERVICE_PATH
echo -e '		<string>'$OUTPUT_DIR$EXE_NAME'</string>' >> $LAUNCHD_SERVICE_PATH
echo -e '		<string>--urls</string>' >> $LAUNCHD_SERVICE_PATH
echo -e '		<string>http://'$HOSTNAME':'$PORT'</string>' >> $LAUNCHD_SERVICE_PATH
echo -e '		    </array>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <key>RunAtLoad</key>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <true/>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <key>StandardOutPath</key>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <string>'$HOME'/Library/Application Support/starsky/logs/daemon_'$SERVICE_NAME'.log</string>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <key>StandardErrorPath</key>' >> $LAUNCHD_SERVICE_PATH
echo -e '    <string>'$HOME'/Library/Application Support/starsky/logs/daemon_'$SERVICE_NAME'.log</string>' >> $LAUNCHD_SERVICE_PATH
echo  -e '  </dict>' >> $LAUNCHD_SERVICE_PATH
echo -e '</plist>' >> $LAUNCHD_SERVICE_PATH


echo "next: enable"
launchctl bootstrap "gui/"$UID $LAUNCHD_SERVICE_PATH

echo "end"
echo "to restart: "

echo "rm "$OUTPUT_DIR"app__data.db* && ""launchctl stop com."$SERVICE_NAME" && sleep 10 && ""launchctl start com."$SERVICE_NAME" && ""launchctl list com."$SERVICE_NAME
