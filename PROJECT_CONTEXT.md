# Inventor - AutoGrid2D — shared project context

Last updated: 2026-10-16 (America/Los_Angeles).

This brief preserves the accepted decisions and current status for work on another computer or in a new chat. It is not the full chat transcript. Read it together with README.md and the actual rule before making changes.

## Project and purpose

Repository: https://github.com/Hollenbeckj/Inventor---AutoGrid2D

Primary source: `CreateGrid2D.iLogicVb`, an Autodesk Inventor external iLogic rule for drawing views.

The original request was to audit an existing macro that creates a grid inside a four-sided shape, fix errors, and improve its behavior. This is the **Inventor** project, separate from the similarly named AutoCAD AutoGrid2D project and the Inventor AutoElectricalLayout project.

The working computer's project folder was moved from OneDrive into a local development folder. Folder locations and installed external-rule paths are machine-specific; a second PC does not need the identical development path.

## Accepted behavior

### Selection and boundary recovery

- Open an Inventor drawing and select one straight segment on each of four outer sides of a single drawing view, in any order.
- Closed four-edge boundaries use matching endpoints directly.
- For interrupted convex outlines such as rectangles, rotated rectangles, and trapezoids, infer the corners by extending the four selected supporting lines.
- Selected segments must lie between the inferred corners. Reject repeated picks on one supporting side, degenerate sides, crossed/ambiguous selections, and invalid outlines.
- Concave outlines still require four edges with meeting endpoints. Do not extend the convex recovery workflow to disconnected concave outlines without a separately justified design.

### Spacing, layout, and appearance

- Ask for model X and Y spacing, aligned to drawing-sheet axes, including when the drawing view is rotated.
- Explicit units such as `12 in` or `300 mm` are accepted; bare numbers use the drawing document's length units.
- Apply the drawing view's scale once when converting spacing to sheet-space geometry.
- Create a view sketch named `Grid`. If it already exists, tell the user to delete or rename it before rerunning; do not silently replace it.
- Ordinary grid: gray, continuous, clipped to the boundary, **0.18 mm** printed line weight, on layer `2D-Grid`.
- Center: polygon **area centroid**, not the bounding-box center. This identifies the center of the booth space.
- Center axes: black horizontal and vertical sketch lines, long-dash-dot, clipped to the full interior boundary, **1.00 mm** printed weight, pattern scale **4.0**, on layer `2D-Grid-Center`.
- The user explicitly wanted fewer centerline dash repeats and thicker centerline strokes. This was not a request to change the whole grid's spacing.
- Center circle: full circle at the area centroid, matching the center axes' color, dash pattern, pattern scale, layer, and line weight.
- Circle radius: **half the smaller selected X/Y grid spacing**, transformed correctly into sketch space. The circle stays complete even if it extends outside the outline.
- Suppress an ordinary grid line coincident with a bold center axis.
- If spacing is too large to produce ordinary grid lines, create the center axes and circle with an explanation rather than an empty sketch.

### Geometry and document handling

- Center axes are sketch entities, not Inventor native hole-centermark annotations.
- A strongly concave shape's centroid may lie outside its outline; explain this and clip each axis to the interior.
- Preserve existing layer formatting; apply explicit formatting overrides to new entities. Existing target layers must be visible and printable.
- Validate selection, boundary, spacing, clipping, and counts before changing the drawing.
- Layer and sketch creation share a transaction; commit only after the document update succeeds. Report cleanup errors.
- Bound creation to 2,000 grid positions and 2,000 final entities, including center axes and circle.
- Generated geometry is a **snapshot**, not model-associative. Rerun after model-boundary changes.
- The supported model-distance workflow assumes an ordinary orthographic view with uniform scale. Do not claim support for broken or perspective views.

## Source map

- `CreateGrid2D.iLogicVb`: current editable rule.
- `original/CreateGrid2D.iLogicVb`: preserved reference copy; do not treat it as the current implementation.
- `README.md`: usage, current behavior, and validation details.
- `tests/Run-Tests.ps1`: compiles the rule and runs the geometry harness without starting Inventor.
- `tests/GeometryTests.vb`: regression tests for actual rule helpers.
- `icons/Module1.CreateGrid2D.Large.bmp` and `icons/Module1.CreateGrid2D.Small.bmp`: toolbar icons.
- `tests/build/` and `*.backup-*` are ignored generated/backup files.

Preserve the repository's .gitattributes policy: CRLF for iLogic/VB/PowerShell source, LF for Markdown, BMP files binary.

## Verified and reported status

The previous development chat, **Audit grid creation macro**, records these results:

- Full rule compiled against the locally installed Inventor 2027 interop.
- The latest regression suite passed **18,440 assertions**, including boundary recovery, permutations of pick order/edge directions, clipping, centroid coordinates, spacing conversion, invalid boundaries, and limits.
- The interrupted-edge fix and center-circle feature were copied into the development rule and the installed external rule, with a backup.
- The user then tested the same previously failing example drawing and reported that it worked well.
- The changes were subsequently committed. Historical chat statements about whether particular commits were pushed may be stale; check Git before making claims.

During preparation of this brief, the repository's current README and rule header were read from GitHub and describe the interrupted-edge recovery, center circle, 1.00 mm center weight, and 4.0 pattern scale. No macro source was modified, no tests were rerun, and no new live Inventor check was performed for this documentation-only handoff.

The user's successful example test is meaningful evidence, but is not a full interactive validation matrix or proof for every shape, scale, and layer configuration.

## Remaining validation / next steps

Before a new functional change, verify the current Git state, installed rule, and latest user request. There is no new feature request implied by this handoff.

Recommended live Inventor checks still include:
1. A rectangle whose grid spacing lands on the center: bold dash-dot axes without duplicate ordinary lines.
2. Existing center layers whose old formatting differs: new entities must still have the explicit selected style.
3. Rotated rectangles and trapezoids, including shortened/interrupted sides; inspect recovered corners and all clipped endpoints.
4. Cancel at each selection/input stage and verify no unwanted layer/sketch remains.
5. Large spacing: center axes and complete circle only; check the circle diameter against the smaller grid spacing at different view scales.
6. One Undo after a successful run: sketch and newly created layers should be removed together.
7. Printed/PDF line appearance, dash density, and circle appearance in the actual drawing workflow.

Run `tests/Run-Tests.ps1` for relevant code changes. It currently expects Windows .NET Framework's VB compiler and Inventor 2027's interop at paths defined in the script. Check those prerequisites on the second PC; do not assume another Inventor version is already verified.

Editing a Git checkout does not automatically update Inventor's installed external rule. When installation is requested, verify the intended destination, preserve a backup, and check that installed and source files match.

## Cross-computer continuity

- Use the same ChatGPT account/workspace and a cloud-backed ChatGPT project named **Inventor - AutoGrid2D** for shared ongoing discussions.
- Upload this file as a project source, or provide it via the connected GitHub repository tools.
- For coding on a second PC, clone/pull the repository, open the folder, and ask the assistant to read AGENTS.md, this file, and README.md.
- GitHub exchanges code and these notes; it does not sync a local Codex transcript or install the macro into Inventor.
- Keep this brief updated after meaningful decisions, fixes, confirmed test results, installation changes, or unresolved issues. Never silently turn proposed work into completed status.
