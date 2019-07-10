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
                x.CreateMap<Lib.Models.SearchResult, ApiControllers.v1.Models.SearchResult>();
                x.CreateMap<Lib.Models.Location, ApiControllers.v1.Models.Location>()
                    .Include<Lib.Models.PointOfInterest, ApiControllers.v1.Models.PointOfInterest>();
                x.CreateMap<Lib.Models.PointOfInterest, ApiControllers.v1.Models.PointOfInterest>();
                x.CreateMap<Lib.Models.Suggestion, ApiControllers.v1.Models.Suggestion>();
            });

            config.AssertConfigurationIsValid();

            mapper = config.CreateMapper();
            return mapper;
        }
    }
}
