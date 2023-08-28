FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DDD.Api/DDD.Api.csproj", "DDD.Api/"]
COPY ["DDD.Application/DDD.Application.csproj", "DDD.Application/"]
COPY ["DDD.Domain/DDD.Domain.csproj", "DDD.Domain/"]
COPY ["DDD.Infrastructure/DDD.Infrastructure.csproj", "DDD.Infrastructure/"]
RUN dotnet restore "DDD.Api/DDD.Api.csproj"
COPY . .
WORKDIR "/src/DDD.Api"
RUN dotnet build "DDD.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DDD.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "DDD.Api.dll"]
