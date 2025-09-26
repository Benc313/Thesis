using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Services.RaceService.Interfaces;

namespace ThesisBackend.Controllers;

[Route("api/v1/races")]
[ApiController]
public class RaceController : ControllerBase
{
    private readonly IRaceService _raceService;
    private readonly IValidator<RaceRequest> _validator;

    public RaceController(IRaceService raceService, IValidator<RaceRequest> validator)
    {
        _raceService = raceService;
        _validator = validator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRace(int id)
    {
        var result = await _raceService.GetRaceAsync(id);
        return result.Success ? Ok(result.Race) : NotFound(new { message = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRaces()
    {
        var result = await _raceService.GetAllRacesAsync();
        return Ok(result.Races);
    }
    
    [HttpGet("getMeetsF")]
    public async Task<ActionResult<List<SmallEventResponse>>> GetAllRacesWithFilter([FromQuery] LocationQuery query)
    {
        var result = await _raceService.GetFilteredRacesAsync(query);
        return result.Success ? Ok(result.Races) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRace(RaceRequest request)
    {
        if (!int.TryParse(HttpContext.Request.Headers["User-Id"], out var userId))
        {
            return Unauthorized("Invalid or missing User-Id header.");
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var result = await _raceService.AddRaceAsync(request, userId);
        return result.Success ? Ok(result.Race) : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRace(int id, RaceRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var result = await _raceService.UpdateRaceAsync(request, id);
        return result.Success ? Ok(result.Race) : NotFound(new { message = result.ErrorMessage });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRace(int id)
    {
        var result = await _raceService.DeleteRaceAsync(id);
        return result.Success ? Ok() : NotFound(new { message = result.ErrorMessage });
    }

    [HttpPost("{raceId}/join/{userId}")]
    public async Task<IActionResult> JoinRace(int raceId, int userId)
    {
        var result = await _raceService.JoinRaceAsync(raceId, userId);
        return result.Success ? Ok() : BadRequest(new { message = result.ErrorMessage });
    }
}