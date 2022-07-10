FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SkyPlaylistManager/SkyPlaylistManager.csproj", "SkyPlaylistManager/"]
RUN dotnet restore "SkyPlaylistManager/SkyPlaylistManager.csproj"
COPY . .
WORKDIR "/src/SkyPlaylistManager"
RUN dotnet build "SkyPlaylistManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SkyPlaylistManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SkyPlaylistManager.dll"]
