from pathlib import Path
from pdf_parser import extract_pages
from chunker import chunk_pages
from embedder import insert_chunks


# Map each PDF file to its metadata
# Update filenames to match what you actually have in pdfs/
PDF_CATALOG = [
    {
        "filename": "ai596e.pdf",
        "source_document": "FAO Irrigation Manual",
        "topic": "irrigation",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "i2215e.pdf",
        "source_document": "FAO Save and Grow",
        "topic": "general",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "i3284e.pdf",
        "source_document": "FAO Greenhouse Vegetables (Mediterranean)",
        "topic": "general",
        "crop_type": "vegetables",
        "language": "en",
    },
    {
        "filename": "FAO-plant-nutrition-for-food-security.pdf",
        "source_document": "FAO Plant Nutrition for Food Security",
        "topic": "npk",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "ca7556en.pdf",
        "source_document": "FAO Crop Production Manual",
        "topic": "general",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "ca2930en.pdf",
        "source_document": "FAO Sustainable Crop Production",
        "topic": "general",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "ho-240-w.pdf",
        "source_document": "Purdue Extension — Soil pH (HO-240-W)",
        "topic": "soil_ph",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "FS-1054 Soil pH and Nutrient Availbility_Update_12_2021.pdf",
        "source_document": "UMD Extension — Soil pH & Nutrient Availability",
        "topic": "soil_ph",
        "crop_type": "general",
        "language": "en",
    },
    {
        "filename": "Soil-Fertility-and-Nutrient-Management.pdf",
        "source_document": "Midwest Vegetable Guide — Soil Fertility & Nutrient Management",
        "topic": "npk",
        "crop_type": "vegetables",
        "language": "en",
    },
# --- New English additions ---
{
    "filename": "Penn_State_Tree_Fruit_Produc.pdf",
    "source_document": "Penn State — Tree Fruit Production Guide",
    "topic": "general",
    "crop_type": "fruit_trees",
    "language": "en",
},
{
    "filename": "B-1312_7.pdf",
    "source_document": "UGA — Commercial Tomato Production Handbook",
    "topic": "general",
    "crop_type": "tomato",
    "language": "en",
},
{
    "filename": "epsom-salts.pdf",
    "source_document": "WSU — Epsom Salts: Miracle, Myth, or Marketing?",
    "topic": "npk",
    "crop_type": "general",
    "language": "en",
},
{
    "filename": "C-1179_2.pdf",
    "source_document": "UGA — Fertilizing the Home Garden (C-1179)",
    "topic": "npk",
    "crop_type": "general",
    "language": "en",
},
{
    "filename": "cropsheets.pdf",
    "source_document": "UGA — Fertilizer Recommendations by Crops",
    "topic": "npk",
    "crop_type": "general",
    "language": "en",
},
# --- New Arabic additions ---
{
    "filename": "good-practices-guide-agriculture-arabic_6.pdf",
    "source_document": "ESCWA — Good Agricultural Practices Guide (Arabic)",
    "topic": "general",
    "crop_type": "general",
    "language": "ar",
},
{
    "filename": "national-good-agricultural-practices-guideline-for-vegetables-and-fruits-arabic_0.pdf",
    "source_document": "ESCWA — National GAP for Vegetables & Fruits (Arabic)",
    "topic": "general",
    "crop_type": "vegetables",
    "language": "ar",
},
{
    "filename": "OrganicFarmInArbReg2020.pdf",
    "source_document": "AOAD — Organic Farming Guide in the Arab Region (Arabic)",
    "topic": "general",
    "crop_type": "general",
    "language": "ar",
},
]


def ingest_all(pdf_dir: str = "pdfs", skip_existing: bool = True):
    """
    Ingest every PDF in the catalog. Skips any that are already in the DB
    (based on source_document name) unless skip_existing=False.
    """
    import psycopg2
    from config import DB_CONFIG
    
    # Check what's already ingested
    existing = set()
    if skip_existing:
        conn = psycopg2.connect(**DB_CONFIG)
        try:
            with conn.cursor() as cur:
                cur.execute("SELECT DISTINCT source_document FROM knowledge_chunks")
                existing = {row[0] for row in cur.fetchall()}
        finally:
            conn.close()
        if existing:
            print(f"Already ingested: {existing}")
    
    for entry in PDF_CATALOG:
        pdf_path = Path(pdf_dir) / entry["filename"]
    
        if not pdf_path.exists():
            print(f"⚠️  Skipping {entry['filename']} — file not found")
            continue
        
        if entry["source_document"] in existing:
            print(f"⏭️  Skipping '{entry['source_document']}' — already ingested")
            continue
        
        print(f"\n{'='*70}")
        print(f"📄 Processing: {entry['source_document']}")
        print(f"{'='*70}")
        
        try:                                              # ← add this
            lang = entry.get("language", "en")
            
            if lang == "ar":
                from pdf_parser_ar import extract_pages_ar
                pages = extract_pages_ar(str(pdf_path))
            else:
                pages = extract_pages(str(pdf_path))
            
            print(f"   Pages: {len(pages)}")
            
            if not pages:                                 # ← safety check
                print(f"   ⚠️  No pages extracted, skipping")
                continue
            
            chunks = chunk_pages(pages)
            print(f"   Chunks: {len(chunks)}")
            
            insert_chunks(
                chunks,
                source_document=entry["source_document"],
                language=lang,
                topic=entry["topic"],
                crop_type=entry["crop_type"],
            )
        except Exception as e:                            # ← and this
            print(f"   ❌ Failed to process '{entry['source_document']}': {e}")
            import traceback
            traceback.print_exc()
            continue
        
        
        
    lang = entry.get("language", "en")

    if lang == "ar":
        from pdf_parser_ar import extract_pages_ar
        pages = extract_pages_ar(str(pdf_path))
    else:
        pages = extract_pages(str(pdf_path))

    print(f"   Pages: {len(pages)}")

    chunks = chunk_pages(pages)
    print(f"   Chunks: {len(chunks)}")

    insert_chunks(
        chunks,
        source_document=entry["source_document"],
        language=lang,                    # ← from catalog
        topic=entry["topic"],
        crop_type=entry["crop_type"],
    )
    
    # Summary
    conn = psycopg2.connect(**DB_CONFIG)
    try:
        with conn.cursor() as cur:
            cur.execute("""
                SELECT source_document, COUNT(*) 
                FROM knowledge_chunks 
                GROUP BY source_document 
                ORDER BY source_document
            """)
            print(f"\n{'='*70}")
            print("📊 FINAL CORPUS STATE")
            print('='*70)
            total = 0
            for src, count in cur.fetchall():
                print(f"   {count:>5} chunks  |  {src}")
                total += count
            print(f"   {'─'*40}")
            print(f"   {total:>5} chunks total")
    finally:
        conn.close()


if __name__ == "__main__":
    ingest_all()