# Этап 1: Сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем ТОЛЬКО csproj файл (или все csproj если их несколько)
COPY *.csproj ./

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем ВСЕ остальные файлы проекта
COPY . ./

# Публикуем приложение
RUN dotnet publish -c Release -o /app/out

# Этап 2: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Настройка порта (для .NET 8+)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BackendApp.dll"]