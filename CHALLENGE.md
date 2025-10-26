# 🎧 Pulse Jukebox — Hackathon Challenge ⚡️🔥

Once upon a hackathon…  
A team of developers found a mysterious repo called **Neon Pulse Jukebox** — a party playlist platform that looked amazing on paper but refused to start. 🧩💥  
The containers were scrambled, the builds were broken, and the lights were off.  

Your mission?  
**Fix the stack.**  
**Make the lights blink.**  
**Bring the party back to life.** 🎶

---

## 💿 What You’re Working With
Pulse Jukebox is a **hackathon-ready, containerized party playlist platform** made up of six services that need a little TLC to work together:

- **web** → Vite + React SPA (served by nginx) 🎨  
- **api** → ASP.NET Core (REST + SignalR) ⚙️  
- **stream** → Node/Express + `yt-dlp` + `ffmpeg` 📺  
- **proxy** → nginx router for `/`, `/api`, `/hub`, `/stream`, `/hls` 🌐  
- **mongo** → MongoDB 7 🗃️  
- **redis** → Redis 7 (cache + rate limiting) ⚡  

Everything runs on one shared bridge network.  
Only the **proxy** exposes ports to your host — it’s the club bouncer guarding the front door. 🚪🎧  

> 💡 *Goal:* Learn how to wire up real services, fix broken containers, and deploy like a pro. No magic buttons — just curiosity and caffeine.

---

## 🍴 Step Zero — Fork It
Before you do anything wild, fork **`jpe230/jukebox`** into **your own GitHub account.**  
No PRs straight to main. Forks first, chaos later. 😏

---

## ⚠️ House Rules (Read Before You Ship)
- 🧩 **No `.env` files allowed.**  
  You **must hardcode** all environment variables directly inside `docker-compose.yml` under each service’s `environment:` block.
- 🤖 **Copilot rules:**  
  You can use **GitHub Copilot in “Ask” mode only** with **GPT-4o**.  
  - ✅ “Ask” mode is fine — get hints, snippets, or explanations.  
  - ❌ Using “Agent Mode” or any other model = instant disqualification.  
  - ❌ Other AI assistants or agents = nope.  
  This is *your* challenge, not the AI’s victory lap. 🧠💥
- 👌 **Ask first, code later**  
  You can ask to organizers, don't be shy
- 📦 **Keep your commits clean.**  
  No `.env` uploads, no sensitive info, no lazy “fix everything” commits.

Example:
```yaml
services:
  api:
    environment:
      ASPNETCORE_URLS: http://+:5000
      MONGO_CONNECTION: mongodb://mongo:27017/jukebox
      REDIS_CONNECTION: redis:6379
```

---

## 🧠 Expected Architecture (TL;DR)
| Service | Tech | Purpose | 
|----------|------|----------|
| **web** | React + nginx | SPA |
| **api** | ASP.NET Core | REST + SignalR |
| **stream** | Node + ffmpeg | Media stream |
| **proxy** | nginx | Router for all |
| **mongo** | MongoDB 7 | Persistent DB |
| **redis** | Redis 7 | Cache + rate limit |

🕸️ Everything on one Docker network
🌐 Only **proxy:80** is public

---

## 🛠️ Local Setup (Your Redemption Arc)
1. **Clone your fork** and create a feature branch.  
2. **Fix what’s broken:**
   - Some Dockerfiles are *intentionally sabotaged*. Rebuild them with **multi-stage** builds.  
   - Compare your `docker-compose.yml` against the architecture above and restore missing services, envs, ports, or healthchecks.  
   - Align internal ports across all configs — misalignment breaks routing faster than you think.  
3. **Hardcode your env vars** in `docker-compose.yml` (per the rules).  
4. **Build and run** using Podman (or Docker, if needed).  
5. **Tail the logs** until every container goes green 🟢 and the SPA serves through the proxy.  

> 🎉 Bonus: document your journey — screenshots, notes, or a mini timeline of “what broke and how you fixed it” are gold.

---

## 🧩 Validation Time 🔍
Prove it works:
- Show **screenshots** or **terminal logs** that confirm:
  - ✅ SPA loads via the proxy  
  - ✅ API endpoints respond
  - ✅ Only the proxy is exposed under port **80**
- Add a quick write-up in your PR:
  - What went wrong?  
  - What did you fix?  
  - What did you learn?

---

## 🚀 Ship It (PR Flow)
1. Push your branch to your fork.  
2. Open a **Pull Request** back to `jpe230/jukebox`.  
3. In your PR description:
   - 📸 Include proof (screenshots/logs) that everything runs end-to-end.  
   - 🧩 Add a short summary of your debugging and decisions.  
4. ✅ **Definition of Done:**  
   Your PR triggers a **successful PullPreview deployment.**

---

## 🧭 Tips for Cool Hackers
- 💚 Add healthchecks — fastest way to spot bad containers.  
- 🔗 Keep your service names and aliases consistent.  
- 🧹 Organize your Compose file; neat YAML = happy reviewers.  
- 🧍 Remember: containers are just computers with stage fright.

---

### 🌈 Epilogue
The repo started quiet. The logs were red.  
But line by line, you brought rhythm back to the code.  
Now the lights pulse, the beats loop, and the jukebox lives again.  

**Good luck, builder.** You got this. 🧠💪🎶  
*Now go make the lights blink.* ✨
