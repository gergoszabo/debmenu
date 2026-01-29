# DebMenu - Copilot Instructions

DebMenu is a Node.js TypeScript application that crawls restaurant menus in Debrecen, Hungary, extracts daily offers using Google Gemini AI, and publishes them to AWS S3. This document provides guidance for Copilot when working on tasks in this repository.

## Project Overview

- **Purpose**: Automated daily menu aggregation for multiple restaurants
- **Language**: TypeScript (using `.mts` modules)
- **Runtime**: Node.js
- **Key Services**: Google Gemini API (AI/image processing), AWS S3 (storage), Nodemailer (email)
- **Version**: 2.0.0

## Code Standards

### Language & Style
- Use **TypeScript** exclusively for new features
- Write code using **ESM modules** (`.mts` files, not `.ts`)
- Follow idiomatic TypeScript patterns and best practices
- Use `async/await` for asynchronous operations, avoid callbacks

### File Organization
- **Source files**: Place all new TypeScript code in `src/` directory
- **Configuration**: Use `env.mts` for environment variables and paths
- **Module imports**: Use relative paths with full `.mts` extension (e.g., `./google.mts`)

### Code Quality
- Use strict TypeScript (`strict: true` in tsconfig)
- Add type annotations for function parameters and return types
- Prefer interfaces over type aliases for object shapes
- Use meaningful variable and function names that describe purpose

## Development Workflow

### Prerequisites
- Node.js v24 or higher
- npm for dependency management
- Google Gemini API key
- AWS credentials (for S3 integration)
- SMTP credentials (optional, for email features)

### Build & Test
```bash
npm install              # Install dependencies
npm start                # Run the scraper
npm run build            # Build for production (creates staging/debmenu.cjs)
npm run clean            # Clean cache and results
```

### Key npm Scripts
- `start`: Executes `node src/index.mts` - runs the main scraper and menu generation
- `build`: Runs esbuild to bundle the application
- `deploy`: Uses SCP to deploy to on-premises server
- `clean`: Removes cache, results, and staging directories

## Repository Structure

```
src/
├── index.mts              # Main entry point, orchestrates all scrapers
├── generateTodayMenu.mts  # Generates HTML output from extracted data
├── google.mts             # Google Gemini API integration for image/text processing
├── aws.mts                # AWS S3 upload and storage functionality
├── email.mts              # Email notification service using Nodemailer
├── forest.mts             # Forest étterem web scraper
├── govinda.mts            # Govinda étterem web scraper
├── huse.mts               # Hüse vendéglő web scraper
├── manna.mts              # Manna étterem web scraper
└── viktora.mts            # Viktória étterem web scraper
esbuild.mts               # Build configuration
deploy.mts                # Deployment script (uses SCP)
env.mts                   # Environment variables and path configuration
```

## Configuration & Environment Variables

The application requires these environment variables (set in `.env` or environment):

**Required:**
- `GEMINI_API_KEY` - Google Gemini API key for menu extraction

**AWS Configuration (required for S3 uploads):**
- `AWS_REGION` - AWS region (e.g., `eu-central-2`)
- `AWS_ACCESS_KEY_ID` - AWS access key
- `AWS_SECRET_ACCESS_KEY` - AWS secret key

**Email Configuration (optional):**
- `SMTP_SERVER` - SMTP server address
- `SMTP_PORT` - SMTP port number
- `SMTP_USERNAME` - SMTP authentication username
- `SMTP_PASSWORD` - SMTP authentication password
- `SMTP_RECIPIENT` - Email recipient address

**Deployment Configuration (optional):**
- `ONPREM_DOMAIN` - On-premises server domain
- `ONPREM_USER` - On-premises server username

## Key Implementation Patterns

### Web Scrapers
Each restaurant has a dedicated scraper module (e.g., `forest.mts`, `govinda.mts`). These modules should:
- Export a `getXxxOffers()` async function that returns daily menu data
- Export a `website` constant with the restaurant's URL
- Return data as a structured object with dates as keys and menu items as arrays
- Handle errors gracefully and log meaningful messages

### Data Processing Flow
1. Scrapers fetch raw data from restaurant websites
2. Google Gemini AI processes images and extracts structured menu information
3. Data is written to `result.json` with timestamps and restaurant details
4. `generateTodayMenu.mts` creates HTML representation of the data
5. HTML files are uploaded to AWS S3 bucket

### Type System
- Define types for menu data structures (see `MenuData` in `generateTodayMenu.mts`)
- Use consistent naming for offer objects (with `website` property for restaurant URLs)
- Ensure functions have explicit return types

## Best Practices for Contributions

### When Making Changes
1. Ensure all TypeScript compiles without errors
2. Follow the existing module structure and naming conventions
3. Use the established error handling patterns
4. Test with `npm start` before submitting changes
5. Update relevant parts of `.env.example` if adding new environment variables

### When Adding New Features
- If adding a new restaurant: create a new scraper module in `src/[restaurant].mts` following existing patterns
- If modifying data processing: ensure changes are compatible with the HTML generation logic
- If adding integrations: create a new module in `src/` and import it in `index.mts`

### When Fixing Bugs
- Identify the specific module causing the issue
- Ensure fixes maintain backward compatibility with the data format
- Test the complete pipeline with `npm start`

## Deployment

The application supports two deployment methods:

### AWS S3 (Primary)
- Results are published to an S3 bucket
- Output is available as static HTML files
- Configure AWS credentials in environment variables

### On-Premises Deployment
- Uses SCP to copy bundled application to server
- Run `npm run deploy` after building
- Requires `ONPREM_DOMAIN` and `ONPREM_USER` environment variables

## Helpful Links

- [Google Gemini API Documentation](https://ai.google.dev/)
- [AWS SDK for JavaScript](https://docs.aws.amazon.com/AWSJavaScriptSDK/latest/)
- [Nodemailer Documentation](https://nodemailer.com/)
- [esbuild Documentation](https://esbuild.github.io/)

## Notes for Copilot

- When adding dependencies, ensure they're compatible with Node.js v24+
- Respect the existing project structure - add new scrapers as separate modules
- When working with external APIs, handle rate limiting and errors gracefully
- The project uses timestamps and caching to optimize API calls - preserve these patterns
- Maintain the separation of concerns: scrapers, data processing, storage, and notifications are distinct modules
