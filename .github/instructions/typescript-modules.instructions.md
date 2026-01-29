---
applyTo: "**/src/**/*.mts"
---

## TypeScript Module Guidelines

When working with TypeScript modules in the `src/` directory, follow these patterns:

### Module Structure
- Use ESM exports: `export async function`, `export const`
- Use `import type` for type-only imports to avoid circular dependencies
- Import from other modules using relative paths with `.mts` extension

### Async Operations
- Always use `async/await` syntax
- Add explicit return types to async functions (e.g., `Promise<MenuData>`)
- Handle errors with try-catch or error propagation

### Example Function Signature
```typescript
export async function getSomethingOffers(): Promise<DailyOffers> {
    try {
        // implementation
        return data;
    } catch (error) {
        console.error('Error getting offers:', error);
        throw error;
    }
}
```

### Logging
- Use `console.log()` for informational messages
- Use `console.error()` for errors
- Include context in log messages (e.g., restaurant name, operation being performed)

### Error Handling
- Don't silently fail - always log or throw errors
- Provide meaningful error messages
- Let errors propagate up to the main handler for central handling
