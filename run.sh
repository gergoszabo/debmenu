#docker run --name debmenu --rm -d -v $(pwd):/app:rw -v app-cache:/cache -v ~/.microsoft/user-secrets:/root/.microsoft/user-secrets:ro mcr.microsoft.com/dotnet/sdk:10.0 bash -c "cd /app && dotnet run"
#dotnet run
cd ./bin/Release/net10.0/linux-x64/publish && ./debmenu
