import fitz  # PyMuPDF

def extract_pages_ar(pdf_path: str) -> list[dict]:
    """PyMuPDF-based extractor — handles Arabic/RTL PDFs reliably."""
    doc = fitz.open(pdf_path)
    pages = []
    for page_num, page in enumerate(doc, start=1):
        text = page.get_text()
        if len(text.strip()) < 50:
            continue
        pages.append({"page_number": page_num, "text": text})
    doc.close()
    print(f"   [PyMuPDF] Extracted {len(pages)} pages")
    return pages