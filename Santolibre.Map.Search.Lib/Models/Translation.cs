namespace Santolibre.Map.Search.Lib.Models
{
    public class Translation
    {
        public string NormalizedTarget { get; set; }
        public string DisplayTarget { get; set; }
        public string OosTag { get; set; }
        public float Confidence { get; set; }
        public string PrefixWord { get; set; }
        public BackTranslation[] BackTranslations { get; set; }
    }
}
