using Microsoft.Extensions.Configuration;
using Moq;

namespace Santolibre.Map.Search.LibTest
{
    public static class ConfigurationHelper
    {
        public static void SetupKeyValuePair(this Mock<IConfiguration> configuration, string key, string value)
        {
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.Value).Returns(value);
            configuration.Setup(c => c.GetSection(key)).Returns(configurationSection.Object);
        }
    }
}
