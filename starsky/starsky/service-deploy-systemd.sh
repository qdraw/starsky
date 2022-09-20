#!/bin/bash

SERVICE_NAME=starsky
EXE_NAME=starsky

if ! command -v systemctl &> /dev/null
then
    echo "systemctl could not be found"
    exit 1
fi

OUTPUT_DIR="$(dirname "$(realpath "$0")")"

PORT=5000
# Port 4823 an example port number

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

echo "[Unit]" > $SYSTEMD_SERVICE_PATH
echo -e "\nDescription=${EXE_NAME}" >> $SYSTEMD_SERVICE_PATH
# This is the directory where our published files are
echo -e "[Service]" >> $SYSTEMD_SERVICE_PATH
echo -e "Description=${EXE_NAME}" >> $SYSTEMD_SERVICE_PATH
echo -e "WorkingDirectory=${OUTPUT_DIR}" >> $SYSTEMD_SERVICE_PATH
# We set up `dotnet` PATH in Step 1. The second one is path of our executable
echo -e "\nExecStart=${OUTPUT_DIR}${EXE_NAME} --urls \"http://0.0.0.0:${PORT}\"" >> $SYSTEMD_SERVICE_PATH
echo -e "\nRestart=always" >> $SYSTEMD_SERVICE_PATH
# Restart service after 10 seconds if the dotnet service crashes
echo -e "\nRestartSec=10" >> $SYSTEMD_SERVICE_PATH
echo -e "\nKillSignal=SIGINT" >> $SYSTEMD_SERVICE_PATH
echo -e "\nSyslogIdentifier=${OUTPUT_DIR}" >> $SYSTEMD_SERVICE_PATH
# We can even set environment variables
echo -e "\nEnvironment=ASPNETCORE_ENVIRONMENT=Production" >> $SYSTEMD_SERVICE_PATH
echo -e "\nEnvironment=DOTNET_PRINT_TELEMETRY_MESSAGE=false" >> $SYSTEMD_SERVICE_PATH
echo -e "\n" >> $SYSTEMD_SERVICE_PATH
echo -e "\n[Install]" >> $SYSTEMD_SERVICE_PATH
# When a systemd user instance starts, it brings up the per user target default.target
echo -e "\nWantedBy=default.target" >> $SYSTEMD_SERVICE_PATH

# for debug
echo -e "\nExecStart=${OUTPUT_DIR}${EXE_NAME} --urls \"http://0.0.0.0:${PORT}\""


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
