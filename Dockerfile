# Используем runtime образ для меньшего размера
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build образ
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MetaPlApi.csproj", "."]
RUN dotnet restore "MetaPlApi.csproj"
COPY . .
RUN dotnet build "MetaPlApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MetaPlApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MetaPlApi.dll"]
