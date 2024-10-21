using Microsoft.AspNetCore.Mvc;
using StealTheCatsAPI.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StealTheCatsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly ICatService _catService;

        public CatsController(ICatService catService)
        {
            _catService = catService;
        }

        [HttpPost("fetch")]
        [SwaggerOperation(Summary = "Fetch 25 cat images from TheCatApi and save them to the " +
            "database and and downloads them to a file directory.",
            Description = "Fetches 25 random cat images from TheCatApi and stores them in " +
            "the local database and downloads them to a file directory.")]
        [SwaggerResponse(200, "Cats successfully fetched and saved.")]
        [SwaggerResponse(500, "An error occurred while fetching cats.")]
        public async Task<IActionResult> FetchAndSaveCats()
        {
            await _catService.FetchAndSaveCatsAsync();
            return Ok("25 cats fetched and saved successfully.");
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Retrieve a cat by its ID",
            Description = "Retrieves a specific cat's details by its ID.")]
        [SwaggerResponse(200, "Cat details retrieved successfully.")]
        [SwaggerResponse(400, "Invalid ID provided.")]
        [SwaggerResponse(404, "Cat not found.")]
        public async Task<IActionResult> GetCatById([FromRoute, SwaggerParameter(Description = "The ID of the cat to be retrieved")] int id)
        {
            if (id <= 0)
                return BadRequest("Id must be greater than zero.");

            var cat = await _catService.GetCatByIdAsync(id);
            if (cat == null)
                return NotFound();

            return Ok(cat);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve cats by tag with paging support",
            Description = "Retrieves a list of cats with a specific tag and pagination.")]
        [SwaggerResponse(200, "List of cats retrieved successfully.")]
        [SwaggerResponse(400, "Invalid request parameters.")]
        [SwaggerResponse(404, "No cats found with the specified tag.")]
        public async Task<IActionResult> GetCatsByTag(
            [FromQuery, SwaggerParameter(Description = "The page number for pagination, starting from 1")] int page,
            [FromQuery, SwaggerParameter(Description = "The number of items per page")] int pageSize,
            [FromQuery, SwaggerParameter(Description = "The tag to filter cats by")] string tag)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");
            if (!string.IsNullOrEmpty(tag))
            {
                var catsWithTag = await _catService.GetCatsByTagAsync(tag, page, pageSize);
                return Ok(catsWithTag);
            }

            var cats = await _catService.GetCatsAsync(page, pageSize);
            return Ok(cats);
        }
    }
}

