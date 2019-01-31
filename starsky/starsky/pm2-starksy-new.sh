#!/bin/bash

cd "$(dirname "$0")"

export ASPNETCORE_URLS="http://localhost:4823/"
export ASPNETCORE_ENVIRONMENT="Production"

echo "Copy the App Insights key string and press [ENTER]:"
echo "for example: "
echo "11111111-2222-3333-4444-555555555555"
echo ">>>"
read -p "Enter: " INSTRUMENTATIONKEY

export APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATIONKEY

chmod +x starsky

pm2 start --name starsky ./starsky
echo "starsky started"

pm2 status
