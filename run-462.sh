#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
cd OpenSystem.Demo
dotnet run --project OpenSystem.Demo.csproj --framework net462 "$@"
