using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Santolibre.Map.Search.Lib.Models;
using Santolibre.Map.Search.Lib.Services;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IMapper _mapper;

        public SearchController(ISearchService searchService, IMapper mapper)
        {
            _searchService = searchService;
            _mapper = mapper;
        }

        [Route("search/{searchTerm}")]
        [HttpGet]
        public IActionResult Search(string searchTerm, double? latitude = null, double? longitude = null, double? searchRadius = 5)
        {
            var searchResult = _searchService.Search(searchTerm, latitude, longitude, searchRadius.Value);
            return Ok(_mapper.Map<SearchResult>(searchResult));
        }
    }
}
