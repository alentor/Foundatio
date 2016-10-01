using System;
using System.Net.Http;
using Foundatio.Caching;
using Newtonsoft.Json;

namespace Foundatio00 {
    public class Program {
        private static readonly InMemoryCacheClient sr_cache = new InMemoryCacheClient {
                                                                   MaxItems = 3
                                                               };
        private static readonly HttpClient sr_httpClient = new HttpClient();

        public static void Main (string[] args) {
            while (true) {
                Console.WriteLine("Please enter a Date [yyyy-mm-dd]:");
                string dateStr = Console.ReadLine();
                DateTime date;
                if (DateTime.TryParse(dateStr, out date) == false) {
                    Console.WriteLine("Invalid Date.");
                    continue;
                }
                if (date.Date > DateTime.UtcNow.Date) {
                    Console.WriteLine("Cannot specify a date in the future.");
                    continue;
                }
                CurrencyExchange exchange = GetCurrencyRate(date);
                if (exchange.Error == null) {
                    decimal canadian;
                    exchange.Rates.TryGetValue("CAD", out canadian);
                    Console.WriteLine($"USD to CAD = {canadian}");
                }
                else {
                    Console.WriteLine($"Error: {exchange.Error}");
                }
            }
        }

        private static CurrencyExchange GetCurrencyRate (DateTime date) {
            string key = date.Date.ToString("yyyy-MM-dd");
            CacheValue <CurrencyExchange> cachedExchange = sr_cache.GetAsync <CurrencyExchange>(key).Result;
            Console.WriteLine(sr_cache.ExistsAsync(key).Result);
            if (cachedExchange.HasValue) {
                Console.WriteLine("Found in cache");
                return cachedExchange.Value;
            }
            Console.WriteLine("Fetching from service");
            HttpResponseMessage response = sr_httpClient.GetAsync($"http://api.fixer.io/{key}?base=USD").Result;
            string json = response.Content.ReadAsStringAsync().Result;
            CurrencyExchange exchange = JsonConvert.DeserializeObject <CurrencyExchange>(json);
            if (exchange.Error == null) {
                sr_cache.AddAsync(key, exchange).Wait();
            }
            return exchange;
        }
    }
}