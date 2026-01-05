using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementInfoEditable.MVVM.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace RevitElementInfoEditable.Services
{
    // Runs a Transaction and applies edited parameter values back to elements. its used for applying edits 
    public class ExternalEventBridge : IExternalEventHandler
    {
        private ObservableCollection<ElementParameterRow>? _pending;
        private UIApplication? _uiapp;
        private readonly ExternalEvent _externalEvent;

        public ExternalEventBridge()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        public void RaiseApply(ObservableCollection<ElementParameterRow> rows)
        {
            _pending = rows;
            _externalEvent.Raise();
        }

        public void Execute(UIApplication app)
        {
            _uiapp ??= app;
            var uidoc = _uiapp?.ActiveUIDocument;
            if (_pending is null || uidoc == null) return;

            var doc = uidoc.Document;

            using var tx = new Transaction(doc, "Apply instance parameter values");
            tx.Start();

            foreach (var r in _pending)
            {
                var el = doc.GetElement(new ElementId(r.ElementId));
                if (el is null) continue;

                var p = el.LookupParameter(r.ParameterName);
                if (p is null || p.IsReadOnly) continue; // skip read-only params

                try
                {
                    switch (p.StorageType)
                    {
                        case StorageType.String:
                            p.Set(r.Value ?? string.Empty);
                            break;

                        case StorageType.Integer:
                            // Handle Yes/No (boolean) and integers
                            if (TryParseYesNoOrInt(r.Value, out var iVal))
                                p.Set(iVal);
                            else
                                p.SetValueString(r.Value); // fallback
                            break;

                        case StorageType.Double:
                            // Unit-aware setting; Revit parses formatted strings (project units)
                            p.SetValueString(r.Value);
                            break;

                        case StorageType.ElementId:
                            if (int.TryParse(r.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                                p.Set(new ElementId(id));
                            else
                                p.SetValueString(r.Value); // fallback
                            break;

                        default:
                            p.SetValueString(r.Value);
                            break;
                    }
                }
                catch
                {
                    // You may accumulate failures to report after commit
                    continue;
                }
            }

            tx.Commit();
            _pending = null;
        }

        public string GetName() => "RevitElementInfoEditable ExternalEventBridge";

        private static bool TryParseYesNoOrInt(string? input, out int intValue)
        {
            intValue = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // Yes/No semantics: true/yes => 1, false/no => 0
            var s = input.Trim().ToLowerInvariant();
            if (s == "true" || s == "yes") { intValue = 1; return true; }
            if (s == "false" || s == "no") { intValue = 0; return true; }

            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue);
        }
    }
}
