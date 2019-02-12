using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.LibTest
{
    [TestClass]
    [DeploymentItem("TestData/Localization/country-names.en.json")]
    public class MapQuestSearchServiceLiveTest
    {
        private void Search(string locationQuery, bool isValid)
        {
            // Arrange
            var localizationService = new LocalizationService(@"TestData\Localization");
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("MapQuestSearchApiKey", "API-KEY");

            var mapQuestSearchService = new MapQuestSearchService(localizationService, configuration.Object);

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
            Search("Luzern, Schweiz", true);
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
