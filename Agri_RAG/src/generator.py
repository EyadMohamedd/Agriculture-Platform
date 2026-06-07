from groq import Groq
from langdetect import detect, LangDetectException
from typing import Optional
from config import GROQ_API_KEY, GROQ_MODEL
from retriever import search

MIN_SIMILARITY_THRESHOLD = 0.45
MIN_STRONG_MATCHES = 2
# Configure Groq once at module load
_client = Groq(api_key=GROQ_API_KEY)


def detect_language(text: str) -> str:
    """Detect 'ar' or 'en' from the query text. Defaults to 'en'."""
    try:
        lang = detect(text)
        return "ar" if lang == "ar" else "en"
    except LangDetectException:
        return "en"


def build_sensor_context(sensor_data: dict, language: str = "en") -> str:
    """
    Format sensor readings + active alerts into a natural-language block
    that the LLM can reason over.
    
    Expected shape (matches the API's flat structure after Pydantic aliasing):
    {
        "temperature": 34.2,
        "soil_ph": 5.3,
        "soil_moisture": 18,
        "nitrogen": 12,
        "phosphorus": 8,
        "potassium": 85,
        "rainfall_mm": 0,              # instantaneous reading
        "farm_location": "Cairo, Egypt",
        "crop_type": "tomato",
        "alerts": [
            {"severity": "high", "sensor": "soil_ph", "message": "..."},
        ]
    }
    """
    if language == "ar":
        header = "البيانات الحالية للمزرعة:"
        readings_label = "قراءات المستشعرات:"
        alerts_label = "التنبيهات النشطة:"
        no_alerts = "لا توجد تنبيهات نشطة."
    else:
        header = "Current farm state:"
        readings_label = "Sensor readings:"
        alerts_label = "Active alerts:"
        no_alerts = "No active alerts."
    
    # Helper: only add a line if the value actually exists
    def fmt(label: str, value, unit: str = "") -> Optional[str]:
        if value is None or value == "":
            return None
        return f"- {label}: {value}{(' ' + unit) if unit else ''}"
    
    lines = [header]
    
    # Farm metadata (top-level fields from API)
    farm_meta = [
        fmt("Crop", sensor_data.get("crop_type")),
        fmt("Location", sensor_data.get("farm_location")),
    ]
    lines.extend(line for line in farm_meta if line)
    lines.append("")
    
    # Sensor readings (also top-level in the API schema)
    lines.append(readings_label)
    readings_lines = [
        fmt("Temperature", sensor_data.get("temperature"), "°C"),
        fmt("Soil pH", sensor_data.get("soil_ph")),
        fmt("Soil moisture", sensor_data.get("soil_moisture"), "%"),
        fmt("Nitrogen (N)", sensor_data.get("nitrogen"), "mg/kg"),
        fmt("Phosphorus (P)", sensor_data.get("phosphorus"), "mg/kg"),
        fmt("Potassium (K)", sensor_data.get("potassium"), "mg/kg"),
    ]
    lines.extend(line for line in readings_lines if line)
    
    # Rainfall gets its own explicit clarification since semantics matter
    if sensor_data.get("rainfall_mm") is not None:
        lines.append(
            f"- Rainfall: {sensor_data['rainfall_mm']} mm "
            f"(instantaneous reading, not a cumulative total)"
        )
    
    lines.append("")
    lines.append(alerts_label)
    
    # Alerts — handle both dict and Pydantic-model cases defensively
    alerts = sensor_data.get("alerts", [])
    if alerts:
        for a in alerts:
            # Support both dict-style and attribute-style access
            if isinstance(a, dict):
                severity = a.get("severity", "info")
                sensor = a.get("sensor", "unknown")
                message = a.get("message", "")
            else:
                severity = getattr(a, "severity", "info")
                sensor = getattr(a, "sensor", "unknown")
                message = getattr(a, "message", "")
            lines.append(f"- [{severity.upper()}] {sensor}: {message}")
    else:
        lines.append(no_alerts)
    
    return "\n".join(lines)


def build_prompt(
    question: str,
    sensor_context: str,
    retrieved_chunks: list[dict],
    language: str = "en",
) -> str:
    """Construct the final prompt to send to Gemini."""
    
    # Format retrieved knowledge with source attribution
    knowledge_block = []
    for i, chunk in enumerate(retrieved_chunks, start=1):
        knowledge_block.append(
            f"[Source {i}: {chunk['source_document']}, page {chunk['page_number']}]\n"
            f"{chunk['chunk_text']}"
        )
    knowledge_text = "\n\n".join(knowledge_block)
    
    if language == "ar":
        system_instructions = """أنت مستشار زراعي خبير تساعد مزارعًا في الوقت الفعلي. المزارع يعتمد على نصيحتك - ليس لديه وصول إلى خبراء آخرين.

مهمتك:
1. قدم توصيات واضحة وقابلة للتنفيذ بناءً على بيانات المستشعرات والمصادر المرفقة
2. كن محددًا - قل "أضف كمية X من Y" وليس "فكر في إضافة شيء ما"
3. اربط كل نصيحة بقراءات المستشعرات الفعلية للمزارع
4. اذكر رقم المصدر عند الاستشهاد (مثلاً: وفقاً للمصدر 2)

قواعد صارمة:
- لا تقل للمزارع أبداً "استشر خبيراً" أو "اطلب مساعدة متخصصة" - أنت الخبير الذي استشاره
- لا ترفض تقديم توصية إذا كانت المصادر تحتوي على أي معلومات ذات صلة - اجمع ما هو متاح في أفضل إجابة ممكنة
- إذا كانت المصادر لا تغطي الموضوع بالفعل، قل "المصادر المتاحة لا تتناول هذا مباشرة، ولكن بناءً على المعلومات ذات الصلة..." وقدم أفضل نصيحة مدروسة مما هو متاح
- استخدم فقط المعلومات من بيانات المزرعة والمصادر المرفقة - لا تخترع أرقاماً أو حقائق محددة
- كن مباشراً وعملياً، مثل مهندس زراعي متمرس يتحدث مع مزارع في حقله
- ركّز الإجابة - بدون تحفظات طويلة أو تردد"""
        
        template = f"""{system_instructions}

---
{sensor_context}
---

المعرفة الزراعية المتاحة:
{knowledge_text}

---

سؤال المزارع: {question}

إجابتك:"""
    else:
        system_instructions = """You are an expert agricultural advisor helping a farmer in real-time. The farmer relies on YOUR advice — they do NOT have access to other experts.

Your job:
1. Give clear, actionable recommendations based on the farmer's sensor data and the provided sources
2. Be specific — say "add X amount of Y" not "consider adding something"
3. Connect every piece of advice to the farmer's actual sensor readings
4. Cite source numbers when referencing specific information (e.g., "According to Source 2...")

STRICT RULES:
- NEVER tell the farmer to "consult an expert," "seek professional help," or "get additional information from specialists" — YOU are the expert they consulted
- NEVER refuse to give a recommendation if the sources contain ANY relevant information — synthesize what's available into the best possible answer
- If the sources genuinely don't cover the topic, say "The available sources don't directly address this, but based on the related information..." and still provide your best reasoned advice from what IS available
- Use ONLY information from the farm data and provided sources — do not invent specific numbers or facts
- Be direct and practical, like an experienced agronomist talking to a farmer in their field
- Keep answers focused — no long disclaimers, no hedging
- If the user's question is about a crop different from the one being monitored
- by sensors, answer the question based on the retrieved sources and only mention sensor readings if they are directly relevant to the question.
"""
        
        template = f"""{system_instructions}

---
{sensor_context}
---

Available agricultural knowledge:
{knowledge_text}

---

Farmer's question: {question}

Your answer:"""
    
    return template

LOW_CONFIDENCE_MESSAGES = {
    "en": (
        "I don't have strong source material to answer this question reliably. "
        "This could mean the question is outside the scope of our agricultural "
        "knowledge base (FAO, UGA, Penn State Extension, ESCWA/AOAD Arabic sources). "
        "I'd recommend consulting a local agricultural extension agent for a confident answer."
    ),
    "ar": (
        "لا تتوفر لدي مصادر قوية كافية للإجابة على هذا السؤال بشكل موثوق. "
        "قد يعني هذا أن السؤال خارج نطاق قاعدة المعرفة الزراعية المتاحة "
        "(FAO، UGA، Penn State Extension، المصادر العربية ESCWA/AOAD). "
        "أوصي باستشارة مرشد زراعي محلي للحصول على إجابة موثوقة."
    ),
}
def answer_question(
    question: str,
    sensor_data: dict,
    top_k: int = 5,
) -> dict:
    """
    Main RAG function: take a farmer's question + their farm's sensor data,
    return a grounded answer.
    
    Returns:
        {
            "answer": str,
            "language": "en" or "ar",
            "sources": list of retrieved chunks (for traceability),
        }
    """
    # 1. Detect language
    language = detect_language(question)
    
    # 2. Retrieve relevant chunks (optionally filter by language later when Arabic corpus exists)
    chunks = search(query=question, top_k=top_k)
    
    if not chunks:
         return {
        "answer": LOW_CONFIDENCE_MESSAGES[language],
        "language": language,
        "sources": [],
        "confidence": "none",
        "reason": "No sources retrieved",
        }
        
    top_similarity = max(s["similarity"] for s in chunks)
    strong_matches = sum(1 for s in chunks if s["similarity"] >= MIN_SIMILARITY_THRESHOLD)

    if strong_matches < MIN_STRONG_MATCHES:
        return {
        "answer": LOW_CONFIDENCE_MESSAGES[language],
        "language": language,
        "sources": chunks,
        "confidence": "low",
        "top_similarity": round(top_similarity, 3),
        "strong_matches": strong_matches,
        "reason": f"Only {strong_matches} chunks above threshold {MIN_SIMILARITY_THRESHOLD} (need {MIN_STRONG_MATCHES})",
    }
    if top_similarity < MIN_SIMILARITY_THRESHOLD:
        return {
        "answer": LOW_CONFIDENCE_MESSAGES[language],
        "language": language,
        "sources": chunks,  
        "confidence": "low",
        "top_similarity": round(top_similarity, 3),
        "threshold": MIN_SIMILARITY_THRESHOLD,
        "reason": f"Top similarity {top_similarity:.3f} below threshold {MIN_SIMILARITY_THRESHOLD}",
            }
    # 3. Build sensor context
    sensor_context = build_sensor_context(sensor_data, language=language)
    
    # 4. Build the full prompt
    prompt = build_prompt(question, sensor_context, chunks, language=language)
    
# 5. Call Groq
    response = _client.chat.completions.create(
        model=GROQ_MODEL,
        messages=[
            {"role": "user", "content": prompt}
        ],
        temperature=0.2,  
        max_tokens=1024,
    )
    answer = response.choices[0].message.content.strip()
    
    return {
        "answer": answer,
        "language": language,
        "sources": [
            {
                "source_document": c["source_document"],
                "page_number": c["page_number"],
                "similarity": c["similarity"],
            }
            for c in chunks
        ],
        "confidence": "high" if top_similarity >= 0.65 else "medium",
        "top_similarity": round(top_similarity, 3),
    }


if __name__ == "__main__":
    # Simulated sensor data (later this comes from your friend's backend)
    fake_sensor_data = {
        "crop_type": "tomato",
        "farm_location": "Cairo, Egypt",
        "temperature": 38.5,
        "soil_ph": 5.1,
        "soil_moisture": 22,
        "nitrogen": 15,
        "phosphorus": 40,
        "potassium": 180,
        "rainfall_mm": 0,
        "alerts": [
            {"severity": "high", "sensor": "soil_ph", "message": "pH below safe range for tomatoes"},
            {"severity": "medium", "sensor": "soil_moisture", "message": "Moisture low"},
        ],
    }
    
    test_questions = [
        "My pH reading is low, what should I do?",
        "Is my soil moisture level safe for tomatoes right now?",
        "The temperature is high — will my crop survive?",
    ]
    
    for q in test_questions:
        print(f"\n{'='*70}")
        print(f"❓ {q}")
        print('='*70)
        
        result = answer_question(q, fake_sensor_data)
        
        print(f"\n🌐 Language: {result['language']}")
        print(f"\n💬 Answer:\n{result['answer']}")
        print(f"\n📚 Sources:")
        for s in result["sources"]:
            print(f"   - {s['source_document']} (p.{s['page_number']}, sim={s['similarity']:.3f})")