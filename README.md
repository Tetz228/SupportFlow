# SupportFlow

SupportFlow — учебное B2B web-приложение для приёма и обработки обращений клиентов. Цель проекта — пройти полный цикл разработки production-like приложения на современном .NET-стеке.

## Цели

- построить модульный монолит с явными границами между модулями;
- изучить vertical slice architecture на реальных пользовательских сценариях;
- освоить аутентификацию, авторизацию и изоляцию данных организаций;
- работать с PostgreSQL и Entity Framework Core;
- писать unit-, integration- и end-to-end-тесты;
- настроить логирование, метрики, CI/CD и развёртывание.

## Архитектура

Приложение разрабатывается как модульный монолит. `SupportFlow.Api` является composition root и подключает модули в один процесс.

Текущие модули:

- `Identity` — пользователи и аутентификация;
- `Organizations` — организации, участники и роли;
- `Tickets` — обращения, их статусы и жизненный цикл.

Модули не ссылаются на API и друг на друга. Межмодульные контракты будут добавляться только при появлении реальной необходимости.

## Документация

- [Обзор документации](docs/README.md);
- [границы MVP](docs/product/mvp.md);
- [модули и правила зависимостей](docs/architecture/modules.md);
- [ADR-0001: использовать модульный монолит](docs/adr/0001-use-modular-monolith.md);
- [ADR-0002: разделить владение данными по модулям](docs/adr/0002-use-module-owned-dbcontexts.md).

## Технологии

- C# 14;
- .NET 10;
- ASP.NET Core 10;
- PostgreSQL 18;
- Entity Framework Core 10;
- Npgsql 10;
- Docker Compose;
- xUnit;
- `WebApplicationFactory` для интеграционных тестов;
- Central Package Management.

Планируется добавить OpenTelemetry и React с TypeScript.

## Структура репозитория

```text
SupportFlow/
├── src/
│   ├── SupportFlow.Api/
│   └── Modules/
│       ├── SupportFlow.Modules.Identity/
│       ├── SupportFlow.Modules.Organizations/
│       └── SupportFlow.Modules.Tickets/
├── tests/
│   └── SupportFlow.IntegrationTests/
├── docs/
├── .editorconfig
├── .env.example
├── compose.yaml
├── Directory.Build.props
├── Directory.Packages.props
├── dotnet-tools.json
├── global.json
└── SupportFlow.slnx
```

## Требования

- .NET SDK 10;
- Docker Desktop с Docker Compose;
- Git.

Нужная версия SDK задана в `global.json`.

Локальные .NET-инструменты, включая `dotnet-ef`, восстанавливаются командой:

```powershell
dotnet tool restore
```

## Локальный PostgreSQL

Создайте локальный файл с переменными окружения из шаблона:

```powershell
Copy-Item .env.example .env
```

Замените значение `POSTGRES_PASSWORD` в `.env` на собственный локальный пароль. Файл `.env` игнорируется Git и не должен попадать в репозиторий.

Запустите PostgreSQL:

```powershell
docker compose up -d
```

Проверьте состояние контейнера:

```powershell
docker compose ps
```

Остановить контейнер можно командой:

```powershell
docker compose down
```

Именованный Docker volume сохраняет данные между перезапусками и пересозданиями контейнера.

## Локальная строка подключения

API получает строку подключения из стандартной секции .NET Configuration `ConnectionStrings`. Для локальной разработки пароль хранится через User Secrets вне репозитория:

```powershell
dotnet user-secrets set "ConnectionStrings:SupportFlow" "Host=127.0.0.1;Port=5432;Database=supportflow;Username=supportflow;Password=<локальный-пароль>" --project .\src\SupportFlow.Api\SupportFlow.Api.csproj
```

Значения порта, базы, пользователя и пароля должны совпадать с локальным `.env`. Production-секреты не должны храниться через User Secrets.

## Запуск API

Из корня репозитория:

```powershell
dotnet run --project .\src\SupportFlow.Api\SupportFlow.Api.csproj --launch-profile http
```

После запуска health endpoint доступен по адресу:

```text
http://localhost:5185/health
```

## Сборка

```powershell
dotnet restore
dotnet build --no-restore
```

## Тесты

```powershell
dotnet test
```

Интеграционные тесты запускают API через in-memory `TestServer` и не требуют отдельного запущенного web-сервера.

Тестовый host получает собственную фиктивную строку подключения и не зависит от User Secrets разработчика. Пока health endpoint не проверяет базу данных, соединение с PostgreSQL во время теста не открывается.

## Текущее состояние

Реализован начальный каркас проекта:

- настроены общие правила сборки;
- версии NuGet-пакетов управляются централизованно;
- созданы API и три модуля;
- добавлен `/health`;
- добавлен интеграционный тест health endpoint;
- настроено локальное окружение PostgreSQL в Docker Compose;
- подключены EF Core и провайдер Npgsql;
- добавлен внутренний `OrganizationsDbContext` со схемой `organizations`;
- модуль `Organizations` зарегистрирован в API через публичную точку входа;
- версия `dotnet-ef` закреплена локальным tool manifest.

Бизнес-функциональность ещё не реализована.
