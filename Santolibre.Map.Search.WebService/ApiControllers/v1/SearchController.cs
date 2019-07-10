using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Santolibre.Map.Search.Lib.Services;
using Santolibre.Map.Search.WebService.ApiControllers.v1.Models;

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
        public IActionResult Search(string searchTerm, double? latitude = null, double? longitude = null)
        {
            var searchResult = _searchService.GetSearchResult(searchTerm, latitude, longitude);
            return Ok(_mapper.Map<SearchResult>(searchResult));
        }
    }
}
