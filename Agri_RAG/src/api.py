"""
FastAPI server exposing the agri_rag system to remote clients.
Runs on your laptop, exposed via ngrok to the public internet.
"""
import os
import time
from typing import Optional
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException, Header
from pydantic import BaseModel, Field
from dotenv import load_dotenv
from pydantic import ConfigDict

# Load env from project root (one level up from src/)
load_dotenv(os.path.join(os.path.dirname(__file__), "..", ".env"))

from generator import answer_question
from embedder import get_model  # for warmup


# ══════════════════════════════════════════════════════════
# Request / Response Schemas
# ══════════════════════════════════════════════════════════


class SensorAlert(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    severity: str
    sensor: str
    message: str


class SensorData(BaseModel):
    """Sensor readings. Accepts both snake_case and PascalCase keys."""
    model_config = ConfigDict(populate_by_name=True)
    
    temperature: Optional[float] = Field(None, alias="Temperature")
    soil_ph: Optional[float] = Field(None, alias="Soil_Ph")
    soil_moisture: Optional[float] = Field(None, alias="Soil_Moisture")
    nitrogen: Optional[float] = Field(None, alias="Nitrogen")
    phosphorus: Optional[float] = Field(None, alias="Phosphorus")
    potassium: Optional[float] = Field(None, alias="Potassium")
    rainfall_mm: Optional[float] = Field(None, alias="Rainfall_mm")
    farm_location: Optional[str] = Field(None, alias="Farm_Location")
    crop_type: Optional[str] = Field(None, alias="Crop_Type")
    alerts: list[SensorAlert] = Field(default_factory=list, alias="Alerts")


class AskRequest(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    
    query: str = Field(..., min_length=1, alias="Question")
    sensor_data: SensorData = Field(default_factory=SensorData, alias="sensor_data")


class SourceInfo(BaseModel):
    source_document: str
    page_number: int
    similarity: float


class AskResponse(BaseModel):
    answer: str
    language: str
    confidence: str
    top_similarity: float
    sources: list[SourceInfo]
    processing_time_ms: int


# ══════════════════════════════════════════════════════════
# App setup — with model warmup on startup
# ══════════════════════════════════════════════════════════

@asynccontextmanager
async def lifespan(app: FastAPI):
    print("🔥 Warming up embedding model...")
    get_model()  # load the sentence transformer once, not per-request
    print("✅ Model ready. API is live.")
    yield
    print("👋 Shutting down.")


app = FastAPI(
    title="Agri-RAG API",
    description="Bilingual agricultural advisor with sensor integration",
    version="1.0.0",
    lifespan=lifespan,
)




# ══════════════════════════════════════════════════════════
# Routes
# ══════════════════════════════════════════════════════════

@app.get("/health")
def health():
    """Liveness check — your friend's backend can poll this to verify connectivity."""
    return {"status": "ok", "service": "agri-rag", "version": "1.0.0"}


@app.post("/ask", response_model=AskResponse)
def ask(request: AskRequest):
    """Main endpoint — takes a farmer's question + sensor data, returns a grounded answer."""
    start = time.time()
    
    try:
        sensor_dict = request.sensor_data.model_dump()
        sensor_dict["alerts"] = [a for a in sensor_dict.get("alerts", [])]
        
        result = answer_question(request.query, sensor_dict)
        elapsed_ms = int((time.time() - start) * 1000)
        
        return AskResponse(
            answer=result["answer"],
            language=result.get("language", "en"),
            confidence=result.get("confidence", "unknown"),
            top_similarity=result.get("top_similarity", 0.0),
            sources=[SourceInfo(**s) for s in result.get("sources", [])],
            processing_time_ms=elapsed_ms,
        )
    except Exception as e:
        import traceback
        print(f"❌ Error in /ask:")
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=f"Internal error: {str(e)}")