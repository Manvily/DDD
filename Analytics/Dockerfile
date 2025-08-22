# ===== RUNTIME (HTTP) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5002

# ===== BUILD (DEBUG) =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# -- cache restore (najpierw same csproj) --
COPY ["Analytics/src/Analytics.Api/Analytics.Api.csproj", "Analytics/src/Analytics.Api/"]
COPY ["Analytics/src/Analytics.Application/Analytics.Application.csproj", "Analytics/src/Analytics.Application/"]
COPY ["Analytics/src/Analytics.Infrastructure/Analytics.Infrastructure.csproj", "Analytics/src/Analytics.Infrastructure/"]
COPY ["Analytics/src/Analytics.Domain/Analytics.Domain.csproj", "Analytics/src/Analytics.Domain/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["DDD.Domain/DDD.Domain.csproj", "DDD.Domain/"]

RUN dotnet restore "Analytics/src/Analytics.Api/Analytics.Api.csproj"

# -- reszta źródeł --
COPY Analytics/src Analytics/src
COPY Shared Shared
COPY DDD.Domain DDD.Domain

WORKDIR /src/Analytics/src/Analytics.Api
# budujemy w Debug, żeby mieć PDB do debuggera
RUN dotnet build Analytics.Api.csproj -c Debug -o /app/build

# ===== DEBUG STAGE (runtime + PDB) =====
FROM base AS debug
WORKDIR /app
COPY --from=build /app/build .
# URL-e ustawisz z docker-compose przez ASPNETCORE_URLS
ENTRYPOINT ["dotnet", "Analytics.Api.dll"]
