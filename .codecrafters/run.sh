#!/bin/sh
#
# This script is used to run your program on CodeCrafters
#
# This runs after .codecrafters/compile.sh
#
# Learn more: https://codecrafters.io/program-interface

set -e # Exit on failure

# Install .NET 8 if not present
if [ ! -d "/usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.0" ]; then
    echo "Installing .NET 8..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version 8.0.0
    export PATH="$HOME/.dotnet:$PATH"
fi

exec /tmp/codecrafters-build-http-server-csharp/codecrafters-http-server "$@"
