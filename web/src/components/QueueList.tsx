import { Play, Trash2 } from "lucide-react";
import clsx from "classnames";
import type { Track, TrackStatus } from "../types";
import { VoteButtons } from "./VoteButtons";

interface Props {
  tracks: Track[];
  isHost: boolean;
  onVote: (trackId: string, value: 1 | -1) => void;
  onPlay?: (trackId: string) => void;
  onRemove?: (trackId: string) => void;
}

const statusAccent: Partial<Record<TrackStatus, string>> = {
  Playing: "bg-emerald-400/10 text-emerald-200 border border-emerald-400/20",
  Ready: "bg-brand-400/20 text-brand-100 border border-brand-400/20",
  Queued: "bg-white/10 text-white/70 border border-white/10",
  Preparing: "bg-amber-400/10 text-amber-200 border border-amber-300/20",
  Error: "bg-rose-500/10 text-rose-200 border border-rose-500/20"
};

export function QueueList({ tracks, isHost, onVote, onPlay, onRemove }: Props) {
  if (tracks.length === 0) {
    return (
      <div className="glass-card p-6 text-center text-white/70">
        Queue is empty. Paste a YouTube link to keep the momentum going.
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      {tracks.map((track) => (
        <div
          key={track.id}
          className="glass-card flex flex-col gap-4 border-white/10 bg-white/10 p-4 backdrop-blur-md transition hover:border-white/20 hover:bg-white/20 md:flex-row md:items-start"
        >
          <img
            src={track.thumbnailUrl || "/album-placeholder.png"}
            alt={track.title}
            className="h-24 w-full rounded-2xl object-cover shadow-glass-soft md:h-28 md:w-36"
          />
          <div className="flex-1 space-y-3 text-left">
            <div className="flex flex-wrap items-center gap-3">
              <span
                className={clsx(
                  "rounded-full px-3 py-1 text-xs font-medium uppercase tracking-[0.25em]",
                  statusAccent[track.status] ?? "bg-white/10 text-white/70 border border-white/10"
                )}
              >
                {track.status}
              </span>
              <span className="text-xs text-white/50">{formatDuration(track.durationMs)}</span>
            </div>
            <div className="space-y-1">
              <p className="text-lg font-semibold text-white md:text-xl">{track.title}</p>
              <p className="text-sm text-white/60">{track.channel}</p>
            </div>
            <p className="text-xs text-white/40">Added by {track.addedBy}</p>
          </div>
          <div className="flex items-center justify-between gap-4 md:flex-col md:items-end">
            <VoteButtons score={track.score} onVote={(value) => onVote(track.id, value)} />
            {isHost && (
              <div className="flex gap-2 md:flex-col">
                <button
                  className="rounded-full border border-white/10 bg-white/10 p-2 transition hover:border-brand-400/60 hover:bg-brand-400/20"
                  onClick={() => onPlay?.(track.id)}
                  aria-label="Play track"
                >
                  <Play size={18} />
                </button>
                <button
                  className="rounded-full border border-white/10 bg-white/10 p-2 transition hover:border-rose-400/60 hover:bg-rose-400/20"
                  onClick={() => onRemove?.(track.id)}
                  aria-label="Remove track"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

function formatDuration(durationMs: number) {
  if (!durationMs || durationMs <= 0) {
    return "Length unknown";
  }
  const totalSeconds = Math.round(durationMs / 1000);
  const seconds = totalSeconds % 60;
  const minutes = Math.floor((totalSeconds / 60) % 60);
  const hours = Math.floor(totalSeconds / 3600);

  const parts = [];
  if (hours > 0) {
    parts.push(hours.toString());
    parts.push(minutes.toString().padStart(2, "0"));
  } else {
    parts.push(minutes.toString());
  }
  parts.push(seconds.toString().padStart(2, "0"));
  return parts.join(":");
}
