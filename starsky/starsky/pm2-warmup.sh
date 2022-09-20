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
MAXCOUNTER=30
while [ $COUNTER -lt $MAXCOUNTER ]; do
	CURLOUTPUT=`curl -X GET -IL "$URL"/api/account/status -o /dev/null -w '%{http_code}\n' -s`
	if [ $CURLOUTPUT != "401" ]; then
		if ! (($COUNTER % 2)); then
			echo "$COUNTER - $CURLOUTPUT - retry"
		fi
		sleep 3s
		let COUNTER=COUNTER+1
	else
		echo "$COUNTER - $CURLOUTPUT - done"
    let COUNTER=MAXCOUNTER+1 # to exit the while loop
	fi
done

if [[ $COUNTER == $MAXCOUNTER  ]]; then
 echo "for debug:"
 curl -X GET -IL "$URL"/api/account/status   
 echo "!> FAIL Tried more than "$MAXCOUNTER" Times"
 exit 1
fi

# To make Search Suggestions at start faster
CURLSUGGESTOUTPUT=`curl -X GET -LI "$URL"/api/suggest/inflate -o /dev/null -w '%{http_code}\n' -s`
echo "!> done ~ -sug:$CURLSUGGESTOUTPUT"
