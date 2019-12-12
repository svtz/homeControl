FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore "src/homeControl.NooliteService/homeControl.NooliteService.csproj"
WORKDIR "/build/src/homeControl.NooliteService"
RUN dotnet publish "homeControl.NooliteService.csproj" -c Release -o /app/bin

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim-arm32v7 as final

# Install libusb
RUN apt-get update \
    && apt-get install -y --no-install-recommends libusb-1.0-0 \
    && rm -rf /var/lib/apt/lists/* \
    && ln -s /lib/arm-linux-gnueabihf/libusb-1.0.so.0 /lib/arm-linux-gnueabihf/libusb-1.0.so

WORKDIR /app
COPY --from=build /app/bin .

ENTRYPOINT ["dotnet", "homeControl.NooliteService.dll"]