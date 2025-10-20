#!/bin/bash

if command -v tput &> /dev/null
then
    COLOR_REST="$(tput sgr0)"
    COLOR_RED="$(tput setaf 1)"
    COLOR_GREEN="$(tput setaf 2)"
    COLOR_BLUE="$(tput setaf 4)"
else
    COLOR_REST=""
    COLOR_RED=""
    COLOR_GREEN=""
    COLOR_BLUE=""
fi

PRINTF_COLOR_FORMAT_NEWLINE='%s%s%s\n'
PRINTF_COLOR_FORMAT='%s%s%s'

START_TIME=$(date +%s)

NO_CACHE_ARG=""
UNIT_TEST_RUN_ARG="--build-arg UNIT_TEST_RUN=true"
PROGRESS_LOGGER_ARG=""
SERVICES_TO_BUILD=()

ARGUMENTS=("$@")
for ((i = 1; i <= $#; i++ )); do
  CURRENT=$(($i-1))
  if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
  then
      echo "This script will build and up docker compose"
      echo "first argument, for example:"
      echo "./compose-build-up.sh starsky"
      echo "append: \"--no-cache\" to disable cache"
      echo "append: \"--test\" to run unit tests (default true)"
      echo "append \"--logger\" to enable plain logger (default false)"
      echo "to debug: export DOCKER_BUILDKIT=0"
      exit 0
  fi
      
  if [[ ${ARGUMENTS[CURRENT]} == "--no-cache" ]];
  then
      NO_CACHE_ARG="--no-cache"
  elif [[ ${ARGUMENTS[CURRENT]} == "--logger" ]]
  then
       PROGRESS_LOGGER_ARG+="--progress=plain"
  elif [[ ${ARGUMENTS[CURRENT]} == "--test" ]]
  then
       UNIT_TEST_RUN_ARG="--build-arg UNIT_TEST_RUN=true"
  elif [[ ${ARGUMENTS[CURRENT]} == "up" ]]
  then
      SERVICES_TO_BUILD+=(".")
  else 
      SERVICES_TO_BUILD+=(${ARGUMENTS[CURRENT]})
  fi
done

if (! docker stats --no-stream &> /dev/null); then
  if [[ "$(uname)" == "Darwin" ]]; then
    # On Mac OS this would be the terminal command to launch Docker
    open /Applications/Docker.app
  elif [[ "$(uname -s)" == *"MINGW64_NT"* ]]; then
  printf "$PRINTF_COLOR_FORMAT_NEWLINE" $COLOR_RED "Make sure Docker Desktop is running and restart this script" $COLOR_REST
    echo "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    exit 1
  fi
  
  printf "$PRINTF_COLOR_FORMAT_NEWLINE" $COLOR_BLUE "Waiting for Docker to launch..." $COLOR_REST
  # Wait until Docker daemon is running and has completed initialisation
  while (! docker stats --no-stream &> /dev/null); do
    printf $PRINTF_COLOR_FORMAT $COLOR_GREEN '..' $COLOR_REST
    # Docker takes a few seconds to initialize
    sleep 2
  done
fi
echo ""
# end docker startup


if [[ ${#SERVICES_TO_BUILD[@]} -eq 0 ]]; 
then
  echo "build all services "$NO_CACHE_ARG" "$UNIT_TEST_RUN_ARG
  docker compose build $PROGRESS_LOGGER_ARG $NO_CACHE_ARG $UNIT_TEST_RUN_ARG

  echo "up everything"
  docker compose --env-file .env -f docker-compose.yml up --timeout 1000 --detach --remove-orphans
    
else
  for SERVICE_NAME in "${SERVICES_TO_BUILD[@]}"
  do
    if [[ ${SERVICE_NAME} != "." ]];
    then
      SERVICE_NAME_LOWER_CASE=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]')
            
      echo "build "$SERVICE_NAME_LOWER_CASE" service "$NO_CACHE_ARG" "$UNIT_TEST_RUN_ARG
      docker compose build $PROGRESS_LOGGER_ARG $SERVICE_NAME_LOWER_CASE $NO_CACHE_ARG $UNIT_TEST_RUN_ARG
      if [[ $? != 0 ]] 
      then 
      printf "$PRINTF_COLOR_FORMAT_NEWLINE" $COLOR_RED 'Build failed' $COLOR_REST
      printf "$PRINTF_COLOR_FORMAT_NEWLINE" $COLOR_RED '        NOT going to run up all service' $COLOR_REST
        exit 1
      fi
    fi    
  done
    
  echo "up all services"
  docker compose --env-file .env -f docker-compose.yml up --timeout 1000 --detach --no-build --remove-orphans
fi

# To display duration
END_TIME=$(date +%s)
DURATION=$(($END_TIME-$START_TIME))
echo "$(($DURATION / 60)) minutes and $(($DURATION % 60)) seconds elapsed."


