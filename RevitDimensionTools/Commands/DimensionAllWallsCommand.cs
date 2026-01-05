using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitDimensionTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DimensionAllWallsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            // Dimensions cannot be placed in a generic 3D view 
            if (view == null || view.ViewType == ViewType.ThreeD)
            {
                message = "Please run in a plan/section/elevation view.";
                return Result.Cancelled;
            }

            //Collect all visible walls in the current view
            var walls = new FilteredElementCollector(doc, view.Id)
              .OfCategory(BuiltInCategory.OST_Walls)
              .WhereElementIsNotElementType()
              .Cast<Wall>()
              .ToList();

            if (walls.Count == 0)
            {
                TaskDialog.Show("Add Dimensions", "No walls found in the active view.");
                return Result.Succeeded;
            }
            // Use a TransactionGroup to wrap per‑wall transactions
            int created = 0;
            using (TransactionGroup tg = new TransactionGroup(doc, "Add Dimensions for All Walls"))
            {
                tg.Start();

                foreach (var wall in walls)
                {
                    try
                    {
                        // Direction along the wall 
                        LocationCurve? lc = wall.Location as LocationCurve;
                        if (lc == null) continue;

                        Curve c = lc.Curve;
                        XYZ start = c.GetEndPoint(0);
                        XYZ end = c.GetEndPoint(1);
                        XYZ dir = (end - start).Normalize();

                        // Outward normal of the wall
                        XYZ outward = wall.Orientation; // exterior side normal at start
                        if (outward == null) outward = XYZ.BasisY;

                        XYZ offsetVec = outward.Normalize().Multiply(UnitUtils.ConvertToInternalUnits(150.0, UnitTypeId.Millimeters));

                        // Find references of the two end-cap faces (planar faces with normal parallel to dir)
                        (Reference? rStart, Reference? rEnd) = GetEndFaceReferences(doc, wall, start, end, dir);
                        if (rStart == null || rEnd == null) continue;

                        // Build the dimension line (along the wall direction, slightly offset outwards)
                        Line dimLine = Line.CreateBound(start + offsetVec, end + offsetVec);

                        // Create the dimension
                        using (Transaction t = new Transaction(doc, $"Dimension wall {wall.Id.IntegerValue}"))
                        {
                            t.Start();
                            ReferenceArray refs = new ReferenceArray();
                            refs.Append(rStart);
                            refs.Append(rEnd);

                            Dimension dim = doc.Create.NewDimension(view, dimLine, refs);
                            // Optionally set a specific DimensionType: dim.DimensionType = ...
                            t.Commit();
                            created++;
                        }
                    }
                    catch
                    {
                        // Skip problematic walls and continue
                    }
                }

                tg.Assimilate();
            }

            TaskDialog.Show("Add Dimensions", $"Created {created} wall dimensions.");
            return Result.Succeeded;
        }

        /// Returns references to the two "end-cap" faces of a wall 
        private static (Reference? rStart, Reference? rEnd) GetEndFaceReferences(Document doc, Wall wall, XYZ start, XYZ end, XYZ dir)
        {
            Reference? rStart = null;
            Reference? rEnd = null;
            double bestStart = double.MaxValue;
            double bestEnd = double.MaxValue;

            Options opt = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };

            GeometryElement geo = wall.get_Geometry(opt);
            foreach (GeometryObject go in geo)
            {
                Solid? solid = go as Solid;
                if (solid == null || solid.Faces.Size == 0) continue;

                foreach (Face f in solid.Faces)
                {
                    PlanarFace? pf = f as PlanarFace;
                    if (pf == null) continue;

                    // Face normal pointing out of solid
                    XYZ n = pf.FaceNormal.Normalize(); // planar face normal
                    double alignment = Math.Abs(n.DotProduct(dir)); // near 1 when parallel to wall direction

                    // We look for faces whose normals are parallel to wall direction (end caps)
                    if (alignment > 0.999) 
                    {
                        XYZ origin = pf.Origin;
                        double ds = origin.DistanceTo(start);
                        double de = origin.DistanceTo(end);

                        if (ds < bestStart)
                        {
                            bestStart = ds;
                            rStart = pf.Reference; // stable geometry reference needed by NewDimension
                        }
                        if (de < bestEnd)
                        {
                            bestEnd = de;
                            rEnd = pf.Reference;
                        }
                    }
                }
            }
            return (rStart, rEnd);
        }
    }
}
