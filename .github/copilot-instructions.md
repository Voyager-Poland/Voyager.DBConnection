# Copilot Instructions for Voyager.DBConnection

## Overview
- Purpose: Small, provider-agnostic ADO.NET wrapper around `DbProviderFactory` for executing SQL with optional events/features and two calling styles: exceptions or Result pattern.
- Targets: net48, net6.0, net8.0 (see [Directory.Build.props](../Directory.Build.props)). Result pattern is preferred across the org (see [requirements/AI-INSTRUCTIONS.md](../requirements/AI-INSTRUCTIONS.md)).

## Architecture & Key Types
- Core: [Database](../src/Voyager.DBConnection/Database.cs) manages connections/transactions and builds commands; [Connection](../src/Voyager.DBConnection/Connection.cs) executes and throws mapped exceptions; [DbCommandExecutor](../src/Voyager.DBConnection/DbCommandExecutor.cs) executes and returns `Voyager.Common.Results.Result<T>`.
- Commands: Implement `ICommandFactory` or `IDbCommandFactory` to construct a configured `DbCommand` (see [Interfaces](../src/Voyager.DBConnection/Interfaces)). Use [DbCommandExtensions](../src/Voyager.DBConnection/Extensions/DbCommandExtensions.cs) for fluent parameters.
- Events/Features: Subscribe via `IRegisterEvents.AddEvent(Action<SqlCallEvent>)` and attach cross‑cutting features via `IFeature` + `AddFeature()` (see [EventHost](../src/Voyager.DBConnection/EventHost.cs), [FeatureHost](../src/Voyager.DBConnection/FeatureHost.cs), [Events](../src/Voyager.DBConnection/Events)).
- Transactions: `Database.BeginTransaction()` returns a disposable `Transaction` that enlists subsequent commands (see [Transaction.cs](../src/Voyager.DBConnection/Transaction.cs)).

## Usage Patterns (Prefer these)
- Result pattern (new code):
  - Build a command via factory → execute with [DbCommandExecutor](../src/Voyager.DBConnection/DbCommandExecutor.cs) → handle `Result<T>`.
  - Example shape: `executor.ExecuteReader(factory, consumer)` or `ExecuteAndBind(factory, cmd => Result.From(...))`.
- Exception style (legacy interop):
  - Use [Connection](../src/Voyager.DBConnection/Connection.cs): `ExecuteNonQuery`, `ExecuteScalar`, `GetReader`. Exceptions are transformed by `IExceptionPolicy`.
- Parameters: Always use fluent extensions: `.WithInputParameter(...)`, `.WithOutputParameter(...)`, `.GetParameterValue<T>(...)`. The old `Database.AddInParameter/AddOutParameter` APIs are `[Obsolete]` and scheduled for removal in 5.0 (see [Database.cs](../src/Voyager.DBConnection/Database.cs#L41-L151)).
- Provider specifics: Parameter name prefix is inferred (`@` for SQL Server/Postgres/MySQL/SQLite, `:` for Oracle) by [DbCommandExtensions](../src/Voyager.DBConnection/Extensions/DbCommandExtensions.cs#L55-L82). Pass names without a prefix; the extension will add it.

## Critical Conventions
- Prefer `DbCommandExecutor` + `Result<T>` for new APIs; convert unexpected exceptions to `Error` via `IMapErrorPolicy` (see [DefaultMapError](../src/Voyager.DBConnection/DefaultMapError.cs)).
- Do not return nulls in public APIs. Use `Result<T>` and empty collections per org rules (see [requirements/AI-INSTRUCTIONS.md](../requirements/AI-INSTRUCTIONS.md)).
- Public surface should include XML docs; build treats many analyzer warnings as errors (see [build/Build.CodeQuality.props](../build/Build.CodeQuality.props)).
- Keep `IDisposable` lifetimes correct: dispose `Connection`/`DbCommandExecutor` and `Transaction`.

## Build, Test, CI
- Local:
  - Restore/build: `dotnet restore` → `dotnet build -c Release`.
  - Run tests: `dotnet test -c Release --framework net6.0` and `--framework net8.0` (Linux runners don’t build `net48`).
- CI: See [.github/workflows/ci.yml](./workflows/ci.yml).
  - Builds Release, runs tests for net6/net8, packs library, and on pushes to `main` or tags `v*` publishes to GitHub Packages and (for public repos) NuGet.org.
- Versioning: MinVer derives versions from tags with `v` prefix (see [build/Build.Versioning.props](../build/Build.Versioning.props)).

## Integration Packages
- Logging: `Voyager.DBConnection.Logging` provides a feature that can be added via `connection.AddFeature(...)` (see usage sketch in [README.md](../README.md)).
- SQL Server: `Voyager.DBConnection.MsSql` offers a concrete `SqlConnection : Connection` for convenience (see `MsSql` section in [README.md](../README.md)).

## Testing Patterns
- Tests use NUnit + Moq; see examples in [ConnectionTest.cs](../src/Voyager.DBConnection.Test/ConnectionTest.cs) and [DataBaseTest.cs](../src/Voyager.DBConnection.Test/DataBaseTest.cs).
- Test doubles for ADO.NET live in [TestMocks.cs](../src/Voyager.DBConnection.Test/TestMocks.cs) to avoid real DBs.

## Useful References
- Root docs: [README.md](../README.md)
- APIs: [Interfaces](../src/Voyager.DBConnection/Interfaces), [Events](../src/Voyager.DBConnection/Events), [Extensions](../src/Voyager.DBConnection/Extensions)
- Publish scripts (manual/Windows): [scripts/PublishNuget.ps1](../scripts/PublishNuget.ps1)

If anything is unclear or you need more examples (e.g., a minimal `ICommandFactory` + `IGetConsumer` pair), ask and I’ll add them here.
