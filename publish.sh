#! /bin/bash

#set -x #echo on
#set +x #echo off

# config
nuget_suffix_datetime=$(date -u '+%Y%m%d-%H%M%S')
nuget_suffix="beta-${nuget_suffix_datetime}"
script_dir=
buildpre=0
buildpost=0
project=
project_type=
project_dir=
publish_dir=
publish_nuget_dir=
nuget_build=0
action=none

for arg in "$@" 
do
    if [[ " $arg " =~ " buildpre " ]]; then
        buildpre=1
    elif [[ " $arg " =~ " buildpost " ]]; then
        buildpost=1  
    elif [[ "$arg" == MaxRunSoftware.Utilities.* ]]; then
        project="$arg"
    elif [[ " $arg " =~ " Debug " ]]; then
        project_type="Debug"
    elif [[ " $arg " =~ " Release " ]]; then
        project_type="Release"
    elif [[ " $arg " =~ " clean " ]]; then
        action="clean"
    elif [[ " $arg " =~ " nuget " ]]; then
        action="nuget"
    fi
    
done


if [[ $buildpre = 1 ]] || [[ $buildpost = 1 ]]; then exit 0; fi



# https://stackoverflow.com/a/50265513
# https://github.com/codeforester/base/blob/master/lib/stdlib.sh
exit_if_error() {
  local exit_code=$1
  shift
  [[ $exit_code ]] &&                       # do nothing if no error code passed
    ((exit_code != 0)) && {                 # do nothing if error code is 0
      printf '\nERROR: %s\n\n' "$@" >&2     # we can use better logging here
      exit "$exit_code"                     # we could also check to make sure error code is numeric when passed
    }
}

is_not_empty() {
    if [ -n "$1" ]; then return 0; else return 1; fi
}

is_empty() {
    if is_not_empty $1; then return 1; else return 0; fi
}


dir_exists() {
    if is_not_empty $1 && [ -d "$1" ]; then return 0; else return 1; fi  # https://stackoverflow.com/a/59839
}

dir_not_exists() {
    if dir_exists $1; then return 1; else return 0; fi
}

check_dir_exists() {
    if dir_not_exists $1; then
        exit_if_error 1 "Directory does not exist: $1"
    fi
}



# https://stackoverflow.com/a/246128
script_dir=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
script_dir=$(realpath "$script_dir")
check_dir_exists $script_dir

publish_dir="${script_dir}/publish"
publish_nuget_dir="${publish_dir}/nuget"

echo ""
echo " ----------------- SETTINGS ----------------- "
echo "        SCRIPT_DIR: $script_dir"
echo "         BUILD_PRE: $buildpre"
echo "        BUILD_POST: $buildpost"
echo "           PROJECT: $project"
echo "      PROJECT_TYPE: $project_type"
echo "       PUBLISH_DIR: $publish_dir"
echo " PUBLISH_NUGET_DIR: $publish_nuget_dir"
echo "       NUGET_BUILD: $nuget_build"
echo "       ACTION: $action"


if dir_not_exists $publish_dir; then mkdir -p $publish_dir; fi
if dir_not_exists $publish_nuget_dir; then mkdir -p $publish_nuget_dir; fi


if [[ " $action " =~ " clean " ]]; then
    find "${script_dir}" -type d -name bin -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;
    find "${script_dir}" -type d -name obj -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;
fi

if [[ " $action " =~ " nuget " ]]; then
    echo "Building NUGET packages"
    
    find "${script_dir}" -type d -name bin -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;
    find "${script_dir}" -type d -name obj -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;

    
    echo "  Removing existing packages from $publish_nuget_dir"
    if dir_exists $publish_nuget_dir; then rm -rfv $publish_nuget_dir; fi
    if dir_not_exists $publish_nuget_dir; then mkdir -p $publish_nuget_dir; fi

    dotnet build "${script_dir}/MaxRunSoftware.Utilities.sln" --nologo --include-symbols --include-source --version-suffix beta1
    dotnet pack "${script_dir}/MaxRunSoftware.Utilities.sln" --output "${publish_nuget_dir}" --nologo --include-symbols --include-source --version-suffix "${nuget_suffix}"
     
fi

echo ""
