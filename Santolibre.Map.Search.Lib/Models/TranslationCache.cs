using System;
using System.Collections.Generic;
using System.Text;

namespace Santolibre.Map.Search.Lib.Models
{
    public class TranslationCache
    {
        public string Id { get; set; }
        public Dictionary<string, Translations> Terms { get; set; } = new Dictionary<string, Translations>();
    }
}
