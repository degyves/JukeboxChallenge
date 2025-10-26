using System.Net.Http.Json;
using PartyJukebox.Api.Dtos;
using Xunit;

namespace PartyJukebox.Api.Tests;

public class RoomsEndpointsTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoomsEndpointsTests(TestingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateRoom_ThenJoin_Succeeds()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest("Host"));
        createResponse.EnsureSuccessStatusCode();
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateRoomResponse>();
        Assert.NotNull(createBody);
        Assert.False(string.IsNullOrWhiteSpace(createBody!.Code));
        Assert.False(string.IsNullOrWhiteSpace(createBody.HostSecret));

        var joinResponse = await _client.PostAsJsonAsync($"/api/rooms/{createBody.Code}/join", new JoinRoomRequest("Guest", null));
        joinResponse.EnsureSuccessStatusCode();
        var joinBody = await joinResponse.Content.ReadFromJsonAsync<JoinRoomResponse>();
        Assert.NotNull(joinBody);
        Assert.Equal("Guest", joinBody!.Role.ToString());
    }

    [Fact]
    public async Task AddTrack_AndVote_UpdatesScore()
    {
        var create = await _client.PostAsJsonAsync("/api/rooms", new CreateRoomRequest("Host"));
        var room = await create.Content.ReadFromJsonAsync<CreateRoomResponse>();
        Assert.NotNull(room);

        var join = await _client.PostAsJsonAsync($"/api/rooms/{room!.Code}/join", new JoinRoomRequest("Guest", null));
        var guest = await join.Content.ReadFromJsonAsync<JoinRoomResponse>();
        Assert.NotNull(guest);

        var addTrack = await _client.PostAsJsonAsync($"/api/rooms/{room.Code}/tracks", new
        {
            youtubeUrl = "https://www.youtube.com/watch?v=video123",
            addedByUserId = guest!.UserId
        });
        addTrack.EnsureSuccessStatusCode();
        var trackDto = await addTrack.Content.ReadFromJsonAsync<TrackDto>();
        Assert.NotNull(trackDto);

        var vote = await _client.PostAsJsonAsync($"/api/rooms/{room.Code}/tracks/{trackDto!.Id}/vote", new VoteRequest(1, guest.UserId));
        vote.EnsureSuccessStatusCode();
        var updated = await vote.Content.ReadFromJsonAsync<TrackDto>();
        Assert.NotNull(updated);
        Assert.Equal(1, updated!.Score);
    }
}
