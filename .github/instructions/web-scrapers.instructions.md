---
applyTo: "Restaurants/**/*.cs"
---

## Web Scraper (Restaurant) Implementation Guidelines

Each restaurant is a class in `Restaurants/` deriving from the abstract `Restaurant` base and registered as `IRestaurant` in `Program.cs`. Follow the existing patterns.

### Class shape

```csharp
public class Forest(string url, IHttpClientFactory httpClientFactory, IInferenceProvider inferenceProvider,
    ILogger logger, List<string> extraInstructions, IHttpResourceStateStore stateStore, IRestaurantResultCache resultCache)
    : Restaurant(url, httpClientFactory, inferenceProvider, logger, extraInstructions, stateStore, resultCache)
{
    // Override GetOffersAsync() to pick the workflow, and/or override individual steps.
}
```

### Return contract

`GetOffersAsync()` returns `Task<Dictionary<string, List<string>>>` — keys are dates in `YYYY-MM-DD`, values are the menu items for that day. The base class already week-filters to the current calendar week (`FilterOutdatedOffers`).

### Workflows

- **Image workflow** (default): fetch page → `GetImageLinkFromUrl()` → OCR via inference → parse. Override `GetImageLinkFromHtml` / `ExtractTask` when the image URL or extraction task is unusual.
- **Text workflow**: fetch page text → extract via inference → parse. Use it when the menu is plain HTML text rather than an image.
- Pick by overriding `GetOffersAsync()` to call `GetOffersWithCachingAsync(() => ImageWorkflow())` or `... TextWorkflow()`.

### Inference prompts

Build prompts from `PromptConstants` (`Utils/Prompts.cs`) plus per-restaurant prompt files (e.g. `ForestPrompts.cs`). Keep the shared response-structure and date-grounding instructions; add restaurant-specific instructions via the constructor's `extraInstructions`.

### Caching & cost

- Do not bypass `GetOffersWithCachingAsync` — it reuses cached offers when the page's ETag/LastModified is unchanged.
- Route inference through the injected `IInferenceProvider` (already wrapped by `CachedInferenceProvider`) and call `TrackInference(result)` for any inference result so total token cost is reported.

### Error handling & logging

- Log with Serilog using the `[Class] ...` + `GetType().Name` convention and `TimedOperation` for scoped timing.
- Throw specific exceptions for domain failures (see `MannaMenuCannotBeParsedException`, `MannaHetiMenuNotFoundException`); avoid bare `new Exception(...)`.
