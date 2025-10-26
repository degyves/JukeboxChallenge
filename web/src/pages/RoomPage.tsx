import type { FormEvent } from "react";
import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ApiClient } from "../services/api";
import { useJukeboxStore } from "../state/jukeboxStore";
import { QueueList } from "../components/QueueList";
import { GlassButton } from "../components/GlassButton";
import { NowPlaying } from "../components/NowPlaying";
import { SearchModal } from "../components/SearchModal";
import { useRoomHub } from "../hooks/useRoomHub";
import { QrCard } from "../components/QrCard";

export function RoomPage() {
  const { code } = useParams<{ code: string }>();
  const navigate = useNavigate();
  const session = useJukeboxStore((s) => s.session);
  const room = useJukeboxStore((s) => s.room);
  const queue = useJukeboxStore((s) => s.queue);
  const setRoom = useJukeboxStore((s) => s.setRoom);
  const setQueue = useJukeboxStore((s) => s.setQueue);
  const [urlInput, setUrlInput] = useState("");
  const [showSearch, setShowSearch] = useState(false);
  const [toast, setToast] = useState<string>();
  const [loading, setLoading] = useState(false);

  const isHost = session?.role === "Host";
  useRoomHub(!!session);

  useEffect(() => {
    if (!session || !code || session.roomCode !== code) {
      navigate("/");
      return;
    }

    const initialise = async () => {
      try {
        const [roomSummary, queueData] = await Promise.all([
          ApiClient.getRoom(code),
          ApiClient.getQueue(code)
        ]);
        setRoom(roomSummary);
        setQueue(queueData.tracks);
      } catch {
        setToast("Failed to load room data.");
      }
    };
    initialise();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [code, session?.roomCode]);

  const nowPlaying = useMemo(() => {
    if (room?.nowPlayingTrackId) {
      return queue.find((track) => track.id === room.nowPlayingTrackId);
    }
    return queue.find((track) => track.status === "Playing");
  }, [room, queue]);

  const joinUrl = `${window.location.origin}/join/${code ?? ""}`;

  const addTrack = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!session || !code || !urlInput.trim()) {
      return;
    }
    setLoading(true);
    try {
      const meta = await fetchYoutubeMeta(urlInput.trim());
      await ApiClient.addTrack(code, {
        youtubeUrl: urlInput.trim(),
        addedByUserId: session.userId,
        videoId: meta?.videoId ?? undefined,
        title: meta?.title ?? undefined,
        channel: meta?.channel ?? undefined,
        durationMs: typeof meta?.durationMs === "number" ? meta!.durationMs : undefined,
        thumbnailUrl: meta?.thumbnailUrl ?? undefined
      });
      setUrlInput("");
      setToast("Track added!");
    } catch {
      setToast("Could not add track. Maybe it's already queued?");
    } finally {
      setLoading(false);
    }
  };

  async function fetchYoutubeMeta(url: string) {
    try {
      const oembedUrl = `https://www.youtube.com/oembed?url=${encodeURIComponent(url)}&format=json`;
      const resp = await fetch(oembedUrl);
      if (!resp.ok) return null;
      const body = await resp.json();
      const params = new URLSearchParams(new URL(url).search);
      const v = params.get("v") ?? null;
      return {
        videoId: v,
        title: body.title ?? null,
        channel: body.author_name ?? null,
        durationMs: null,
        thumbnailUrl: body.thumbnail_url ?? null
      };
    } catch {
      return null;
    }
  }

  const handleVote = async (trackId: string, value: 1 | -1) => {
    if (!session || !code) return;
    try {
      await ApiClient.voteTrack(code, trackId, { userId: session.userId, value });
    } catch {
      setToast("Vote failed – slow your roll?");
    }
  };

  const handlePlay = async (trackId: string) => {
    if (!session?.hostSecret || !code) return;
    try {
      await ApiClient.playbackState(code, { trackId, hostSecret: session.hostSecret });
    } catch {
      setToast("Play command failed.");
    }
  };

  const handleSkip = async () => {
    if (!session?.hostSecret || !code) return;
    try {
      await ApiClient.setNext(code, { hostSecret: session.hostSecret });
    } catch {
      setToast("Skip failed.");
    }
  };

  const handleRemove = async (trackId: string) => {
    if (!session?.hostSecret || !code) return;
    try {
      await ApiClient.removeTrack(code, trackId, session.hostSecret);
      setToast("Track removed.");
    } catch {
      setToast("Failed to remove track.");
    }
  };

  if (!session) {
    return null;
  }

  return (
    <main className="app-grid">
      <div className="layout-shell">
        <header className="flex flex-col gap-4 text-white">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="section-title">Room code</p>
              <h1 className="text-3xl font-semibold text-white sm:text-4xl">#{code}</h1>
            </div>
            <div className="rounded-full border border-white/10 bg-white/10 px-4 py-2 text-sm text-white/70">
              Logged in as {session.role}
            </div>
          </div>
        </header>

        <section className="grid gap-6 lg:grid-cols-[2fr_1fr]">
          <div className="flex flex-col gap-6">
            <NowPlaying
              track={nowPlaying}
              isHost={isHost}
              onEnded={handleSkip}
              onProgress={(ms) => {
                if (!room) return;
                setRoom({ ...room, playbackState: { ...room.playbackState, positionMs: ms } });
              }}
            />

            <div className="glass-panel space-y-5 p-6">
              <div className="flex flex-col gap-1 text-left">
                <p className="section-title">Quick add</p>
                <h2 className="text-xl font-semibold text-white">Drop a YouTube link</h2>
              </div>
              <form onSubmit={addTrack} className="flex flex-col gap-3 md:flex-row md:items-center">
                <input
                  value={urlInput}
                  onChange={(event) => setUrlInput(event.target.value)}
                  placeholder="Paste a YouTube URL"
                  className="frosted-input md:flex-1"
                />
                <div className="flex flex-col gap-2 md:flex-row">
                  <GlassButton type="submit" disabled={loading} className="md:w-auto">
                    {loading ? "Adding..." : "Add"}
                  </GlassButton>
                  <GlassButton
                    type="button"
                    variant="outline"
                    onClick={() => setShowSearch(true)}
                    className="md:w-auto"
                  >
                    Search
                  </GlassButton>
                </div>
              </form>
            </div>

            <QueueList
              tracks={queue}
              isHost={isHost}
              onVote={handleVote}
              onPlay={isHost ? handlePlay : undefined}
              onRemove={isHost ? handleRemove : undefined}
            />
          </div>

          <aside className="flex flex-col gap-4">
            <div className="glass-card space-y-3 p-6 text-left text-white/70">
              <h3 className="section-title">Playback</h3>
              <p>Host device controls playback. Guests enjoy the music on-site while votes shape the queue.</p>
              {isHost && (
                <GlassButton onClick={handleSkip} className="w-full" variant="outline">
                  Skip track
                </GlassButton>
              )}
            </div>
            <div className="glass-card space-y-2 p-6 text-left text-white/60">
              <p>Votes reorder the queue instantly. Rate limits: add 5/min, vote 30/min.</p>
              <p>Host secret stays local. Keep this tab open to keep the beats alive.</p>
            </div>
            {isHost && (
              <QrCard
                roomCode={code!}
                joinUrl={joinUrl}
                onCopy={() => navigator.clipboard.writeText(joinUrl)}
              />
            )}
          </aside>
        </section>
      </div>

      {toast && <div className="toast-panel">{toast}</div>}

      <SearchModal
        open={showSearch}
        onClose={() => setShowSearch(false)}
        onSelect={async (videoId) => {
          if (!session || !code) return;
          try {
            const url = `https://www.youtube.com/watch?v=${videoId}`;
            const meta = await fetchYoutubeMeta(url);
            await ApiClient.addTrack(code, {
              youtubeUrl: url,
              addedByUserId: session.userId,
              videoId: meta?.videoId ?? undefined,
              title: meta?.title ?? undefined,
              channel: meta?.channel ?? undefined,
              durationMs: typeof meta?.durationMs === "number" ? meta!.durationMs : undefined,
              thumbnailUrl: meta?.thumbnailUrl ?? undefined
            });
            setToast("Track added!");
          } catch {
            setToast("Could not queue search result.");
          }
        }}
      />
    </main>
  );
}
