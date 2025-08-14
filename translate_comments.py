#!/usr/bin/env python3
from __future__ import annotations
"""
Batch-translate code comments across a project.

Supported languages by file extension:
- C/C++/C#/Java/JavaScript/TypeScript/Go: .c, .h, .hpp, .cpp, .cc, .cs, .java, .js, .jsx, .ts, .tsx, .go
  - Line comments: //
  - Block comments: /* ... */
- Python: .py
  - Line comments: #
  - (Optional) Docstrings: triple quotes
- Shell/Config: .sh, .bash, .zsh, .ini, .cfg, .conf, .toml (treated as # line comments where applicable)
- HTML/XML/Markdown: .html, .htm, .xml, .md (HTML/XML comments only: <!-- ... -->)

Translation providers:
- DeepL: set environment variable DEEPL_API_KEY, install `pip install deepl`
- OpenAI: set environment variable OPENAI_API_KEY, install `pip install openai`

Usage examples:
  # Dry-run on C#/C++ only, print planned changes
  python translate_comments.py ./my_project --exts .cs,.cpp,.h --dry-run

  # Actually translate to Korean using DeepL, keep originals, and backup files
  DEEPL_API_KEY=... python translate_comments.py ./my_project --provider deepl --dst ko --keep-original --backup

  # Translate Python comments + docstrings to English, excluding venv and build folders
  OPENAI_API_KEY=... python translate_comments.py . --provider openai --dst en --include-docstrings --exclude 'venv|build|dist'
"""

import os
import re
import sys
import argparse
from pathlib import Path
from typing import List, Tuple, Optional

# ---------------------------- Utilities ----------------------------

def debug(msg: str):
    if os.environ.get("TC_VERBOSE"):
        print(f"[DEBUG] {msg}", file=sys.stderr)

def chunk_text(text: str, max_chars: int = 3800) -> List[str]:
    """Split text into chunks by paragraphs/newlines while respecting max_chars."""
    if len(text) <= max_chars:
        return [text]
    parts = []
    buf = []
    size = 0
    for line in text.splitlines(keepends=True):
        if size + len(line) > max_chars and buf:
            parts.append(''.join(buf))
            buf = [line]
            size = len(line)
        else:
            buf.append(line)
            size += len(line)
    if buf:
        parts.append(''.join(buf))
    return parts

def detect_provider(explicit: Optional[str]) -> str:
    if explicit:
        return explicit.lower()
    if os.environ.get("DEEPL_API_KEY"):
        return "deepl"
    if os.environ.get("OPENAI_API_KEY"):
        return "openai"
    return "none"

# ---------------------------- Translation Backends ----------------------------

def translate_deepl(text: str, target_lang: str) -> str:
    import deepl  # type: ignore
    auth = os.environ["DEEPL_API_KEY"]
    translator = deepl.Translator(auth)
    chunks = chunk_text(text, 4500)  # keep some headroom
    out = []
    for ch in chunks:
        res = translator.translate_text(ch, target_lang=target_lang.upper())
        out.append(res.text if hasattr(res, "text") else str(res))
    return ''.join(out)

def translate_openai(text: str, target_lang: str, model: Optional[str] = None) -> str:
    """Minimal OpenAI translation using a single prompt."""
    model = model or os.environ.get("OPENAI_TRANSLATE_MODEL", "gpt-4o-mini")
    try:
        from openai import OpenAI  # type: ignore
        client = OpenAI()
        system = f"You are a professional translator. Translate to {target_lang}. Keep code, punctuation, and formatting. Translate only human-language comment text."
        out = []
        for ch in chunk_text(text, 8000):
            rsp = client.responses.create(
                model=model,
                input=[
                    {"role": "system", "content": system},
                    {"role": "user", "content": ch},
                ],
            )
            out.append(rsp.output_text)
        return ''.join(out)
    except Exception as e:
        try:
            import openai  # type: ignore
            openai.api_key = os.environ["OPENAI_API_KEY"]
            system = f"You are a professional translator. Translate to {target_lang}. Keep code and formatting; translate only natural language."
            out = []
            for ch in chunk_text(text, 3500):
                rsp = openai.ChatCompletion.create(
                    model=model,
                    messages=[
                        {"role": "system", "content": system},
                        {"role": "user", "content": ch},
                    ],
                    temperature=0.0,
                )
                out.append(rsp.choices[0].message["content"])  # type: ignore
            return ''.join(out)
        except Exception as e2:
            raise RuntimeError(f"OpenAI translation failed: {e}\nFallback error: {e2}")

def translate_text(text: str, target_lang: str, provider: str, openai_model: Optional[str]) -> str:
    if provider == "deepl":
        return translate_deepl(text, target_lang)
    elif provider == "openai":
        return translate_openai(text, target_lang, openai_model)
    else:
        raise RuntimeError("No translation provider configured. Set DEEPL_API_KEY or OPENAI_API_KEY, or pass --provider.")

# ---------------------------- Comment Parsing ----------------------------

CLIKE_EXT = {".c",".h",".hpp",".hxx",".hh",".cpp",".cc",".cxx",".cs",".java",".js",".jsx",".ts",".tsx",".go",".m",".mm",".hlsl",".hlsli",".fx",".fxh",".usf",".ush",".glsl",".vert",".frag",".geom",".tesc",".tese",".comp",".wgsl",".metal"}
PY_EXT = {".py"}
SHELL_EXT = {".sh",".bash",".zsh",".env",".ini",".cfg",".conf",".toml",".properties",".yml",".yaml"}
HTML_EXT = {".html",".htm",".xml",".md",".xhtml",".svg"}

def find_comments_clike(src: str) -> List[Tuple[int,int,str]]:
    """Return list of (start, end, kind) where kind in {"line", "block"} for C-like syntax, skipping strings."""
    res: List[Tuple[int,int,str]] = []
    i, n = 0, len(src)
    in_str: Optional[str] = None
    escape = False
    while i < n:
        ch = src[i]
        if in_str:
            if escape:
                escape = False
            elif ch == '\\':
                escape = True
            elif ch == in_str:
                in_str = None
            i += 1
            continue
        if ch in ('"', "'", '`'):
            in_str = ch
            i += 1
            continue
        if ch == '/' and i+1 < n:
            nxt = src[i+1]
            if nxt == '/':
                start = i
                i += 2
                while i < n and src[i] != '\n':
                    i += 1
                end = i
                res.append((start, end, "line"))
                continue
            elif nxt == '*':
                start = i
                i += 2
                while i+1 < n and not (src[i] == '*' and src[i+1] == '/'):
                    i += 1
                i = min(i+2, n)
                end = i
                res.append((start, end, "block"))
                continue
        i += 1
    return res

def find_comments_python(src: str, include_docstrings: bool=False) -> List[Tuple[int,int,str]]:
    res: List[Tuple[int,int,str]] = []
    n = len(src)
    i = 0
    in_str: Optional[str] = None
    triple: Optional[str] = None
    escape = False
    while i < n:
        ch = src[i]
        if in_str:
            if escape:
                escape = False
            elif ch == '\\':
                escape = True
            elif triple:
                if src.startswith(triple, i):
                    in_str = None
                    i += len(triple)
                    triple = None
                    continue
            elif ch == in_str:
                in_str = None
            i += 1
            continue
        if src.startswith('"""', i) or src.startswith("'''", i):
            quote = '"""' if src.startswith('"""', i) else "'''"
            start = i
            in_str = quote[0]
            triple = quote
            if include_docstrings:
                j = i + 3
                while j+2 < n and not src.startswith(quote, j):
                    j += 1
                end = min(j+3, n)
                res.append((start, end, "docstring"))
                i = end
                continue
            i += 3
            continue
        if ch == '#':
            start = i
            i += 1
            while i < n and src[i] != '\n':
                i += 1
            end = i
            res.append((start, end, "line"))
            continue
        if ch in ('"', "'"):
            in_str = ch
            i += 1
            continue
        i += 1
    return res

def find_comments_shell(src: str) -> List[Tuple[int,int,str]]:
    res: List[Tuple[int,int,str]] = []
    i, n = 0, len(src)
    in_str: Optional[str] = None
    escape = False
    while i < n:
        ch = src[i]
        if in_str:
            if escape:
                escape = False
            elif ch == '\\':
                escape = True
            elif ch == in_str:
                in_str = None
            i += 1
            continue
        if ch in ('"', "'"):
            in_str = ch
            i += 1
            continue
        if ch == '#':
            start = i
            i += 1
            while i < n and src[i] != '\n':
                i += 1
            end = i
            res.append((start, end, "line"))
            continue
        i += 1
    return res

def find_comments_html(src: str) -> List[Tuple[int,int,str]]:
    res: List[Tuple[int,int,str]] = []
    for m in re.finditer(r'<!--(.*?)-->', src, flags=re.DOTALL):
        res.append((m.start(), m.end(), "block"))
    return res

HTML_EXT = {".html",".htm",".xml",".md",".xhtml",".svg"}

def extract_comment_text(src: str, start: int, end: int, kind: str, ext: str) -> Tuple[str, str, str]:
    raw = src[start:end]
    if ext in HTML_EXT:
        inner = raw[4:-3]  # <!-- ... -->
        return raw[:4], inner, raw[-3:]
    if ext == ".py":
        if kind == "line":
            m = re.match(r'(\s*#\s?)(.*?)(\s*)$', raw, flags=re.DOTALL)
            if m:
                return m.group(1), m.group(2), m.group(3)
        elif kind == "docstring":
            if raw.startswith('"""') and raw.endswith('"""'):
                return '"""', raw[3:-3], '"""'
            if raw.startswith("'''") and raw.endswith("'''"):
                return "'''", raw[3:-3], "'''"
    if kind == "line" and raw.startswith("//"):
        m = re.match(r'(//\s?)(.*?)(\s*)$', raw, flags=re.DOTALL)
        if m:
            return m.group(1), m.group(2), m.group(3)
    if kind == "block" and raw.startswith("/*") and raw.endswith("*/"):
        inner = raw[2:-2]
        return "/*", inner, "*/"
    if kind == "line" and raw.strip().startswith("#"):
        m = re.match(r'(\s*#\s?)(.*?)(\s*)$', raw, flags=re.DOTALL)
        if m:
            return m.group(1), m.group(2), m.group(3)
    return "", raw, ""

def rebuild_comment(prefix: str, translated: str, suffix: str, keep_original: bool, original: Optional[str]=None) -> str:
    if keep_original and original and original.strip() and translated.strip() and translated.strip() != original.strip():
        if "\n" in translated or "\n" in original:
            return f"{prefix}{translated.strip()}\n(Original: {original.strip()}){suffix}"
        else:
            return f"{prefix}{translated.strip()} (Original: {original.strip()}){suffix}"
    return f"{prefix}{translated}{suffix}"

DEFAULT_EXTS = sorted(CLIKE_EXT | PY_EXT | SHELL_EXT | HTML_EXT)

def should_skip(path: Path, exclude_pattern: Optional[str]) -> bool:
    if exclude_pattern and re.search(exclude_pattern, str(path).replace("\\","/")):
        return True
    parts = {p.lower() for p in path.parts}
    blocked = {"node_modules","dist","build","bin","obj",".git",".idea",".vscode","__pycache__",".venv","venv"}
    return any(b in parts for b in blocked)

def target_looks_same(text: str, dst: str) -> bool:
    if dst.lower().startswith("ko"):
        hangul = sum(1 for ch in text if '\uac00' <= ch <= '\ud7af')
        return hangul >= max(5, len(text) // 3)
    if dst.lower().startswith("en"):
        ascii_letters = sum(1 for ch in text if ch.isascii() and ch.isalpha())
        return ascii_letters >= max(10, int(len(text) * 0.6))
    return False

def process_file(path: Path, args) -> Tuple[int,int,int]:
    try:
        src = path.read_text(encoding=args.encoding, errors="ignore")
    except Exception as e:
        print(f"[WARN] Failed to read {path}: {e}", file=sys.stderr)
        return (0,0,0)

    ext = path.suffix.lower()
    if ext in CLIKE_EXT:
        spans = find_comments_clike(src)
    elif ext in PY_EXT:
        spans = find_comments_python(src, include_docstrings=args.include_docstrings)
    elif ext in SHELL_EXT:
        spans = find_comments_shell(src)
    elif ext in HTML_EXT:
        spans = find_comments_html(src)
    else:
        return (0,0,0)

    if not spans:
        return (0,0,0)

    spans.sort(key=lambda t: t[0])
    out = []
    cursor = 0
    translated_count = 0

    for (start, end, kind) in spans:
        out.append(src[cursor:start])
        prefix, text, suffix = extract_comment_text(src, start, end, kind, ext)
        original_text = text

        if args.skip_if_target and target_looks_same(text, args.dst):
            out.append(src[start:end])
        else:
            if args.dry_run:
                mark = f"{prefix}[TRANSLATE to {args.dst}]: {text}{suffix}"
                out.append(mark)
            else:
                try:
                    translated = translate_text(text, args.dst, args.provider, args.openai_model)
                except Exception as e:
                    print(f"[ERROR] Translation failed in {path} [{kind}] at {start}:{end}: {e}", file=sys.stderr)
                    translated = text
                out.append(rebuild_comment(prefix, translated, suffix, args.keep_original, original_text))
                if translated != text:
                    translated_count += 1
        cursor = end

    out.append(src[cursor:])
    new_src = ''.join(out)

    if not args.dry_run and new_src != src:
        if args.backup:
            bak = path.with_suffix(path.suffix + ".bak")
            try:
                bak.write_text(src, encoding=args.encoding, errors="ignore")
            except Exception as e:
                print(f"[WARN] Failed to write backup {bak}: {e}", file=sys.stderr)
        try:
            path.write_text(new_src, encoding=args.encoding, errors="ignore")
        except Exception as e:
            print(f"[ERROR] Failed to write {path}: {e}", file=sys.stderr)
            return (len(spans), translated_count, 0)

    return (len(spans), translated_count, (len(new_src) - len(src)) if not args.dry_run else 0)

def iter_files(root: Path, exts: List[str]) -> List[Path]:
    out = []
    for p in root.rglob("*"):
        if p.is_file() and p.suffix.lower() in exts:
            out.append(p)
    return out

def parse_args():
    parser = argparse.ArgumentParser(description="Batch-translate code comments across a project.")
    parser.add_argument("root", nargs="?", default=".", help="Project root directory.")
    parser.add_argument("--exts", default=",".join(sorted(DEFAULT_EXTS)), help="Comma-separated file extensions to process.")
    parser.add_argument("--exclude", default=None, help="Regex to exclude paths (applied to full path).")
    parser.add_argument("--dst", default="ko", help="Target language (e.g., ko, en, ja, fr).")
    parser.add_argument("--provider", choices=["deepl","openai","none"], default=None, help="Translation provider; auto if not set.")
    parser.add_argument("--openai-model", default=None, help="OpenAI model name (if provider=openai).")
    parser.add_argument("--include-docstrings", action="store_true", help="Include Python docstrings (triple quotes).")
    parser.add_argument("--keep-original", action="store_true", help="Keep original text next to the translation.")
    parser.add_argument("--skip-if-target", action="store_true", help="Skip comments that already look like target language.")
    parser.add_argument("--dry-run", action="store_true", help="Do not write files; mark places that would be translated.")
    parser.add_argument("--backup", action="store_true", help="Write .bak backups before modifying files.")
    parser.add_argument("--encoding", default="utf-8", help="File encoding to read/write (default utf-8)." )
    args = parser.parse_args()

    args.exts = [e.strip().lower() for e in args.exts.split(",") if e.strip()]
    prov = detect_provider(args.provider)
    if prov == "none" and not args.dry_run:
        print("[ERROR] No translation provider configured. Set DEEPL_API_KEY or OPENAI_API_KEY, or pass --provider (deepl/openai).", file=sys.stderr)
        sys.exit(2)
    args.provider = prov
    return args

def main():
    args = parse_args()
    root = Path(args.root).resolve()
    if not root.exists():
        print(f"[ERROR] Root not found: {root}", file=sys.stderr)
        sys.exit(1)

    files = [p for p in iter_files(root, args.exts) if not should_skip(p, args.exclude)]
    print(f"[INFO] Scanning {len(files)} files under {root} ...")

    total_comments = 0
    total_translated = 0
    total_delta = 0
    for p in files:
        c, t, d = process_file(p, args)
        total_comments += c
        total_translated += t
        total_delta += d

    mode = "DRY-RUN" if args.dry_run else "APPLIED"
    print(f"[{mode}] Files: {len(files)}, Comments seen: {total_comments}, Translated: {total_translated}, Size delta: {total_delta} bytes")

if __name__ == "__main__":
    main()
