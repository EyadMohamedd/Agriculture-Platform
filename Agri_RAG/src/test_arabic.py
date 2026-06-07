import io
from generator import answer_question

# Simulated sensor readings — farm in Cairo, stressed tomato crop
fake_sensor_data = {
    "temperature": 34.2,
    "soil_ph": 5.3,
    "soil_moisture": 18,
    "nitrogen": 12,
    "phosphorus": 8,
    "potassium": 85,
    "rainfall_last_7_days": 0,
    "farm_location": "Cairo, Egypt",
    "crop_type": "tomato",
    "alerts": [
        {
            "severity": "high",
            "sensor": "soil_ph",
            "message": "Low soil pH detected (5.3, target 6.0-6.8)",
        },
        {
            "severity": "high",
            "sensor": "soil_moisture",
            "message": "Low soil moisture (18%, target 40-60%)",
        },
        {
            "severity": "medium",
            "sensor": "nitrogen",
            "message": "Low nitrogen levels detected",
        },
    ],
}

queries_en = [
    "My tomato leaves are yellowing, what's wrong?",
    "What pH is ideal for growing tomatoes?",
    "How should I fertilize my peach trees?",
]

queries_ar = [
    "ما هي أفضل درجة حموضة التربة لزراعة الطماطم؟",
    "كيف أعالج نقص النيتروجين في المحاصيل؟",
    "ما هي أفضل الممارسات لزراعة الخضروات؟",
]

out_of_domain_queries = [
    "What is the capital of France?",
    "How do I train a neural network?",
    "ما هو أفضل هاتف ذكي في 2026؟",  
    "Recipe for chocolate cake",
]

with io.open("../test_output.txt", "w", encoding="utf-8") as f:
    for q in queries_en + queries_ar + out_of_domain_queries:
        f.write(f"\n{'='*70}\n")
        f.write(f"Q: {q}\n")
        f.write('='*70 + '\n')
        try:
            result = answer_question(q, fake_sensor_data)
            
            # Confidence indicator
            conf = result.get("confidence", "unknown")
            top_sim = result.get("top_similarity", "n/a")
            
            f.write(f"\n[Language: {result.get('language', '?')}]")
            f.write(f" [Confidence: {conf}]")
            f.write(f" [Top similarity: {top_sim}]\n\n")
            
            f.write(f"ANSWER:\n{result['answer']}\n\n")
            
            # Sources
            f.write(f"SOURCES ({len(result['sources'])} chunks retrieved):\n")
            for i, src in enumerate(result['sources'], start=1):
                sim = src['similarity']
                indicator = "🟢" if sim >= 0.65 else "🟡" if sim >= 0.50 else "🔴"
                f.write(
                    f"  {indicator} [{i}] {src['source_document']} "
                    f"(page {src['page_number']}) — similarity: {sim:.3f}\n"
                )
            
            # If guardrail triggered, show reason
            if "reason" in result:
                f.write(f"\n⚠️  Guardrail: {result['reason']}\n")
        except Exception as e:
            f.write(f"ERROR: {e}\n")
            import traceback
            f.write(traceback.format_exc())

print("Done! Open test_output.txt in VS Code.")