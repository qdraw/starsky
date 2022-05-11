#!/bin/bash

CURRENT_DIR=$(dirname "$0")
cd $CURRENT_DIR


USAGE=$(cat <<-END
    [Unit]
    Description=S
    
    [Service]
    # This is the directory where our published files are
    WorkingDirectory=/var/www/sample-app
    # We set up `dotnet` PATH in Step 1. The second one is path of our executable
    ExecStart=/usr/bin/dotnet /var/www/sample-app/SampleApp.dll --urls "http://0.0.0.0:5000"
    Restart=always
    # Restart service after 10 seconds if the dotnet service crashes
    RestartSec=10
    KillSignal=SIGINT
    SyslogIdentifier=sample-app-log
    # We can even set environment variables
    Environment=ASPNETCORE_ENVIRONMENT=Production
    Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
    
    [Install]
    # When a systemd user instance starts, it brings up the per user target default.target
    WantedBy=default.target
END
)
    




systemctl --user daemon-reload
systemctl --user enable sample-app.service

systemctl --user start sample-app.service