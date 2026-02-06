# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["MetaPlApi.csproj", "."]
RUN dotnet restore "MetaPlApi.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "MetaPlApi.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "MetaPlApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

# Прямой запуск без cd
ENTRYPOINT ["dotnet", "MetaPlApi.dll"]
