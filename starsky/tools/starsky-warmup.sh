#!/bin/bash
CURRENTDIR=$(dirname "$0")

if [ ! -f $CURRENTDIR/.starskyenv ]; then
	echo ">> Please add this file: \`$CURRENTDIR/.starskyenv\`<<"
	exit
fi

pushd $CURRENTDIR

BEARER=$(grep BEARER .starskyenv | cut -d '=' -f 2- | tr -d '"')
URL=$(grep STARSKYURL .starskyenv | cut -d '=' -f 2- | tr -d '"')

## remove last slash from url
URL=${URL%/}

if [[ $URL == "" ]]; then
	URL="http://localhost:5000" # no slash
fi


if [[ $BEARER == "" ]]; then
	echo ">> Please BEARER and URL to \`$CURRENTDIR/.starskyenv\`<<"
	echo "BEARER=base64 hased string with the content: username:password"
	echo "STARSKYURL=http://localhost:5000"
	popd
	exit
fi

COUNTER=0
MAXCOUNTER=30
while [ $COUNTER -lt $MAXCOUNTER ]; do
	CURLOUTPUT=`curl -X GET --header "Authorization: Basic $BEARER" -IL "$URL"/account?json=true -o /dev/null -w '%{http_code}\n' -s`
	if [ $CURLOUTPUT != "200" ]; then
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

CURLENVOUTPUT=`curl -X GET --header "Authorization: Basic $BEARER" -LI "$URL"/api/env -o /dev/null -w '%{http_code}\n' -s`
CURLSEARCHOUTPUT=`curl -X GET --header "Authorization: Basic $BEARER" -LI "$URL"/search?t= -o /dev/null -w '%{http_code}\n' -s`
CURLIMPORTOUTPUT=`curl -X GET --header "Authorization: Basic $BEARER" -LI "$URL"/import -o /dev/null -w '%{http_code}\n' -s`

echo "!> done ~ env:$CURLENVOUTPUT - search:$CURLSEARCHOUTPUT - import:$CURLIMPORTOUTPUT"
popd
