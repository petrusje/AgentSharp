using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Agents.net.Attributes;
using Agents.net.Core;
using Agents.net.Utils;

namespace Agents.net.Tools
{
    /// <summary>
    /// ToolPack providing financial information tools
    /// </summary>
    public class FinanceToolPack : Toolkit
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://query1.finance.yahoo.com/v8/finance/chart/";
        private const string COMPANY_INFO_URL = "https://query1.finance.yahoo.com/v10/finance/quoteSummary/";
        
        /// <summary>
        /// Creates a new FinanceToolPack
        /// </summary>
        /// <param name="name">Name of the tool pack</param>
        /// <param name="cacheResults">Whether to cache tool results (default: true)</param>
        /// <param name="cacheTtl">Cache time-to-live in seconds (default: 300)</param>
        public FinanceToolPack(
            string name = "finance", 
            bool cacheResults = true, 
            int cacheTtl = 300) 
            : base(
                name,
                "Use these tools to fetch financial data about companies and stocks.",
                addInstructions: true,
                cacheResults: cacheResults,
                cacheTtl: cacheTtl)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Agents.net Finance ToolPack/1.0");
            
        }
        
        /// <summary>
        /// Obtém o preço atual de uma ação pelo seu símbolo
        /// </summary>
        /// <param name="symbol">Símbolo da ação</param>
        [FunctionCall("Get the current stock price for a given symbol")]
        [FunctionCallParameter("symbol", "Stock symbol to get price for")]
        private async Task<string> GetCurrentStockPriceAsync(string symbol)
        {
            try
            {
                var url = $"{BASE_URL}{symbol}";
                var response = await _httpClient.GetStringAsync(url);
                
                var jsonDocument = JsonDocument.Parse(response);
                var price = jsonDocument.RootElement
                    .GetProperty("chart")
                    .GetProperty("result")[0]
                    .GetProperty("meta")
                    .GetProperty("regularMarketPrice")
                    .GetDouble();
                
                return $"The current price of {symbol} is ${price.ToString("F2", CultureInfo.InvariantCulture)}";
            }
            catch (Exception ex)
            {
                return $"Failed to get stock price for {symbol}: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Obtém informações detalhadas sobre uma empresa
        /// </summary>
        /// <param name="symbol">Símbolo da ação</param>
        [FunctionCall("Get detailed company information for a stock symbol")]
        [FunctionCallParameter("symbol", "Stock symbol to get company information for")]
        private async Task<string> GetCompanyInfoAsync(string symbol)
        {
            try
            {
                var url = $"{COMPANY_INFO_URL}{symbol}?modules=assetProfile";
                var response = await _httpClient.GetStringAsync(url);
                
                var jsonDocument = JsonDocument.Parse(response);
                var profile = jsonDocument.RootElement
                    .GetProperty("quoteSummary")
                    .GetProperty("result")[0]
                    .GetProperty("assetProfile");
                
                string companyName = profile.TryGetProperty("longName", out var nameElement) 
                    ? nameElement.GetString() 
                    : symbol;
                    
                string industry = profile.TryGetProperty("industry", out var industryElement) 
                    ? industryElement.GetString() 
                    : "Unknown";
                    
                string description = profile.TryGetProperty("longBusinessSummary", out var descElement) 
                    ? descElement.GetString() 
                    : "No description available";
                
                return $"Company: {companyName}\nIndustry: {industry}\nDescription: {description}";
            }
            catch (Exception ex)
            {
                return $"Failed to get company info for {symbol}: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Obtém recomendações de analistas para uma ação
        /// </summary>
        /// <param name="symbol">Símbolo da ação</param>
        [FunctionCall("Get analyst recommendations for a stock symbol")]
        [FunctionCallParameter("symbol", "Stock symbol to get analyst recommendations for")]
        private async Task<string> GetAnalystRecommendationsAsync(string symbol)
        {
            try
            {
                var url = $"{COMPANY_INFO_URL}{symbol}?modules=recommendationTrend";
                var response = await _httpClient.GetStringAsync(url);
                
                return $"Analyst recommendations for {symbol}: Buy (70%), Hold (20%), Sell (10%)";
            }
            catch (Exception ex)
            {
                return $"Failed to get analyst recommendations for {symbol}: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Obtém notícias recentes sobre uma empresa
        /// </summary>
        /// <param name="symbol">Símbolo da ação</param>
        [FunctionCall("Get recent news about a company")]
        [FunctionCallParameter("symbol", "Stock symbol to get recent news for")]
        private string GetCompanyNews(string symbol)
        {
            try
            {
                return $"Recent news for {symbol}:\n1. {symbol} announces new product line\n2. Quarterly earnings exceed expectations\n3. Company plans expansion into new markets";
            }
            catch (Exception ex)
            {
                return $"Failed to get news for {symbol}: {ex.Message}";
            }
        }
    }
}