#!/bin/bash


# For insiders only
# req: node.js installed

# azure devops
ORGANIZATION="qdraw"
DEVOPSPROJECT="starsky"
DEVOPSRELEASEIDS=( 9 )
DEVOPSRELEASENAMES=( Azure )
NODE="/usr/local/bin/node"

ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--id 9;10"
        echo "--name Azure"
        echo "--token STARSKY_DEVOPS_PAT"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--id" ]];
    then
        DEVOPSRELEASEIDS=($(echo "${ARGUMENTS[CURRENT]}" | tr ';' "\n"))
    fi

    if [[ ${ARGUMENTS[PREV]} == "--name" ]];
    then
        DEVOPSRELEASENAMES=( "${ARGUMENTS[CURRENT]}" )
        # todo fix that it works with multiple names
    fi

    if [[ ${ARGUMENTS[PREV]} == "--token" ]];
    then
        STARSKY_DEVOPS_PAT="${ARGUMENTS[CURRENT]}"
    fi
  fi
done






# STARSKY_DEVOPS_PAT <= use this one
# export STARSKY_DEVOPS_PAT=""

if [[ -z $STARSKY_DEVOPS_PAT ]]; then
  echo "enter your PAT: and press enter"
  read STARSKY_DEVOPS_PAT
fi


TRIGGER_SINGLE_NAME () {
  ENVID=$1
  ENVNAME=$2
  RELEASEID=$3

  echo ">"
  echo "1.$ENVID 2. $ENVNAME 3.$RELEASEID"
  echo "<"


  URL="https://vsrm.dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/Release/releases/"$RELEASEID"/environments/"$ENVID"?api-version=5.1-preview.6"
  PATCH=" {    \"status\": \"inProgress\",    \"scheduledDeploymentTime\": null,"
  PATCH+="  \"comment\": null,    \"variables\": {}  }"
  RESULT=$(curl -X PATCH $URL -u ":$STARSKY_DEVOPS_PAT"  -H "Content-Type: application/json" -d "$PATCH")
}

GET_DATA () {
    LOCALDEVOPSDEFID=$1

    POST="{ \"definitionId\": $LOCALDEVOPSDEFID,\"description\": \"Trigger from script\","
    POST+="\"isDraft\": false,\"reason\": \"none\",\"manualEnvironments\": null}"


    URL="https://vsrm.dev.azure.com/"$ORGANIZATION"/"$DEVOPSPROJECT"/_apis/release/releases?api-version=5.1"

    RESULT=$(curl -X POST $URL -u ":$STARSKY_DEVOPS_PAT"  -H "Content-Type: application/json" -d "$POST")

    RELEASEID=$($NODE -pe 'JSON.parse(process.argv[1]).id' "$RESULT")

    # list of ids
    ENVIDSUNDEF=$($NODE -pe 'for (var item of JSON.parse(process.argv[1]).environments){console.log(item.id+";")}' "$RESULT")
    ENVIDSSTRING=$(echo ${ENVIDSUNDEF//undefined/})
    ENVIDS=($(echo $ENVIDSSTRING | tr '; ' "\n"))

    # list of names
    ENVNAMESUNDEF=$($NODE -pe 'for (var item of JSON.parse(process.argv[1]).environments){console.log(item.name+";")}' "$RESULT")
    ENVNAMESSTRING=$(echo ${ENVNAMESUNDEF//undefined/})
    ENVNAMES=($(echo $ENVNAMESSTRING | tr '; ' "\n"))


    # todo fix that it works with multiple stages
    for i in "${!ENVNAMES[@]}"; do
      SINGLENAME="${ENVNAMES[$i]}"
      echo $SINGLENAME
      if [[ " ${DEVOPSRELEASENAMES[@]} " =~ " ${SINGLENAME} " ]]; then
        for j in "${!ENVIDS[@]}"; do
          ENVID="${ENVIDS[$i]}"
          if [[ $j == $i  ]]; then
            echo "trigger: "
            TRIGGER_SINGLE_NAME $ENVID $SINGLENAME $RELEASEID
          fi
        done
      fi
    done
}

for i in "${DEVOPSRELEASEIDS[@]}"
do
    GET_DATA $i
done
