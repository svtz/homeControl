FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore "src/homeControl.NooliteF/homeControl.NooliteF.csproj"
WORKDIR "/build/src/homeControl.NooliteF"
RUN dotnet publish "homeControl.NooliteF.csproj" -c Release -o /app/bin

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm64v8 as final
WORKDIR /app
COPY --from=build /app/bin .
ENTRYPOINT ["dotnet", "homeControl.NooliteF.dll"]