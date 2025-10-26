require("dotenv").config();
const express = require("express");
const cors = require("cors");
const pino = require("pino");
const ytdl = require("ytdl-core");
const ytSearch = require("yt-search");

const logger = pino({
  level: process.env.LOG_LEVEL || "info",
  transport:
    process.env.NODE_ENV === "production"
      ? undefined
      : {
          target: "pino-pretty",
          options: { translateTime: true }
        }
});

const PORT = process.env.PORT || 4000;
const SEARCH_LIMIT = Number.parseInt(process.env.SEARCH_LIMIT ?? "10", 10);

const app = express();
app.use(cors());
app.use(express.json({ limit: "1mb" }));

app.get("/healthz", (_req, res) => res.json({ status: "ok" }));

app.get("/resolve", async (req, res) => {
  const input = req.query.input;
  if (!input || typeof input !== "string") {
    return res.status(400).json({ error: "input query param required" });
  }

  try {
    const videoId = normaliseVideoId(input);
    if (!videoId) {
      return res.status(400).json({ error: "Invalid YouTube URL or id" });
    }

    const info = await ytdl.getBasicInfo(videoId);
    const meta = parseMetadata(info);
    res.json({ item: meta });
  } catch (err) {
    logger.error({ err, input }, "Failed to resolve input");
    res.status(502).json({ error: "Resolve failed" });
  }
});

app.get("/search", async (req, res) => {
  const query = req.query.q;
  if (!query || typeof query !== "string") {
    return res.status(400).json({ error: "q query param required" });
  }

  try {
    const results = await ytSearch({ query, type: "video" });
    const items = (results.videos ?? results).slice(0, SEARCH_LIMIT).map(parseSearchResult).filter(Boolean);
    res.json({ items });
  } catch (err) {
    logger.error({ err, query }, "Search failed");
    res.status(502).json({ error: "Search failed" });
  }
});

app.listen(PORT, () => {
  logger.info({ port: PORT }, "Stream service up");
});

function normaliseVideoId(input) {
  const trimmed = input.trim();
  if (!trimmed) {
    return null;
  }

  if (ytdl.validateID(trimmed)) {
    return trimmed;
  }

  if (ytdl.validateURL(trimmed)) {
    try {
      return ytdl.getURLVideoID(trimmed);
    } catch (err) {
      logger.warn({ err, input }, "Could not extract video id from URL");
      return null;
    }
  }

  // Basic fallback for raw ids
  return /^[a-zA-Z0-9_-]{11}$/.test(trimmed) ? trimmed : null;
}

function parseMetadata(info) {
  const details = info.videoDetails;
  const durationMs = toDurationMs(details.lengthSeconds);
  const thumbnail =
    selectThumbnail(details.thumbnails) ||
    details.thumbnail_url ||
    details.thumbnails?.[0]?.url ||
    "";

  return {
    videoId: details.videoId,
    title: details.title,
    channel: details.author?.name ?? "Unknown",
    durationMs,
    thumbnailUrl: thumbnail
  };
}

function parseSearchResult(video) {
  if (!video?.videoId) {
    return null;
  }

  return {
    videoId: video.videoId,
    title: video.title ?? "Unknown title",
    channel: video.author?.name ?? video.author?.user ?? "Unknown",
    durationMs: toDurationMs(video.duration?.seconds ?? video.seconds ?? video.timestamp),
    thumbnailUrl: video.image ?? video.thumbnail ?? ""
  };
}

function toDurationMs(value) {
  if (!value) {
    return 0;
  }

  if (typeof value === "number" && Number.isFinite(value)) {
    return Math.max(0, Math.round(value * 1000));
  }

  if (typeof value === "string") {
    const parts = value
      .split(":")
      .map((v) => Number.parseInt(v, 10))
      .filter((v) => !Number.isNaN(v));
    if (parts.length === 0) {
      return 0;
    }
    let seconds = 0;
    for (let i = 0; i < parts.length; i += 1) {
      const power = parts.length - i - 1;
      seconds += parts[i] * Math.pow(60, power);
    }
    return seconds * 1000;
  }

  return 0;
}

function selectThumbnail(thumbnails) {
  if (!Array.isArray(thumbnails) || thumbnails.length === 0) {
    return "";
  }

  const candidates = thumbnails.filter((thumb) => thumb?.url);
  if (candidates.length === 0) {
    return "";
  }

  const best = candidates.reduce((prev, current) => {
    const prevScore = (prev.width ?? 0) * (prev.height ?? 0);
    const currentScore = (current.width ?? 0) * (current.height ?? 0);
    return currentScore > prevScore ? current : prev;
  });

  return best.url ?? "";
}
