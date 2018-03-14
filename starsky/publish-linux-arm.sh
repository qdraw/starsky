#!/bin/bash
mkdir linux-arm
pushd starsky
dotnet publish -c release -r linux-arm --output ../linux-arm
popd
pushd starsky-cli
dotnet publish -c release -r linux-arm --output ../linux-arm
popd
