using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThesisBackend.Data;
using ThesisBackend.Domain.Messages;
using ThesisBackend.Domain.Models;
using ThesisBackend.Services.MeetService.Interfaces;

namespace ThesisBackend.Controllers;

[Route("api/v1/Meet")]
[ApiController]
public class MeetController : ControllerBase
{
	private readonly IMeetService _meetService;
	private readonly IValidator<MeetRequest> _meetRequestValidator;
	private readonly ILogger<MeetController> _logger;

	public MeetController(IMeetService meetService, IValidator<MeetRequest> meetRequestValidator, ILogger<MeetController> logger)
	{
		_meetService = meetService;
		_meetRequestValidator = meetRequestValidator;
		_logger = logger;
	}

	
	[HttpPost("addMeet/{userId}")]
    public async Task<ActionResult<MeetResponse>> AddMeet(MeetRequest meetRequest, int userId)
    {
        _logger.LogInformation("AddMeet request for user {UserId}", userId);
        var validationResult = await _meetRequestValidator.ValidateAsync(meetRequest);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var result = await _meetService.AddMeetAsync(meetRequest, userId);
        return result.Success ? Ok(result.MeetResponse) : Problem(result.ErrorMessage);
    }

    [HttpPut("joinMeet/{meetId}/{userId}")]
    public async Task<IActionResult> JoinMeet(int meetId, int userId)
    {
        _logger.LogInformation("JoinMeet request for user {UserId} and meet {MeetId}", userId, meetId);
        var result = await _meetService.JoinMeetAsync(meetId, userId);
        return result.Success ? Ok() : Problem(result.ErrorMessage);
    }

    [HttpPut("updateMeet/{meetId}")]
    public async Task<ActionResult<MeetResponse>> UpdateMeet(MeetRequest meetRequest, int meetId)
    {
        _logger.LogInformation("UpdateMeet request for meet {MeetId}", meetId);
        var validationResult = await _meetRequestValidator.ValidateAsync(meetRequest);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var result = await _meetService.UpdateMeetAsync(meetRequest, meetId);
        return result.Success ? Ok(result.MeetResponse) : Problem(result.ErrorMessage);
    }

    [HttpGet("getMeet/{meetId}")]
    public async Task<ActionResult<MeetResponse>> GetMeet(int meetId)
    {
        _logger.LogInformation("GetMeet request for meet {MeetId}", meetId);
        var result = await _meetService.GetMeetAsync(meetId);
        return result.Success ? Ok(result.MeetResponse) : NotFound(result.ErrorMessage);
    }

    [HttpDelete("deleteMeet/{meetId}")]
    public async Task<IActionResult> DeleteMeet(int meetId)
    {
        _logger.LogInformation("DeleteMeet request for meet {MeetId}", meetId);
        var result = await _meetService.DeleteMeetAsync(meetId);
        return result.Success ? Ok() : NotFound(result.ErrorMessage);
    }

    [HttpGet("getMeets")]
    public async Task<ActionResult<List<SmallEventResponse>>> GetAllMeets()
    {
        _logger.LogInformation("GetAllMeets request");
        var result = await _meetService.GetAllMeetsAsync();
        return result.Success ? Ok(result.Meets) : NotFound(result.ErrorMessage);
    }

    [HttpGet("getMeetsF")]
    public async Task<ActionResult<List<SmallEventResponse>>> GetAllMeetsWithFilter([FromQuery] LocationQuery query)
    {
        _logger.LogInformation("GetAllMeetsWithFilter request with query: {@Query}", query);
        var result = await _meetService.GetFilteredMeetsAsync(query);
        return result.Success ? Ok(result.Meets) : NotFound(result.ErrorMessage);
    }
}