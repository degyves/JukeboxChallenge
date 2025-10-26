using Microsoft.AspNetCore.Mvc;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Mapping;
using PartyJukebox.Api.Models;
using PartyJukebox.Api.Services;

namespace PartyJukebox.Api.Controllers;

[ApiController]
[Route("api/rooms/{code}")]
public class QueueController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ITrackService _trackService;

    public QueueController(IRoomService roomService, ITrackService trackService)
    {
        _roomService = roomService;
        _trackService = trackService;
    }

    [HttpGet("queue")]
    public async Task<ActionResult<QueueResponse>> GetQueue([FromRoute] string code, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        var queue = await _trackService.GetOrderedQueueAsync(room, cancellationToken);
        return Ok(new QueueResponse(queue.Select(q => q.ToDto())));
    }

    [HttpPost("tracks")]
    public async Task<ActionResult<TrackDto>> AddTrack([FromRoute] string code, [FromBody] EnqueueTrackRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        try
        {
            var track = await _trackService.EnqueueAsync(room, request, cancellationToken);
            return Ok(track.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("tracks/{trackId}/vote")]
    public async Task<ActionResult<TrackDto>> Vote([FromRoute] string code, [FromRoute] string trackId, [FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        var track = await _trackService.GetByIdAsync(trackId, cancellationToken);
        if (track is null)
        {
            return NotFound();
        }

        try
        {
            track = await _trackService.RecordVoteAsync(room, track, request.Value, request.UserId, cancellationToken);
            return Ok(track.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("tracks/{trackId}")]
    public async Task<IActionResult> Remove([FromRoute] string code, [FromRoute] string trackId, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        if (!Request.Headers.TryGetValue("x-host-secret", out var hostSecret) ||
            !await _roomService.EnsureHostAsync(room, hostSecret.ToString()))
        {
            return Forbid();
        }

        var track = await _trackService.GetByIdAsync(trackId, cancellationToken);
        if (track is null)
        {
            return NotFound();
        }

        await _trackService.RemoveTrackAsync(room, track, cancellationToken);
        return NoContent();
    }

    [HttpPost("next")]
    public async Task<IActionResult> Next([FromRoute] string code, [FromBody] NextTrackRequest request, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        if (!await _roomService.EnsureHostAsync(room, request.HostSecret))
        {
            return Forbid();
        }

        var next = await _trackService.PromoteNextTrackAsync(room, cancellationToken);
        if (next is null)
        {
            await _roomService.UpdateNowPlayingAsync(room, null, cancellationToken);
            await _roomService.UpdatePlaybackStateAsync(room, PlaybackStatus.Idle, 0, cancellationToken);
            return Ok();
        }

        await _roomService.UpdateNowPlayingAsync(room, next.Id, cancellationToken);
        await _roomService.UpdatePlaybackStateAsync(room, PlaybackStatus.Buffering, 0, cancellationToken);

        return Ok(next.ToDto());
    }
}
