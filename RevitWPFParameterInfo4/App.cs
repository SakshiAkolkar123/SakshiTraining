
using System;

using Autodesk.Revit.UI;

namespace RevitWPFParameterInfo4
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            const string tabName = "Parameter Information";
            const string panelName = "Tool";

            try { app.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel = null;
            foreach (var p in app.GetRibbonPanels(tabName))
            {
                if (p.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase))
                {
                    panel = p;
                    break;
                }
            }
            if (panel == null) panel = app.CreateRibbonPanel(tabName, panelName);

            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            var pbData = new PushButtonData(
                name: "ParameterInfoButton",
                text: "Show Parameters",
                assemblyName: assemblyPath,
                // IMPORTANT: Full namespace where the command lives
                className: "RevitWPFParameterInfo4.Command.ParameterInfoCommand"
            );

            var pb = panel.AddItem(pbData) as PushButton;
            if (pb != null)
            {
                pb.ToolTip = "Select any element to view all parameters in a WPF window.";
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
