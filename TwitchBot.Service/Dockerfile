FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
ARG uid=1000
ARG gid=1000
RUN groupadd -r appuser -g ${gid} && useradd -u ${uid} -r -g appuser -m -d /home/appuser -s /sbin/nologin -c "Build User" appuser
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
ARG uid=1000
ARG gid=1000
RUN groupadd -r appuser -g ${gid} && useradd -u ${uid} -r -g appuser -m -d /home/appuser -s /sbin/nologin -c "Build User" appuser
WORKDIR /src
COPY ["TwitchBot.Service/TwitchBot.Service.csproj", "TwitchBot.Service/"]
RUN dotnet restore "TwitchBot.Service/TwitchBot.Service.csproj"
COPY . .
WORKDIR "/src/TwitchBot.Service"
RUN dotnet build "TwitchBot.Service.csproj" -c Release -o /app/build

FROM build AS linuxBuild
RUN dotnet publish "TwitchBot.Service.csproj" -c Release -o /app/publish
RUN chown appuser:appuser -R /app

FROM base AS final
WORKDIR /app
COPY --from=linuxBuild /app/publish .
USER appuser
ENTRYPOINT ["dotnet", "TwitchBot.Service.dll"]