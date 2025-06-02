using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Agents.net.Attributes;
using Agents.net.Tools;

public class SearchToolPack : ToolPack
{
    private const string DuckDuckGoSearchUrl = "https://duckduckgo.com/html/?q={0}";


    [FunctionCall("Search the web using DuckDuckGo and return the top 5 result snippets, content only, no links.")]
    [FunctionCallParameter("query", "The search query string")]
    public async Task<string> SearchWebAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A consulta não pode ser nula ou vazia.", "query");

        string html = await FetchSearchHtmlAsync(query).ConfigureAwait(false);

        var result = "";
        if (!string.IsNullOrEmpty(html))
        {
            MatchCollection matches = Regex.Matches(
                html,
                "<a[^>]*class=\"[^\"]*result__a[^\"]*\"[^>]*>(.*?)</a>.*?<div[^>]*class=\"[^\"]*result__snippet[^\"]*\"[^>]*>(.*?)</div>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            int count = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string snippetHtml = matches[i].Groups[2].Value;
                string snippet = CleanText(snippetHtml);
                if (!string.IsNullOrEmpty(snippet))
                {
                    count++;
                    result += "[" + count + "] " + snippet + Environment.NewLine;
                    if (count >= 5)
                        break;
                }
            }

            if (count == 0)
            {
                matches = Regex.Matches(
                    html,
                    "<a[^>]*class=\"[^\"]*result__a[^\"]*\"[^>]*>(.*?)</a>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                for (int i = 0; i < matches.Count; i++)
                {
                    string textHtml = matches[i].Groups[1].Value;
                    string snippet = CleanText(textHtml);
                    if (!string.IsNullOrEmpty(snippet))
                    {
                        count++;
                        result += "[" + count + "] " + snippet + Environment.NewLine;
                        if (count >= 5)
                            break;
                    }
                }
            }
        }
        if (string.IsNullOrWhiteSpace(result))
            return "Nenhum resultado relevante encontrado para esta pesquisa.";
        Console.WriteLine("DuckDuckGo Search Results:");
        Console.WriteLine("========================================");
        Console.WriteLine("Search Query: " + query);
        Console.WriteLine("========================================");
        Console.WriteLine("Top 5 Results:");
        Console.WriteLine("Result: " + result);
        return result.Trim();
    }

    [FunctionCall("Get a DuckDuckGo Instant Answer for a query")]
    [FunctionCallParameter("query", "The query string for an instant answer")]
    public async Task<string> GetInstantAnswerAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A consulta não pode ser nula ou vazia.", "query");

        string html = await FetchSearchHtmlAsync(query).ConfigureAwait(false);

        if (string.IsNullOrEmpty(html))
            return string.Empty;

        string[] patterns = new string[]
        {
            "<div[^>]*class=\"[^\"]*zci__body[^\"]*\"[^>]*>(.*?)</div>",
            "<div[^>]*class=\"[^\"]*module__body[^\"]*\"[^>]*>(.*?)</div>",
            "<div[^>]*class=\"[^\"]*cw--c[^\"]*\"[^>]*>(.*?)</div>",
            "<div[^>]*class=\"[^\"]*answer__text[^\"]*\"[^>]*>(.*?)</div>",
            "<div[^>]*class=\"[^\"]*zci__content[^\"]*\"[^>]*>(.*?)</div>"
        };

        for (int i = 0; i < patterns.Length; i++)
        {
            Match match = Regex.Match(html, patterns[i], RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (match.Success)
            {
                string answer = CleanText(match.Groups[1].Value);
                if (!string.IsNullOrWhiteSpace(answer))
                    return answer;
            }
        }

        Match wikiMatch = Regex.Match(
            html,
            "<div[^>]*class=\"[^\"]*sidebar-modules[^\"]*\"[^>]*>.*?<div[^>]*class=\"[^\"]*module__body[^\"]*\"[^>]*>(.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (wikiMatch.Success)
        {
            string wikiText = CleanText(wikiMatch.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(wikiText))
                return wikiText;
        }

        return string.Empty;
    }

    private static async Task<string> FetchSearchHtmlAsync(string query)
    {
        string url = string.Format(DuckDuckGoSearchUrl, WebUtility.UrlEncode(query));
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }

    private static string CleanText(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;
        string text = Regex.Replace(html, "<.*?>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}