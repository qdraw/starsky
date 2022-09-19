#!/bin/bash

SERVICE_NAME=starsky


if ! command -v systemctl &> /dev/null
then
    echo "systemctl could not be found"
    exit 1
fi

SCRIPT_DIR=$(dirname "$0")
cd $SCRIPT_DIR

mkdir -p ~/.config/systemd/user/

SYSTEMD_SERVICE_PATH="~/.config/systemd/user/"$SERVICE_NAME".service"

echo "[Unit]" > $SYSTEMD_SERVICE_PATH
echo -e "\nDescription=S" >> $SYSTEMD_SERVICE_PATH
echo -e "\n[Service]" >> $SYSTEMD_SERVICE_PATH
echo -e "\nDescription=S" >> $SYSTEMD_SERVICE_PATH
echo -e "\nWorkingDirectory=${SCRIPT_DIR}" >> $SYSTEMD_SERVICE_PATH
echo -e "\nExecStart=${SCRIPT_DIR}/starsky --urls "http://0.0.0.0:5001"" >> $SYSTEMD_SERVICE_PATH
echo -e "\nRestart=always" >> $SYSTEMD_SERVICE_PATH
echo -e "\nRestartSec=10" >> $SYSTEMD_SERVICE_PATH
echo -e "\nKillSignal=SIGINT" >> $SYSTEMD_SERVICE_PATH
echo -e "\nSyslogIdentifier=${SCRIPT_DIR}" >> $SYSTEMD_SERVICE_PATH
echo -e "\nEnvironment=ASPNETCORE_ENVIRONMENT=Production" >> $SYSTEMD_SERVICE_PATH
echo -e "\nEnvironment=DOTNET_PRINT_TELEMETRY_MESSAGE=false" >> $SYSTEMD_SERVICE_PATH
echo -e "\n" >> $SYSTEMD_SERVICE_PATH
echo -e "\n[Install]" >> $SYSTEMD_SERVICE_PATH
echo -e "\nWantedBy=default.target" >> $SYSTEMD_SERVICE_PATH

#echo <<< EOL
#    [Unit]
#    Description=S
#    
#    [Service]
#    # This is the directory where our published files are
#    WorkingDirectory=${SCRIPT_DIR}
#    # We set up `dotnet` PATH in Step 1. The second one is path of our executable
#    ExecStart=${SCRIPT_DIR}/starsky --urls "http://0.0.0.0:5001"
#    Restart=always
#    # Restart service after 10 seconds if the dotnet service crashes
#    RestartSec=10
#    KillSignal=SIGINT
#    SyslogIdentifier=${SCRIPT_DIR}
#    # We can even set environment variables
#    Environment=ASPNETCORE_ENVIRONMENT=Production
#    Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
#    
#    [Install]
#    # When a systemd user instance starts, it brings up the per user target default.target
#    WantedBy=default.target
#EOL >> $SYSTEMD_SERVICE_PATH;



systemctl --user daemon-reload
systemctl --user enable $SERVICE_NAME".service"

systemctl --user start $SERVICE_NAME".service"

echo "end"

# credits to https://amelspahic.com/deploy-net-6-application-with-github-actions-to-self-hosted-linux-machine-virtual-private-server-raspberry-pi
