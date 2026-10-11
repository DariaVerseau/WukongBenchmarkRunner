# Black Myth: Wukong Benchmark Automation Tool

Консольное приложение на C# (.NET 8) для автоматического запуска **Black Myth: Wukong Benchmark Tool** в двух режимах: **CPU-тест** и **GPU-тест**.

Инструмент применяет настройки, запускает бенчмарк, дожидается его завершения и автоматически парсит результаты из логов.

---

## 📋 Возможности

- Сбор характеристик ПК (CPU, GPU, RAM, OS) через WMI.
- Автоматическое применение настроек через `GameUserSettings.ini`.
- Запуск бенчмарка с аргументами командной строки.
- Ожидание завершения с таймаутом (защита от зависания).
- Парсинг логов и извлечение метрик (FPS, Score).
- Вывод итогового отчёта в консоль.

---

## ⚙️ Требования

- **Windows 10/11**
- **.NET 8 SDK** ([скачать](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Black Myth: Wukong Benchmark Tool** (бесплатно в Steam)
- Права на запись в `%LOCALAPPDATA%\b1\`

---

## 🚀 Установка и запуск

### 1. Клонирование репозитория

```bash
git clone https://github.com/your-username/WukongBenchmarkRunner.git
cd WukongBenchmarkRunner# WukongBenchmarkRunner
