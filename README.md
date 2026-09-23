# CrossApp

Рішення складається з двох проєктів:

- `src/Core` — бібліотека, яка збирає інформацію про середовище виконання.
- `src/Cli` — консольний застосунок, який отримує дані з Core і виводить їх.

Залежність між проєктами: `Cli → Core`.

## Структура

CrossApp/
- CrossApp.sln
- src/
  - Core/
    - Core.csproj
    - EnvironmentInfo.cs
  - Cli/
    - Cli.csproj
    - Program.cs

## Команди

Зібрати рішення:

```powershell
dotnet build CrossApp.sln