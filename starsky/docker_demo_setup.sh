#!/bin/bash

## Use this only in a docker env.
## This script uses /app

if [[ ! $(cat /proc/1/sched | head -n 1 | grep init) ]]; then {
    echo "in docker (continue)"
} else {
    echo "This script is only usefull witin the docker container"
    exit 1
} fi

SOURCE_DIR=/app
APPLICATION_DIR=/app/starsky/out/
STORAGE_FOLDER=/app/starsky/out/storageFolder

function makeDemoUser {
  starskyadmincli=($(find $SOURCE_DIR -type f -name "starskyadmincli.csproj"))
  dotnet run --project ${starskyadmincli[0]} --configuration Release -- --connection "Data Source="$APPLICATION_DIR"/app__data.db" --basepath $STORAGE_FOLDER --name demo@qdraw.nl --password demo@qdraw.nl
}

function getSamplePhotos {
  mkdir -p $STORAGE_FOLDER/vsm
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_134303_DSC00279_e.jpg --output $STORAGE_FOLDER"/vsm/20190530_134303_DSC00279_e.jpg"
  curl https://media.qdraw.nl/download/starsky-sample-photos/20190530_142906_DSC00373_e.jpg --output $STORAGE_FOLDER"/vsm/20190530_142906_DSC00373_e.jpg"

  mkdir -p $STORAGE_FOLDER/den-bosch
  curl https://media.qdraw.nl/download/starsky-sample-photos/20201213_151447_DSC00249.jpg --output $STORAGE_FOLDER"/den-bosch/20201213_151447_DSC00249.jpg"
  curl https://media.qdraw.nl/download/starsky-sample-photos/20201213_151447_DSC00249.arw --output $STORAGE_FOLDER"/den-bosch/20201213_151447_DSC00249.arw"
  curl https://media.qdraw.nl/download/starsky-sample-photos/20201213_151447_DSC00249.xmp --output $STORAGE_FOLDER"/den-bosch/20201213_151447_DSC00249.xmp"

  mkdir -p $STORAGE_FOLDER/vernant
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_102002_dsc03419_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_102002_dsc03419_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_102313_dsc03422_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_102313_dsc03422_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_111108_dsc03533_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_111108_dsc03533_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_114257_dsc03552_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_114257_dsc03552_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_121712_dsc03582_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_121712_dsc03582_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_132228_dsc03601_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_132228_dsc03601_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_141638_dsc03621_e_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_141638_dsc03621_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/vernant-in-de-franse-alpen-2020/1000/20200823_142023_dsc03635_e2_kl1k.jpg --output $STORAGE_FOLDER"/vernant/20200823_142023_dsc03635_e2_kl1k.jpg"

  mkdir -p $STORAGE_FOLDER/gers
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_103842_dsc02901_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_103842_dsc02901_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_110124_dsc02915_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_110124_dsc02915_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_110309_dsc02923_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_110309_dsc02923_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_111303_dsc02934_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_111303_dsc02934_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_111408_dsc02936_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_111408_dsc02936_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_112430_dsc02971_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_112430_dsc02971_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_114302_dsc03000_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_114302_dsc03000_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_115245_dsc03002_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_115245_dsc03002_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_124618_dsc03036_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_124618_dsc03036_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_132630_dsc03055_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_132630_dsc03055_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_134151_dsc03076_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_134151_dsc03076_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_142311_dsc03131_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_142311_dsc03131_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_144514_dsc03139_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_144514_dsc03139_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_144726_dsc03141_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_144726_dsc03141_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_151603_dsc03149_e2_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_151603_dsc03149_e2_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_152745_dsc03164_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_152745_dsc03164_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_154226_dsc03201_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_154226_dsc03201_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_154435_dsc03214_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_154435_dsc03214_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_155353_dsc03221_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_155353_dsc03221_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_160248_dsc03231_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_160248_dsc03231_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_160529_dsc03252_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_160529_dsc03252_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_161547_dsc03262_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_161547_dsc03262_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_162357_dsc03273_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_162357_dsc03273_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_163409_dsc03308_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_163409_dsc03308_e_kl1k.jpg"
  curl https://media.qdraw.nl/log/gers-in-de-franse-alpen-2020/1000/20200822_164141_dsc03321_e_kl1k.jpg --output $STORAGE_FOLDER"/gers/20200822_164141_dsc03321_e_kl1k.jpg"

  starskysynchronizecli=($(find $SOURCE_DIR -type f -name "starskysynchronizecli.csproj"))
  dotnet run --project ${starskysynchronizecli[0]} --configuration Release -- --basepath $STORAGE_FOLDER --connection "Data Source="$APPLICATION_DIR"/app__data.db" -v
}

function start_pushd {
  echo "go to: "$APPLICATION_DIR
  mkdir -p $APPLICATION_DIR
  pushd $APPLICATION_DIR
}

function  end_popd {
  popd
}

if [ -z "$E_ISDEMO" ]; then
    echo "NO PARAM PASSED"
else
    echo $E_ISDEMO
    start_pushd
    makeDemoUser
    getSamplePhotos
    end_popd
fi
