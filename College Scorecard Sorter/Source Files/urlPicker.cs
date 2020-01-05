using System;

public static class urlPicker
{
    public static string Url = "";
    public static string BaseUrl = "https://api.data.gov/ed/collegescorecard/v1/schools.json";
    public static string Filters = "?latest.programs.cip_4_digit.code=1107";
    public static string Metadata = "&per_page=100";
    public static string PageCount = "&page=0";
    public static string WantedData = "&fields=school.name,latest.programs.cip_4_digit.earnings.median_earnings,latest.programs.cip_4_digit.code,latest.programs.cip_4_digit.title";
    public static string APIKey = "&api_key=[REDACTED]";

    public static int validInt()
    {
        int number = 0;
        string input = Console.ReadLine();
        while (!Int32.TryParse(input, out number))
        {
            Console.WriteLine("Input is invalid, please try again.");
            input = Console.ReadLine();
        }
        return number;
    }

    public static void getAPIKey()
    {
        Console.Write("Unfortunately, I can't give away my data.gov API Key. Please enter your API key:");
        APIKey = "&api_key=" + Console.ReadLine();
    }

    public static void UpdateFilter(string filters)
    {
        Filters = "?" + filters;
    }

    public static void UpdatePageCount(int page)
    {
        PageCount = "&page=" + page;
    }

    public static void UpdateUrl()
    {
        Url = BaseUrl + Filters + Metadata + PageCount + WantedData + APIKey;
    }
}
