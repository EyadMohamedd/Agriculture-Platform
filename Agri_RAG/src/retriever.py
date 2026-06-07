import psycopg2
from pgvector.psycopg2 import register_vector
from embedder import get_model
from config import DB_CONFIG


def search(
    query: str,
    top_k: int = 5,
    language: str = None,
    topic: str = None,
    crop_type: str = None,
) -> list[dict]:
    """
    Find the most relevant chunks for a given query.
    
    Args:
        query: the farmer's question (Arabic or English)
        top_k: how many chunks to return
        language: optional filter — 'en' or 'ar'
        topic: optional filter — e.g., 'irrigation'
        crop_type: optional filter — e.g., 'tomato'
    
    Returns:
        list of {id, source_document, page_number, chunk_text, similarity}
    """
    # Embed the query using the SAME model that embedded the chunks
    model = get_model()
    query_embedding = model.encode(
        query,
        normalize_embeddings=True,
        convert_to_numpy=True,
    ).tolist()
    
    # Build the SQL dynamically based on which filters are provided
    filters = []
    params = [query_embedding]
    
    if language:
        filters.append("language = %s")
        params.append(language)
    if topic:
        filters.append("topic = %s")
        params.append(topic)
    if crop_type:
        filters.append("crop_type = %s")
        params.append(crop_type)
    
    where_clause = f"WHERE {' AND '.join(filters)}" if filters else ""
    
    sql = f"""
        SELECT 
            id,
            source_document,
            page_number,
            chunk_text,
            1 - (embedding <=> %s::vector) AS similarity
        FROM knowledge_chunks
        {where_clause}
        ORDER BY embedding <=> %s::vector
        LIMIT %s
    """
    params.append(query_embedding)  # for the ORDER BY
    params.append(top_k)
    
    # Execute
    conn = psycopg2.connect(**DB_CONFIG)
    register_vector(conn)
    
    try:
        with conn.cursor() as cur:
            cur.execute(sql, params)
            rows = cur.fetchall()
    finally:
        conn.close()
    
    results = [
        {
            "id": row[0],
            "source_document": row[1],
            "page_number": row[2],
            "chunk_text": row[3],
            "similarity": float(row[4]),
        }
        for row in rows
    ]
    
    return results


if __name__ == "__main__":
    # Try some realistic farmer questions
    test_queries = [
        "How often should I irrigate my crops?",
        "What is the ideal soil moisture level?",
        "How do I calculate crop water requirements?",
        "My soil is too dry, what should I do?",
    ]
    
    for query in test_queries:
        print(f"\n{'='*70}")
        print(f"QUERY: {query}")
        print('='*70)
        
        results = search(query, top_k=3)
        
        for i, r in enumerate(results, start=1):
            print(f"\n[{i}] Similarity: {r['similarity']:.3f}")
            print(f"    Source: {r['source_document']}, page {r['page_number']}")
            print(f"    Preview: {r['chunk_text'][:250]}...")