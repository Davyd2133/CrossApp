# CrossApp

Рішення містить два проєкти:

- `src/Core` — бібліотека, яка збирає інформацію про середовище виконання.
- `src/Cli` — консольний застосунок, який отримує інформацію з Core та показує її в консолі.

Напрямок залежності: `Cli → Core`.

## Структура проєкту

```text
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

У майбутньому в Core будуть каталоги:

- `Core/Dto/` — типи для передавання даних;
- `Core/Domain/` — доменні сутності;
- `Core/Storage/` — реалізації сховищ.

## Збірка та запуск

Зібрати рішення:

```powershell
dotnet build CrossApp.sln
```

Запустити консольний застосунок:

```powershell
dotnet run --project src/Cli
```

## Публікація для Windows

Self-contained — разом із .NET Runtime:

```powershell
dotnet publish src/Cli/Cli.csproj -c Release -r win-x64 --self-contained true -o publish/self-contained
```

Framework-dependent — без .NET Runtime:

```powershell
dotnet publish src/Cli/Cli.csproj -c Release -r win-x64 --self-contained false -o publish/framework-dependent
```

## Порівняння публікацій

| RID | Режим | Розмір каталогу, МіБ | Чи потрібен встановлений Runtime |
|---|---|---:|---|
| win-x64 | self-contained | ВПИШИ_РОЗМІР_1 | Ні |
| win-x64 | framework-dependent | ВПИШИ_РОЗМІР_2 | Так, .NET 8 |

Self-contained публікація містить .NET Runtime, тому її каталог більший.
Framework-dependent публікація використовує .NET Runtime, встановлений на комп’ютері.

## Multi-targeting

На комп’ютері встановлено лише .NET SDK 8.0.425. Тому опційний multi-targeting
для `net8.0` і `net9.0` не виконувався. Проєкти залишено з цільовим
фреймворком `net8.0`.