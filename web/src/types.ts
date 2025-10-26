export type PlaybackStatus = "Idle" | "Buffering" | "Playing" | "Paused" | "Ended";

export interface PlaybackState {
  status: PlaybackStatus;
  positionMs: number;
  updatedAt: string;
}

export interface RoomSummary {
  id: string;
  code: string;
  isActive: boolean;
  nowPlayingTrackId: string | null;
  playbackState: PlaybackState;
}

export type TrackStatus = "Queued" | "Preparing" | "Ready" | "Error" | "Playing" | "Played";

export interface Track {
  id: string;
  roomId: string;
  source: string;
  videoId: string;
  title: string;
  channel: string;
  durationMs: number;
  thumbnailUrl: string;
  addedBy: string;
  votes: {
    up: number;
    down: number;
  };
  score: number;
  status: TrackStatus;
  createdAt: string;
}

export interface UserSession {
  roomCode: string;
  userId: string;
  role: "Host" | "Guest";
  hostSecret?: string;
}
