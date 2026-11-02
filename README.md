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
пространствами имён внутри них по мере реализации. В Core добавлен
`Collections/DynamicArray<T>`, `Collections/SinglyLinkedList<T>` и
`Collections/DoublyLinkedList<T>`. Tests проверяет их публичные контракты
и загрузку Core как библиотеки.

Порядок работы: академическая постановка и лабораторный полигон, самостоятельная
реализация автора, ревью корректности и надёжности, затем отдельное ревью
архитектуры, кода и API. Автор исправляет решение на каждом этапе. После обоих
этапов нейросеть оформляет XML-комментарии, тесты и полноценную лабораторную
демонстрацию. Полные правила: [тестовая лаборатория](docs/testing.md).

## Стек и зависимости

- Целевая платформа — `net10.0`. SDK выбирается из установленного окружения;
  `global.json` разрешает стабильные версии без закрепления номера.
  Версия C# выбирается SDK по целевой платформе.
- Тестовые зависимости: `xunit.v3.mtp-off`, `Microsoft.NET.Test.Sdk`,
  `xunit.runner.visualstudio`.
- Версии пакетов заданы в тестовом `.csproj`, транзитивные зависимости —
  в `packages.lock.json` каждого проекта.
- Включены nullable reference types, implicit usings и обработка предупреждений
  как ошибок. В `.editorconfig` настройки текстовых файлов общие, а правила
  C#-стиля и Rider inspections ограничены `tests/`. В Tests дополнительно
  включена проверка стиля при сборке;
  [кодовые соглашения](tests/CODE_CONVENTIONS.md) задают правила review.

Core не требует сторонних пакетов. Tests использует VSTest для `dotnet test`.
`xunit.v3.mtp-off` — официальный вариант xUnit v3, отключающий
MTP для работы с VSTest на .NET 10; выбор вариантов описан в
[документации xUnit](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform).
`OutputType=Exe` в Tests нужен инфраструктуре xUnit v3; приложение практикума
и ручной `Main` не создаются. Core имеет `OutputType=Library`.

Обновлениями установленных инструментов управляет пользователь. Нейросеть
читает фактические версии из локального `environment.local.json`; отсутствующие
сведения исследует на текущем устройстве и дописывает в кэш. Файл исключён из Git.
Порядок работы и сброса кэша: [окружение разработчика](docs/environment.md).

## Сборка и проверка

Нужен установленный стабильный .NET SDK с поддержкой целевой платформы проекта
и runtime для запуска тестов. Команды выполняются в корне репозитория;
путь к `dotnet` берётся из локального кэша окружения:

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
изменённые lock-файлы в тот же коммит. Обновление установленного SDK или IDE
само по себе не обновляет NuGet-зависимости проекта.

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
