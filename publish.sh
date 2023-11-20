#! /bin/bash

#set -x #echo on
#set +x #echo off

exit 0  # don't do anything

# config
script_dir=
buildpre=0
buildpost=0
project=
project_type=
project_dir=
publish_dir=
publish_nuget_dir=

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
    fi
    
done


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


if is_not_empty $project; then 
    project_dir_raw="${script_dir}/${project}"
    project_dir=$(realpath -q "$project_dir_raw")
    exit_if_error $? "Directory does not exist: ${project_dir_raw}"
    echo "       PROJECT_DIR: $project_dir"
    check_dir_exists $project_dir
fi

if is_empty $project_type; then
    exit_if_error 1 "project_type not defined, should be Debug or Release"
fi

project_compiled_dir_raw="${project_dir}/bin/${project_type}"
echo " PROJECT_COMPILED_DIR: $project_compiled_dir_raw"

echo ""

if dir_not_exists $publish_dir; then mkdir -p $publish_dir; fi
if dir_not_exists $publish_nuget_dir; then mkdir -p $publish_nuget_dir; fi


if is_not_empty $project_dir; then
    project_compiled_dir=$(realpath -q "$project_compiled_dir_raw")
    echo ""
    if dir_not_exists $project_compiled_dir; then
        echo "Skipping directory because it does not exist: $project_compiled_dir_raw"
    else
        if [[ $buildpre = 1 ]]; then
            echo "Removing existing packages from $project_compiled_dir"
            rm -fv ${project_compiled_dir}/*.nupkg
        fi
        
        if [[ $buildpost = 1 ]]; then                
set -x #echo on
            echo "Removing existing packages from $publish_nuget_dir"
            rm -fv ${publish_nuget_dir}/${project}.*.nupkg
set +x #echo off

            echo "Copying compiled nuget packages from $project_compiled_dir to ${publish_nuget_dir}"
            find "$project_compiled_dir" -iname '*.nupkg' -exec cp -prv '{}' "${publish_nuget_dir}/" ';'
            
            #for filename in $script_dir/publish/nuget/*.nupkg; do
                #echo "  ->  $filename"
                #nuget add "$filename" -source $script_dir/publish/packages
            #done
        fi
        
    fi
    
fi

echo ""





