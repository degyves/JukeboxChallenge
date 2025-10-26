using PartyJukebox.Api.Models;

namespace PartyJukebox.Api.Services;

public interface IQueueOrderingService
{
    IEnumerable<Track> OrderQueue(IEnumerable<Track> tracks);
}

public class QueueOrderingService : IQueueOrderingService
{
    public IEnumerable<Track> OrderQueue(IEnumerable<Track> tracks)
    {
        return tracks
            .OrderByDescending(t => GetStatusPriority(t.Status))
            .ThenByDescending(t => t.Score)
            .ThenBy(t => t.CreatedAt);
    }

    private static int GetStatusPriority(TrackStatus status) => status switch
    {
        TrackStatus.Ready => 3,
        TrackStatus.Preparing => 2,
        TrackStatus.Queued => 1,
        TrackStatus.Playing => 4,
        TrackStatus.Played => 0,
        TrackStatus.Error => -1,
        _ => 0
    };
}
