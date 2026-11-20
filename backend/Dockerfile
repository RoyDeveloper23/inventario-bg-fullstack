FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InventarioBg.API/InventarioBg.API.csproj", "InventarioBg.API/"]
COPY ["InventarioBg.Application/InventarioBg.Application.csproj", "InventarioBg.Application/"]
COPY ["InventarioBg.Domain/InventarioBg.Domain.csproj", "InventarioBg.Domain/"]
COPY ["InventarioBg.Infrastructure/InventarioBg.Infrastructure.csproj", "InventarioBg.Infrastructure/"]
RUN dotnet restore "InventarioBg.API/InventarioBg.API.csproj"

COPY . .
WORKDIR "/src/InventarioBg.API"
RUN dotnet publish "InventarioBg.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventarioBg.API.dll"]