#!/bin/bash

SERVICE_NAME=starsky
EXE_NAME=starsky

if ! command -v systemctl &> /dev/null
then
    echo "systemctl could not be found"
    exit 1
fi

# realpath is not support using os x
OUTPUT_DIR="$(dirname "$(realpath "$0")")"

PORT=5000
# Port 4823 an example port number

# command line args
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--port 4823"
        exit 0
    fi

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

echo $PORT

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



mkdir -p $HOME"/.config/systemd/user/"

SYSTEMD_SERVICE_PATH=$HOME"/.config/systemd/user/"$SERVICE_NAME".service"
if [ ! -f $SYSTEMD_SERVICE_PATH ]
then
    echo "next: create "$SYSTEMD_SERVICE_PATH
    touch $SYSTEMD_SERVICE_PATH
else 
    echo "next: going to overwrite "$SYSTEMD_SERVICE_PATH
fi

if systemctl --user --type service | grep -q "$SERVICE_NAME";then
    echo "next: stop and disable $SERVICE_NAME "    
    systemctl --user stop $SERVICE_NAME".service"
    systemctl --user disable $SERVICE_NAME".service"
fi

echo "[Unit]" > $SYSTEMD_SERVICE_PATH # overwrite!
echo -e "Description=${EXE_NAME}" >> $SYSTEMD_SERVICE_PATH
# This is the directory where our published files are
echo -e "\n[Service]" >> $SYSTEMD_SERVICE_PATH
echo -e "Description=${EXE_NAME}" >> $SYSTEMD_SERVICE_PATH
echo -e "WorkingDirectory=${OUTPUT_DIR}" >> $SYSTEMD_SERVICE_PATH
# We set up `dotnet` PATH in Step 1. The second one is path of our executable
echo -e "ExecStart=${OUTPUT_DIR}${EXE_NAME} --urls \"http://localhost:${PORT}\"" >> $SYSTEMD_SERVICE_PATH
echo -e "Restart=always" >> $SYSTEMD_SERVICE_PATH
# Restart service after 10 seconds if the dotnet service crashes
echo -e "RestartSec=10" >> $SYSTEMD_SERVICE_PATH
echo -e "KillSignal=SIGINT" >> $SYSTEMD_SERVICE_PATH
echo -e "SyslogIdentifier=${OUTPUT_DIR}" >> $SYSTEMD_SERVICE_PATH
# We can even set environment variables
echo -e "Environment=ASPNETCORE_ENVIRONMENT=Production" >> $SYSTEMD_SERVICE_PATH
echo -e "Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false" >> $SYSTEMD_SERVICE_PATH
echo -e "\n[Install]" >> $SYSTEMD_SERVICE_PATH
# When a systemd user instance starts, it brings up the per user target default.target
echo -e "WantedBy=default.target" >> $SYSTEMD_SERVICE_PATH

echo "next: create $SERVICE_NAME "
systemctl --user daemon-reload

echo "next: enable"
systemctl --user enable $SERVICE_NAME".service"

echo "next: start"
systemctl --user start $SERVICE_NAME".service"

echo "next: is failed"
systemctl --user is-failed $SERVICE_NAME".service"

echo "next: cat"
systemctl --user cat $SERVICE_NAME".service"

echo "end"
# credits to https://amelspahic.com/deploy-net-6-application-with-github-actions-to-self-hosted-linux-machine-virtual-private-server-raspberry-pi
