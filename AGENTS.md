# Inventor - AutoGrid2D — continuing work

Before making changes, read PROJECT_CONTEXT.md, README.md, the current CreateGrid2D.iLogicVb, and the relevant tests. This is the Inventor external iLogic project, not the AutoCAD grid or Inventor electrical-layout project.

- Preserve the accepted behavior recorded in PROJECT_CONTEXT.md unless the user's latest request changes it. Verify actual source and repository state rather than relying on historical chat summaries.
- Keep PROJECT_CONTEXT.md current after substantive discussions or work: decisions, changed behavior, verified results, unresolved issues, installation status, and next steps. Date updates. This supports continuity across PCs; it does not synchronize the full chat transcript.
- Distinguish compilation, automated geometry tests, user-reported live results, and comprehensive Inventor validation. Do not claim a live test that was not performed.
- Run tests/Run-Tests.ps1 for relevant functional changes when its Windows/Inventor prerequisites are available; otherwise state the verification limit. The script currently targets Inventor 2027 interop. Do not assume another version has been verified.
- Respect .gitattributes: CRLF for iLogic/VB/PowerShell; LF for Markdown; BMP binary. Keep tests/build and backup files untracked.
- Preserve original/CreateGrid2D.iLogicVb as the reference copy. Edit the current top-level rule.
- A source edit or Git pull does not install the external rule into Inventor. When deployment is requested, verify the intended external-rule path and source identity, preserve a backup, and verify the installed copy. Never overwrite unrelated drawing/model files.
- Preserve validation-before-mutation, transactional document changes, explicit new-entity formatting, geometry limits, centroid semantics, and the stated orthographic-view limitations.
- Keep device-specific paths and setup local where possible. Do not commit credentials, private drawings, proprietary model files, or generated build artifacts.
- Follow the user's authorization for commits, remote pushes, macro installation, and messaging other chats.
