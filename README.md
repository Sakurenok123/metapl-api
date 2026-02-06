# MetaPl API

ASP.NET Core 9.0 Web API для платформы MetaPlatforme.

## Технологии
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- PostgreSQL (Neon.tech)
- JWT аутентификация
- Swagger/OpenAPI

## Развертывание на Railway

1. Склонируйте репозиторий
2. Создайте проект на Railway.app
3. Подключите базу данных Neon.tech
4. Установите переменные окружения:
   - `DATABASE_URL`
   - `JWT_SECRET`
   - `JWT_ISSUER`
   - `JWT_AUDIENCE`
   - `ASPNETCORE_ENVIRONMENT=Production`

## Локальная разработка

```bash
dotnet restore
dotnet run