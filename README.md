# AutoGrid2D

`CreateGrid2D.iLogicVb` is an external Inventor iLogic rule for drawing views.

Select one straight segment on each of four outer sides in one view, in any
order. For a closed boundary, the rule uses the matching endpoints directly.
For a convex outline such as a rectangle, rotated rectangle, or trapezoid,
segments may be split or interrupted by other drawing geometry: the rule
extends their supporting lines to recover the corners. All picked segments
must lie between those corners. Repeated picks on the same straight side,
degenerate sides, and ambiguous outlines are rejected. Concave outlines still
require four edges whose endpoints meet. Enter model
spacing along drawing-sheet X and Y; explicit units such as `12 in` or `300 mm`
are supported. Bare numbers use the drawing document's length units.

The rule creates a view sketch named `Grid` containing:

- Gray grid lines, clipped to the selected boundary, at 0.18 mm line weight.
- Black horizontal and vertical center axes with a long-dash-dot pattern,
  clipped to the entire boundary, at 1.00 mm line weight. The pattern scale is
  4.0, giving longer dashes and gaps with fewer repeats. They pass through the
  polygon's area centroid to identify the center of the booth space.
- A full circle at that center, with radius equal to half the smaller X/Y grid
  spacing. It matches the center axes' black long-dash-dot style, scale, layer,
  and 1.00 mm line weight. The radius is converted through the view and sketch
  transforms. The circle stays complete, even if it extends outside the outline.

The center is the area centroid, which differs from the bounding-box center for
many trapezoids and irregular quadrilaterals. For a rectangle it is the ordinary
center. In a strongly concave shape the centroid can lie outside the outline;
the rule explains this and clips each axis to the interior rather than drawing
across the exterior. These axes are sketch lines, not native hole centermark
annotations. They follow the sheet axes, including in a rotated view.

Grid positions start one spacing from the left/bottom sheet-space bounding
extents. A bold center axis replaces any coincident ordinary grid line. If the
spacing is too large for either family of grid lines, the rule explains that
it will create only the center axes and circle; it never creates an empty sketch.

Layers are `2D-Grid` and `2D-Grid-Center`. New layers are visible and printable.
Existing layer formatting is preserved; the new entities receive explicit
color, line type, and weight overrides (continuous grid, long-dash-dot center
axes). Existing layers must already
be visible and printable. The weights and centerline pattern scale are constants
at the top of the rule.

All selection, validation, input, clipping, and count checks precede document
changes. Layer creation and sketch creation share one transaction. Document
update must succeed before committing. Cleanup errors are reported rather than
silently hidden. There is a limit of 2,000 grid positions and 2,000 final entities
(including center axes and circle) to bound creation time.

The geometry is a snapshot, not an associative model constraint. After a model
boundary change, delete or rename `Grid` and run the rule again. Model-distance
spacing assumes an ordinary orthographic view with a uniform scale; broken or
perspective views are not supported spacing workflows.

## Validation

Run `tests/Run-Tests.ps1` from PowerShell. It compiles the complete rule against
the locally installed Inventor 2027 interop, then exercises the actual geometry
helpers without starting Inventor. It needs the Windows .NET Framework VB
compiler and the Inventor 2027 installation paths specified in that script.

The suite currently passes 18,440 assertions, including all 384 pick-order and
edge-direction combinations for each of four shapes, clipped interval samples,
known centroid coordinates, translated coordinates, invalid boundaries,
spacing conversion, boundary exclusion, and invalid/extreme spacing limits.
Recovery tests cover all pick-order and direction combinations for interrupted
rectangles, rotated rectangles, trapezoids, and translated outlines. They also
reject duplicate supporting sides, out-of-bounds segments, crossed selections,
parallel side sets, and disconnected concave outlines.
Generated test files are ignored under `tests/build/`.

Interactive selection, sketch editing, transaction rollback, and printed line
appearance still need a live Inventor drawing check. Suggested checks:

1. Run on a rectangle with a spacing that lands on its center; confirm that
   the center axes are bold, use a long-dash-dot pattern, and have no duplicate
   ordinary lines. Check an existing `2D-Grid-Center` layer as well: new center
   entities must use the pattern even if that layer was previously continuous.
2. Run on a rotated rectangle and trapezoid; inspect all clipped endpoints.
   Repeat with one shortened segment per side, as when other geometry obscures
   the corners. Pick one segment on each outer side, not interior furniture or
   two portions of the same side. Confirm the recovered full outline is correct.
3. Cancel each input stage; confirm no new layer or sketch is retained.
4. Use spacing larger than the shape; confirm only the center axes and circle
   are created. Confirm circle diameter equals the smaller X/Y spacing at
   different view scales and that its dashed appearance matches the axes.
5. Undo a successful run once; confirm both sketch and newly created layers
   are removed together.

API references: [sheet-to-sketch conversion](https://help.autodesk.com/cloudhelp/2019/ENU/Inventor-API/files/DrawingSketch_SheetToSketchSpace.htm),
[sketch line formatting](https://help.autodesk.com/cloudhelp/2019/ENU/Inventor-API/files/SketchLine.htm),
[document update result](https://help.autodesk.com/cloudhelp/2024/ENU/Inventor-API/files/DrawingDocument_Update2.htm).

## Shared project memory

Read [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) for decisions and current status,
and [context/README.md](context/README.md) for the portable discussion history
and SQLite database. Update these records after substantive project work.
Rebuild and verify the database with Python 3.9+:

```text
python tools/update_context.py
python tools/update_context.py --check
```

Commit the context artifacts with authorized project commits and push when
authorized so another computer can receive them by cloning or pulling.
