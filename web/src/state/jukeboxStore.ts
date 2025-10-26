import { create } from "zustand";
import type { RoomSummary, Track, UserSession } from "../types";

function statusPriority(status: Track["status"]) {
  switch (status) {
    case "Playing":
      return 4;
    case "Ready":
      return 3;
    case "Preparing":
      return 2;
    case "Queued":
      return 1;
    case "Played":
      return 0;
    case "Error":
      return -1;
    default:
      return 0;
  }
}

function orderQueue(tracks: Track[]) {
  return tracks
    .filter((track) => track.status !== "Played")
    .slice()
    .sort((a, b) => {
      const statusDiff = statusPriority(b.status) - statusPriority(a.status);
      if (statusDiff !== 0) return statusDiff;
      const scoreDiff = b.score - a.score;
      if (scoreDiff !== 0) return scoreDiff;
      const aCreated = new Date(a.createdAt).getTime();
      const bCreated = new Date(b.createdAt).getTime();
      return aCreated - bCreated;
    });
}

interface JukeboxState {
  session?: UserSession;
  room?: RoomSummary;
  queue: Track[];
  setSession: (session: UserSession | undefined) => void;
  setRoom: (room: RoomSummary | undefined) => void;
  setQueue: (tracks: Track[]) => void;
  upsertTrack: (track: Track) => void;
  removeTrack: (trackId: string) => void;
  updateTrackMeta: (trackId: string, updater: (track: Track) => Track) => void;
}

export const useJukeboxStore = create<JukeboxState>((set, get) => ({
  session: undefined,
  room: undefined,
  queue: [],
  setSession: (session) => set({ session }),
  setRoom: (room) => set({ room }),
  setQueue: (tracks) => set({ queue: orderQueue(tracks) }),
  upsertTrack: (track) =>
    set(() => {
      const existing = get().queue;
      const index = existing.findIndex((t) => t.id === track.id);
      if (index >= 0) {
        const next = [...existing];
        next[index] = track;
        return { queue: orderQueue(next) };
      }
      return { queue: orderQueue([...existing, track]) };
    }),
  removeTrack: (trackId) =>
    set(() => ({
      queue: orderQueue(get().queue.filter((t) => t.id !== trackId))
    })),
  updateTrackMeta: (trackId, updater) =>
    set(() => {
      const existing = get().queue;
      const idx = existing.findIndex((t) => t.id === trackId);
      if (idx === -1) {
        return {};
      }
      const updated = updater(existing[idx]);
      if (updated.status === "Played") {
        return { queue: orderQueue(existing.filter((t) => t.id !== trackId)) };
      }
      const next = [...existing];
      next[idx] = updated;
      return { queue: orderQueue(next) };
    })
}));
