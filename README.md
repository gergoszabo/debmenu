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
-   Sends email summaries of extracted data
-   Caches processed data for optimization

## Tech Stack

-   **Runtime**: Node.js with TypeScript (`.mts` files)
-   **AI/ML**: Google Gemini API for image processing and OCR
-   **Cloud**: AWS S3 for storage and static hosting
-   **Email**: Nodemailer for email notifications
-   **Build**: esbuild for bundling

## Getting Started

### Prerequisites

-   Node.js (v18+)
-   AWS credentials (for S3 uploads)
-   Google Gemini API key
-   SMTP credentials (optional, for email features)

### Installation

```bash
npm install
```

### Configuration

Set up the following environment variables:

-   `GEMINI_API_KEY` - Google Gemini API key
-   `AWS_REGION` - AWS region for S3
-   `AWS_ACCESS_KEY_ID` - AWS access key
-   `AWS_SECRET_ACCESS_KEY` - AWS secret key
-   `SMTP_SERVER` - SMTP server address (optional)
-   `SMTP_PORT` - SMTP port (optional)
-   `SMTP_USERNAME` - SMTP username (optional)
-   `SMTP_PASSWORD` - SMTP password (optional)
-   `SMTP_RECIPIENT` - Email recipient address (optional)
-   `ONPREM_DOMAIN` - On-premises deployment domain (optional)
-   `ONPREM_USER` - On-premises deployment user (optional)

### Usage

```bash
# Run the scraper and generate menus
npm start

# Build for production
npm run build

# Deploy to on-premises server
npm run deploy

# Clean cache and results
npm run clean
```

## Project Structure

-   `src/` - Source TypeScript modules
    -   `index.mts` - Main entry point
    -   `generateTodayMenu.mts` - HTML generation logic
    -   `google.mts` - Google Gemini AI integration
    -   `aws.mts` - AWS S3 upload functionality
    -   `email.mts` - Email notification service
    -   `[restaurant].mts` - Individual restaurant crawlers (forest, govinda, huse, manna, viktora)
-   `esbuild.mts` - Build configuration
-   `deploy.mts` - Deployment script
-   `env.mts` - Environment and path configuration
-   `check-updated-packages.js` - Dependency update checker

## Licence

GOTO [LICENSE](./LICENSE)
