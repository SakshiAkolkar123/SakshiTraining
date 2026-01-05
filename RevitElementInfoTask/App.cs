using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace RevitElementInfoTask
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            const string TabName = "Element Info";
            const string PanelName = "Element Info Panel";

            // Create a new ribbon tab and panel
            application.CreateRibbonTab(TabName);
            var panel = application.CreateRibbonPanel(TabName, PanelName);

            // Button data
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            var buttonData = new PushButtonData(
                "ElementInfoCommandId",
                "Element",
                assemblyPath,
                "RevitElementInfoTask.Command.ElementInfoCommand"
            );

            // Add the button
            panel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}
