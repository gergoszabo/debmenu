namespace debmenu.Providers.Inference;

/// <summary>
/// The result of a single inference call: the model's text output plus token usage.
/// </summary>
/// <param name="Text">The model's textual response, or null when the provider produced no text.</param>
/// <param name="PromptTokenCount">Number of tokens consumed by the prompt.</param>
/// <param name="CandidatesTokenCount">Number of tokens generated in the response.</param>
/// <param name="TotalTokenCount">Total tokens (prompt + candidates).</param>
public record InferenceResult(string? Text, int PromptTokenCount, int CandidatesTokenCount, int TotalTokenCount);
