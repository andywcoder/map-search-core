using AutoMapper;

namespace Santolibre.Map.Search.WebService
{
    public static class AutoMapper
    {
        public static IMapper CreateMapper()
        {
            IMapper mapper = null;

            var config = new MapperConfiguration(x =>
            {
                x.AllowNullCollections = true;
                x.CreateMap<Lib.Models.SearchResult, Controllers.v1.Models.SearchResult>();
                x.CreateMap<Lib.Models.PointOfInterest, Controllers.v1.Models.PointOfInterest>();
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
            return mapper;
        }
    }
}
