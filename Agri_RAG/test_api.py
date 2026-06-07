"""
Test the API locally before sharing with your friend.
Run this in a SEPARATE terminal while run_api.py is running.
"""
import os
import requests
import json
from dotenv import load_dotenv

load_dotenv()

# Change this to the ngrok URL printed by run_api.py
BASE_URL = input("Paste your ngrok URL (e.g. https://cherty-mimetic-julienne.ngrok-free.dev/ask): ").strip().rstrip("/")

API_SECRET = os.getenv("API_SECRET")
headers = {"Authorization": f""}

# Test 1: Health check (no auth needed)
print("\n🩺 Health check...")
r = requests.get(f"{BASE_URL}/health")
print(f"   Status: {r.status_code}")
print(f"   Body: {r.json()}")

# Test 2: Ask endpoint — English query with sensor data
print("\n🌾 Asking English question...")
payload = {
    "query": "My tomato leaves are yellowing, what's wrong?",
    "sensor_data": {
        "temperature": 34.2,
        "soil_ph": 5.3,
        "soil_moisture": 18,
        "nitrogen": 12,
        "farm_location": "Cairo, Egypt",
        "crop_type": "tomato",
        "alerts": [
            {"severity": "high", "sensor": "soil_ph", "message": "Low soil pH (5.3)"},
            {"severity": "medium", "sensor": "nitrogen", "message": "Low nitrogen"}
        ]
    }
}
r = requests.post(f"{BASE_URL}/ask", headers=headers, json=payload, timeout=60)
print(f"   Status: {r.status_code}")
if r.status_code == 200:
    result = r.json()
    print(f"   Language: {result['language']}")
    print(f"   Confidence: {result['confidence']} (top similarity: {result['top_similarity']})")
    print(f"   Processing: {result['processing_time_ms']}ms")
    print(f"   Answer: {result['answer'][:300]}...")
    print(f"   Top source: {result['sources'][0]['source_document']}")
else:
    print(f"   Error: {r.text}")

# Test 3: Arabic query
print("\n🌾 Asking Arabic question...")
payload["query"] = "ما هي أفضل درجة حموضة التربة لزراعة الطماطم؟"
r = requests.post(f"{BASE_URL}/ask", headers=headers, json=payload, timeout=60)
if r.status_code == 200:
    result = r.json()
    print(f"   Language: {result['language']}")
    print(f"   Answer (first 300 chars): {result['answer'][:300]}")

# Test 4: Missing auth (should fail)
print("\n🔒 Testing auth rejection (should be 401)...")
r = requests.post(f"{BASE_URL}/ask", json=payload, timeout=10)
print(f"   Status: {r.status_code} ({'✅ correctly rejected' if r.status_code == 401 else '⚠️ unexpected'})")