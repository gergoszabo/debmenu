# DebMenu - Napi menü Debrecenben

## Why

The question in the office always comes up in the morning before: who wants to eat in X or order something from somewhere.

With this little application, no need to try open many sites in a hurry, the daily offers are collected into a single place.

## Which restaurants are checked?

-   Forest étterem (https://forestetterem.hu)
-   Govinda étterem (https://www.govindadebrecen.hu)
-   Hüse vendéglő (http://www.husevendeglo.hu)
-   Manna étterem (https://www.mannaetterem.hu)
-   Viktória étterem (https://www.viktoriaetterem.hu)

## What it does?

-   Crawls local restaurant pages and extracts daily offers
-   Extracts menu information via an AI inference backend (Google Gemini or a local llama.cpp server) — from menu images (OCR) or directly from page text, depending on the restaurant
-   Generates a static HTML output with daily menus
-   Uploads results to AWS S3 bucket
-   Caches processed data for optimization

## Tech Stack

-   **Runtime**: .NET 11
-   **AI/ML**: Pluggable inference backend — Google Gemini API or a local llama.cpp-compatible server (menu extraction / OCR)
-   **Cloud**: AWS S3 for storage and static hosting

## Getting Started

### Prerequisites

-   .NET 11 SDK
-   AWS credentials (for S3 uploads)
-   An inference backend of your choice:
    - a Google Gemini API key (provider `gemini`), **or**
    - a local llama.cpp-compatible server URL + key (provider `llamacpp`)

### Configuration

Set up the following configuration settings via `dotnet user-secrets` (development) or environment variables (production):

| Configuration Key | Environment Variable | Description |
|---|---|---|
| `Inference:Provider` | `Inference__Provider` | Inference backend: `llamacpp` (default, set in `appsettings.json`) or `gemini`. Selects which provider is validated at startup |
| `Gemini:ApiKey` | `Gemini__ApiKey` | Google Gemini API key (Required when `Inference:Provider` = `gemini`) |
| `Gemini:Model` | `Gemini__Model` | Gemini model name (Optional, defaults to `gemini-3.1-flash-lite`) |
| `LlamaCpp:BaseUrl` | `LlamaCpp__BaseUrl` | llama.cpp-compatible server base URL (Required when `Inference:Provider` = `llamacpp`) |
| `LlamaCpp:ApiKey` | `LlamaCpp__ApiKey` | llama.cpp API key (Required when `Inference:Provider` = `llamacpp`) |
| `LlamaCpp:Model` | `LlamaCpp__Model` | Model served by the llama.cpp server (set in `appsettings.json`) |
| `AWS:SecretAccessKeyId` | `AWS__SecretAccessKeyId` | AWS access key ID (Required) |
| `AWS:SecretAccessKey` | `AWS__SecretAccessKey` | AWS secret access key (Required) |
| `AWS:Region` | `AWS__Region` | AWS region (Configured in `appsettings.json`, e.g. `eu-central-2`) |
| `AWS:Bucket` | `AWS__Bucket` | AWS S3 bucket name (Configured in `appsettings.json`, e.g. `debmenuaws`) |

#### Setting User Secrets locally

```bash
dotnet user-secrets init
# Select the backend, then set its key (Gemini example):
dotnet user-secrets set "Inference:Provider" "gemini"
dotnet user-secrets set "Gemini:ApiKey" "<YOUR_GEMINI_API_KEY>"
# ...or for a local llama.cpp server:
# dotnet user-secrets set "Inference:Provider" "llamacpp"
# dotnet user-secrets set "LlamaCpp:BaseUrl" "http://localhost:8080"
# dotnet user-secrets set "LlamaCpp:ApiKey" "<YOUR_LLAMACPP_API_KEY>"
dotnet user-secrets set "AWS:SecretAccessKeyId" "<YOUR_AWS_ACCESS_KEY_ID>"
dotnet user-secrets set "AWS:SecretAccessKey" "<YOUR_AWS_SECRET_ACCESS_KEY>"
```

### Usage

```bash
# Run the scraper and generate menus
dotnet run

# Build the project
dotnet build

# Publish for Linux x64
dotnet publish -o publish --os linux -a x64
```

## License

See [LICENSE](./LICENSE).
