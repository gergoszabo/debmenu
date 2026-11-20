using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

/// <summary>
/// Scraper for the Hüse vendéglő daily offer. Uses the image workflow with extra instructions
/// to fold the permanent "Állandó ajánlatunk" offering into every day and ground dates to the
/// current year (the menu image omits it).
/// </summary>
public class Huse(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://husevendeglo.hu/napi-ajanlat/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [
            "Make sure the extra offering 'Állandó ajánlatunk' gets added to every day"
        ],
        stateStore,
        resultCache)
{
    /// <summary>
    /// Year-grounding instruction, computed per scrape so a long-lived host never grounds to a
    /// stale year (the menu image omits the year entirely).
    /// </summary>
    private static string YearGroundingInstruction =>
        $"The menu image shows days like 'November 16.' without a year. This is the current week's menu: assign the current year ({DateTime.UtcNow.Year}) to every day so all dates fall within the current calendar week (today is {DateTime.UtcNow:yyyy-MM-dd}). Never output a year other than {DateTime.UtcNow.Year}.";

    /// <summary>
    /// Builds the full image extraction instruction list; the extra instructions are computed
    /// per call so date- and year-grounding stay fresh even if the host crosses midnight.
    /// </summary>
    protected override List<string> BuildImageExtractInstructions()
    {
        return [ExtractTask, .. ExtractInstructions, .. ExtraInstructions, YearGroundingInstruction];
    }
}