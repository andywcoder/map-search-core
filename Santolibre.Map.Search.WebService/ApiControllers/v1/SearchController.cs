using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class SearchController : ControllerBase
    {
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly IMapper _mapper;

        public SearchController(IPointOfInterestService pointOfInterestService, IMapper mapper)
        {
            _pointOfInterestService = pointOfInterestService;
            _mapper = mapper;
        }

        [Route("search/{searchTerm}")]
        [HttpGet]
        public IActionResult Search(string searchTerm, double? latitude = null, double? longitude = null)
        {
            var searchResult = _pointOfInterestService.GetSearchResult(searchTerm, latitude, longitude);
            return Ok(_mapper.Map<SearchResult>(searchResult));
        }
    }
}
