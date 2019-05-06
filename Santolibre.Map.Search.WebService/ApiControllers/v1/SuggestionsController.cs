using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Santolibre.Map.Search.Lib.Services;
using Santolibre.Map.Search.WebService.ApiControllers.v1.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Search.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class SuggestionsController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IMapper _mapper;

        public SuggestionsController(ISearchService searchService, IMapper mapper)
        {
            _searchService = searchService;
            _mapper = mapper;
        }

        [Route("suggestions/{searchTerm}")]
        [HttpGet]
        public IActionResult Suggestions(string searchTerm, double? latitude = null, double? longitude = null, double? searchRadius = 5)
        {
            var suggestions = _searchService.GetSuggestions(searchTerm, latitude, longitude, searchRadius.Value);
            return Ok(_mapper.Map<List<Suggestion>>(suggestions));
        }
    }
}
