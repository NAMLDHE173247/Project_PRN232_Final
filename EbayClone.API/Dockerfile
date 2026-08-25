FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EbayClone.API/EbayClone.API.csproj", "EbayClone.API/"]
RUN dotnet restore "EbayClone.API/EbayClone.API.csproj"
COPY . .
WORKDIR "/src/EbayClone.API"
RUN dotnet build "EbayClone.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EbayClone.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EbayClone.API.dll"]
