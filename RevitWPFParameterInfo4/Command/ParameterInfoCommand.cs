
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Interop;

namespace RevitWPFParameterInfo4.Command
{
    // Revit will load and run this command; it must be public.
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ParameterInfoCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            try
            {
                // Let the user pick any element
                var reference = uidoc.Selection.PickObject(ObjectType.Element, "Select any element to view parameters.");
                if (reference == null) return Result.Cancelled;

                var element = doc.GetElement(reference);
                if (element == null)
                {
                    message = "No element was retrieved.";
                    return Result.Failed;
                }

                // Gather parameter info list
                var list = GetAllParameterInfos(element, doc);

                // Prepare ViewModel
                var vm = new MVVM.ViewModels.ParameterInfoViewModel
                {
                    ElementName = GetElementDisplayName(element, doc),
                    Parameters = new System.Collections.ObjectModel.ObservableCollection<MVVM.Models.ParameterInfo>(list)
                };

                // Create & show WPF dialog (owned by Revit)
                var win = new MVVM.Views.ParameterInfoWindow
                {
                    DataContext = vm
                };

                IntPtr revitHandle = Autodesk.Windows.ComponentManager.ApplicationWindow;
                new WindowInteropHelper(win).Owner = revitHandle;

                win.ShowDialog();
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User pressed ESC during selection
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = $"Unexpected error: {ex.Message}";
                return Result.Failed;
            }
        }

        // ---------- Helpers ----------

        private static string GetElementDisplayName(Element e, Document doc)
        {
            string cat = e.Category?.Name ?? "(No Category)";
            string name = e.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                // Some elements don't have Name; use type name
                name = e.GetType().Name;
            }
            return $"{cat} — {name} (Id {e.Id.IntegerValue})";
        }

        private static IEnumerable<MVVM.Models.ParameterInfo> GetAllParameterInfos(Element element, Document doc)
        {
            var list = new List<MVVM.Models.ParameterInfo>();

            foreach (Parameter p in element.Parameters)
            {
                if (p == null) continue;

                var def = p.Definition;
                string paramName = def?.Name ?? "(Unnamed Parameter)";
                string storageType = p.StorageType.ToString();
                string value = GetParameterValueString(p, doc);
                bool isReadOnly = p.IsReadOnly;

                list.Add(new MVVM.Models.ParameterInfo
                {
                    Name = paramName,
                    Value = value,
                    StorageType = storageType,
                    IsReadOnly = isReadOnly
                });
            }

            // Sort by name for nicer display
            return list.OrderBy(x => x.Name ?? string.Empty);
        }

        private static string GetParameterValueString(Parameter p, Document doc)
        {
            // Prefer Revit's formatted string (unit-aware)
            string formatted = null;
            try { formatted = p.AsValueString(); } catch { /* not all params support it */ }

            if (!string.IsNullOrWhiteSpace(formatted))
                return formatted;

            switch (p.StorageType)
            {
                case StorageType.String:
                    return p.AsString() ?? string.Empty;

                case StorageType.Double:
                    // raw double fallback
                    return p.AsDouble().ToString(CultureInfo.InvariantCulture);

                case StorageType.Integer:
                    return p.AsInteger().ToString(CultureInfo.InvariantCulture);

                case StorageType.ElementId:
                    var id = p.AsElementId();
                    if (id == ElementId.InvalidElementId)
                        return "(Invalid Id)";
                    var el = doc.GetElement(id);
                    return el != null
                        ? $"{el.Name} (Id {id.IntegerValue})"
                        : $"ElementId {id.IntegerValue}";

                case StorageType.None:
                default:
                    return "(None)";
            }
        }
    }
}
