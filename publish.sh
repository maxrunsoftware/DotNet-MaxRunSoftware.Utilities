#! /bin/bash

buildpre=0
buildpost=0

if [[ " $@ " =~ " buildpre " ]]; then
   buildpre=1
   buildpost=0
fi

if [[ " $@ " =~ " buildpost " ]]; then
   buildpre=0
   buildpost=1
fi

# https://stackoverflow.com/a/246128
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

echo SCRIPT_DIR: $SCRIPT_DIR

set -x #echo on

mkdir -p $SCRIPT_DIR/publish/nuget
mkdir -p $SCRIPT_DIR/publish/packages

if [[ $buildpre = 1 ]]; then
    rm -f $SCRIPT_DIR/MaxRunSoftware.Utilities/bin/Debug/*.nupkg
    rm -f $SCRIPT_DIR/MaxRunSoftware.Utilities/bin/Release/*.nupkg
    rm -f $SCRIPT_DIR/publish/nuget/*.nupkg
fi

if [[ $buildpost = 1 ]]; then
    find "$SCRIPT_DIR/MaxRunSoftware.Utilities/bin/Debug" -name '*.nupkg' -exec cp -prv '{}' "$SCRIPT_DIR/publish/nuget/" ';'
    for filename in $SCRIPT_DIR/publish/nuget/*.nupkg; do
        #echo "  ->  $filename"
        nuget add "$filename" -source $SCRIPT_DIR/publish/packages
    done
fi






