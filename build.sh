#!/bin/sh

# Generate localization resource files
python Hitorus-Localization/generate-resx.py
# Build Hiotrus.Web
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh -c 9.0 -InstallDir ./dotnet
./dotnet/dotnet --version
./dotnet/dotnet publish src/Hitorus.Web -c Release -o output