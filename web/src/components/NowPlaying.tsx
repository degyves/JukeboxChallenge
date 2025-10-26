import { useCallback, useEffect, useMemo, useRef } from "react";
import YouTube, { type YouTubeEvent } from "react-youtube";
import type { Track } from "../types";

interface Props {
  track?: Track | null;
  isHost: boolean;
  onEnded?: () => void;
  onProgress?: (timeMs: number) => void;
}

const PLAYER_STATES = {
  ENDED: 0,
  PLAYING: 1,
  PAUSED: 2,
  BUFFERING: 3,
  CUED: 5
};

type Player = {
  playVideo: () => void;
  getCurrentTime?: () => number | Promise<number>;
};

export function NowPlaying({ track, isHost, onEnded, onProgress }: Props) {
  const playerRef = useRef<Player | null>(null);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const hasStartedRef = useRef(false);

  const clearTimer = useCallback(() => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
  }, []);

  const tickProgress = useCallback(() => {
    const player = playerRef.current;
    if (!player || !player.getCurrentTime) {
      return;
    }

    try {
      const value = player.getCurrentTime();
      if (value && typeof (value as Promise<number>).then === "function") {
        (value as Promise<number>)
          .then((seconds) => {
            if (typeof seconds === "number" && Number.isFinite(seconds)) {
              onProgress?.(Math.round(seconds * 1000));
            }
          })
          .catch(() => {
            /* swallow timing errors */
          });
      } else if (typeof value === "number" && Number.isFinite(value)) {
        onProgress?.(Math.round(value * 1000));
      }
    } catch {
      /* swallow timing errors */
    }
  }, [onProgress]);

  const startTimer = useCallback(() => {
    clearTimer();
    if (!isHost) {
      return;
    }
    timerRef.current = setInterval(tickProgress, 1000);
    tickProgress();
  }, [clearTimer, isHost, tickProgress]);

  const handleReady = useCallback(
    (event: YouTubeEvent) => {
      playerRef.current = event.target as unknown as Player;
      if (isHost) {
        (event.target as unknown as Player).playVideo();
      }
    },
    [isHost]
  );

  const handleStateChange = useCallback(
    (event: YouTubeEvent) => {
      playerRef.current = event.target as unknown as Player;
      if (!isHost) {
        return;
      }

      switch (event.data) {
        case PLAYER_STATES.PLAYING:
          hasStartedRef.current = true;
          startTimer();
          break;
        case PLAYER_STATES.ENDED:
          clearTimer();
          if (hasStartedRef.current) {
            onEnded?.();
          }
          break;
        case PLAYER_STATES.PAUSED:
        case PLAYER_STATES.BUFFERING:
        case PLAYER_STATES.CUED:
        default:
          clearTimer();
          break;
      }
    },
    [clearTimer, isHost, onEnded, startTimer]
  );

  useEffect(() => clearTimer, [clearTimer]);

  useEffect(() => {
    if (!isHost) {
      clearTimer();
    }
  }, [clearTimer, isHost]);

  useEffect(() => {
    hasStartedRef.current = false;
    clearTimer();
  }, [clearTimer, track?.id]);

  const playerOptions = useMemo(
    () => ({
      width: "100%",
      height: "100%",
      playerVars: {
        autoplay: isHost ? 1 : 0,
        controls: 1,
        playsinline: 1,
        modestbranding: 1,
        rel: 0
      }
    }),
    [isHost]
  );

  if (!track) {
    return (
      <div className="glass-panel p-6 text-center text-white/70">
        Nothing playing yet. The top ready track will auto-start once the host presses play.
      </div>
    );
  }

  const voteScore = track.votes ? track.votes.up - track.votes.down : 0;

  return (
    <div className="glass-panel flex flex-col gap-6 border-white/10 bg-white/10 p-6 backdrop-blur-md">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:gap-6">
        <img
          src={track.thumbnailUrl || "/album-placeholder.png"}
          alt={track.title}
          className="h-24 w-full max-w-[10rem] rounded-3xl object-cover shadow-glass-soft sm:h-28 md:h-40 md:w-40"
        />
        <div className="flex-1 space-y-4 text-left">
          <p className="section-title text-left">Now playing</p>
          <div className="space-y-1">
            <h2 className="text-2xl font-semibold text-white sm:text-3xl md:text-4xl">{track.title}</h2>
            <p className="text-sm text-white/70 sm:text-base">{track.channel}</p>
          </div>
          <div className="flex flex-wrap items-center gap-3 text-xs text-white/50">
            <span>Votes • {voteScore}</span>
            <span>Added by {track.addedBy}</span>
            {track.durationMs ? <span>{formatDuration(track.durationMs)}</span> : null}
          </div>
          <p className="text-xs text-white/40">
            {isHost
              ? "Keep this browser tab focused and unmuted to broadcast the music for everyone onsite."
              : "Music plays through the host device—coordinate with them to keep the beats flowing."}
          </p>
        </div>
        {isHost && (
          <div className="w-full overflow-hidden rounded-2xl border border-white/10 bg-black/60 shadow-glass-soft md:w-80">
            <div className="aspect-video w-full">
              <YouTube
                videoId={track.videoId}
                opts={playerOptions}
                onReady={handleReady}
                onStateChange={handleStateChange}
                className="h-full w-full"
                iframeClassName="h-full w-full"
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function formatDuration(durationMs: number) {
  if (!durationMs || durationMs <= 0) {
    return null;
  }
  const totalSeconds = Math.round(durationMs / 1000);
  const seconds = totalSeconds % 60;
  const minutes = Math.floor((totalSeconds / 60) % 60);
  const hours = Math.floor(totalSeconds / 3600);

  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
  }
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}
