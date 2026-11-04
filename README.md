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
-   Processes images using Google Gemini AI to extract menu information
-   Generates a static HTML output with daily menus
-   Uploads results to AWS S3 bucket
-   Caches processed data for optimization

## Tech Stack

-   **Runtime**: .NET 10
-   **AI/ML**: Google Gemini API for image processing and OCR
-   **Cloud**: AWS S3 for storage and static hosting

## Getting Started

### Prerequisites

-   .NET 10 SDK
-   AWS credentials (for S3 uploads)
-   Google Gemini API key

### Configuration

Set up the following configuration settings via `dotnet user-secrets` (development) or environment variables / Kubernetes secrets (production):

| Configuration Key | Environment Variable | Description |
|---|---|---|
| `Gemini:ApiKey` | `Gemini__ApiKey` | Google Gemini API key (Required) |
| `Gemini:Model` | `Gemini__Model` | Gemini model name (Optional, defaults to `gemini-3.1-flash-lite`) |
| `AWS:SecretAccessKeyId` | `AWS__SecretAccessKeyId` | AWS access key ID (Required) |
| `AWS:SecretAccessKey` | `AWS__SecretAccessKey` | AWS secret access key (Required) |
| `AWS:Region` | `AWS__Region` | AWS region (Configured in `appsettings.json`, e.g. `eu-central-2`) |
| `AWS:Bucket` | `AWS__Bucket` | AWS S3 bucket name (Configured in `appsettings.json`, e.g. `debmenuaws`) |

#### Setting User Secrets locally

```bash
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "<YOUR_GEMINI_API_KEY>"
dotnet user-secrets set "AWS:SecretAccessKeyId" "<YOUR_AWS_ACCESS_KEY_ID>"
dotnet user-secrets set "AWS:SecretAccessKey" "<YOUR_AWS_SECRET_ACCESS_KEY>"
```

Or update `K3S/debmenu-secrets.yaml` with proper values and apply via `kubectl apply -f K3S/debmenu-secrets.yaml`.

### Usage

```bash
# Run the scraper and generate menus
dotnet run

# Build the project
dotnet build

# Publish for Linux x64
dotnet publish -o publish --os linux -a x64

# Deploy to Kubernetes cluster (builds Docker image, prepares manifests, and applies to K3s)
dotnet msbuild -t:K3sDeploy
```

## License

See [LICENSE](./LICENSE).
