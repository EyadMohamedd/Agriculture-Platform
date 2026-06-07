from pypdf import PdfReader
from pathlib import Path

def extract_pages(pdf_path: str) -> list[dict]:
    """
    Extract text from a PDF, page by page.
    Returns: list of {'page_number': int, 'text': str}
    """
    reader = PdfReader(pdf_path)
    pages = []
    
    for page_num, page in enumerate(reader.pages, start=1):
        text = page.extract_text() or ""
        text = text.strip()
        
        # Skip pages that are essentially empty (likely images/scanned pages)
        if len(text) < 50:
            continue
            
        pages.append({
            "page_number": page_num,
            "text": text,
        })
    
    return pages


if __name__ == "__main__":
    # Quick test: point this at one of your PDFs
    test_pdf = "pdfs/ai596e.pdf"  # FAO Irrigation Manual (or whatever you renamed it)
    pages = extract_pages(test_pdf)
    
    print(f"Extracted {len(pages)} pages")
    print(f"\nFirst page preview:")
    print(pages[0]["text"][:500])
    print("...")