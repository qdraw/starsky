#!/bin/bash
pushd starsky
dotnet publish -c release -r linux-arm
popd
pushd starsky-cli
dotnet publish -c release -r linux-arm
popd
