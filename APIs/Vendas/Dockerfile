# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj para otimizar restore
COPY APIs/Vendas/*.csproj ./APIs/Vendas/
COPY Shared/Shared.csproj ./Shared/

RUN dotnet restore /src/APIs/Vendas/Vendas.csproj

# Copia todo o código
COPY APIs/Vendas/. ./APIs/Vendas/
COPY Shared/. ./Shared/

# Publica em Release
RUN dotnet publish /src/APIs/Vendas/Vendas.csproj -c Release -o /app/publish

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5002

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Vendas.dll"]

