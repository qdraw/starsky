#!/bin/bash

# Script goals:
# Uses starsky-$RUNTIME.zip to deploy and setup pm2 instance
# When zip file does not exist, download it from github releases
# overwrite pm2 name if already exist

PM2NAME="starsky"
PORT="4823"
RUNTIME="linux-arm" # defaults to your current os
case $(uname -m) in
  "aarch64")
    RUNTIME="linux-arm64"
    ;;

  "armv7l")
    RUNTIME="linux-arm"
    ;;

  "x86_64")
    if [ $(uname) = "Darwin" ]; then
        RUNTIME="osx-x64"
    fi
    if [ $(uname) = "Linux" ]; then
        RUNTIME="linux-x64"
    fi
    ;;
esac

CURRENT_DIR=$(dirname "$0")
OUTPUT_DIR=$CURRENT_DIR

# command line args
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do

    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--name pm2name"
        echo "--runtime linux-arm"
        echo "     (or:) --runtime linux-arm64"
        echo "     (or:) --runtime osx-x64"
        echo "     (or:) --runtime win7-x64"
        echo "(optional) --port 4823"
        echo "(optional) --anywhere (to allow access from anywhere, defaults to false)"
        echo "(optional) --appinsights - to ask for app insights keys"
        exit 0
    fi

    # When true, allow access from anywhere not only localhost
    # defaults to false
    # only used on creation, when enabled you need to manual remove a pm2 instance
    if [[ ${ARGUMENTS[CURRENT]} == "--anywhere" ]];
    then
        ANYWHERE=true
    fi

    if [[ ${ARGUMENTS[CURRENT]} == "--appinsights" ]];
    then
        USEAPPINSIGHTS=true
    fi

    if [ $i -gt 1 ]; then
        PREV=$(($i-2))
        CURRENT=$(($i-1))
        
        if [[ ${ARGUMENTS[PREV]} == "--name" ]];
        then
            PM2NAME="${ARGUMENTS[CURRENT]}"
        fi
        
        if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
        then
            RUNTIME="${ARGUMENTS[CURRENT]}"
        fi
        
        if [[ ${ARGUMENTS[PREV]} == "--port" ]];
        then
            PORT="${ARGUMENTS[CURRENT]}"
        fi
        
        if [[ ${ARGUMENTS[PREV]} == "--output" ]];
        then
            OUTPUT_DIR="${ARGUMENTS[CURRENT]}"
        fi
    fi
done

# add slash if not exists
LAST_CHAR_OUTPUT_DIR=${OUTPUT_DIR:length-1:1}
[[ $LAST_CHAR_OUTPUT_DIR != "/" ]] && OUTPUT_DIR="$OUTPUT_DIR/"; :

if [ ! -d $OUTPUT_DIR ]; then
    echo "FAIL "$OUTPUT_DIR" does not exist "
    exit 1
fi

if [ -f $OUTPUT_DIR"Startup.cs" ]; then # output dir should have slash at end
    echo "FAIL: You should not run this folder from the source folder"
    echo "copy this file to the location to run it from"
    echo "end script due failure"
    exit 1
fi


# settings
echo "run with the following parameters "

if [ "$ANYWHERE" = true ] ; then
    ANYWHERESTATUSTEXT="--anywhere $ANYWHERE"
fi
if [ "$USEAPPINSIGHTS" = true ] ; then
    USEAPPINSIGHTSSTATUSTEXT="--appinsights $USEAPPINSIGHTS"
fi

echo "--name" $PM2NAME " --runtime" $RUNTIME "--port" $PORT $USEAPPINSIGHTSSTATUSTEXT $ANYWHERESTATUSTEXT

cd $OUTPUT_DIR

if ! command -v pm2 &> /dev/null
then
    echo "warning: pm2 is missing, the script continues but skips the last step"
    echo "cannot stop current service"
else
  echo "check if service exist >"
  pm2 describe $PM2NAME > /dev/null
  HASDESCRIBE=$?

  if [ "${HASDESCRIBE}" -eq 0 ]; then
    echo "stop service"
    pm2 stop $PM2NAME
  fi;
fi


# remove current installation

# Keep UserViews over a release
if [ -d "WebHtmlPublish/UserViews" ]; then
  cp -r "WebHtmlPublish/UserViews" "UserViews/"
fi

# delete files in www-root
if [ -f starsky.dll ]; then
    echo "delete dlls so, and everything except pm2 helpers, and"
    echo "    configs, temp, thumbnailTempFolder, deploy zip, sqlite database"

    LSOUTPUT=$(ls)
    for ENTRY in $LSOUTPUT
    do
        if [[ $ENTRY != "appsettings"* 
        && $ENTRY != "pm2-"*
        && $ENTRY != "service-"*
        && $ENTRY != "thumbnailTempFolder"
        && $ENTRY != "temp"
        && $ENTRY != "UserViews"* # Keep UserViews
        && $ENTRY != "starsky-"*
        && $ENTRY != *".db" ]];
        then
            rm -rf "$ENTRY"
        else
            echo "     > skip: $ENTRY"
        fi
    done
else
   echo "> skip: starsky.dll File not found"
fi

# new settings:

HOSTNAME="localhost"
if [ "$ANYWHERE" = true ] ; then
    HOSTNAME="*"
fi
export ASPNETCORE_URLS="http://"$HOSTNAME":"$PORT"/"
export ASPNETCORE_ENVIRONMENT="Production"

# only asked with --appinsights true parameter
if [ ! -z "$USEAPPINSIGHTS" ];
then
    echo "Copy the App Insights key string and press [ENTER]:"
    echo "for example: "
    echo "11111111-2222-3333-4444-555555555555"
    echo " >> THIS VALUE IS IGNORED BY CLI APPLICATIONS <<"
    echo ">>>"
    read -p "Enter: " INSTRUMENTATIONKEY
    export APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATIONKEY
fi

if [ -f starsky-$RUNTIME.zip ]; then
   echo "upgrade existing zip file"
   echo "going to unzip starsky-$RUNTIME.zip"

    unzip -q -o starsky-$RUNTIME.zip -x "pm2-new-instance.sh"
else
   echo "continue > starsky-$RUNTIME.zip File not found"
   echo "try to download latest release"

   # Get latest stable from Github Releases
   # check also 'install-latest-release'
   curl -s https://api.github.com/repos/qdraw/starsky/releases/latest \
   | grep "browser_download_url.*starsky-$RUNTIME.zip" \
   | cut -d ":" -f 2,3 \
   | tr -d \" \
   | wget -qi -

   if [ -f starsky-$RUNTIME.zip ]; then
      echo "use latest stable,"
      echo "going to unzip starsky-$RUNTIME.zip"
      unzip -q -o starsky-$RUNTIME.zip -x "pm2-new-instance.sh"
   else
      echo "FAILED > starsky-$RUNTIME.zip Download failed; exit now"
      exit 1
   fi
fi

echo "reset rights if those are wrong"
/usr/bin/find . -type d -exec chmod 755 {} \;
/usr/bin/find . -type f -exec chmod 644 {} \;

# to restore the content UserViews
if [ -d "UserViews" ]; then
  cp -fr "UserViews" "WebHtmlPublish"
  rm -rf "UserViews"
fi

# execute rights for specific files
if [ -f starskygeocli ]; then
    chmod +rwx ./starskygeocli
fi

if [ -f starskyimportercli ]; then
    chmod +rwx ./starskyimportercli
fi

if [ -f starskysynchronizecli ]; then
    chmod +rwx ./starskysynchronizecli
fi

if [ -f starskythumbnailcli ]; then
    chmod +rwx ./starskythumbnailcli
fi

if [ -f starskythumbnailmetacli ]; then
    chmod +rwx ./starskythumbnailmetacli
fi

if [ -f starskywebftpcli ]; then
    chmod +rwx ./starskywebftpcli
fi

if [ -f starskywebhtmlcli ]; then
    chmod +rwx ./starskywebhtmlcli
fi

if [ -f starskyadmincli ]; then
    chmod +rwx ./starskyadmincli
fi

if [ -f pm2-deploy-on-env.sh ]; then
    chmod +rwx ./pm2-deploy-on-env.sh
fi

if [ -f pm2-install-latest-release.sh ]; then
    chmod +rwx ./pm2-install-latest-release.sh
fi

if [ -f pm2-restore-x-rights.sh ]; then
    chmod +rwx ./pm2-restore-x-rights.sh
fi

if [ -f pm2-warmup.sh ]; then
    chmod +rwx ./pm2-warmup.sh
fi

if [ -f pm2-new-instance.sh ]; then
    chmod +rwx ./pm2-new-instance.sh
fi

if [ -f pm2-download-azure-devops.sh ]; then
    chmod +rwx ./pm2-download-azure-devops.sh
fi

if [ -f github-artifacts-download.sh ]; then
    chmod +rwx ./github-artifacts-download.sh
fi

if [ -f starsky ]; then
    chmod +rwx ./starsky
fi

if [ -f starskygeocli ]; then
    echo "run starskygeocli to auto download dependencies"
    ./starskygeocli -h > /dev/null 2>&1
fi

if [ -f dependencies/exiftool-unix/exiftool ]; then
    chmod +rwx dependencies/exiftool-unix/exiftool
fi

ISIMPORTEROK=999
if [ -f starskyimportercli ]; then
    echo "run starskyimportercli to check if runtime matches the system"
    ./starskyimportercli -h > /dev/null 2>&1
    ISIMPORTEROK=$?
fi

# symlink
if [[ $ISIMPORTEROK -eq 0 ]]; then
  echo "creating symlinks in user bin (~/bin)"
  DIRNAME="$(pwd)"
  mkdir -p ~/bin
  ln -sfn $DIRNAME"/starskygeocli" ~/bin/starskygeocli
  ln -sfn $DIRNAME"/starskyimportercli" ~/bin/starskyimportercli
  ln -sfn $DIRNAME"/starskysynchronizecli" ~/bin/starskysynchronizecli
  ln -sfn $DIRNAME"/starskythumbnailcli" ~/bin/starskythumbnailcli
  ln -sfn $DIRNAME"/starskythumbnailmetacli" ~/bin/starskythumbnailmetacli
  ln -sfn $DIRNAME"/starskywebftpcli" ~/bin/starskywebftpcli
  ln -sfn $DIRNAME"/starskywebhtmlcli" ~/bin/starskywebhtmlcli
  ln -sfn $DIRNAME"/starskyadmincli" ~/bin/starskyadmincli
  ln -sfn $DIRNAME"/starsky" ~/bin/starsky
else
  echo "> skip symlink creation due wrong architecture"
fi

if ! command -v pm2 &> /dev/null
then
    echo "FAIL pm2 is missing run: "
    echo "sudo npm install -g pm2"
    echo "and run it again to add it to pm2"
    echo ""
    echo "!> to warmup, you need to run:"
    echo $OUTPUT_DIR"pm2-warmup.sh --port "$PORT
    exit 1
fi

if [ -f starsky ] && [[ $ISIMPORTEROK -eq 0 ]]; then

    pm2 describe $PM2NAME > /dev/null
    HASDESCRIBE=$?

    echo "check if service exist >"
    SHOULDSCRIPTPATH=$(pwd)"/starsky"
    # this is what is actualy is:
    SCRIPTPATHWITHFLUFF=$(pm2 describe $PM2NAME | grep "script path")
    # removes text script path
    SCRIPTPATHWITHFLUFF2=${SCRIPTPATHWITHFLUFF##*script path }
    # replace |
    SCRIPTPATHWITHFLUFF3="${SCRIPTPATHWITHFLUFF2//\│/}"
    # remove space at beginning
    SCRIPTPATH=${SCRIPTPATHWITHFLUFF3##*( )}
    echo "<"

    echo "new config: ""$SCRIPTPATH"
    echo "path in pm2 config: ""$SHOULDSCRIPTPATH"

    # contains in needed because the string does not match 100%
    if [[ $SCRIPTPATH != *"$SHOULDSCRIPTPATH"* ]]; then
        echo "remove pm2 instance due script path on a different location "
        echo "script path:" $SCRIPTPATH
        echo "should script path: " $SHOULDSCRIPTPATH
        pm2 delete $PM2NAME
        pm2 save --force
        # now the service does not exist anymore
        HASDESCRIBE=1
    else
        echo "skip removal of pm2 service because the path is the same"
    fi

    if [ "${HASDESCRIBE}" -ne 0 ]; then
      echo "add new service"
      pm2 start --name $PM2NAME ./starsky
    else
      echo "start existing service"
      pm2 start $PM2NAME
    fi;

    echo "pm2 show status of " $PM2NAME
    pm2 status
    echo "--"

    echo "> AUTO SAVE DONE - pm2 save " $PM2NAME
    pm2 save --force

    echo "Done and saved :)"
    echo ""
    echo "!> to warmup, you need to run:"
    echo "./pm2-warmup.sh --port "$PORT
else
    echo "FAIL skipped adding to pm2 due missing starsky file or wrong architecture"
    exit 1
fi
