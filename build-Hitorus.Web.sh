#!/bin/sh

# Generate Hitorus.Web localization resource files
python generate-resx.py 2
# Build Hiotrus.Web
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet
./dotnet/dotnet workload install wasm-tools
./dotnet/dotnet publish src/Hitorus.Web -c Release -o output
rm **/*.br
rm **/*.gz