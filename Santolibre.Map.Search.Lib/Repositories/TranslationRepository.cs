using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Santolibre.Map.Search.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Santolibre.Map.Search.Lib.Repositories
{
    public class TranslationRepository : ITranslationRepository
    {
        private readonly IConfiguration _configuration;

        public TranslationRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TranslationResult[]> GetTranslationAsync(Language from, Language to, IEnumerable<string> terms)
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    var requestBody = JsonConvert.SerializeObject(terms.Select(x => new { Text = x }));
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri($"https://api.cognitive.microsofttranslator.com/dictionary/lookup?api-version=3.0&from={from.ToString().ToLower()}&to={to.ToString().ToLower()}");
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.GetValue<string>("AppSettings:AzureTranslatorSubscriptionKey"));

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var translationResults = JsonConvert.DeserializeObject<TranslationResult[]>(responseBody);
                    return translationResults;
                }
            }
        }
    }
}
