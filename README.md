# 🎉 System Resource Monitor API v2.0

**ОНОВЛЕНО З REAL-TIME ПІДТРИМКОЮ!** 🚀

---

## 🆕 ЩО НОВОГО В ВЕРСІЇ 2.0:

### ⚡ SignalR Real-Time Push
- Автоматичне оновлення даних на Frontend без polling
- WebSocket з'єднання для миттєвих updates
- Endpoint: `https://localhost:7083/hubs/metrics`

### 🎮 Керування процесами
- `POST /api/processactions/kill/{id}` - Вбити процес
- `GET /api/processactions/{id}/details` - Детальна інфо про процес

### 📤 Експорт даних
- `GET /api/export/json` - Експорт у JSON
- `GET /api/export/csv` - Експорт у CSV
- `GET /api/export/statistics` - Експорт статистики

### 💯 System Health Score
- `GET /api/system/health-score` - Оцінка системи 0-100
- Рекомендації по оптимізації
- Статуси: Excellent, Good, Fair, Poor, Critical

---

## 🚀 ШВИДКИЙ СТАРТ:

### 1. Відкрити проект
```
Подвійний клік на SystemResourceMonitorAPI.sln
```

### 2. Налаштувати Connection String
Відкрий `appsettings.json` і вставиш СВІЙ Connection String:
```json
"DefaultConnection": "Server=DESKTOP-74P5AB3\\SQLEXPRESS;Database=SystemMonitorDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### 3. Створити БД (якщо ще не створена)
```powershell
Add-Migration InitialCreate
Update-Database
```

### 4. Запустити
Натисни **F5**

✅ ГОТОВО! Swagger: https://localhost:7083/swagger

---

## 📡 НОВІ ENDPOINTS:

### SignalR Hub
```javascript
// Frontend підключення:
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7083/hubs/metrics')
  .build();

connection.on('ReceiveMetrics', (metrics) => {
  console.log('New metrics:', metrics);
  // Оновити UI
});

connection.on('ReceiveAlert', (alert) => {
  console.log('New alert:', alert);
  // Показати notification
});

await connection.start();
```

### Process Actions
```bash
# Вбити процес
POST /api/processactions/kill/1234

# Детальна інфо
GET /api/processactions/1234/details
```

### Export
```bash
# CSV експорт
GET /api/export/csv?minutes=60

# JSON експорт
GET /api/export/json?minutes=60
```

### Health Score
```bash
GET /api/system/health-score

Response:
{
  "overallScore": 85,
  "overallStatus": "Good",
  "cpuScore": 90,
  "ramScore": 80,
  "diskScore": 85,
  "recommendations": [
    "✅ System is running optimally"
  ]
}
```

---

## 🔧 ТЕХНІЧНІ ЗМІНИ:

- ✅ Додано `SignalR` для real-time
- ✅ Оновлено `MetricsBackgroundService` (push через SignalR)
- ✅ Новий `MetricsHub` для WebSocket
- ✅ Нові контролери: `ProcessActionsController`, `ExportController`, `SystemController`
- ✅ Нові DTOs: `HealthScoreDto`
- ✅ Покращені алерти з рекомендаціями

---

## ⚙️ КОНФІГУРАЦІЯ:

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "ТВІЙ_CONNECTION_STRING"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForSystemResourceMonitor2024!@#$%^&*()_+MustBe256BitsLong",
    "Issuer": "SystemResourceMonitorAPI",
    "Audience": "SystemResourceMonitorClient"
  }
}
```

---

## 🎯 ГОТОВНІСТЬ ДО FRONTEND:

Проект **ГОТОВИЙ** для підключення React Frontend!

CORS налаштований для:
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)
- `http://localhost:5174` (Vite alternative)

SignalR endpoint:
- `https://localhost:7083/hubs/metrics`

---

## 📦 ВЕРСІЯ:

**2.0.0** - Real-Time Ready! 🚀

Попередня версія: 1.0.0

---

**Готово до створення Frontend! 💪**
