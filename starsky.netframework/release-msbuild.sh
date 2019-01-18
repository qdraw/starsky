#!/bin/bash
nuget restore
MSBuild "starskySyncFramework.csproj" /P:Configuration=release
