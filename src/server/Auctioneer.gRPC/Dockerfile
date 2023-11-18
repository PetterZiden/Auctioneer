FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Auctioneer.gRPC/Auctioneer.gRPC.csproj", "src/Auctioneer.gRPC/"]
RUN dotnet restore "src/Auctioneer.gRPC/Auctioneer.gRPC.csproj"
COPY . .
WORKDIR "/src/src/Auctioneer.gRPC"
RUN dotnet build "Auctioneer.gRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auctioneer.gRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auctioneer.gRPC.dll"]
