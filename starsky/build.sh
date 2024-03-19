#!/usr/bin/env bash

bash --version 2>&1 | head -n 1

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/build/_build.csproj"
TEMP_DIRECTORY="$SCRIPT_DIR//.nuke/temp"

# for CI installs
DOTNET_GLOBAL_FILE="$SCRIPT_DIR//global.json"
DOTNET_INSTALL_URL="https://dot.net/v1/dotnet-install.sh"
DOTNET_CHANNEL="Current"

NVM_RC_FILE="$SCRIPT_DIR/.nvmrc"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_MULTILEVEL_LOOKUP=0
export DOTNET_NOLOGO=1
export NUKE_TELEMETRY_OPTOUT=true

DOTNET_MAC_OS_PKG_X64="https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-SDK_VERSION-macos-x64-installer"
DOTNET_MAC_OS_PKG_ARM64="https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-SDK_VERSION-macos-arm64-installer"

###########################################################################
# EXECUTION
###########################################################################

function FirstJsonValue {
    perl -nle 'print $1 if m{"'"$1"'": "([^"]+)",?}' <<< "${@:2}"
}

function SET_DOTNET_VERSION_TO_VAR {
    # If global.json exists, load expected version
    if [[ -f "$DOTNET_GLOBAL_FILE" ]]; then
        DOTNET_VERSION=$(FirstJsonValue "version" "$(cat "$DOTNET_GLOBAL_FILE")")
        if [[ "$DOTNET_VERSION" == ""  ]]; then
            unset DOTNET_VERSION
        fi
    fi
}

function INSTALL_DOTNET_VIA_WEBSITE_PKG {
     if [[ "$(uname -m)" == "x86_64" ]]; then
         DOTNET_MAC_OS_PKG_X64_VERSION=$(sed "s/\SDK_VERSION/$DOTNET_VERSION/g" <<< $DOTNET_MAC_OS_PKG_X64)
         RESULT=$(curl -s $DOTNET_MAC_OS_PKG_X64_VERSION -X GET | grep 'window.location = "')
     else 
         DOTNET_MAC_OS_PKG_ARM64_VERSION=$(sed "s/\SDK_VERSION/$DOTNET_VERSION/g" <<< $DOTNET_MAC_OS_PKG_ARM64)
         RESULT=$(curl -s $DOTNET_MAC_OS_PKG_ARM64_VERSION -X GET | grep 'window.location = "')          
     fi

     RESULT1=$(sed "s/\window.open(\"//g" <<< $RESULT)
     RESULT2=$(sed "s/\", \"_self\");//g" <<< $RESULT1)
     RESULT3=$(sed "s/\window.location = \"//g" <<< $RESULT2)
     RESULT4=$(sed 's/";//g' <<< $RESULT3)
     RESULT5=`echo $RESULT4 | sed 's/ *$//g'`
     URL=${RESULT4%$'\r'}
      
     if [[ "$URL" == https* && "$URL" == *.pkg* ]]; then 
        echo "next download from: "$URL
        echo "   afterwards you will be asked for a password to install dotnet"
        mkdir -p $SCRIPT_DIR"/.nuke/temp/installer/"
        curl -s -o $SCRIPT_DIR"/.nuke/temp/installer/"$DOTNET_VERSION".pkg" $URL
        echo "package is downloaded, next install dotnet"
        echo "sudo installer -pkg "$SCRIPT_DIR"/.nuke/temp/installer/"$DOTNET_VERSION".pkg -target /"
        sudo installer -pkg $SCRIPT_DIR"/.nuke/temp/installer/"$DOTNET_VERSION".pkg" -target /
        rm -rf $SCRIPT_DIR"/.nuke/temp/installer/"
     else 
        echo "SKIP: mis match in url"             
     fi
     if [[ -f $HOME"/.zprofile" ]]; then
        source ~/.zprofile
     fi
     if [[ -f $HOME"/.zshrc" ]]; then
        source ~/.zshrc
     fi 
}

echo "     os: " "$(uname)" "ci: " $CI "tfbuild: "  $TF_BUILD  " install check: " $FORCE_INSTALL_CHECK
# install dotnet via website   
if [[ "$(uname)" == "Darwin" && $CI != true && $TF_BUILD != true ]] || [[ "$(uname)" == "Darwin" && "$FORCE_INSTALL_CHECK" == true ]]; then
    SET_DOTNET_VERSION_TO_VAR
    if [ -x "$(command -v dotnet)" ]; then
        if [[ $(dotnet --info) != *$DOTNET_VERSION* ]]; then
             echo "dotnet version mismatch, installing $DOTNET_VERSION" $(uname -m)
             INSTALL_DOTNET_VIA_WEBSITE_PKG
        fi
    else
        echo "dotnet not installed, installing $DOTNET_VERSION"
        INSTALL_DOTNET_VIA_WEBSITE_PKG
        echo path $PATH
    fi
fi


# If dotnet CLI is installed globally and it matches requested version, use for execution
if [ -x "$(command -v dotnet)" ] && dotnet --version &>/dev/null; then
    export DOTNET_EXE="$(command -v dotnet)"
else
    SET_DOTNET_VERSION_TO_VAR

    echo "next: install dotnet $DOTNET_VERSION via dotnet-install.sh"

    # Download install script
    DOTNET_INSTALL_FILE="$TEMP_DIRECTORY/dotnet-install.sh"
    mkdir -p "$TEMP_DIRECTORY"
    curl -Lsfo "$DOTNET_INSTALL_FILE" "$DOTNET_INSTALL_URL"
    chmod +x "$DOTNET_INSTALL_FILE"

    # Install by channel or version
    DOTNET_DIRECTORY="$TEMP_DIRECTORY/dotnet-unix"
    if [[ -z ${DOTNET_VERSION+x} ]]; then
        "$DOTNET_INSTALL_FILE" --install-dir "$DOTNET_DIRECTORY" --channel "$DOTNET_CHANNEL" --no-path
    else
        "$DOTNET_INSTALL_FILE" --install-dir "$DOTNET_DIRECTORY" --version "$DOTNET_VERSION" --no-path
    fi
    export DOTNET_EXE="$DOTNET_DIRECTORY/dotnet"
fi

# check if nodejs is installed
# depends on NVM_RC_FILE, SCRIPT_DIR variables
if [[ "$(uname)" == "Darwin" && $CI != true && $TF_BUILD != true ]] || [[ "$(uname)" == "Darwin" && "$FORCE_INSTALL_CHECK" == true ]]; then
    if [ -x "$(command -v npm)" ] && npm --version &>/dev/null && [ -d "${HOME}/.nvm/" ]; then
        echo "   npm installed"
        if [ -x "$(command -v brew)" ] && brew --version &>/dev/null; then
            if [ -f $(brew --prefix nvm)/nvm.sh ]; then
              echo 'sourcing nvm from $(brew --prefix nvm)/nvm.sh'
              chmod +x $(brew --prefix nvm)/nvm.sh
              set +ue +o pipefail
              source $(brew --prefix nvm)/nvm.sh &>/dev/null
              set -eo pipefail
            fi
        fi
                    
        if [ -f $NVM_RC_FILE ]; then
            ACTIVE_VERSION=$(node --version || true)
            GLOBAL_NPM=$(which npm || echo "not found on PATH")
            NVM_RC_FILE_CONTENT="$(cat $NVM_RC_FILE)"
                        
            if [ "${ACTIVE_VERSION%%$NVM_RC_FILE_CONTENT*}" ] || [ ! -e "$GLOBAL_NPM" ]; then
              echo "expected node '$NVM_RC_FILE_CONTENT' and npm on path"
              echo "but was '$ACTIVE_VERSION' and npm:'$GLOBAL_NPM'"
              nvm install $NVM_RC_FILE_CONTENT
              cd $SCRIPT_DIR
              nvm use
            fi
        fi
    else
        if [ -x "$(command -v brew)" ] && brew --version &>/dev/null; then
            echo "homebrew installed"

            if [ ! -d "${HOME}/.nvm/" ]; then
                echo "next: install nvm"

                brew install nvm
                mkdir -p $HOME/.nvm

                if [[ "$(uname -m)" == "x86_64" ]]; then               
                    echo 'export NVM_DIR="$HOME/.nvm"' >> $HOME/.zshrc
                    echo ' [ -s "/usr/local/opt/nvm/nvm.sh" ] && \. "/usr/local/opt/nvm/nvm.sh"  # This loads nvm' >> $HOME/.zshrc
                    echo '[ -s "/usr/local/opt/nvm/etc/bash_completion.d/nvm" ] && \. "/usr/local/opt/nvm/etc/bash_completion.d/nvm"  # This loads nvm bash_completion' >> $HOME/.zshrc
                else 
                    echo 'export NVM_DIR="$HOME/.nvm"' >> $HOME/.zshrc
                    echo ' [ -s "/opt/homebrew/opt/nvm/nvm.sh" ] && \. "/opt/homebrew/opt/nvm/nvm.sh"  # This loads nvm' >> $HOME/.zshrc
                    echo '[ -s "/opt/homebrew/opt/nvm/etc/bash_completion.d/nvm" ] && \. "/opt/homebrew/opt/nvm/etc/bash_completion.d/nvm"  # This loads nvm bash_completion' >> $HOME/.zshrc             
                fi 
            else    
                echo "nvm installed"
            fi
            
            chmod +x $(brew --prefix nvm)/nvm.sh

            if [ -f $(brew --prefix nvm)/nvm.sh ]; then
              echo 'sourcing nvm from $(brew --prefix nvm)/nvm.sh'
              set +ue +o pipefail
              source $(brew --prefix nvm)/nvm.sh &>/dev/null
              set -eo pipefail
            fi

            # https://github.com/nvm-sh/nvm/issues/1290#issuecomment-737964030
            if [ -f $NVM_RC_FILE ]; then
                NVM_RC_FILE_CONTENT="$(cat $NVM_RC_FILE)"
                nvm install $NVM_RC_FILE_CONTENT
                nvm use node
                cd $SCRIPT_DIR
                nvm use
            fi
        fi 
    fi 
fi


echo "Microsoft (R) .NET SDK version $("$DOTNET_EXE" --version)"
echo "        next: _build project"

CURRENT_PWD="$(pwd)"
cd $SCRIPT_DIR

"$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet

if [[ ! -f $SCRIPT_DIR"/build/bin/Debug/_build.deps.json" ]]; then
    echo "Retry: File not found: $SCRIPT_DIR/build/bin/Debug/_build.deps.json"
    "$DOTNET_EXE" build "$BUILD_PROJECT_FILE" /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
fi

echo "        next: run _build project"
"$DOTNET_EXE" run --project "$BUILD_PROJECT_FILE" --no-build -- --no-logo "$@"
DOTNET_EXE_EXIT_CODE=$?

cd $CURRENT_PWD

if [ $DOTNET_EXE_EXIT_CODE -eq 0 ] 
then 
  echo "OK" 
else 
  exit $DOTNET_EXE_EXIT_CODE
fi
