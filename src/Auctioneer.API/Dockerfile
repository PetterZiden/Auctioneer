FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Auctioneer.API/Auctioneer.API.csproj", "Auctioneer.API/"]
RUN dotnet restore "Auctioneer.API/Auctioneer.API.csproj"
COPY . .
WORKDIR "/src/Auctioneer.API"
RUN dotnet build "Auctioneer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctioneer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auctioneer.API.dll"]
