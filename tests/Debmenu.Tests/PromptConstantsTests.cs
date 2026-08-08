using System.Text.Json;
using debmenu.Utils;

namespace Debmenu.Tests;

public class PromptConstantsTests
{
    [Fact]
    public void ExtractInstruction_ContainsResponseStructure()
        => Assert.Contains("Respond only with json.", PromptConstants.ExtractInstruction);

    [Fact]
    public void ExtractInstruction_ContainsDateGrounding()
        => Assert.Contains($"The current date is {DateTime.UtcNow:yyyy-MM-dd}.", PromptConstants.ExtractInstruction);

    [Fact]
    public void ExtractInstruction_ContainsYearGrounding()
        => Assert.Contains($"The current year is {DateTime.UtcNow.Year}.", PromptConstants.ExtractInstruction);

    [Fact]
    public void DateGrounding_UsesIsoDateFormat() =>
        Assert.Matches(@"The current date is \d{4}-\d{2}-\d{2}\.", PromptConstants.DateGrounding);

    [Fact]
    public void ResponseStructure_IsValidJsonDocument()
    {
        var json = PromptConstants.ResponseStructure;
        var start = json.IndexOf('{');
        var end = json.LastIndexOf('}') + 1;
        var embedded = json[start..end];

        using var doc = JsonDocument.Parse(embedded);
        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
    }
}