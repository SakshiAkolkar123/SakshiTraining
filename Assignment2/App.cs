using System;
using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace Assignment2
{
    // Creates the tab "Revit training" and panel "Basic commands", and add 2 btns to panel
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // 1) Create the Ribbon Tab
            const string tabName = "Revit training";
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch
            {
                // Tab might already exist — ignore
            }

            // 2) Create the Ribbon Panel
            const string panelName = "Basic commands";
            RibbonPanel panel = null;
            try
            {
                panel = application.CreateRibbonPanel(tabName, panelName);
            }
            catch
            {
                // If panel already exists, find it
                foreach (RibbonPanel p in application.GetRibbonPanels(tabName))
                {
                    if (p.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase))
                    {
                        panel = p;
                        break;
                    }
                }
            }

            // Resolve the assembly path (used for icons relative to DLL)
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath) ?? "";
            string icon16Path = Path.Combine(assemblyDir, "Resources", "icon16.png");
            string icon32Path = Path.Combine(assemblyDir, "Resources", "icon32.png");


            var sakshiBtnData = new PushButtonData(
                name: "HelloSakshiBtn",
                text: "Hello World",
                assemblyName: assemblyPath,
                className: "Assignment2.CmdHelloWorld"
            )
            {
                ToolTip = "Shows a message: Hello World"
            };

            var revitBtnData = new PushButtonData(
                name: "HelloRevitBtn",
                text: "Second Tool",
                assemblyName: assemblyPath,
                className: "Assignment2.CmdSecondTool"
            )
            {
                ToolTip = "Shows a message: Second Tool"
            };


            // 5) Add buttons to the panel
            PushButton sakshiBtn = panel.AddItem(sakshiBtnData) as PushButton;
            PushButton revitBtn = panel.AddItem(revitBtnData) as PushButton;

            // 6) Assign icons (small = 16×16, large = 32×32)
            if (sakshiBtn != null && File.Exists(icon16Path))
            {
                sakshiBtn.Image = LoadPng(icon16Path);        // 16x16
                // Optional large image for hover
                if (File.Exists(icon32Path))
                    sakshiBtn.LargeImage = LoadPng(icon32Path); // 32x32
            }

            if (revitBtn != null && File.Exists(icon32Path))
            {
                revitBtn.LargeImage = LoadPng(icon32Path);     // 32x32
                // Optional small image for compact display
                if (File.Exists(icon16Path))
                    revitBtn.Image = LoadPng(icon16Path);      // 16x16
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        // Loads a PNG from disk into a BitmapSource suitable for Revit ribbon icons.
        private static BitmapSource LoadPng(string path)
        {
            var img = new BitmapImage();

            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriSource = new Uri(path, UriKind.Absolute);

            return img;
        }
    }
}
