
SERVERNAME=x
EXTERNALFOLDER=/opt/starsky/starsky

CURRENTDIR=$(dirname "$0")
PWDPREV=$(pwd)

RUNTIME="linux-arm"
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
    ;;
esac

# get arguments
ARGUMENTS=("$@")

for ((i = 1; i <= $#; i++ )); do
  if [ $i -gt 1 ]; then
    PREV=$(($i-2))
    CURRENT=$(($i-1))

    if [[ ${ARGUMENTS[CURRENT]} == "--help" ]];
    then
        echo "--runtime linux-arm64"
        echo "--branch master"
        echo "--token anything"
    fi

    if [[ ${ARGUMENTS[PREV]} == "--runtime" ]];
    then
        RUNTIME="${ARGUMENTS[CURRENT]}"
    fi

  fi
done

RUNTIMEZIP="starsky-"$RUNTIME".zip"

cd $CURRENTDIR

if test -f "$RUNTIMEZIP"; then
    echo "$RUNTIMEZIP exists."
    scp $RUNTIMEZIP $SERVERNAME:$EXTERNALFOLDER

else 
    echo `pwd`/$RUNTIMEZIP" not found"
    cd ../../starsky
    if test -f "$RUNTIMEZIP"; then
        scp $RUNTIMEZIP $SERVERNAME:$EXTERNALFOLDER
    else
        echo `pwd`/$RUNTIMEZIP" not found"
        
    fi
fi


# go back
cd $PWDPREV