namespace Santolibre.Map.Search.Lib.Services
{
    public interface IMaintenanceService
    {
        void ImportPointsOfInterest(string filename);
        void RemoveOldPointsOfInterest(int days);
        void CompactPointsOfInterest();
        void AnalyzeIndexTerms();
    }
}
