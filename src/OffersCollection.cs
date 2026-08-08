namespace debmenu;

public record OffersCollection(
    Dictionary<string, Dictionary<string, List<string>>> Offers,
    int PromptTokenCount,
    int CandidatesTokenCount,
    int TotalTokenCount);
