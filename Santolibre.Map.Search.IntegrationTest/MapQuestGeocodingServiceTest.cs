using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Santolibre.Map.Search.Geocoding.MapQuest;

namespace Santolibre.Map.Search.IntegrationTest
{
    [TestClass]
    public class MapQuestGeocodingServiceTest
    {
        private void Search(string locationQuery, bool isValid)
        {
            // Arrange
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:MapQuestSearchApiKey", "Fmjtd%7Cluur29u72q%2C7l%3Do5-90rwq0");

            var mapQuestSearchService = new MapQuestGeocodingService(configuration.Object);

            // Act
            var location = mapQuestSearchService.Search(locationQuery);

            // Assert
            if (isValid)
                Assert.IsNotNull(location);
            else
                Assert.IsNull(location);
        }

        [TestMethod]
        public void Search_Country_Location()
        {
            Search("Schweiz", true);
        }

        [TestMethod]
        public void Search_CityGerman_Location()
        {
            Search("Luzern, Schweiz", false);
        }

        [TestMethod]
        public void Search_CityEnglish_Location()
        {
            Search("Lucerne, Switzerland", true);
        }

        [TestMethod]
        public void Search_CityMixedLanguages_Location()
        {
            Search("Lucerne, Schweiz", true);
        }

        [TestMethod]
        public void Search_ZipCode_Location()
        {
            Search("6005, Schweiz", true);
        }

        [TestMethod]
        public void Search_Address_Location()
        {
            Search("Mythenstrasse 7, Luzern, Schweiz", true);
        }

        [TestMethod]
        public void Search_AddressInvalidHouseNumber_Location()
        {
            Search("Mythenstrasse 100, Luzern, Schweiz", true);
        }

        [TestMethod]
        public void Search_WrongAddressInvalidStreet_Location()
        {
            Search("Wallisergasse 20, Luzern, Schweiz", false);
        }

        [TestMethod]
        public void Search_WrongAddressInvalidCity_Location()
        {
            Search("Los Angeles, Schweiz", false);
        }
    }
}
