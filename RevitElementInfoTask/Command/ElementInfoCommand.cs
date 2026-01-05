using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Data;
using System.IO;
using RevitElementInfoTask.MVVM.ViewModels;
using RevitElementInfoTask.MVVM.Views;

namespace RevitElementInfoTask.Command
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ElementInfoCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get document & selection
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds == null || selectedIds.Count == 0)
            {
                TaskDialog.Show("Element Info", "Select one or more elements and try again.");
                return Result.Succeeded;
            }

            // Build DataSet (acts as "configuration" XML)
            var ds = new DataSet("ElementInfo");
            var dt = new DataTable("Elements");
            dt.Columns.Add("ElementId", typeof(int));
            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("Family", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Level", typeof(string));
            dt.Columns.Add("Name", typeof(string));

            // Populate the table from selected elements
            foreach (var id in selectedIds)
            {
                var el = doc.GetElement(id);
                if (el == null) continue;

                // Category
                string category = el.Category?.Name ?? string.Empty;

                // Family
                string familyName = string.Empty;
                if (el is FamilyInstance fiForFamily)
                {
                    // In Revit 2025, Symbol is available on FamilyInstance; guard for null
                    familyName = fiForFamily.Symbol?.FamilyName ?? string.Empty;
                }
                else if (el is ElementType etForFamily)
                {
                    // ElementType.FamilyName is available for many types; guard if not
                    familyName = etForFamily.FamilyName ?? string.Empty;
                }

                // Type
                string typeName = string.Empty;
                if (el is FamilyInstance fiForType)
                {
                    typeName = fiForType.Symbol?.Name ?? string.Empty;
                }
                else if (el is ElementType etForType)
                {
                    typeName = etForType.Name ?? string.Empty;
                }
                else
                {
                    typeName = el.Name ?? string.Empty;
                }

                // Level
                string levelName = string.Empty;
                if (el is FamilyInstance fiForLevel)
                {
                    // FamilyInstance does not expose .Level; use LevelId
                    Level lvl = doc.GetElement(fiForLevel.LevelId) as Level;
                    levelName = lvl?.Name ?? string.Empty;
                }
                else
                {
                    Level lvl = null;

                    // Many elements expose LevelId directly
                    if (el.LevelId != ElementId.InvalidElementId)
                    {
                        lvl = doc.GetElement(el.LevelId) as Level;
                    }
                    else
                    {
                        // Fallback: try the LEVEL_PARAM
                        Parameter pLevel = el.get_Parameter(BuiltInParameter.LEVEL_PARAM);
                        if (pLevel != null && pLevel.StorageType == StorageType.ElementId)
                        {
                            var lid = pLevel.AsElementId();
                            if (lid != ElementId.InvalidElementId)
                                lvl = doc.GetElement(lid) as Level;
                        }
                    }

                    levelName = lvl?.Name ?? string.Empty;
                }

                dt.Rows.Add(id.IntegerValue, category, familyName, typeName, levelName, el.Name ?? string.Empty);
            }

            // Add the table to the dataset
            ds.Tables.Add(dt);

            // Write XML (with schema)
            string xmlPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ElementInfo.config.xml"
            );
            ds.WriteXml(xmlPath, XmlWriteMode.WriteSchema);

            // Open MVVM window and bind from XML DataSet (modeless)
            var vm = new ElementInfoViewModel(xmlPath);
            var win = new MainWindow { DataContext = vm };
            win.Show();

            return Result.Succeeded;
        }
    }
}