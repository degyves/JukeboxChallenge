# 🎧 Pulse Jukebox

**Pulse Jukebox** is a web-only, containerized party jukebox that turns any browser into a collaborative music room.  
A **host** opens a room, **guests** join via PIN or QR code, everyone drops YouTube links, and the **top-voted track** plays — HLS-streamed directly on the host’s browser.

Everything updates live over **SignalR**, and the entire stack runs seamlessly in **Docker**.

---

## 🚀 Tech Stack Overview

| Service | Description |
|----------|-------------|
| **web** | React + Vite + Tailwind — cyberpunk-themed SPA with Zustand store, SignalR client, and HLS.js playback. |
| **api** | ASP.NET Core 8 REST API + SignalR hub, MongoDB persistence, Redis-based rate limiting. |
| **stream** | Node.js + Express wrapper around `yt-dlp` + `ffmpeg` for resolving videos and generating HLS segments. |
| **proxy** | nginx reverse proxy that fronts the SPA, API, SignalR hub, and serves `/hls` from a shared volume. |
| **mongo / redis** | MongoDB for state, Redis for caching and throttling — all orchestrated with Docker Compose. |

### 🕸️ Architecture Diagram

```
Guests ─▶ proxy (80) ─▶ web SPA
       └─────────────▶ api ─▶ mongo / redis
       └─────────────▶ stream ─▶ shared HLS volume ─▶ proxy /hls ─▶ host (HLS.js)
```

---

## 🧩 Getting Started

### 1️⃣ Prerequisites
- Docker + Docker Compose v2
- Open ports: `8080`, `4000`, and `5173`

### 2️⃣ Bootstrap the stack
```bash
docker compose up --build
```
Then visit 👉 [http://localhost:8080](http://localhost:8080) to create your first room.

### 3️⃣ Runtime Flow
- The **host** creates a room and keeps their tab open (only the host plays audio).
- **Guests** join via PIN or QR code.
- Guests paste YouTube URLs or search; votes reorder the queue live.

---

## 💻 Local Development

### 🧠 API (`/api`)
```bash
cd api
dotnet restore
ASPNETCORE_URLS=http://localhost:8080 dotnet run
```
Environment variables (see `.env.example`):
```
Mongo__ConnectionString
Mongo__Database
Redis__ConnectionString
Stream__BaseUrl
CORS_ORIGINS
```

---

### 🌐 Web (`/web`)
```bash
cd web
npm install
npm run dev   # http://localhost:5173
```
Set `VITE_API_BASE` and `VITE_HUB_URL` to point to your API.

---

### 🎬 Stream (`/stream`)
```bash
cd stream
npm install
npm run dev
```
Requires `yt-dlp` and `ffmpeg` installed locally.  
Environment:
```
API_BASE_URL
HLS_ROOT
```

---

## 🧪 Testing

### API
Runs integration tests with Mongo2Go, stubbed stream, and rate limiting:
```bash
dotnet test
```

### Web
Lint the front end:
```bash
cd web
npm run lint
```

---

## ⚙️ API Highlights

| Endpoint | Description |
|-----------|--------------|
| `POST /api/rooms` | Create a room (returns code, host secret, host user ID) |
| `POST /api/rooms/{code}/join` | Join a room |
| `GET /queue`, `POST /tracks`, `POST /tracks/{id}/vote`, `DELETE /tracks/{id}` | Manage the queue (host header `x-host-secret` required for deletes) |
| `POST /play`, `/pause`, `/seek`, `/next` | Playback controls |
| `POST /api/ingest/hls-ready` | Stream service webhook for ready HLS segments |
| **SignalR Hub `/hub/rooms`** | Real-time events: `RoomSync`, `TrackAdded`, `QueueUpdated`, `PlaybackState`, etc. |

---

## 📎 Notes & Assumptions

- 🎧 Only the host browser plays audio — no automatic failover.
- 🚦 Rate limits: 5 track adds/min and 30 votes/min per user (Redis-keyed).
- 📂 HLS segments stored under `/var/media/<room>/<track>`.
- 🔍 Search modal talks directly to the Stream service (user-facing toasts on errors).
- 🧱 MongoDB ensures indexes (unique room code) on API startup.

---

### 🪩 Built for parties. Tuned for engineers.
Real-time beats, cyberpunk vibes, and fully containerized chaos.
