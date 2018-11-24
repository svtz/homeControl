FROM microsoft/dotnet:2.1-runtime-stretch-slim-arm32v7

# Install libusb
RUN apt-get update \
    && apt-get install -y --no-install-recommends libusb-1.0-0 \
    && rm -rf /var/lib/apt/lists/* \
    && ln -s /lib/arm-linux-gnueabihf/libusb-1.0.so.0 /lib/arm-linux-gnueabihf/libusb-1.0.so

COPY ./publish /publish
WORKDIR /publish
ENTRYPOINT ["dotnet", "homeControl.NooliteService.dll"]