#!/bin/sh
set -e

NUGET_API_KEY=$1
if [ -z "$NUGET_API_KEY" ]; then
  echo "Error: NuGet API key is required as the first argument."
  exit 1
fi

MODULE="Namespaceator"
CSPROJ="$MODULE.csproj"

cd ../src/$MODULE

VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$CSPROJ")

echo "----------Building $MODULE version $VERSION----------"

dotnet restore
dotnet pack -c Release

echo "----------Publishing $MODULE version $VERSION----------"

cd bin/Release

dotnet nuget push $MODULE.$VERSION.nupkg --source https://api.nuget.org/v3/index.json --api-key $NUGET_API_KEY

echo "----------Successfully published $MODULE version $VERSION----------"
