FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore "src/homeControl.ConfigurationStore/homeControl.ConfigurationStore.csproj"
WORKDIR "/build/src/homeControl.ConfigurationStore"
RUN dotnet publish "homeControl.ConfigurationStore.csproj" -c Release -o /app/bin

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm64v8 as final
WORKDIR /app
COPY --from=build /app/bin .
ENTRYPOINT ["dotnet", "homeControl.ConfigurationStore.dll"]