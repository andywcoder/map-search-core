using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Repositories;
using Santolibre.Map.Search.Lib.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Santolibre.Map.Search.IntegrationTest
{
    [TestClass]
    public class TranslationServiceTest
    {
        [TestMethod]
        public void GetTranslation_TermList_TranslatedTermList()
        {
            // Arrange
            var from = Language.EN;
            var to = Language.DE;
            var terms = new List<string> { "amenity", "table tennis", "bbq", "toilet", "bar", "playground" };
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:AzureTranslatorSubscriptionKey", "411e7531b1f040ed95d55b1825648653");
            var translationService = new TranslationService(new TranslationRepository(configuration.Object));

            // Act
            translationService.PopulateCache(new List<(Language, Language, string, string)>()
            {
                (from, to, "bar", "bar")
            });
            var translatedTerms = translationService.GetTranslation(from, to, terms);

            // Assert
            Assert.AreEqual(terms.Count, translatedTerms.Count);
        }
    }
}
