using Santolibre.Map.Search.Lib.Models;

namespace Santolibre.Map.Search.Lib.Services
{
    public interface IMaintenanceService
    {
        void ImportPointsOfInterest(string filename, Language language);
        void RemoveOldPointsOfInterest(int days);
        void CompactPointsOfInterest();
        void AnalyzeIndexTerms();
        void UpdateTranslationCache(Language from, Language to, bool selectInconclusive);
        void PopulateTranslationCache(Language language);
    }
}
