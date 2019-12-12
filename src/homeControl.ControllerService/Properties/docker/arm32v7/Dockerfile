FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore "src/homeControl.ControllerService/homeControl.ControllerService.csproj"
WORKDIR "/build/src/homeControl.ControllerService"
RUN dotnet publish "homeControl.ControllerService.csproj" -c Release -o /app/bin

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm32v7 as final
WORKDIR /app
COPY --from=build /app/bin .
ENTRYPOINT ["dotnet", "homeControl.ControllerService.dll"]