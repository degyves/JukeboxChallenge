import axios from "axios";
import type { RoomSummary, Track } from "../types";

const API_BASE = (import.meta.env.VITE_API_BASE ?? "http://localhost:8080/api").replace(/\/$/, "");
const STREAM_BASE = (import.meta.env.VITE_STREAM_BASE ?? "http://localhost:4000").replace(/\/$/, "");
const HUB_BASE = import.meta.env.VITE_HUB_URL;

const api = axios.create({
  baseURL: API_BASE,
  timeout: 5000
});

export interface CreateRoomRequest {
  displayName: string;
}

export interface CreateRoomResponse {
  code: string;
  hostSecret: string;
  userId: string;
}

export interface JoinRoomRequest {
  displayName: string;
  hostSecret?: string;
}

export interface JoinRoomResponse {
  userId: string;
  role: "Host" | "Guest";
}

export interface QueueResponse {
  tracks: Track[];
}

export const ApiClient = {
  async createRoom(payload: CreateRoomRequest) {
    const { data } = await api.post<CreateRoomResponse>("/rooms", payload);
    return data;
  },
  async joinRoom(code: string, payload: JoinRoomRequest) {
    const { data } = await api.post<JoinRoomResponse>(`/rooms/${code}/join`, payload);
    return data;
  },
  async getRoom(code: string) {
    const { data } = await api.get<RoomSummary>(`/rooms/${code}`);
    return data;
  },
  async getQueue(code: string) {
    const { data } = await api.get<QueueResponse>(`/rooms/${code}/queue`);
    return data;
  },
  async addTrack(code: string, payload: { youtubeUrl?: string; query?: string; addedByUserId: string; videoId?: string; title?: string; channel?: string; durationMs?: number; thumbnailUrl?: string }) {
    const { data } = await api.post<Track>(`/rooms/${code}/tracks`, payload);
    return data;
  },
  async voteTrack(code: string, trackId: string, payload: { userId: string; value: 1 | -1 }) {
    const { data } = await api.post<Track>(`/rooms/${code}/tracks/${trackId}/vote`, payload);
    return data;
  },
  async removeTrack(code: string, trackId: string, hostSecret: string) {
    await api.delete(`/rooms/${code}/tracks/${trackId}`, {
      headers: { "x-host-secret": hostSecret }
    });
  },
  async setNext(code: string, payload: { hostSecret: string }) {
    const { data } = await api.post<Track | void>(`/rooms/${code}/next`, payload);
    return data;
  },
  async playbackState(code: string, payload: { trackId: string; hostSecret: string }) {
    await api.post(`/rooms/${code}/playback/play`, payload);
  },
  async pause(code: string, payload: { hostSecret: string }) {
    await api.post(`/rooms/${code}/playback/pause`, payload);
  },
  async seek(code: string, payload: { hostSecret: string; positionMs: number }) {
    await api.post(`/rooms/${code}/playback/seek`, payload);
  }
};

export const HubConfig = {
  url: HUB_BASE ?? `${API_BASE.replace(/\/api$/, "")}/hub/rooms`
};

export async function searchStream(query: string) {
  const { data } = await axios.get<{ items: TrackMeta[] }>(`${STREAM_BASE}/search`, {
    params: { q: query }
  });
  return data.items;
}

interface TrackMeta {
  videoId: string;
  title: string;
  channel: string;
  durationMs: number;
  thumbnailUrl: string;
}
