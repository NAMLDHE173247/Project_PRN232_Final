FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EbayClone.MVC/EbayClone.MVC.csproj", "EbayClone.MVC/"]
RUN dotnet restore "EbayClone.MVC/EbayClone.MVC.csproj"
COPY . .
WORKDIR "/src/EbayClone.MVC"
RUN dotnet build "EbayClone.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EbayClone.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EbayClone.MVC.dll"]
