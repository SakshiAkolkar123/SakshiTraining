using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBasics
{
    public class CmdFamilyBasics : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1) Collect all FamilyInstance elements
                var instances = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();

                if (instances.Count == 0)
                {
                    TaskDialog.Show("FamilyBasics", "No FamilyInstance elements found in the active document.");
                    return Result.Succeeded;
                }

                // 2) Group by Category
                var groups = instances
                    .Where(fi => fi.Category != null)
                    .GroupBy(fi => fi.Category.Name)
                    .OrderBy(g => g.Key)
                    .ToList();

                // 3) Prepare output folder
                string projectNameSafe = MakeSafeName(doc.Title);
                string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                              "RevitExports", "FamilyBasics", projectNameSafe);
                Directory.CreateDirectory(baseDir);

                // 4) Write per-category files and count
                var summaryLines = new List<string>();
                foreach (var group in groups)
                {
                    string categoryNameSafe = MakeSafeName(group.Key);
                    string filePath = Path.Combine(baseDir, $"{categoryNameSafe}.txt");

                    var sb = new StringBuilder();
                    sb.AppendLine($"Category: {group.Key}");
                    sb.AppendLine($"Count: {group.Count()}");
                    sb.AppendLine(new string('-', 40));

                    foreach (var fi in group)
                    {
                        AppendFamilyInstanceInfo(doc, fi, sb);
                        AppendSimpleGeometryInfo(doc, fi, sb);
                        sb.AppendLine();
                    }

                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                    summaryLines.Add($"{group.Key}: {group.Count()} -> {filePath}");
                }

                // 5) Write a summary file
                File.WriteAllLines(Path.Combine(baseDir, "_Summary.txt"), summaryLines, Encoding.UTF8);

                TaskDialog.Show("FamilyBasics", $"Export complete.\nFolder:\n{baseDir}");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private static void AppendFamilyInstanceInfo(Document doc, FamilyInstance fi, StringBuilder sb)
        {
            // Basic info
            string famName = fi.Symbol?.Family?.Name ?? "(No Family)";
            string symName = fi.Symbol?.Name ?? "(No Symbol)";
            string elemId = fi.Id.IntegerValue.ToString();

            // Instance vs type
            string typeOrInstance = fi is ElementType ? "Type" : "Instance";

            sb.AppendLine($"Family Name: {famName}");
            sb.AppendLine($"Family Symbol Name: {symName}");
            sb.AppendLine($"ElementId: {elemId}");
            sb.AppendLine($"Is Element Type or Instance: {typeOrInstance}");
        }

        private static void AppendSimpleGeometryInfo(Document doc, FamilyInstance fi, StringBuilder sb)
        {
            // Geometry extraction options
            var opt = new Options
            {
                ComputeReferences = false,
                IncludeNonVisibleObjects = false,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geomElem = fi.get_Geometry(opt);

            var solids = new List<Solid>();
            CollectSolids(geomElem, solids);

            int faceCount = 0;
            int edgeCount = 0;
            double totalVolume = 0.0;
            double totalArea = 0.0;

            foreach (var solid in solids)
            {
                if (solid == null || solid.Faces.IsEmpty || solid.Edges.IsEmpty) continue;
                faceCount += solid.Faces.Size;
                edgeCount += solid.Edges.Size;
                if (solid.Volume > 0) totalVolume += solid.Volume;
                if (solid.SurfaceArea > 0) totalArea += solid.SurfaceArea;
            }

            sb.AppendLine("Geometry:");
            sb.AppendLine($"  Solids: {solids.Count}");
            sb.AppendLine($"  Faces: {faceCount}");
            sb.AppendLine($"  Edges: {edgeCount}");

            // Show volume/area if available (in Revit internal units; you may convert if desired)
            if (totalVolume > 0)
            {
                sb.AppendLine($"  Total Volume (internal units): {totalVolume}");
            }
            else
            {
                sb.AppendLine("  Total Volume: (not available)");
            }

            if (totalArea > 0)
            {
                sb.AppendLine($"  Total Surface Area (internal units): {totalArea}");
            }
            else
            {
                sb.AppendLine("  Total Surface Area: (not available)");
            }
        }

        private static void CollectSolids(GeometryElement geomElem, List<Solid> solids)
        {
            if (geomElem == null) return;

            foreach (GeometryObject obj in geomElem)
            {
                switch (obj)
                {
                    case Solid solid:
                        if (solid.Volume > 0) solids.Add(solid);
                        break;
                    case GeometryInstance gi:
                        GeometryElement instGeom = gi.GetInstanceGeometry();
                        CollectSolids(instGeom, solids);

                        GeometryElement symGeom = gi.GetSymbolGeometry();
                        CollectSolids(symGeom, solids);
                        break;
                    case Mesh mesh:
                        // Ignored per requirement: retrieve solids only
                        break;
                }
            }
        }

        private static string MakeSafeName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}

