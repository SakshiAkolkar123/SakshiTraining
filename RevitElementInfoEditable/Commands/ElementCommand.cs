
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitElementInfoEditable.MVVM.Models;

using System.Collections.Generic;
using System.Linq;

namespace RevitElementInfoEditable.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ElementCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            var uiapp = data.Application;
            var uidoc = uiapp.ActiveUIDocument;
            if (uidoc == null)
            {
                TaskDialog.Show("Element Info", "No active document.");
                return Result.Cancelled;
            }

            var doc = uidoc.Document;
            var ids = uidoc.Selection.GetElementIds();

            // Enforce single element selection
            if (ids.Count != 1)
            {
                TaskDialog.Show("Element Info",
                    "Please select exactly one element, then click the button again.");
                return Result.Succeeded;
            }

            var id = ids.First();
            var el = doc.GetElement(id);
            if (el is null)
            {
                TaskDialog.Show("Element Info", "Selected element was not found.");
                return Result.Succeeded;
            }

            // Collect all editable instance parameters for this element
            var rows = new List<ElementParameterRow>();
            foreach (Parameter p in el.Parameters)
            {
                if (p == null || p.IsReadOnly) continue;

                rows.Add(new ElementParameterRow
                {
                    ElementId = id.IntegerValue,
                    ElementName = el.Name,
                    ParameterName = p.Definition?.Name ?? "(Unnamed)",
                    StorageType = p.StorageType.ToString(),
                    Value = ReadParamValueString(p)
                });
            }

            // Push into shared VM and show pane
            var vm = RevitElementInfoEditable.App.ViewModel;
            vm.Rows.Clear();
            foreach (var r in rows) vm.Rows.Add(r);

            var paneId = new DockablePaneId(RevitElementInfoEditable.App.PaneGuid);
            var pane = uiapp.GetDockablePane(paneId);
            pane.Show();

            return Result.Succeeded;
        }

        private static string ReadParamValueString(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.String: return p.AsString() ?? "";
                case StorageType.Integer: return p.AsInteger().ToString();
                case StorageType.Double: return p.AsValueString() ?? p.AsDouble().ToString();
                case StorageType.ElementId:
                    var eid = p.AsElementId();
                    return (eid?.IntegerValue ?? 0).ToString();
                default: return p.AsValueString() ?? "";
            }
        }
    }
}
