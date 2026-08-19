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
- [ADR-0001: использовать модульный монолит](docs/adr/0001-use-modular-monolith.md).

## Технологии

- C# 14;
- .NET 10;
- ASP.NET Core 10;
- PostgreSQL 18;
- Docker Compose;
- xUnit;
- `WebApplicationFactory` для интеграционных тестов;
- Central Package Management.

Планируется добавить Entity Framework Core, OpenTelemetry и React с TypeScript.

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
├── .env.example
├── compose.yaml
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
└── SupportFlow.slnx
```

## Требования

- .NET SDK 10;
- Docker Desktop с Docker Compose;
- Git.

Нужная версия SDK задана в `global.json`.

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

## Текущее состояние

Реализован начальный каркас проекта:

- настроены общие правила сборки;
- версии NuGet-пакетов управляются централизованно;
- созданы API и три модуля;
- добавлен `/health`;
- добавлен интеграционный тест health endpoint;
- настроено локальное окружение PostgreSQL в Docker Compose.

Бизнес-функциональность ещё не реализована.
