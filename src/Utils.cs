using System.Globalization;

public class Utils
{
    public static string[] DAYS_LOWER_HU = [
        "hétfö",
        "kedd",
        "szerda",
        "csütörtök",
        "péntek",
        "szombat",
        "vasárnap"
    ];

    public static List<string> MONTHS_LOWER_HU = [
        "január",
        "február",
        "március",
        "április",
        "május",
        "junius",
        "julius",
        "augusztus",
        "szeptember",
        "oktober",
        "november",
        "december"
     ];

    public static List<DateTime> GenerateHungarianFormattedDateRange(DateTime startDate, DateTime endDate)
    {
        List<DateTime> dates = [];
        for (int i = 0; i < 5; i++)
        {
            // dates.Add(startDate.AddDays(i).ToString("yyyy. MMMM dd.", CultureInfo.GetCultureInfo("hu")));
            dates.Add(startDate.AddDays(i));
        }
        return dates;
    }
}
