#!/usr/bin/env bash

dotnet build
dotnet publish
cd ./src/bin/Release/net10.0/linux-x64/publish/ && ./debmenu

