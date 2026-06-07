# Agri-RAG API — Setup Guide

> Bilingual agricultural advisor API (Arabic + English).  
> Powered by pgvector, DistilBERT embeddings, and Groq LLaMA 3.3.

---

## What you need before starting

| Requirement | Version | How to check |
|---|---|---|
| Python | 3.10 or higher | `python --version` |
| Docker Desktop | Any recent version | `docker --version` |
| Git | Optional | — |
| Free disk space | ~2 GB | — |

You also need two free accounts (takes 2 minutes each):
- **Groq** (for the AI model): https://console.groq.com
- **ngrok** (to expose the API publicly): https://dashboard.ngrok.com

---

## Files you received

```
agri_rag_clean/
├── src/                  ← all Python source code
├── run_api.py            ← the launcher script
├── requirements.txt      ← Python dependencies
└── README.md             ← this file

agri_db.dump              ← the vector database (keep this separately)
```

---

## Step 1 — Set up the database

This restores 11,971 pre-embedded agricultural knowledge chunks into a local PostgreSQL database. You only do this once.

### 1a. Start the pgvector container

Open PowerShell (or Terminal on Mac/Linux) and run:

```powershell
docker run -d --name agri-pgvector `
    -e POSTGRES_PASSWORD=mincraft `
    -e POSTGRES_DB=agri_db `
    -p 5433:5432 `
    pgvector/pgvector:pg16
```

Wait about 10 seconds for the database to fully boot.

### 1b. Restore the data

Make sure `agri_db.dump` is in your current folder, then run these three commands one by one:

```powershell
docker cp .\agri_db.dump agri-pgvector:/tmp/
```

```powershell
docker exec -it agri-pgvector psql -U postgres -d agri_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

```powershell
docker exec -it agri-pgvector pg_restore -U postgres -d agri_db /tmp/agri_db.dump
```

The restore will take about 30–60 seconds. No output = normal.

### 1c. Verify it worked

```powershell
docker exec -it agri-pgvector psql -U postgres -d agri_db -c "SELECT COUNT(*) FROM knowledge_chunks;"
```

You should see:

```
 count
-------
 11971
(1 row)
```

If you see 11971 — database is ready. ✅  
If you see 0 or an error — do NOT continue, something went wrong in the restore.

---

## Step 2 — Get your API keys

### Groq API key (for the AI model)

1. Go to https://console.groq.com
2. Sign up for a free account
3. Go to **API Keys** → **Create API Key**
4. Copy the key (starts with `gsk_...`)

### ngrok auth token (to expose the API publicly)

1. Go to https://dashboard.ngrok.com
2. Sign up for a free account
3. Go to **Your Authtoken** (left sidebar)
4. Copy the token

---

## Step 3 — Create your `.env` file

Inside the `agri_rag_clean/` folder, create a new file called exactly `.env` (note the dot at the start).

Paste this into it and fill in your own keys:

```
DB_HOST=localhost
DB_PORT=5433
DB_NAME=agri_db
DB_USER=postgres
DB_PASSWORD=mincraft
GROQ_API_KEY=paste_your_groq_key_here
GROQ_MODEL=llama-3.3-70b-versatile
NGROK_AUTH_TOKEN=paste_your_ngrok_token_here
REQUIRE_AUTH=false
```

> ⚠️ Never share this file with anyone. It contains your private API keys.

---

## Step 4 — Install Python dependencies

Open PowerShell inside the `agri_rag_clean/` folder.

### 4a. Create a virtual environment

```powershell
python -m venv venv
```

### 4b. Activate it

**Windows:**
```powershell
venv\Scripts\activate
```

**Mac/Linux:**
```bash
source venv/bin/activate
```

You should see `(venv)` at the start of your terminal line.

### 4c. Install PyTorch first

> PyTorch needs its own special install command — don't skip this step.

**Windows or Linux (no GPU / CPU only):**
```powershell
pip install torch --index-url https://download.pytorch.org/whl/cpu
```

**If you have an NVIDIA GPU:**
```powershell
pip install torch --index-url https://download.pytorch.org/whl/cu121
```

### 4d. Install everything else

```powershell
pip install -r requirements.txt
```

This will take 3–5 minutes. It downloads the embedding model and all dependencies.

---

## Step 5 — Run the API

Make sure:
- Docker Desktop is open and running
- The `agri-pgvector` container is running (`docker start agri-pgvector`)
- Your venv is activated (`(venv)` shows in terminal)

Then run:

```powershell
python run_api.py
```

You should see output like this:

```
🔥 Warming up embedding model...
✅ Model ready. API is live.

======================================================================
🌾 Agri-RAG API is now live!
======================================================================
Local:   http://127.0.0.1:XXXXX
Public:  https://abc123-def456.ngrok-free.app

SEND TO YOUR TEAM:
  URL:     https://abc123-def456.ngrok-free.app/ask
======================================================================
```

**Copy the public ngrok URL** — this is what your backend calls.

> ⚠️ The ngrok URL changes every time you restart the script. You'll need to update it in your backend each time.

> ⚠️ Keep this terminal window open while the API is in use. Closing it disconnects everything.

---

## Step 6 — Test it works

Open a **second** PowerShell window (keep the first one running), activate venv, then run:

```powershell
python test_api.py
```

When prompted, paste the ngrok URL from Step 5.

Expected output:

```
🩺 Health check...
   Status: 200

🌾 Asking English question...
   Status: 200
   Language: en
   Confidence: high
   Answer: Based on the available sources...

🌾 Asking Arabic question...
   Language: ar
   Answer: وفقاً للمصادر المتاحة...

🔒 Testing auth rejection (should be 401)...
   Status: 200  ← (auth is disabled, this is expected)
```

If you see Status 200 on the first two — the API is working end to end. ✅

---

## API Reference

### Health check

```
GET /health
```

No headers needed. Returns:
```json
{"status": "ok", "service": "agri-rag", "version": "1.0.0"}
```

---

### Ask a question

```
POST /ask
Content-Type: application/json
```

**Request body:**

```json
{
  "Question": "What is the ideal soil pH for tomatoes?",
  "sensor_data": {
    "Temperature": 34.2,
    "Soil_Ph": 5.3,
    "Soil_Moisture": 18,
    "Nitrogen": 12,
    "Phosphorus": 8,
    "Potassium": 85,
    "Rainfall_mm": 0,
    "Farm_Location": "Cairo, Egypt",
    "Crop_Type": "tomato",
    "Alerts": [
      {
        "severity": "high",
        "sensor": "soil_ph",
        "message": "Soil pH below safe range"
      }
    ]
  }
}
```

All `sensor_data` fields are optional — send what you have. Minimum valid request:

```json
{
  "Question": "How do I treat nitrogen deficiency?",
  "sensor_data": {}
}
```

**Response:**

```json
{
  "answer": "Based on the available sources...",
  "language": "en",
  "confidence": "high",
  "top_similarity": 0.721,
  "sources": [
    {
      "source_document": "FAO Greenhouse Vegetables (Mediterranean)",
      "page_number": 359,
      "similarity": 0.721
    }
  ],
  "processing_time_ms": 2150
}
```

**Field notes:**

| Field | Values | Meaning |
|---|---|---|
| `language` | `"en"` or `"ar"` | Auto-detected from the question |
| `confidence` | `"high"`, `"medium"`, `"low"`, `"none"` | Based on retrieval similarity scores |
| `top_similarity` | 0.0 – 1.0 | Higher = more relevant sources found |
| `processing_time_ms` | integer | Includes embedding + retrieval + LLM call |

**Arabic questions work automatically:**

```json
{
  "Question": "ما هي أفضل درجة حموضة التربة لزراعة الطماطم؟",
  "sensor_data": { "Soil_Ph": 5.3, "Crop_Type": "tomato", "Alerts": [] }
}
```

---

## Sensor data field reference

| JSON field | Unit | Description |
|---|---|---|
| `Temperature` | °C | Air temperature |
| `Soil_Ph` | 0–14 | Soil acidity/alkalinity |
| `Soil_Moisture` | % | Volumetric water content |
| `Nitrogen` | mg/kg | Soil nitrogen level |
| `Phosphorus` | mg/kg | Soil phosphorus level |
| `Potassium` | mg/kg | Soil potassium level |
| `Rainfall_mm` | mm | **Instantaneous reading** from rain gauge — not a cumulative total |
| `Farm_Location` | string | e.g. `"Cairo, Egypt"` |
| `Crop_Type` | string | e.g. `"tomato"`, `"wheat"`, `"peach"` |
| `Alerts` | array | Active sensor alerts (see format below) |

**Alert format:**
```json
{
  "severity": "high",
  "sensor": "soil_ph",
  "message": "pH below safe range for tomatoes"
}
```
`severity` values: `"high"`, `"medium"`, `"low"`

---

## Troubleshooting

### "NGROK_AUTH_TOKEN missing from .env"
Your `.env` file is missing or the token field is empty. Check that:
- The file is named `.env` (not `env.txt` or `.env.txt`)
- It's inside the `agri_rag_clean/` folder
- The `NGROK_AUTH_TOKEN` line has your actual token

### "ModuleNotFoundError"
Your venv isn't activated. Run `venv\Scripts\activate` first.

### Database connection error
The pgvector container isn't running. Run:
```powershell
docker start agri-pgvector
```

### "count = 0" after restore
The dump restore failed silently. Re-run Step 1b from scratch. Make sure the container was fully booted before you ran `docker cp`.

### API returns 500 error
Check the terminal running `run_api.py` — it prints the full traceback. Most likely cause: Groq API key is wrong or expired.

### ngrok URL stopped working
Your laptop went to sleep or the terminal was closed. Re-run `python run_api.py` and update the URL in your backend.

---

## Typical response times

| Phase | Time |
|---|---|
| Embedding the query | ~100ms |
| Vector search (HNSW) | ~10ms |
| Groq LLM call | ~1500–3000ms |
| **Total** | **~2–4 seconds** |

Response time depends mostly on Groq's servers. From Egypt, expect 2–4 seconds per request under normal conditions.

---

## Knowledge base coverage

The system has 11,971 chunks from 17 agricultural sources:

| Language | Sources | Chunks |
|---|---|---|
| English | FAO, UGA, Penn State, WSU, Purdue, UMD, Midwest Extension | ~9,900 |
| Arabic | ESCWA, AOAD | ~701 |

Topics covered: soil pH, NPK fertilization, irrigation, pest management, crop-specific guides (tomatoes, fruit trees, vegetables), organic farming, greenhouse production.

Out-of-scope questions (e.g. "what is the capital of France?") are automatically detected and refused with a polite message — the system will not hallucinate answers outside its knowledge domain.
