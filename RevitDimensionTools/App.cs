using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitDimensionTools
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom tab + panel
            const string tabName = "Revit Tools";
            try { application.CreateRibbonTab(tabName); } 
            catch { /* tab may already exist */ }
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Dimensions");

            //button 
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            var pbd = new PushButtonData(
              "BtnDimensionAllWalls",
              "Add Dimensions",
              assemblyPath,
              "RevitDimensionTools.DimensionAllWallsCommand");

            // Optional icon
            PushButton btn = panel.AddItem(pbd) as PushButton;
            btn!.ToolTip = "Place aligned dimensions for all walls in the active view.";

            // (Optional) btn.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitDimensionTools;component/Resources/icon32.png"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }
}
