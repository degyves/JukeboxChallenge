using Microsoft.AspNetCore.Mvc;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Models;
using PartyJukebox.Api.Services;

namespace PartyJukebox.Api.Controllers;

[ApiController]
[Route("api/rooms/{code}/playback")]
public class PlaybackController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ITrackService _trackService;

    public PlaybackController(IRoomService roomService, ITrackService trackService)
    {
        _roomService = roomService;
        _trackService = trackService;
    }

    [HttpPost("play")]
    public async Task<IActionResult> Play([FromRoute] string code, [FromBody] PlayTrackRequest request, CancellationToken cancellationToken)
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

        var track = await _trackService.GetByIdAsync(request.TrackId, cancellationToken);
        if (track is null)
        {
            return NotFound();
        }

        await _trackService.PromoteNextTrackAsync(room, cancellationToken);
        await _roomService.UpdateNowPlayingAsync(room, track.Id, cancellationToken);
        await _roomService.UpdatePlaybackStateAsync(room, PlaybackStatus.Playing, 0, cancellationToken);

        return Ok();
    }

    [HttpPost("pause")]
    public async Task<IActionResult> Pause([FromRoute] string code, [FromBody] PauseTrackRequest request, CancellationToken cancellationToken)
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

        await _roomService.UpdatePlaybackStateAsync(room, PlaybackStatus.Paused, room.PlaybackState.PositionMs, cancellationToken);
        return Ok();
    }

    [HttpPost("seek")]
    public async Task<IActionResult> Seek([FromRoute] string code, [FromBody] SeekTrackRequest request, CancellationToken cancellationToken)
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

        await _roomService.UpdatePlaybackStateAsync(room, PlaybackStatus.Playing, request.PositionMs, cancellationToken);
        return Ok();
    }
}
