FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DDD.Api/DDD.Api.csproj", "DDD.Api/"]
COPY ["DDD.Application/DDD.Application.csproj", "DDD.Application/"]
COPY ["DDD.Domain/DDD.Domain.csproj", "DDD.Domain/"]
COPY ["Shared/Shared.Domain/Shared.Domain.csproj", "Shared/Shared.Domain/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
COPY ["DDD.Infrastructure/DDD.Infrastructure.csproj", "DDD.Infrastructure/"]
RUN dotnet restore "DDD.Api/DDD.Api.csproj"

COPY DDD.Api DDD.Api
COPY DDD.Application DDD.Application
COPY DDD.Domain DDD.Domain
COPY DDD.Infrastructure DDD.Infrastructure
COPY Shared Shared

WORKDIR /src/DDD.Api
RUN dotnet build DDD.Api.csproj -c Debug -o /app/build

FROM base AS debug
WORKDIR /app
COPY --from=build /app/build .
# URL-e podasz z docker-compose (ASPNETCORE_URLS)
ENTRYPOINT ["dotnet", "DDD.Api.dll"]
