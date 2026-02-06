# Используем образ с .NET 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем и восстанавливаем зависимости
COPY ["MetaPlApi.csproj", "."]
RUN dotnet restore "MetaPlApi.csproj"

# Копируем остальной код и собираем
COPY . .
RUN dotnet build "MetaPlApi.csproj" -c Release -o /app/build

# Публикуем приложение
RUN dotnet publish "MetaPlApi.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway автоматически устанавливает PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "MetaPlApi.dll"]
