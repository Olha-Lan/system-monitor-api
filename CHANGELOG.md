# 📝 CHANGELOG

## [2.0.0] - 2026-03-14

### 🎉 Великі нововведення:

#### ⚡ SignalR Real-Time
- **Додано:** `Hubs/MetricsHub.cs` - SignalR hub для real-time
- **Додано:** WebSocket endpoint `/hubs/metrics`
- **Оновлено:** `MetricsBackgroundService` тепер пушить дані через SignalR
- **Результат:** Frontend отримує дані автоматично без polling!

#### 🎮 Керування процесами
- **Додано:** `Controllers/ProcessActionsController.cs`
- **Endpoints:**
  - `POST /api/processactions/kill/{id}` - Вбити процес
  - `GET /api/processactions/{id}/details` - Детальна інформація
- **Безпека:** Захист від вбивства критичних системних процесів

#### 📤 Експорт даних
- **Додано:** `Controllers/ExportController.cs`
- **Endpoints:**
  - `GET /api/export/json?minutes=60` - JSON експорт
  - `GET /api/export/csv?minutes=60` - CSV експорт
  - `GET /api/export/statistics?minutes=60` - Статистика

#### 💯 System Health Score
- **Додано:** `Controllers/SystemController.cs`
- **Endpoint:** `GET /api/system/health-score`
- **Функції:**
  - Оцінка системи 0-100
  - Окремі оцінки для CPU/RAM/Disk
  - Статуси: Excellent, Good, Fair, Poor, Critical
  - Розумні рекомендації

### 🔄 Покращення:

#### BackgroundJobs/MetricsBackgroundService.cs
- Додано підтримку SignalR
- Автоматична відправка метрик через WebSocket
- Автоматична відправка алертів

#### Program.cs
- Додано `builder.Services.AddSignalR()`
- Додано `app.MapHub<MetricsHub>("/hubs/metrics")`
- Оновлено startup message (v2.0)

#### DTOs/ConfigDtos.cs
- Додано `HealthScoreDto`

### 📦 NuGet Пакети:

Жодних нових пакетів не треба! SignalR вже є в ASP.NET Core 8!

---

## [1.0.0] - 2026-03-07

### ✨ Початковий реліз:

- JWT Authentication
- CPU/RAM/Disk/Network/Process моніторинг
- Background Service (збір кожні 5 сек)
- In-memory історія (500 записів)
- Alerts система
- REST API endpoints
- Swagger документація
- SQL Server БД
