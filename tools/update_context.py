"""Build/check the portable handoff database using only Python's standard library."""
import argparse
from contextlib import closing
import hashlib
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
HISTORY = ROOT / "context" / "history.json"
DATABASE = ROOT / "context" / "project_context.sqlite"
DOCUMENTS = ("AGENTS.md", "PROJECT_CONTEXT.md", "README.md", "context/README.md")
SOURCES = ("CreateGrid2D.iLogicVb", "tests/GeometryTests.vb", "tests/Run-Tests.ps1",
           "tools/update_context.py", ".gitattributes")


def digest(text):
    # Normalize newlines so LF/CRLF checkouts have the same identity on any PC.
    return hashlib.sha256(text.replace("\r\n", "\n").encode("utf-8")).hexdigest()


def inputs():
    history = json.loads(HISTORY.read_text(encoding="utf-8-sig"))
    if history.get("schema_version") != 1:
        raise ValueError("Unsupported history schema")
    ids = [event["id"] for event in history["events"]]
    if len(ids) != len(set(ids)):
        raise ValueError("History event IDs must be unique")
    for event in history["events"]:
        for key in ("date", "kind", "summary", "details", "verification"):
            if key not in event:
                raise ValueError(f"Event {event['id']} is missing {key}")
    documents = [(p, (ROOT / p).read_text(encoding="utf-8-sig")) for p in DOCUMENTS]
    sources = [(p, digest((ROOT / p).read_text(encoding="utf-8-sig"))) for p in SOURCES]
    return history, documents, sources


def check_database(history, documents, sources):
    # Read-only connection must never create or modify a database in --check mode.
    with closing(sqlite3.connect(DATABASE.resolve().as_uri() + "?mode=ro", uri=True)) as connection:
        assert connection.execute("PRAGMA integrity_check").fetchone()[0] == "ok", "Integrity check failed"
        stored = dict(connection.execute("SELECT key, value FROM metadata"))
        assert json.loads(stored["history_json"]) == history, "History snapshot is stale"
        rows = list(connection.execute("SELECT record_json FROM events ORDER BY sequence"))
        assert [json.loads(row[0]) for row in rows] == history["events"], "Event rows are stale"
        actual_documents = dict(connection.execute("SELECT path, contents FROM documents"))
        assert actual_documents == dict(documents), "Context documents are stale"
        for path, text in documents:
            saved_hash = connection.execute("SELECT sha256 FROM documents WHERE path=?", (path,)).fetchone()[0]
            assert saved_hash == digest(text), f"Document hash mismatch: {path}"
        assert dict(connection.execute("SELECT path, sha256 FROM sources")) == dict(sources), "Source hashes are stale"


def build_database(history, documents, sources):
    DATABASE.parent.mkdir(parents=True, exist_ok=True)
    handle, temporary = tempfile.mkstemp(prefix="context-", suffix=".sqlite", dir=DATABASE.parent)
    os.close(handle)
    try:
        with closing(sqlite3.connect(temporary)) as connection:
            connection.executescript("""
                PRAGMA user_version = 1;
                CREATE TABLE metadata (key TEXT PRIMARY KEY, value TEXT NOT NULL);
                CREATE TABLE documents (path TEXT PRIMARY KEY, contents TEXT NOT NULL, sha256 TEXT NOT NULL);
                CREATE TABLE sources (path TEXT PRIMARY KEY, sha256 TEXT NOT NULL);
                CREATE TABLE events (
                    id TEXT PRIMARY KEY, sequence INTEGER UNIQUE NOT NULL,
                    date TEXT NOT NULL, kind TEXT NOT NULL, summary TEXT NOT NULL,
                    record_json TEXT NOT NULL);
            """)
            head = subprocess.run(["git", "rev-parse", "HEAD"], cwd=ROOT, capture_output=True, text=True)
            metadata = {
                "schema_version": "1", "project": history["project"],
                "timezone": history["timezone"], "updated": history["updated"],
                "history_json": json.dumps(history, ensure_ascii=False, sort_keys=True),
                "base_commit_at_build": head.stdout.strip() if head.returncode == 0 else "uncommitted",
                "scope": "Project discussion summaries and document snapshots; not the full chat transcript."
            }
            connection.executemany("INSERT INTO metadata VALUES (?, ?)", metadata.items())
            connection.executemany("INSERT INTO documents VALUES (?, ?, ?)",
                                   [(p, text, digest(text)) for p, text in documents])
            connection.executemany("INSERT INTO sources VALUES (?, ?)", sources)
            connection.executemany("INSERT INTO events VALUES (?, ?, ?, ?, ?, ?)", [
                (event["id"], index, event["date"], event["kind"], event["summary"],
                 json.dumps(event, ensure_ascii=False, sort_keys=True))
                for index, event in enumerate(history["events"])])
            connection.commit()
        os.replace(temporary, DATABASE)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="Check snapshots and integrity without modifying files")
    args = parser.parse_args()
    history, documents, sources = inputs()
    if not args.check:
        build_database(history, documents, sources)
    check_database(history, documents, sources)
    print(f"PASS: {len(history['events'])} history records, {len(documents)} context snapshots, "
          f"{len(sources)} source hashes; SQLite integrity and JSON parity verified.")


if __name__ == "__main__":
    main()
