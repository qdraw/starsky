#!/usr/bin/env bash

## WARMUP WITHOUT LOGIN

ARGUMENTS=("$@")

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

  fi
done

URL="http://localhost:$PORT"
# no slash
URL=${URL%/}

echo "EXAMPLE: bash pm2-warmup.sh --port $PORT"
echo "Running on: "$URL

COUNTER=0
MAX_COUNTER=40
while [ $COUNTER -lt $MAX_COUNTER ]; do
	CURL_OUTPUT=`curl -X GET -IL "$URL"/api/account/status -o /dev/null -w '%{http_code}\n' -s`
	if [ $CURL_OUTPUT != "401" ] && [ $CURL_OUTPUT != "406" ]; then
		if ! (($COUNTER % 2)); then
			echo "$COUNTER - $CURL_OUTPUT - retry"
		fi
		sleep 3
		let COUNTER=COUNTER+1
	else
		echo "$COUNTER - $CURL_OUTPUT - done"
    let COUNTER=MAX_COUNTER+1 # to exit the while loop
	fi
done

if [[ $COUNTER == $MAX_COUNTER  ]]; then
 echo "!> FAIL Tried more than "$MAX_COUNTER" Times"
 exit 1
fi

# To make Search Suggestions at start faster
CURL_SUGGEST_OUTPUT=`curl -X GET -LI "$URL"/api/suggest/inflate -o /dev/null -w '%{http_code}\n' -s`
echo "!> done ~ -sug:$CURL_SUGGEST_OUTPUT"
