---
applyTo: "**/src/**.mts"
---

## Web Scraper Implementation Guidelines

When creating or modifying web scrapers (e.g., `forest.mts`, `govinda.mts`), follow these patterns:

### Required Exports
```typescript
export const website = 'https://example-restaurant.com';

export async function getRestaurantOffers(): Promise<DailyOffers> {
    // Implementation
}
```

### Data Structure
Return data with dates as keys and menu items as arrays:
```typescript
{
    '2025-01-29': [
        'Chicken soup',
        'Grilled salmon with vegetables',
        'Rice pilaf'
    ],
    '2025-01-30': [
        'Beef stew',
        'Pasta primavera'
    ]
}
```

### Image Extraction Pattern
When dealing with menu images:
1. Fetch or screenshot the image
2. Pass to Google Gemini API for OCR/analysis
3. Extract structured menu data
4. Handle cases where images are unavailable

### Error Handling
- Return empty object `{}` for temporary issues (e.g., website down)
- Log all errors with context (restaurant name, URL, error details)
- Do not throw errors that would stop the entire scraping process

### Caching
- Use Google's caching mechanism (via hash-based file storage) to avoid re-processing identical images
- Store cache in the `CACHE_DIR` directory
