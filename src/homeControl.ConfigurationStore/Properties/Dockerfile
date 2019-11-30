FROM mcr.microsoft.com/dotnet/core/runtime:3.0.1-buster-slim-arm64v8

COPY ./publish /publish
WORKDIR /publish
ENTRYPOINT ["dotnet", "homeControl.ConfigurationStore.dll"]