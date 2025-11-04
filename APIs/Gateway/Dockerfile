# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj para otimizar restore
COPY APIs/Gateway/*.csproj ./APIs/Gateway/
COPY Shared/Shared.csproj ./Shared/

RUN dotnet restore /src/APIs/Gateway/Gateway.csproj

# Copia todo o código
COPY APIs/Gateway/. ./APIs/Gateway/
COPY Shared/. ./Shared/

# Publica em Release
RUN dotnet publish /src/APIs/Gateway/Gateway.csproj -c Release -o /app/publish

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Gateway.dll"]
