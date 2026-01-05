using Autodesk.Revit.UI;

using System.Reflection;

namespace RevitAssignment3
{
    public class App : IExternalApplication
    {
        private const string TabName = "Parameter Information";
        private const string PanelName = "Tools";

        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                app.CreateRibbonTab(TabName);
            }
            catch (ArgumentException)
            {
                // Tab already exists
            }

            var existingPanels = app.GetRibbonPanels(TabName);
            RibbonPanel panel = existingPanels.FirstOrDefault(p => p.Name == PanelName)
                                ?? app.CreateRibbonPanel(TabName, PanelName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var pbd = new PushButtonData(
                name: "btnParameterInformation",
                text: "Parameter Information",
                assemblyName: assemblyPath,
                className: "RevitAssignment3.Command.ParameterInspectorCommand" // ✅ fixed
            )
            {
                ToolTip = "Select an element to list all its parameters (built-in, shared, project/family)."
            };

            panel.AddItem(pbd);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            return Result.Succeeded;
        }
    }
}
