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
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly IMapper _mapper;

        public SuggestionsController(IPointOfInterestService pointOfInterestService, IMapper mapper)
        {
            _pointOfInterestService = pointOfInterestService;
            _mapper = mapper;
        }

        [Route("suggestions/{searchTerm}")]
        [HttpGet]
        public IActionResult Suggestions(string searchTerm, double latitude, double longitude)
        {
            var suggestions = _pointOfInterestService.GetSuggestions(searchTerm, latitude, longitude);
            return Ok(_mapper.Map<List<Suggestion>>(suggestions));
        }
    }
}
