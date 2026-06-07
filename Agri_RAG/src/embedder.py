import psycopg2
from psycopg2.extras import execute_values
from pgvector.psycopg2 import register_vector
from sentence_transformers import SentenceTransformer
from tqdm import tqdm
import numpy as np

from config import DB_CONFIG, EMBEDDING_MODEL


# Lazy-loaded singleton — we only want to load the model once
_model = None

def get_model() -> SentenceTransformer:
    global _model
    if _model is None:
        print(f"Loading embedding model: {EMBEDDING_MODEL}")
        print("(First time only — this downloads ~470MB from HuggingFace)")
        _model = SentenceTransformer(EMBEDDING_MODEL)
        print(f"Model loaded. Embedding dimension: {_model.get_sentence_embedding_dimension()}")
    return _model


def embed_texts(texts: list[str], batch_size: int = 32) -> np.ndarray:
    """Generate embeddings for a list of texts. Returns (N, 384) array."""
    model = get_model()
    embeddings = model.encode(
        texts,
        batch_size=batch_size,
        show_progress_bar=True,
        convert_to_numpy=True,
        normalize_embeddings=True,  # important for cosine similarity
    )
    return embeddings


def insert_chunks(
    chunks: list[dict],
    source_document: str,
    language: str = "en",
    topic: str = None,
    crop_type: str = None,
):
    """
    Embed chunks and insert them into the knowledge_chunks table.
    
    Args:
        chunks: output from chunker.chunk_pages()
        source_document: filename or title (e.g., "FAO Irrigation Manual")
        language: 'en' or 'ar'
        topic: optional — e.g., 'irrigation', 'soil_ph'
        crop_type: optional — e.g., 'general', 'tomato'
    """
    if not chunks:
        print("No chunks to insert.")
        return
    
    # Generate embeddings
    print(f"\nEmbedding {len(chunks)} chunks...")
    texts = [c["chunk_text"] for c in chunks]
    embeddings = embed_texts(texts)
    
    # Prepare rows for bulk insert
    rows = [
        (
            source_document,
            chunk["page_number"],
            chunk["chunk_index"],
            chunk["chunk_text"],
            language,
            topic,
            crop_type,
            embedding.tolist(),  # pgvector wants a list
        )
        for chunk, embedding in zip(chunks, embeddings)
    ]
    
    # Connect and insert
    print(f"Inserting into database...")
    conn = psycopg2.connect(**DB_CONFIG)
    register_vector(conn)
    
    try:
        with conn.cursor() as cur:
            execute_values(
                cur,
                """
                INSERT INTO knowledge_chunks 
                    (source_document, page_number, chunk_index, chunk_text, 
                     language, topic, crop_type, embedding)
                VALUES %s
                """,
                rows,
                template="(%s, %s, %s, %s, %s, %s, %s, %s::vector)",
                page_size=100,
            )
        conn.commit()
        print(f"✅ Inserted {len(rows)} chunks for '{source_document}'")
    except Exception as e:
        conn.rollback()
        print(f"❌ Insert failed: {e}")
        raise
    finally:
        conn.close()


if __name__ == "__main__":
    # Test with one PDF end-to-end
    from pdf_parser import extract_pages
    from chunker import chunk_pages
    
    test_pdf = "pdfs/ai596e.pdf"
    
    pages = extract_pages(test_pdf)
    print(f"Pages: {len(pages)}")
    
    chunks = chunk_pages(pages)
    print(f"Chunks: {len(chunks)}")
    
    insert_chunks(
        chunks,
        source_document="FAO Irrigation Manual",
        language="en",
        topic="irrigation",
        crop_type="general",
    )
    
    print("\nDone! Check your database.")