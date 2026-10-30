# Practice.DataStructures

Практикум по структурам данных и алгоритмам на C#: собственные коллекции,
деревья, представления графов, обходы, остовы, пути и сортировки.

Требования и критерии приёмки: [спецификация](docs/specification.md).
Источник — [документ практикума](https://docs.google.com/document/d/1gc4Alrd9FbaK45gFsvVi9R8c1b2ZkauCpzki4IRLPGg/edit),
общие правила и раздел 4, включая порядок изучения.

## Структура решения

| Проект | Назначение |
| --- | --- |
| `src/Practice.DataStructures.Core` | Библиотека: реализации и публичные контракты, которые пишет автор практикума |
| `tests/Practice.DataStructures.Tests` | xUnit: контрактные тесты, инварианты, тестовые данные и лабораторные сценарии; ссылается на Core |

Репозиторий, solution и префикс пространств имён: `Practice.DataStructures`.
В решении ровно два проекта. Разделение заданий выполняется папками и
пространствами имён внутри них по мере реализации. Core пока пуст;
контракты структур ещё предстоит определить. В Tests есть одна проверка
загрузки Core как библиотеки для проверки настройки тестового стенда.

## Стек и зависимости

- .NET 10, C# 14; SDK `10.0.301` закреплён в `global.json`.
  Разрешены последующие патчи той же feature band (`latestPatch`), prerelease запрещены.
- [xunit.v3.mtp-off 4.0.2](https://www.nuget.org/packages/xunit.v3.mtp-off/4.0.2),
  [Microsoft.NET.Test.Sdk 18.11.0](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.11.0),
  [xunit.runner.visualstudio 4.0.1](https://www.nuget.org/packages/xunit.runner.visualstudio/4.0.1).
- Версии пакетов заданы в тестовом `.csproj`, транзитивные зависимости —
  в `packages.lock.json` каждого проекта.
- Включены nullable reference types, implicit usings и обработка предупреждений
  как ошибок. В `.editorconfig` настройки текстовых файлов общие, а правила
  C#-стиля и Rider inspections ограничены `tests/`. В Tests дополнительно
  включена проверка стиля при сборке;
  [кодовые соглашения](tests/CODE_CONVENTIONS.md) задают правила review.

Core не требует сторонних пакетов. Tests использует VSTest для `dotnet test`.
`xunit.v3.mtp-off` — официальный вариант xUnit v3 той же версии, отключающий
MTP для работы с VSTest на .NET 10; выбор вариантов описан в
[документации xUnit](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).
`OutputType=Exe` в Tests нужен инфраструктуре xUnit v3; приложение практикума
и ручной `Main` не создаются. Core имеет `OutputType=Library`.

## Сборка и проверка

Нужен .NET SDK из `global.json`. Команды выполняются в корне репозитория:

```powershell
dotnet restore Practice.DataStructures.sln --locked-mode
dotnet build Practice.DataStructures.sln --configuration Release --no-restore
dotnet format Practice.DataStructures.sln --verify-no-changes --no-restore
dotnet test Practice.DataStructures.sln --configuration Release --no-build --settings .runsettings
```

Для обычной разработки достаточно `dotnet test`: путь к `.runsettings`
задан в тестовом проекте. По умолчанию исключены `Category=Laboratory` и
`Category=Performance`. Правила тестов, явного запуска лабораторных сценариев
и настройки Rider: [тестовая лаборатория](docs/testing.md).

При осознанном изменении версий обновите тестовый `.csproj`, выполните
`dotnet restore Practice.DataStructures.sln --force-evaluate` и включите
изменённые lock-файлы в тот же коммит. Мажорные обновления выполняются отдельно.

## Git

Локальный репозиторий инициализирован с веткой `main`. Перед первым коммитом:

```powershell
git status --short
git add .
git diff --cached --check
git diff --cached --stat
git commit -m "chore: bootstrap data structures and algorithms practice"
```

Git использует ваши существующие настройки имени и email; при необходимости
задайте их локально через `git config user.name` и `git config user.email`.
