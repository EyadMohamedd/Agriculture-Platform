"""
Launches the FastAPI server on your laptop and exposes it to the public
internet via ngrok. Share the printed URL with your friend.
"""
import os
import sys
import socket
import threading
import time
from pathlib import Path

import uvicorn
from pyngrok import ngrok, conf
from dotenv import load_dotenv

# Add src/ to path so we can import api.py
sys.path.insert(0, str(Path(__file__).parent / "src"))

load_dotenv()

NGROK_TOKEN = os.getenv("NGROK_AUTH_TOKEN", "").strip()
API_SECRET = os.getenv("API_SECRET", "").strip()

if not NGROK_TOKEN:
    raise SystemExit("❌ NGROK_AUTH_TOKEN missing from .env")
if not API_SECRET:
    raise SystemExit("❌ API_SECRET missing from .env")


def find_free_port() -> int:
    s = socket.socket()
    s.bind(("", 0))
    port = s.getsockname()[1]
    s.close()
    return port


def main():
    port = find_free_port()
    
    # Configure ngrok
    conf.get_default().auth_token = NGROK_TOKEN
    tunnel = ngrok.connect(port, "http")
    public_url = tunnel.public_url
    
    # Force https (ngrok sometimes gives http by default; https is what you want)
    if public_url.startswith("http://"):
        public_url = public_url.replace("http://", "https://", 1)
    
    print("\n" + "=" * 70)
    print("🌾 Agri-RAG API is now live!")
    print("=" * 70)
    print(f"Local:   http://127.0.0.1:{port}")
    print(f"Public:  {public_url}")
    print()
    print("SEND TO YOUR FRIEND:")
    print(f"  URL:     {public_url}/ask")
    print(f"  Header:  Authorization: Bearer {API_SECRET}")
    print()
    print("Health check:")
    print(f"  curl {public_url}/health")
    print("=" * 70)
    print("\n⚠️  Keep this window open! Closing it will disconnect your friend.")
    print("Press Ctrl+C to stop.\n")
    
    # Run uvicorn in the foreground (not a thread — we want to block here)
    uvicorn.run(
        "api:app",
        host="0.0.0.0",
        port=port,
        log_level="info",
    )


if __name__ == "__main__":
    main()