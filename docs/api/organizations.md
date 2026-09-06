# API модуля Organizations

Модуль `Organizations` предоставляет HTTP API для управления организациями. На текущем этапе реализовано создание организации.

## Создать организацию

```http
POST /api/organizations/
Content-Type: application/json
```

### Запрос

```json
{
  "name": "  Acme Corporation  "
}
```

Правила для `name`:

- значение обязательно;
- `null`, пустая строка и строка только из пробелов недопустимы;
- внешние пробелы удаляются перед сохранением;
- длина нормализованного имени не должна превышать 200 символов.

### Успешный ответ

```http
HTTP/1.1 201 Created
Location: /api/organizations/{id}
Content-Type: application/json
```

```json
{
  "id": "019c2f7b-4a12-7c3d-8e4f-123456789abc",
  "name": "Acme Corporation"
}
```

Идентификатор создаётся приложением в формате UUIDv7. В ответе и базе данных возвращается нормализованное имя без внешних пробелов.

### Ошибка валидации

При некорректном имени API возвращает стандартный формат Problem Details с ошибками полей:

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json
```

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Organization name is required."
    ]
  }
}
```

Невалидный запрос не создаёт запись в PostgreSQL.

## Локальная проверка

Готовые успешный и неуспешный запросы находятся в `src/SupportFlow.Api/SupportFlow.Api.http` и могут быть выполнены из JetBrains Rider после запуска API.

HTTP-контракт и сохранение данных также проверяются интеграционными тестами `CreateOrganizationEndpointTests` на PostgreSQL из Testcontainers.
