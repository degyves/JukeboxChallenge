using System.ComponentModel.DataAnnotations;

namespace PartyJukebox.Api.Dtos;

public record PlayTrackRequest([Required] string TrackId, [Required] string HostSecret);

public record PauseTrackRequest([Required] string HostSecret);

public record SeekTrackRequest([Required] string HostSecret, [Range(0, int.MaxValue)] int PositionMs);

public record NextTrackRequest([Required] string HostSecret);
