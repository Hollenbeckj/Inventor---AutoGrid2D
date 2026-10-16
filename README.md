# AutoGrid2D

`CreateGrid2D.iLogicVb` is an external Inventor iLogic rule for drawing views.

Select four straight edges in one view, in any order. The edges must form one
closed four-sided boundary without crossings or collinear corners. Enter model
spacing along drawing-sheet X and Y; explicit units such as `12 in` or `300 mm`
are supported. Bare numbers use the drawing document's length units.

The rule creates a view sketch named `Grid` containing:

- Gray grid lines, clipped to the selected boundary, at 0.18 mm line weight.
- Black horizontal and vertical center axes, clipped to the entire boundary,
  at 0.70 mm line weight. They pass through the polygon's area centroid.

The center is the area centroid, which differs from the bounding-box center for
many trapezoids and irregular quadrilaterals. For a rectangle it is the ordinary
center. In a strongly concave shape the centroid can lie outside the outline;
the rule explains this and clips each axis to the interior rather than drawing
across the exterior. These axes are sketch lines, not native hole centermark
annotations. They follow the sheet axes, including in a rotated view.

Grid positions start one spacing from the left/bottom sheet-space bounding
extents. A bold center axis replaces any coincident ordinary grid line. If the
spacing is too large for either family of grid lines, the rule explains that
it will create only the center axes; it never creates an empty sketch.

Layers are `2D-Grid` and `2D-Grid-Center`. New layers are visible and printable.
Existing layer formatting is preserved; the new entities receive explicit
color, continuous line type, and weight overrides. Existing layers must already
be visible and printable. The weights are constants at the top of the rule.

All selection, validation, input, clipping, and count checks precede document
changes. Layer creation and sketch creation share one transaction. Document
update must succeed before committing. Cleanup errors are reported rather than
silently hidden. There is a limit of 2,000 grid positions and 2,000 final segments
(including center axes) to bound creation time.

The geometry is a snapshot, not an associative model constraint. After a model
boundary change, delete or rename `Grid` and run the rule again. Model-distance
spacing assumes an ordinary orthographic view with a uniform scale; broken or
perspective views are not supported spacing workflows.

## Validation

Run `tests/Run-Tests.ps1` from PowerShell. It compiles the complete rule against
the locally installed Inventor 2027 interop, then exercises the actual geometry
helpers without starting Inventor. It needs the Windows .NET Framework VB
compiler and the Inventor 2027 installation paths specified in that script.

The suite currently passes 9,211 assertions, including all 384 pick-order and
edge-direction combinations for each of four shapes, clipped interval samples,
known centroid coordinates, translated coordinates, invalid boundaries,
spacing conversion, boundary exclusion, and invalid/extreme spacing limits.
Generated test files are ignored under `tests/build/`.

Interactive selection, sketch editing, transaction rollback, and printed line
appearance still need a live Inventor drawing check. Suggested checks:

1. Run on a rectangle with a spacing that lands on its center; confirm that
   the center axes are bold and have no duplicate ordinary lines.
2. Run on a rotated rectangle and trapezoid; inspect all clipped endpoints.
3. Cancel each input stage; confirm no new layer or sketch is retained.
4. Use spacing larger than the shape; confirm only the center axes are created.
5. Undo a successful run once; confirm both sketch and newly created layers
   are removed together.

API references: [sheet-to-sketch conversion](https://help.autodesk.com/cloudhelp/2019/ENU/Inventor-API/files/DrawingSketch_SheetToSketchSpace.htm),
[sketch line formatting](https://help.autodesk.com/cloudhelp/2019/ENU/Inventor-API/files/SketchLine.htm),
[document update result](https://help.autodesk.com/cloudhelp/2024/ENU/Inventor-API/files/DrawingDocument_Update2.htm).
