FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /app

COPY FM.LiveSwitch.Connect.sln FM.LiveSwitch.Connect.sln
COPY GitVersion.yml GitVersion.yml
COPY src src
COPY .git .git
RUN dotnet restore
RUN dotnet publish src/FM.LiveSwitch.Connect/FM.LiveSwitch.Connect.csproj -c Release -o lib
RUN rm FM.LiveSwitch.Connect.sln
RUN rm GitVersion.yml
RUN rm -rf src
RUN rm -rf .git

FROM mcr.microsoft.com/dotnet/runtime:3.1
WORKDIR /app
COPY --from=build /app/lib .

RUN apt-get -y update && \
    apt-get install -y --no-install-recommends ffmpeg=7:4.* && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "lsconnect.dll"]