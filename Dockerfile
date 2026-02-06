# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MetaPlApi.csproj", "."]
RUN dotnet restore "MetaPlApi.csproj"
COPY . .
RUN dotnet build "MetaPlApi.csproj" -c Release -o /app/build
RUN dotnet publish "MetaPlApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Railway будет устанавливать PORT через переменные окружения
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
ENTRYPOINT ["dotnet", "MetaPlApi.dll"]
