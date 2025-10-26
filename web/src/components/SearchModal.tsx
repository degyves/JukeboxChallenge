import type { FormEvent } from "react";
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import { AnimatePresence, motion } from "framer-motion";
import { searchStream } from "../services/api";
import { GlassButton } from "./GlassButton";

interface Props {
  open: boolean;
  onClose: () => void;
  onSelect: (videoId: string) => void;
}

interface ResultItem {
  videoId: string;
  title: string;
  channel: string;
  durationMs: number;
  thumbnailUrl: string;
}

export function SearchModal({ open, onClose, onSelect }: Props) {
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [results, setResults] = useState<ResultItem[]>([]);
  const [error, setError] = useState<string>();
  const [portalRoot, setPortalRoot] = useState<HTMLElement | null>(null);

  useEffect(() => {
    if (typeof document !== "undefined") {
      setPortalRoot(document.body);
    }
  }, []);

  useEffect(() => {
    if (!open) {
      setQuery("");
      setLoading(false);
      setResults([]);
      setError(undefined);
    }
  }, [open]);

  if (!portalRoot) {
    return null;
  }

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!query.trim()) return;
    setLoading(true);
    setError(undefined);
    try {
      const items = await searchStream(query.trim());
      setResults(items);
    } catch {
      setError("Could not reach stream service. Try again later.");
    } finally {
      setLoading(false);
    }
  };

  const noResults = !loading && results.length === 0;

  return createPortal(
    <AnimatePresence>
      {open && (
        <motion.div
          key="search-modal"
          className="modal-backdrop"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
        >
          <motion.div
            className="glass-panel mx-4 w-full max-w-3xl space-y-6 p-6"
            initial={{ opacity: 0, y: 30, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 30, scale: 0.95 }}
            transition={{ duration: 0.22, ease: [0.22, 1, 0.36, 1] }}
          >
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-semibold text-white">Search tracks</h2>
              <button onClick={onClose} className="text-xs uppercase tracking-[0.3em] text-white/60">
                Close
              </button>
            </div>

            <motion.form
              onSubmit={submit}
              className="flex flex-col gap-3 sm:flex-row"
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.2 }}
            >
              <input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search for a vibe"
                className="frosted-input flex-1"
              />
              <GlassButton type="submit" disabled={loading} className="sm:w-auto">
                {loading ? "Searching..." : "Search"}
              </GlassButton>
            </motion.form>

            <AnimatePresence>
              {error && (
                <motion.p
                  key="search-error"
                  className="text-sm text-rose-200"
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: 8 }}
                >
                  {error}
                </motion.p>
              )}
            </AnimatePresence>

            <div className="max-h-80 space-y-3 overflow-y-auto">
              {noResults && <p className="text-center text-sm text-white/40">No results yet – try a different search.</p>}
              {results.map((item, index) => (
                <motion.button
                  key={item.videoId}
                  onClick={() => {
                    onSelect(item.videoId);
                    onClose();
                  }}
                  className="flex w-full gap-4 rounded-2xl border border-white/10 bg-white/10 p-3 text-left transition hover:border-brand-400/50 hover:bg-brand-400/20"
                  disabled={loading}
                  initial={{ opacity: 0, y: 12 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.03 }}
                  whileHover={{ scale: 1.01 }}
                  whileTap={{ scale: 0.98 }}
                >
                  <img
                    src={item.thumbnailUrl || "/album-placeholder.png"}
                    alt={item.title}
                    className="h-16 w-24 rounded-xl object-cover"
                  />
                  <div className="flex-1 space-y-1">
                    <p className="text-base font-semibold text-white">{item.title}</p>
                    <p className="text-sm text-white/70">{item.channel}</p>
                    <p className="text-xs text-white/50">{formatDuration(item.durationMs)}</p>
                  </div>
                </motion.button>
              ))}
            </div>
          </motion.div>
        </motion.div>
      )}
    </AnimatePresence>,
    portalRoot
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

  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
  }
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}
