using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitAssignment3.Command
{
    [Transaction(TransactionMode.Manual)]
    public class ParameterInspectorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 3) Prompt user to select any Revit element
                Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Select any Revit element");
                Element elem = doc.GetElement(pickedRef);

                // 4/5) Collect all parameters (built-in + shared + project/family)
                IList<Parameter> parameters = GetAllParameters(elem);

                // 7) Build readable message
                var sb = new StringBuilder();
                sb.AppendLine($"Element: {elem.Name} (Id: {elem.Id.IntegerValue})");
                sb.AppendLine($"Category: {elem.Category?.Name ?? "(None)"}");
                sb.AppendLine(new string('-', 70));

                foreach (Parameter p in parameters)
                {
                    string paramName = SafeDefinitionName(p);
                    string storage = p.StorageType.ToString(); //  Storage type
                    bool isReadOnly = p.IsReadOnly;            //  Read-only flag
                    string value = GetParameterValueString(p, doc); //  Value
                    string origin = GetParameterOrigin(p, doc);     // built-in/shared/project/family
                    //string bindingInfo = GetBindingInfo(p.Definition, doc); // optional binding

                    sb.AppendLine($"• {paramName} [{storage}] {(isReadOnly ? "Read-only" : "Read/Write")}");
                    sb.AppendLine($"   Value: {value}");
                    if (!string.IsNullOrEmpty(origin))
                        sb.AppendLine($"   Kind: {origin}");
                   // if (!string.IsNullOrEmpty(bindingInfo))
                       // sb.AppendLine($"   Binding: {bindingInfo}");
                }

                // 8) Show output in a TaskDialog
                var td = new TaskDialog("Parameter Information")
                {
                    MainInstruction = "Parameters of selected element",
                    MainContent = sb.ToString()
                };
                td.Show();

                return Result.Succeeded;
            }
            catch (OperationCanceledException)
            {
                // Handle cancel gracefully(.NET)
                TaskDialog.Show("Parameter Information", "Selection canceled.");
                return Result.Cancelled;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Revit-specific cancel exception
                TaskDialog.Show("Parameter Information", "Selection canceled.");
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                // Any other error
                message = ex.Message;
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }
        // Retrieves all parameters; prefers GetOrderedParameters (newer Revit),
        // falls back to Element.Parameters for earlier versions
        private static IList<Parameter> GetAllParameters(Element e)
        {
            // Try newer API first
            try
            {
                var ordered = e.GetOrderedParameters();
                if (ordered != null && ordered.Count > 0)
                    return ordered;
            }
            catch
            {
                // Older Revit versions
            }

            // Fallback
            var list = new List<Parameter>();
            foreach (Parameter p in e.Parameters)
            {
                list.Add(p);
            }
            // Consistent ordering by parameter name
            return list.OrderBy(p => p.Definition?.Name ?? string.Empty).ToList();
        }
        

        private static string SafeDefinitionName(Parameter p)
        {
            try
            {
                return p.Definition?.Name ?? "(Unnamed)";
            }
            catch
            {
                return "(Unknown Definition)";
            }
        }

        /// Returns a human-readable value for the parameter.
        /// Uses AsValueString when available to respect project units.
        private static string GetParameterValueString(Parameter p, Document doc)
        {
            // Prefer units-aware display
            try
            {
                string display = p.AsValueString();
                if (!string.IsNullOrEmpty(display))
                    return display;
            }
            catch { /* ignore */ }

            // Raw value fallback
            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.AsString() ?? string.Empty;

                case StorageType.Integer:
                    return p.AsInteger().ToString();

                case StorageType.Double:
                    // Raw double; may represent length/area/volume/etc. in internal units (feet).
                    return p.AsDouble().ToString("G6");

                case StorageType.ElementId:
                    ElementId id = p.AsElementId();
                    if (id == ElementId.InvalidElementId) return "InvalidElementId";

                    Element refEl = doc.GetElement(id);
                    if (refEl != null)
                        return $"{refEl.Name} (Id {id.IntegerValue})";

                    // Some ElementId values may point to categories, etc.
                    Category cat = Category.GetCategory(doc, id);
                    if (cat != null)
                        return $"{cat.Name} (Category)";

                    return id.IntegerValue.ToString();

                case StorageType.None:
                default:
                    return "(None)";
            }
        }

        /// Distinguishes Built-in vs Shared vs Project/Family (Other).

        private static string GetParameterOrigin(Parameter p, Document doc)
        {
            // Shared parameter?
            if (p.IsShared)
            {
                Guid g = p.GUID;
                return g != Guid.Empty ? $"Shared (GUID {g})" : "Shared";
            }

            // Built-in parameter via InternalDefinition
            try
            {
                var internalDef = p.Definition as InternalDefinition;
                if (internalDef != null && internalDef.BuiltInParameter != BuiltInParameter.INVALID)
                {
                    return $"Built-in ({internalDef.BuiltInParameter})";
                }
            }
            catch { /* ignore */ }

            // Project parameter (exists in Document.ParameterBindings)?
            try
            {
                BindingMap map = doc.ParameterBindings;
                Definition def = p.Definition;
                if (map != null && def != null)
                {
                    // Some Revit versions support get_Item/Contains; otherwise iterate
                    if (MapContains(map, def))
                        return "Project Parameter";
                }
            }
            catch { /* ignore */ }

            // Otherwise, likely family/other
            return "Family/Other";
        }

        /// Tries to check whether the map contains the given definition across Revit versions.
        private static bool MapContains(BindingMap map, Definition def)
        {
            try
            {
                // If Contains exists
                var containsMethod = typeof(BindingMap).GetMethod("Contains");
                if (containsMethod != null)
                {
                    return (bool)containsMethod.Invoke(map, new object[] { def });
                }
            }
            catch { /* ignore */ }

            // Fallback: iterate
            var it = map.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Key == def)
                    return true;
            }
            return false;
        }

        /// Tries to retrieve the binding for the given definition across Revit versions.
        private static Binding TryGetBinding(BindingMap map, Definition def)
        {
            // Try indexer/get_Item first
            try
            {
                var getter = typeof(BindingMap).GetMethod("get_Item");
                if (getter != null)
                {
                    return (Binding)getter.Invoke(map, new object[] { def });
                }
            }
            catch { /* ignore */ }

            // Fallback: iterate
            var it = map.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Key == def)
                    return it.Current as Binding;
            }
            return null;
        }
    }


}

