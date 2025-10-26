using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PartyJukebox.Api.Services;

public record StreamMetadata(string VideoId, string Title, string Channel, int DurationMs, string ThumbnailUrl);

public class StreamClientOptions
{
    public const string SectionName = "Stream";

    public string BaseUrl { get; set; } = "http://stream:4000";
}

public interface IStreamClient
{
    Task<StreamMetadata> ResolveAsync(string videoIdOrUrl, CancellationToken cancellationToken);
    Task<StreamMetadata?> SearchAsync(string query, CancellationToken cancellationToken);
}

public class StreamClient : IStreamClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StreamClient> _logger;

    public StreamClient(HttpClient httpClient, IOptions<StreamClientOptions> options, ILogger<StreamClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl.TrimEnd('/'));
        _logger = logger;
    }

    public async Task<StreamMetadata> ResolveAsync(string videoIdOrUrl, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/resolve?input={Uri.EscapeDataString(videoIdOrUrl)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<ResolveResponse>(cancellationToken: cancellationToken)
                  ?? throw new InvalidOperationException("Invalid stream resolve response");
        return dto.ToMetadata();
    }

    public async Task<StreamMetadata?> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/search?q={Uri.EscapeDataString(query)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken: cancellationToken)
                  ?? throw new InvalidOperationException("Invalid stream search response");
        return dto.Items.FirstOrDefault()?.ToMetadata();
    }

    private record ResolveResponse(StreamItem Item)
    {
        public StreamMetadata ToMetadata() => Item.ToMetadata();
    }

    private record SearchResponse(List<StreamItem> Items);

    private record StreamItem(string VideoId, string Title, string Channel, int DurationMs, string ThumbnailUrl)
    {
        public StreamMetadata ToMetadata() => new(VideoId, Title, Channel, DurationMs, ThumbnailUrl);
    }
}
