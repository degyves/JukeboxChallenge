import type { HubConnection } from "@microsoft/signalr";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import { HubConfig } from "../services/api";
import { useJukeboxStore } from "../state/jukeboxStore";
import type { PlaybackState, RoomSummary, Track } from "../types";

interface HubControls {
  connection: HubConnection | null;
  isConnected: boolean;
}

export function useRoomHub(active: boolean): HubControls {
  const session = useJukeboxStore((s) => s.session);
  const setRoom = useJukeboxStore((s) => s.setRoom);
  const setQueue = useJukeboxStore((s) => s.setQueue);
  const upsertTrack = useJukeboxStore((s) => s.upsertTrack);
  const removeTrack = useJukeboxStore((s) => s.removeTrack);
  const updateTrackMeta = useJukeboxStore((s) => s.updateTrackMeta);
  const roomRef = useRef<RoomSummary | null>(null);
  const connectionRef = useRef<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    if (!session || !active) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${HubConfig.url}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    connection.on("RoomSync", (room: RoomSummary, queue: Track[]) => {
      roomRef.current = room;
      setRoom(room);
      setQueue(queue);
    });

    connection.on("QueueUpdated", (trackId: string, score: number, status: Track["status"]) => {
      updateTrackMeta(trackId, (track) => ({ ...track, score, status }));
    });

    connection.on("TrackAdded", (track: Track) => {
      upsertTrack(track);
    });

    connection.on("TrackRemoved", (trackId: string) => {
      removeTrack(trackId);
    });

    connection.on("PlaybackState", (state: PlaybackState) => {
      if (!roomRef.current) {
        return;
      }
      const nextRoom = { ...roomRef.current, playbackState: state };
      roomRef.current = nextRoom;
      setRoom(nextRoom);
    });

    connection.start()
      .then(async () => {
        setConnected(true);
        connectionRef.current = connection;
        await connection.invoke("JoinRoom", session.roomCode);
      })
      .catch((err) => {
        console.error("Failed to connect to hub", err);
      });

    return () => {
      connection.stop().catch((err) => console.error("Hub stop failed", err));
      setConnected(false);
      connectionRef.current = null;
    };
  }, [session?.roomCode, active]);

  return {
    connection: connectionRef.current,
    isConnected: connected
  };
}
