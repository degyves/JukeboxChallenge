using Microsoft.AspNetCore.Mvc;
using PartyJukebox.Api.Dtos;
using PartyJukebox.Api.Mapping;
using PartyJukebox.Api.Services;

namespace PartyJukebox.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ITrackService _trackService;

    public RoomsController(IRoomService roomService, ITrackService trackService)
    {
        _roomService = roomService;
        _trackService = trackService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateRoomResponse>> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var (room, host) = await _roomService.CreateRoomAsync(request.DisplayName, cancellationToken);
        return Ok(new CreateRoomResponse(room.Code, room.HostSecret, host.Id));
    }

    [HttpPost("{code}/join")]
    public async Task<ActionResult<JoinRoomResponse>> JoinRoom([FromRoute] string code, [FromBody] JoinRoomRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var (room, user) = await _roomService.JoinRoomAsync(code.ToUpperInvariant(), request.DisplayName, request.HostSecret, cancellationToken);
            return Ok(new JoinRoomResponse(user.Id, user.Role));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<RoomSummaryDto>> GetRoom([FromRoute] string code, CancellationToken cancellationToken)
    {
        var room = await _roomService.GetByCodeAsync(code.ToUpperInvariant(), cancellationToken);
        if (room is null)
        {
            return NotFound();
        }

        return Ok(room.ToSummaryDto());
    }
}
