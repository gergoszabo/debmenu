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

Set up the following environment variables:

`dotnet user-secrets init` - initialize secret store on pc
`dotnet user-secrets set "Gemini:ApiKey" "KEY"` - Google Gemini API key
`dotnet user-secrets set "AWS:SecretAccessKeyId" "KEY"`- AWS access key ID
`dotnet user-secrets set "AWS:SecretAccessKey" "KEY"` - AWS secret key

### Usage

```bash
# Run the scraper and generate menus
dotnet run

# Build for production
dotnet build

# Deploy to on-premises server
dotnet publish -o publish --os linux -a x64

# Clean cache and results
dotnet clean
```

## Licence

GOTO [LICENSE](./LICENSE)
