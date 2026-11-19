# DebMenu - Copilot Instructions

DebMenu is a **.NET 11 console application** (C#) that crawls restaurant menus in Debrecen, Hungary, extracts daily offers using an AI inference backend, and publishes static HTML to AWS S3. This document guides Copilot when working in this repository. For the full C#/.NET coding standard, see `.pi/skills/dotnet-best-practices/SKILL.md`.

## Project Overview

- **Purpose**: Automated daily menu aggregation for multiple restaurants
- **Language / runtime**: C# on .NET 11 (`debmenu.csproj`, `TargetFramework` = `net11.0`)
- **Shape**: A single console app that runs once per scheduled invocation (cron / Kubernetes) and exits
- **Key services**: Pluggable AI inference (Google Gemini or a local llama.cpp server), AWS S3 for storage + static hosting

## Tech Stack & Dependencies

- C# with primary constructors throughout; `Microsoft.Extensions.Hosting` / `DependencyInjection` / `Http` / `Options.DataAnnotations` for DI and startup config validation
- `Serilog` for structured logging
- `Google.GenAI` (Gemini) and a llama.cpp-compatible HTTP server (local inference)
- `AWSSDK.S3` for uploads

## Architecture

```
Program.cs                     # Host builder, DI registration, provider selection, entry point
Orchestrator.cs / IOrchestrator.cs    # Coordinates the run: collect -> render HTML -> upload
DataCollector.cs / IDataCollector.cs  # Aggregates all restaurant offers + total inference cost
Restaurants/                   # One class per restaurant (Forest, Govinda, Huse, Manna, Viktoria)
  Restaurant.cs                # Abstract base: caching, image/text workflows, parsing, week-filtering
  IRestaurant.cs               # GetOffersAsync() -> Dictionary<string, List<string>>, TotalInferenceCost
Providers/Inference/           # IInferenceProvider + Gemini / LlamaCpp / CachedInferenceProvider + options
Providers/Infrastructure/      # IInfrastructureProvider (AWS S3 upload) + AWSOptions
Caching/                       # HTTP state store + per-restaurant result cache (file-backed)
Logging/TimedOperation.cs      # Scoped timing helper for structured logs
Utils/                         # Html template, PromptConstants, StringUtils
```

## Configuration & Validation

Configuration is bound to strongly-typed options classes and validated at startup via `.ValidateDataAnnotations().ValidateOnStart()` — a missing required value **fails fast** before the run.

- The active backend is chosen by `Inference:Provider` (`gemini` (code default) or `llamacpp`). Only the selected provider's options are bound and validated.
- `GeminiOptions`: `ApiKey`, `Model` (both `[Required]`)
- `LlamaCppOptions`: `BaseUrl`, `Model`, `ApiKey` (all `[Required]`) plus sampling parameters
- `AWSOptions`: `SecretAccessKeyId`, `SecretAccessKey`, `Region`, `Bucket` (all `[Required]`)
- Values come from `appsettings.json` (+ git-ignored `appsettings.development.json`), user-secrets, or environment variables.

## Code Standards (summary — see the skill for the full list)

- **Primary constructors** for DI; interfaces prefixed with `I`; one class per file named to match.
- **XML docs** on public classes/methods/properties (`<summary>`, `<param>`, `<returns>`).
- **async/await** for all I/O; return `Task` / `Task<T>`.
- **Specific exceptions** with descriptive messages (avoid bare `new Exception(...)`); the project already has domain exceptions (e.g. `MannaMenuCannotBeParsedException`).
- **Serilog** structured logging with context (`[{Class}] ...`, `GetType().Name`).
- **Tests**: MSTest + FluentAssertions, AAA pattern, success/failure + null-validation cases (see SKILL.md).

## Data Flow

1. Each `IRestaurant.GetOffersAsync()` fetches its page; if unchanged (ETag / LastModified) it returns cached offers.
2. Offers come from an **image workflow** (OCR via inference) or a **text workflow**.
3. Results are `Dictionary<string, List<string>>` keyed by date (`YYYY-MM-DD`), week-filtered to the current calendar week.
4. `DataCollector` aggregates all restaurants + total token cost; `Orchestrator` renders `index.html` and uploads it to S3.

## Conventions for Changes

- **New restaurant**: add `Restaurants/<Name>.cs` deriving from `Restaurant`, then register it in `Program.cs` (`AddTransient<IRestaurant, <Name>>()`).
- Keep scrapers, inference providers, caching, and orchestration as separate concerns.
- Preserve the ETag/LastModified + result-cache pattern so unchanged pages are not re-fetched or re-inferred.
