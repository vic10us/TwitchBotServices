FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TwitchBot.Service/TwitchBot.Service.csproj", "TwitchBot.Service/"]

RUN dotnet restore "TwitchBot.Service/TwitchBot.Service.csproj"
COPY . .
WORKDIR "/src/TwitchBot.Service"
RUN dotnet build "TwitchBot.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwitchBot.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwitchBot.Service.dll"]