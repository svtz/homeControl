FROM microsoft/dotnet:2.1-runtime-stretch-slim-arm32v7

COPY ./publish /publish
WORKDIR /publish
ENTRYPOINT ["dotnet", "homeControl.ControllerService.dll"]