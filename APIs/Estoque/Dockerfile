# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia csproj para otimizar restore
COPY APIs/Estoque/*.csproj ./APIs/Estoque/
COPY Shared/Shared.csproj ./Shared/

RUN dotnet restore /src/APIs/Estoque/Estoque.csproj

# Copia todo o código
COPY APIs/Estoque/. ./APIs/Estoque/
COPY Shared/. ./Shared/

# Publica em Release
RUN dotnet publish /src/APIs/Estoque/Estoque.csproj -c Release -o /app/publish

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5001

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Estoque.dll"]
