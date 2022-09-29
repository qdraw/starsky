#!/bin/bash
PROCESS_NAME=starsky
PORT="4823"

SCRIPT_DIR=$(dirname "$0")
cd $SCRIPT_DIR

# command line args
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do

    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "(optional) --port 4823"
        echo "(optional) --anywhere (to allow access from anywhere, defaults to false)"
        exit 0
    fi

    # When true, allow access from anywhere not only localhost
    # defaults to false
    # only used on creation, when enabled you need to manual remove a pm2 instance
    if [[ ${ARGUMENTS[CURRENT]} == "--anywhere" ]];
    then
        ANYWHERE=true
    fi

  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[PREV]} == "--port" ]];
    then
        PORT="${ARGUMENTS[CURRENT]}"
    fi
  fi
done

# net core hostname
HOSTNAME="localhost"
if [ "$ANYWHERE" = true ] ; then
    HOSTNAME="*"
fi
echo "HOSTNAME "$HOSTNAME
    
ps_out=`ps -ef | grep $PROCESS_NAME | grep -v 'grep' | grep -v $0`
result=$(echo $ps_out | grep "$PROCESS_NAME")
if [[ "$result" != "" ]];then
    echo "Running, do nothing"
else

    export ASPNETCORE_URLS="http://"$HOSTNAME":"$PORT"/"
    export ASPNETCORE_ENVIRONMENT="Production"
    
    echo "Not Running start"
    ./$PROCESS_NAME
fi