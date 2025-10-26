namespace PartyJukebox.Api.Configuration;

public class RateLimitOptions
{
    public const string SectionName = "RateLimits";

    public int TrackAddsPerMinute { get; set; } = 5;

    public int VotesPerMinute { get; set; } = 30;
}
