from langchain_text_splitters import RecursiveCharacterTextSplitter


def chunk_pages(pages: list[dict], chunk_size: int = 800, chunk_overlap: int = 150) -> list[dict]:
    """
    Split page text into overlapping chunks suitable for embedding.
    
    Args:
        pages: list of {'page_number': int, 'text': str} from pdf_parser
        chunk_size: target chunk size in characters (not tokens — roughly 150-200 words)
        chunk_overlap: how many characters to overlap between adjacent chunks
    
    Returns:
        list of {'page_number': int, 'chunk_index': int, 'chunk_text': str}
    """
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        # Split preference: paragraphs first, then lines, then sentences, then words
        separators=["\n\n", "\n", ". ", "? ", "! ", " ", ""],
        length_function=len,
    )
    
    all_chunks = []
    
    for page in pages:
        page_chunks = splitter.split_text(page["text"])
        
        for i, chunk_text in enumerate(page_chunks):
            # Skip tiny chunks (likely page numbers, headers, footers)
            if len(chunk_text.strip()) < 100:
                continue
            
            all_chunks.append({
                "page_number": page["page_number"],
                "chunk_index": i,
                "chunk_text": chunk_text.strip(),
            })
    
    return all_chunks


if __name__ == "__main__":
    from pdf_parser import extract_pages
    
    test_pdf = "pdfs/ai596e.pdf"  # adjust to your filename
    
    print("Parsing PDF...")
    pages = extract_pages(test_pdf)
    print(f"Got {len(pages)} pages")
    
    print("\nChunking...")
    chunks = chunk_pages(pages)
    print(f"Created {len(chunks)} chunks")
    
    # Show some samples
    print(f"\n--- First chunk (page {chunks[0]['page_number']}) ---")
    print(chunks[0]["chunk_text"][:400])
    
    print(f"\n--- Middle chunk (page {chunks[len(chunks)//2]['page_number']}) ---")
    print(chunks[len(chunks)//2]["chunk_text"][:400])
    
    print(f"\n--- Last chunk (page {chunks[-1]['page_number']}) ---")
    print(chunks[-1]["chunk_text"][:400])
    
    # Sanity stats
    sizes = [len(c["chunk_text"]) for c in chunks]
    print(f"\n--- Stats ---")
    print(f"Total chunks: {len(chunks)}")
    print(f"Avg chunk size: {sum(sizes)//len(sizes)} chars")
    print(f"Min chunk size: {min(sizes)} chars")
    print(f"Max chunk size: {max(sizes)} chars")