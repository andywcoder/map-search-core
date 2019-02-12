using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Santolibre.Map.Search.Lib.Services
{
    public class LocalizationService : ILocalizationService
    {
        private Dictionary<string, Dictionary<string, string>> _countryNames = new Dictionary<string, Dictionary<string, string>>();

        public LocalizationService(string localizationFolder)
        {
            _countryNames.Add("en", JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(localizationFolder, "country-names.en.json"))));
        }

        public string GetCountryName(string countryCode)
        {
            var cultureName = Thread.CurrentThread.CurrentUICulture.Name;
            var languageCode = cultureName.Split('-')[0];

            if (_countryNames.ContainsKey(languageCode))
            {
                var specificCountryNames = _countryNames[languageCode];
                if (countryCode != null && specificCountryNames.ContainsKey(countryCode))
                {
                    return specificCountryNames[countryCode];
                }
            }
            return countryCode;
        }
    }
}
