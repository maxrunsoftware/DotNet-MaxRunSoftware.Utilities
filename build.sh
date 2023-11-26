#! /bin/bash

#set -x #echo on
#set +x #echo off

# config
script_dir=
build_dir="build"
build_nuget_dir="nuget"
git_id=
git_branch=
git_url=
action="none"
build_config="debug"
version_suffix_datetime=$(date -u '+%Y%m%d-%H%M%S')
version_suffix="beta-${version_suffix_datetime}"


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

exit_if_error_last() {
    exit_if_error $? "Command Failed"
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
    if dir_not_exists $1; then exit_if_error 1 "Directory does not exist: $1"; fi
}

file_exists() {
    if is_not_empty $1 && [ -e "$1" ]; then return 0; else return 1; fi  # https://stackoverflow.com/a/40082454
}

file_not_exists() {
    if file_exists $1; then return 1; else return 0; fi
}


str_equals() {
    local x=$( tr '[:upper:]' '[:lower:]' <<<"$1" )
    local y=$( tr '[:upper:]' '[:lower:]' <<<"$2" )
    if [[ " $x " =~ " $y " ]]; then return 0; else return 1; fi
}

str_not_equals() {
    if str_equals $1 $2; then return 1; else return 0; fi
}



declare -a actions_possible=(
    "clean"
    "nuget"
    "nugetpush"
)

declare -a builds_possible=(
    "debug"
    "release"
)

for arg in "$@" 
do
    is_valid_arg=false  # https://stackoverflow.com/a/2953673
    
    for action_possible in "${actions_possible[@]}"
    do
        if str_equals $arg $action_possible; then
            action=$action_possible
            is_valid_arg=true 
        fi        
    done
        
    for build_possible in "${builds_possible[@]}"
    do
        if str_equals $arg $build_possible; then
            build_config=$build_possible
            is_valid_arg=true
        fi        
    done
    
    if [ "$is_valid_arg" = false ] ; then
        exit_if_error 1 "Invalid argument: $arg"
    fi    
done

if str_equals $build_config "debug"; then build_config="Debug"; fi
if str_equals $build_config "release"; then build_config="Release"; fi



# https://stackoverflow.com/a/246128
script_dir=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
script_dir=$(realpath "$script_dir")
check_dir_exists $script_dir

build_dir_path="${script_dir}/${build_dir}"
build_nuget_dir_path="${build_dir_path}/${build_nuget_dir}"

git_id=$(git -C $script_dir rev-parse HEAD)
git_branch=$(git -C $script_dir branch --show-current)
git_url=$(git -C $script_dir config --get remote.origin.url)


echo ""
echo " --------------------- SETTINGS --------------------- "
echo "              SCRIPT_DIR: $script_dir"
echo "               BUILD_DIR: $build_dir_path"
echo "         BUILD_NUGET_DIR: $build_nuget_dir_path"
echo "                  ACTION: $action"
echo "            BUILD_CONFIG: $build_config"
echo "          VERSION_SUFFIX: $version_suffix"
echo "                  GIT_ID: $git_id"
echo "              GIT_BRANCH: $git_branch"
echo "                 GIT_URL: $git_url"
echo ""



clean_dirs() {
    find "${script_dir}" -type d -name bin -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;
    find "${script_dir}" -type d -name obj -prune -mindepth 3 -maxdepth 3 -exec rm -rf {} \;
    
    if dir_exists $build_dir_path; then
        rm -rf $build_dir_path;
        exit_if_error_last
    fi
    
    if dir_exists $build_nuget_dir_path; then
        rm -rf $build_nuget_dir_path;
        exit_if_error_last
    fi
    
}

nuget_create() {
    clean_dirs
    exit_if_error_last
    if dir_not_exists $build_dir_path; then
        mkdir -p $build_dir_path;
        exit_if_error_last
    fi
    
    if dir_not_exists $build_nuget_dir_path; then
        mkdir -p $build_nuget_dir_path;
        exit_if_error_last
    fi
        
    local dotnet_props="\"/p:RepositoryCommit=${git_id};RepositoryBranch=${git_branch};RepositoryUrl=${git_url}\""  # https://stackoverflow.com/a/51485481    
    
    dotnet build "${script_dir}/MaxRunSoftware.Utilities.sln" --configuration "${build_config}" --nologo --version-suffix "${version_suffix}" "${dotnet_props}"
    exit_if_error_last
    dotnet pack "${script_dir}/MaxRunSoftware.Utilities.sln" --configuration "${build_config}" --output "${build_nuget_dir_path}" --nologo --include-symbols --include-source --version-suffix "${version_suffix}" "${dotnet_props}"
    exit_if_error_last
}

nuget_push() {
    nuget_create
    
    set -x #echo on
    keyNuGet=$(<~/Keys/MaxRunSoftware_NuGet.txt)
    for filename in "${build_nuget_dir_path}/*.nupkg"; do
        echo "Pushing to NUGET: $filename"
        dotnet nuget push "$filename" --api-key $keyNuGet --source https://api.nuget.org/v3/index.json --skip-duplicate
    done
    set +x #echo off
}





if str_equals $action "clean"; then clean_dirs;
elif str_equals $action "nuget"; then nuget_create;
elif str_equals $action "nugetpush"; then nuget_push;
fi

echo ""
