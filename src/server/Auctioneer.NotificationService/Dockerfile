FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Auctioneer.NotificationService/Auctioneer.NotificationService.csproj", "src/Auctioneer.NotificationService/"]
COPY ["src/Auctioneer.MessagingContracts/Auctioneer.MessagingContracts.csproj", "src/Auctioneer.MessagingContracts/"]
RUN dotnet restore "src/Auctioneer.NotificationService/Auctioneer.NotificationService.csproj"
COPY . .
WORKDIR "/src/src/Auctioneer.NotificationService"
RUN dotnet build "Auctioneer.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctioneer.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auctioneer.NotificationService.dll"]
