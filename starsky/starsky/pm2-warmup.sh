#!/usr/bin/env bash

## WARMUP WITHOUT LOGIN

ARGUMENTS=("$@")

PORT=5000

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then 
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--port 4823"
    fi
    
    if [[ ${ARGUMENTS[PREV]} == "--port" ]];
    then
        PORT="${ARGUMENTS[CURRENT]}"
    fi
    
  fi
done

URL="http://localhost:$PORT" 
# no slash
URL=${URL%/}

echo "Running on:"
echo $URL

COUNTER=0
MAXCOUNTER=30
while [ $COUNTER -lt $MAXCOUNTER ]; do
	CURLOUTPUT=`curl -X GET -IL "$URL"/account?json=true -o /dev/null -w '%{http_code}\n' -s`
	if [ $CURLOUTPUT != "401" ]; then
		if ! (($COUNTER % 2)); then
			echo "$COUNTER - $CURLOUTPUT - retry"
		fi
		sleep 3s
		let COUNTER=COUNTER+1
	else
		echo "$COUNTER - $CURLOUTPUT - done"
		COUNTER=$MAXCOUNTER
	fi
done

# To make Search Suggestions at start faster
CURLSUGGESTOUTPUT=`curl -X GET -LI "$URL"/suggest/inflate -o /dev/null -w '%{http_code}\n' -s`
echo "!> done ~ -sug:$CURLSUGGESTOUTPUT"
