#!/bin/bash
export ASPNETCORE_URLS="http://localhost:4823/"
export ASPNETCORE_ENVIRONMENT="Production"

pm2 start --name starsky ./starsky
echo "starsky started"

pm2 status
