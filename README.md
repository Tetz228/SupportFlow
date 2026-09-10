# SupportFlow

[![CI](https://github.com/Tetz228/SupportFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/Tetz228/SupportFlow/actions/workflows/ci.yml)

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
- [план разработки и обучения](docs/product/roadmap.md);
- [API модуля Organizations](docs/api/organizations.md);
- [формат ошибок API](docs/api/problem-details.md);
- [модули и правила зависимостей](docs/architecture/modules.md);
- [ADR-0001: использовать модульный монолит](docs/adr/0001-use-modular-monolith.md);
- [ADR-0002: разделить владение данными по модулям](docs/adr/0002-use-module-owned-dbcontexts.md);
- [ADR-0003: разрешить интеграционным тестам доступ к внутреннему контексту модуля](docs/adr/0003-allow-test-access-to-module-persistence.md).

## Технологии

- C# 14;
- .NET 10;
- ASP.NET Core 10;
- PostgreSQL 18;
- Entity Framework Core 10;
- Npgsql 10;
- Docker Compose;
- GitHub Actions;
- xUnit;
- FluentValidation;
- встроенная генерация OpenAPI;
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
│   ├── SupportFlow.IntegrationTests/
│   └── SupportFlow.Modules.Organizations.UnitTests/
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

## Миграции базы данных

Миграции принадлежат конкретным модулям. Для применения миграций модуля `Organizations` выполните из корня репозитория:

```powershell
dotnet ef database update --project .\src\Modules\SupportFlow.Modules.Organizations --startup-project .\src\SupportFlow.Api --context OrganizationsDbContext
```

Команда создаёт схему `organizations`, таблицы модуля и отдельную таблицу истории `organizations.__ef_migrations_history`.

При первом применении Npgsql может записать в лог неудачный `SELECT` из ещё не созданной таблицы истории. Если выполнение продолжается и завершается сообщением `Done`, это ожидаемая первоначальная проверка, а не ошибка миграции.

## Запуск API

Из корня репозитория:

```powershell
dotnet run --project .\src\SupportFlow.Api\SupportFlow.Api.csproj --launch-profile http
```

После запуска health endpoint доступен по адресу:

```text
http://localhost:5185/health
```

### OpenAPI

В окружении `Development` машиночитаемый контракт API доступен по адресу:

```text
http://localhost:5185/openapi/v1.json
```

Документ формируется встроенным генератором ASP.NET Core из маршрутов, типов запросов и ответов, а также metadata endpoint’ов. В `Production` этот endpoint не регистрируется. Готовый запрос для просмотра документа добавлен в `src/SupportFlow.Api/SupportFlow.Api.http`.

### Problem Details

API использует Problem Details как общий машиночитаемый формат ошибок. Если routing или endpoint возвращает ошибочный HTTP-статус без тела, Status Code Pages формирует ответ `application/problem+json` через стандартный `IProblemDetailsService`. Exception Handler Middleware преобразует необработанные исключения в безопасный ответ `500` того же формата без stack trace и внутренних сведений.

Например, неизвестный маршрут возвращает `404` с полями `type`, `title`, `status` и `traceId`. Validation Problem Details сохраняет дополнительный словарь ошибок по полям запроса. `traceId` считается непрозрачным идентификатором и предназначен для последующей корреляции ошибки с серверной диагностикой. Подробный контракт и границы применения описаны в `docs/api/problem-details.md`.

## Сборка

```powershell
dotnet restore
dotnet build --no-restore
```

## Тесты

Быстрые unit-тесты модуля Organizations запускаются без Docker:

```powershell
dotnet test .\tests\SupportFlow.Modules.Organizations.UnitTests\SupportFlow.Modules.Organizations.UnitTests.csproj
```

Интеграционные тесты запускают PostgreSQL через Testcontainers, поэтому перед их выполнением требуется Docker Desktop в режиме Linux-контейнеров:

```powershell
dotnet test .\tests\SupportFlow.IntegrationTests\SupportFlow.IntegrationTests.csproj
```

Полный набор тестов можно по-прежнему запустить одной командой:

```powershell
dotnet test
```

В CI solution сначала собирается целиком, после чего unit- и integration-тесты выполняются отдельными шагами с параметрами `--no-build --no-restore`. Это делает причину сбоя видимой и не повторяет уже выполненные восстановление пакетов и компиляцию.

Тест `/health` запускает API через in-memory `TestServer`, получает собственную фиктивную строку подключения и не зависит от User Secrets разработчика. Этот тест не открывает соединение с PostgreSQL и не требует отдельно запущенного web-сервера.

PostgreSQL-зависимые тесты входят в общую xUnit-коллекцию и используют один контейнер PostgreSQL 18 с базой `supportflow_tests`. `PostgreSqlFixture` запускает контейнер, применяет настоящие миграции и создаёт общий `WebApplicationFactory` один раз на коллекцию. Контейнер освобождается после завершения всех тестов коллекции. При первом запуске может потребоваться загрузка Docker-образов. Локальная база из Docker Compose и её данные для этих тестов не используются.

Перед каждым тестом `CreateOrganizationEndpointTests` библиотека Respawn очищает прикладные таблицы схемы `organizations`, но сохраняет историю миграций EF Core. Это исключает зависимость тестов от порядка выполнения без повторного создания контейнера и схемы. Успешный тест подтверждает `201 Created`, тело ответа, `Location`, нормализацию имени и ровно одну сохранённую запись. Негативные тесты подтверждают Problem Details с кодом `400` и пустую таблицу организаций.

`PostgreSqlFixtureTests` проверяет подключение к тестовой базе, применение миграции и работу очистки: прикладные данные удаляются, а история миграций сохраняется.

`OpenApiEndpointTests` проверяет значимые части контракта создания организации в сгенерированном OpenAPI-документе и подтверждает, что endpoint документации недоступен в окружении `Production`. Эти тесты используют тестовый host ASP.NET Core, но не обращаются к PostgreSQL.

`ProblemDetailsEndpointTests` проверяет, что неизвестный маршрут возвращает `404`, а необработанное исключение — безопасный `500` в формате `application/problem+json`. Серверный сбой создаётся тестовым EF Core interceptor при сохранении организации: тест проходит через настоящий endpoint и middleware pipeline, но не обращается к PostgreSQL и не добавляет искусственный диагностический маршрут в production API. Общая проверка Problem Details подтверждает наличие непустого строкового `traceId` в ответах `400`, `404` и `500`.

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
- версия `dotnet-ef` закреплена локальным tool manifest;
- добавлена доменная модель организации с валидацией имени и UUIDv7;
- добавлены модульные тесты доменных правил организации;
- настроен EF Core-маппинг и добавлена первая миграция модуля `Organizations`;
- добавлен `POST /api/organizations` с сохранением организации в PostgreSQL;
- добавлена валидация запроса создания организации с ответом `400 Bad Request`;
- добавлены unit-тесты валидатора, включая граничную длину и внешние пробелы;
- добавлена общая collection fixture PostgreSQL на Testcontainers и интеграционная проверка подключения к тестовой базе;
- настроены подмена тестовой строки подключения и автоматическое применение миграций;
- настроена очистка прикладных данных через Respawn перед каждым HTTP-тестом;
- добавлены интеграционные тесты успешного и неуспешного создания организации;
- подключена встроенная генерация OpenAPI для окружения разработки;
- добавлены contract-тесты OpenAPI-документа и его недоступности в production-окружении;
- настроен общий формат Problem Details для HTTP-ошибок без тела;
- добавлена глобальная обработка неожиданных исключений без раскрытия внутренних деталей;
- зафиксировано наличие `traceId` в ошибочных HTTP-ответах;
- добавлена автоматическая проверка сборки и тестов в GitHub Actions.

Этап реалистичных интеграционных тестов завершён. Сейчас развивается общий HTTP-контур API: подключены OpenAPI, единый формат Problem Details и безопасная обработка неожиданных исключений.
