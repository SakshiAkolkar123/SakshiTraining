using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace FamilyBasics
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Family Tools";
            try { application.CreateRibbonTab(tabName); } catch { /* tab may already exist */ }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Basics");

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            var pbData = new PushButtonData(
                "FamilyBasicsButton",
                "FamilyBasics",
                assemblyPath,
                "FamilyBasics.CmdFamilyBasics"
            )
            {
                ToolTip = "Export basic info and simple geometry of FamilyInstances per category."
            };

            panel.AddItem(pbData);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}
