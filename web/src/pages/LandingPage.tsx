import type { FormEvent } from "react";
import { useEffect, useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import type { Transition, Variants } from "framer-motion";
import { useNavigate, useParams } from "react-router-dom";
import { ApiClient } from "../services/api";
import { useJukeboxStore } from "../state/jukeboxStore";
import { GlitchTitle } from "../components/GlitchTitle";
import { GlassButton } from "../components/GlassButton";

export function LandingPage() {
  const params = useParams<{ code?: string }>();
  const [displayName, setDisplayName] = useState("");
  const [roomCode, setRoomCode] = useState(params.code ?? "");
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const setSession = useJukeboxStore((s) => s.setSession);

  const cardVariants = useMemo<Variants>(
    () => ({
      hidden: { opacity: 0, y: 28, rotateX: 6 },
      visible: {
        opacity: 1,
        y: 0,
        rotateX: 0,
        transition: { type: "spring" as const, stiffness: 120, damping: 20, mass: 0.9 }
      }
    }),
    []
  );

  const childFade = useMemo<Variants>(
    () => ({
      hidden: { opacity: 0, y: 18 },
      visible: {
        opacity: 1,
        y: 0,
        transition: { type: "spring" as const, stiffness: 120, damping: 20, mass: 0.9 }
      }
    }),
    []
  );

  const errorTransition = useMemo<Transition>(
    () => ({ type: "spring" as const, stiffness: 120, damping: 20, mass: 0.9 }),
    []
  );

  useEffect(() => {
    if (params.code) {
      setRoomCode(params.code.toUpperCase());
    }
  }, [params.code]);

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!displayName.trim()) {
      setError("Enter a DJ alias first.");
      return;
    }
    setLoading(true);
    setError(undefined);
    try {
      const room = await ApiClient.createRoom({ displayName: displayName.trim() });
      setSession({
        roomCode: room.code,
        userId: room.userId,
        role: "Host",
        hostSecret: room.hostSecret
      });
      navigate(`/room/${room.code}`);
    } catch {
      setError("Could not create room. Try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleJoin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!displayName.trim() || !roomCode.trim()) {
      setError("Enter your name and the room PIN.");
      return;
    }
    setLoading(true);
    setError(undefined);
    try {
      const join = await ApiClient.joinRoom(roomCode.trim().toUpperCase(), {
        displayName: displayName.trim()
      });
      setSession({
        roomCode: roomCode.trim().toUpperCase(),
        userId: join.userId,
        role: join.role
      });
      navigate(`/room/${roomCode.trim().toUpperCase()}`);
    } catch {
      setError("Join failed. Double check the code.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <motion.main
      className="relative min-h-screen overflow-hidden"
      initial={{ opacity: 0, filter: "blur(16px)" }}
      animate={{ opacity: 1, filter: "blur(0px)" }}
      transition={{ duration: 0.65, ease: [0.25, 0.1, 0.25, 1] }}
    >
      <motion.div
        className="pointer-events-none absolute -bottom-32 left-1/2 h-96 w-96 -translate-x-1/2 rounded-full bg-accent-400/20 blur-3xl"
        animate={{ y: [-16, 12, -10, 0], opacity: [0.6, 0.9, 0.75, 0.6] }}
        transition={{ duration: 18, repeat: Infinity, ease: "easeInOut" }}
      />
      <div className="relative z-10 mx-auto flex min-h-screen w-full max-w-5xl flex-col items-center justify-center gap-12 px-6 py-16 lg:flex-row lg:items-stretch lg:gap-16">
        <motion.div
          className="flex w-full max-w-xl flex-col justify-center gap-6 text-center lg:text-left"
          variants={cardVariants}
          initial="hidden"
          animate="visible"
        >
          <GlitchTitle align="center">Neon Pulse Jukebox</GlitchTitle>
          <motion.p variants={childFade} className="text-base text-white/70">
            Launch a collaborative YouTube queue in seconds. Share the code, let guests add tracks, and keep the energy flowing with live votes and instant playback control.
          </motion.p>
          <motion.form variants={childFade} onSubmit={handleJoin} className="glass-panel space-y-5 p-6">
            <div className="space-y-1 text-left">
              <p className="section-title">Join a party</p>
              <h2 className="text-xl font-semibold text-white">Enter room code</h2>
            </div>
            <div className="grid gap-3">
              <input
                value={displayName}
                onChange={(event) => setDisplayName(event.target.value)}
                placeholder="Your display name"
                className="frosted-input"
              />
              <input
                value={roomCode}
                onChange={(event) => setRoomCode(event.target.value)}
                placeholder="Room code"
                maxLength={10}
                className="frosted-input uppercase tracking-[0.3em]"
              />
            </div>
            <GlassButton type="submit" className="w-full" size="lg" variant="outline" disabled={loading}>
              {loading ? "Connecting..." : "Join room"}
            </GlassButton>
          </motion.form>
          <AnimatePresence>
            {error && (
              <motion.p
                key="landing-error"
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: 12 }}
                transition={errorTransition}
                className="text-sm text-rose-200"
              >
                {error}
              </motion.p>
            )}
          </AnimatePresence>
        </motion.div>

        <motion.div
          className="flex w-full max-w-xl flex-col justify-center gap-6 text-center lg:text-left"
          variants={cardVariants}
          initial="hidden"
          animate="visible"
          transition={{ delay: 0.12, staggerChildren: 0.1 }}
        >
          <motion.div
            variants={childFade}
            className="glass-card p-6 text-left shadow-2xl shadow-brand-500/10 ring-1 ring-white/5"
          >
            <h2 className="section-title">How it works</h2>
            <ul className="mt-4 space-y-3 text-sm text-white/70">
              <li>1. Create a room using your host profile.</li>
              <li>2. Share the PIN or QR code with your guests.</li>
              <li>3. Let the crowd submit tracks, vote, and shape the night.</li>
            </ul>
          </motion.div>
          <motion.form variants={childFade} onSubmit={handleCreate} className="glass-panel space-y-5 p-6">
            <div className="space-y-1 text-left">
              <p className="section-title">Start a session</p>
              <h2 className="text-xl font-semibold text-white">Create host room</h2>
            </div>
            <input
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
              placeholder="Your display name"
              className="frosted-input"
            />
            <GlassButton type="submit" className="w-full" size="lg" disabled={loading}>
              {loading ? "Spinning up..." : "Create room"}
            </GlassButton>
          </motion.form>
        </motion.div>
      </div>
    </motion.main>
  );
}
