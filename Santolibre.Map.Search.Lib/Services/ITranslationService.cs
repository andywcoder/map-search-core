namespace Santolibre.Map.Search.Lib.Services
{
    public interface ITranslationService
    {
        string[] GetTranslation(string from, string to, string[] terms);
    }
}
