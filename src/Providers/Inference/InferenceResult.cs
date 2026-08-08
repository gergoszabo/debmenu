namespace debmenu.Providers.Inference;

public record InferenceResult(string? Text, int PromptTokenCount, int CandidatesTokenCount, int TotalTokenCount);
