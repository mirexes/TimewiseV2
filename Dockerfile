# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проектов для кэширования restore
COPY src/ServiceDesk.Core/ServiceDesk.Core.csproj src/ServiceDesk.Core/
COPY src/ServiceDesk.Infrastructure/ServiceDesk.Infrastructure.csproj src/ServiceDesk.Infrastructure/
COPY src/ServiceDesk.Application/ServiceDesk.Application.csproj src/ServiceDesk.Application/
COPY src/ServiceDesk.Web/ServiceDesk.Web.csproj src/ServiceDesk.Web/
RUN dotnet restore src/ServiceDesk.Web/ServiceDesk.Web.csproj

# Копируем весь исходный код и публикуем
COPY src/ src/
RUN dotnet publish src/ServiceDesk.Web/ServiceDesk.Web.csproj -c Release -o /app/publish --no-restore

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Создаём директорию для загрузок
RUN mkdir -p /app/wwwroot/uploads/avr /app/wwwroot/uploads/chat

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ServiceDesk.Web.dll"]
