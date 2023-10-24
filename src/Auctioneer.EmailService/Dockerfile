FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Auctioneer.EmailService/Auctioneer.EmailService.csproj", "src/Auctioneer.EmailService/"]
COPY ["src/Auctioneer.MessagingContracts/Auctioneer.MessagingContracts.csproj", "src/Auctioneer.MessagingContracts/"]
RUN dotnet restore "src/Auctioneer.EmailService/Auctioneer.EmailService.csproj"
COPY . .
WORKDIR "/src/src/Auctioneer.EmailService"
RUN dotnet build "Auctioneer.EmailService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctioneer.EmailService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auctioneer.EmailService.dll"]
