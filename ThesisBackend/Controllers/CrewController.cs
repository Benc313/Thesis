using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.CrewService.Interfaces;

namespace ThesisBackend.Controllers;

[Route("api/v1/crews")]
[ApiController]
public class CrewController : ControllerBase
{
    private readonly ICrewService _crewService;
    private readonly IValidator<CrewRequest> _validator;

    public CrewController(ICrewService crewService, IValidator<CrewRequest> validator)
    {
        _crewService = crewService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCrew(CrewRequest request)
    {
        // For simplicity, I'm taking the userId from a header. In a real app, this would come from the JWT token.
        if (!int.TryParse(HttpContext.Request.Headers["User-Id"], out var userId))
        {
            return Unauthorized("Invalid or missing User-Id header.");
        }

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var result = await _crewService.CreateCrewAsync(request, userId);
        return result.Success ? Ok(result.Crew) : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCrew(int id, CrewRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var result = await _crewService.UpdateCrewAsync(request, id);
        return result.Success ? Ok(result.Crew) : NotFound(new { message = result.ErrorMessage });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCrew(int id)
    {
        var result = await _crewService.GetCrewAsync(id);
        return result.Success ? Ok(result.Crew) : NotFound(new { message = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCrews()
    {
        var result = await _crewService.GetAllCrewsAsync();
        return Ok(result.Crews);
    }

    [HttpPost("{crewId}/users")]
    public async Task<IActionResult> AddUserToCrew([FromBody] UserCrewRequest request, int crewId)
    {
        var result = await _crewService.AddUserToCrewAsync(request, crewId);
        return result.Success ? Ok() : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpDelete("{crewId}/users/{userId}")]
    public async Task<IActionResult> RemoveUserFromCrew(int crewId, int userId)
    {
        var result = await _crewService.RemoveUserFromCrewAsync(crewId, userId);
        return result.Success ? Ok() : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCrew(int id)
    {
        var result = await _crewService.DeleteCrewAsync(id);
        return result.Success ? Ok() : NotFound(new { message = result.ErrorMessage });
    }
    
    [HttpGet("{crewId}/events")]
    public async Task<IActionResult> GetEventsForCrew(int crewId)
    {
        var result = await _crewService.GetEventsForCrewAsync(crewId);
        return result.Success ? Ok(result.Events) : NotFound(new { message = result.ErrorMessage });
    }
    
    [HttpPut("{crewId}/users/{userId}")]
    public async Task<IActionResult> UpdateUserRankInCrew(int crewId, int userId, Rank rank)
    {
        var result = await _crewService.UpdateUserRankAsync(crewId, userId, rank);
        return result.Success ? Ok() : BadRequest(new { message = result.ErrorMessage });
    }
}