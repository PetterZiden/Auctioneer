FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Auctioneer.NotifcationService/Auctioneer.NotifcationService.csproj", "src/Auctioneer.NotifcationService/"]
RUN dotnet restore "src/Auctioneer.NotifcationService/Auctioneer.NotifcationService.csproj"
COPY . .
WORKDIR "/src/src/Auctioneer.NotifcationService"
RUN dotnet build "Auctioneer.NotifcationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctioneer.NotifcationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auctioneer.NotifcationService.dll"]
