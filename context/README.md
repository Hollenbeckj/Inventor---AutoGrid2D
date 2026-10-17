# Portable project memory

Start with `../PROJECT_CONTEXT.md` for the current state and `../AGENTS.md` for
the continuing-work instructions. `history.json` is the editable database of
discussion summaries. `project_context.sqlite` mirrors that history and stores
snapshots of the handoff documents plus source hashes for verification.

After each substantive project discussion or change:

1. Update `PROJECT_CONTEXT.md` with decisions, behavior, evidence, deployment
   status, unresolved issues, and next steps. Preserve the distinction between
   proposed work, completed work, automated results, and user-reported testing.
2. Append a dated event with a unique ID to `context/history.json`. Use the
   discussion date in America/Los_Angeles; do not invent precise timestamps or
   verbatim transcripts. Corrections should be additional events.
3. Run `python tools/update_context.py` using Python 3.9 or later. The script
   uses the standard library only. It builds the database atomically from the
   readable files. Do not edit SQLite independently of those sources.
4. Run `python tools/update_context.py --check`. Commit the context, JSON,
   database, and source changes together when the user authorizes a commit.
   Push when the user authorizes syncing to GitHub.

On another computer, clone or pull the repository after these files have been
committed and pushed. Read the Markdown/JSON directly, or open the SQLite file
with any SQLite client. Useful queries:

```sql
SELECT date, kind, summary FROM events ORDER BY sequence;
SELECT contents FROM documents WHERE path = 'PROJECT_CONTEXT.md';
SELECT path, sha256 FROM sources ORDER BY path;
```

The database records the Git base commit at build time; it does not claim that
the files were committed or pushed. Source hashes ignore LF/CRLF differences.
These files are portable project memory, not an automatic backup of the full
Codex chat, a live cloud database, or a backup of private Inventor drawings.
