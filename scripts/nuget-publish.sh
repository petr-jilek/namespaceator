#!/bin/sh
set -e

MODULE="Namespaceator"
CSPROJ="$MODULE.csproj"

cd ../src/$MODULE

VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$CSPROJ")

echo "----------Building $MODULE version $VERSION----------"

dotnet restore
dotnet pack -c Release

echo "----------Publishing $MODULE version $VERSION----------"

cd bin/Release

dotnet nuget push $MODULE.$VERSION.nupkg --source "github"

echo "----------Successfully published $MODULE version $VERSION----------"
