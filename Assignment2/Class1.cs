using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection;
using System.Resources;
using System.Windows.Media.Imaging;


namespace MyCompany.RevitTraining
{
    public class Class1 : IExternalApplication
    {
        private const string TabName = "Revit Training";
        private const string PanelName = "Basic Commands";
        private const string V = "D:\\SakshiTraining\\Assignment2\\Resources\\HelloWorld_16.png";

        public Result OnStartup(UIControlledApplication application)
        {
            // 1) Create the Ribbon Tab
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (ArgumentException)
            {
                // Tab already exists — ignore
            }

            // 2) Create / get the Ribbon Panel
            RibbonPanel panel = null;
            try
            {
                // Revit API provides this overload in modern versions:
                panel = application.CreateRibbonPanel(TabName, PanelName);
            }
            catch
            {
                // Fallback: try to find an existing panel with the same name under the tab
                var panels = application.GetRibbonPanels(TabName);
                panel = panels.FirstOrDefault(p => p.Name.Equals(PanelName, StringComparison.OrdinalIgnoreCase));
                // As a last resort, create under Add-Ins tab (older API)
                if (panel == null)
                    panel = application.CreateRibbonPanel(PanelName);
            }

            // 3) Create PushButtonData items for the two commands
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var helloData = new PushButtonData(
                name: "HelloWorld",                 // internal unique name
                text: "Hello World",                // button text
                assemblyName: assemblyPath,
                className: "MyCompany.RevitTraining.Commands.HelloWorldCommand"
            )
            {
                ToolTip = "Show a greeting in a task dialog."
            };

            var secondData = new PushButtonData(
                name: "SecondTool",
                text: "Second Tool",
                assemblyName: assemblyPath,
                className: "MyCompany.RevitTraining.Commands.SecondToolCommand"
            )
            {
                ToolTip = "Show the tool name in a task dialog."
            };

            // 4) Load icons (16x16 -> Image, 32x32 -> LargeImage)
            // Icons are loaded from the output folder: <dll directory>/Resources/...
            //helloData.Image = Utils.ImageUtils.LoadPngImage("Resources/HelloWorld_16.png");
            //helloData.LargeImage = Utils.ImageUtils.LoadPngImage("Resources/HelloWorld_32.png");

            //secondData.Image = Utils.ImageUtils.LoadPngImage("Resources/SecondTool_16.png");
            //secondData.LargeImage = Utils.ImageUtils.LoadPngImage("Resources/SecondTool_32.png");

            helloData.Image = ImageUtils.LoadPngImage(V);
            // 5) Add buttons to the panel
            panel.AddItem(helloData);
            panel.AddItem(secondData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Nothing special to clean up
            return Result.Succeeded;
        }




        [Transaction(TransactionMode.Manual)]
        public class HelloWorldCommand : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                TaskDialog.Show("Revit Training", "Hello World");
                return Result.Succeeded;
            }
        }


        [Transaction(TransactionMode.Manual)]
        public class SecondToolCommand : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                TaskDialog.Show("Revit Training", "Second Tool");
                return Result.Succeeded;
            }
        }


        public static class ImageUtils
        {
            /// Loads a PNG from a path relative to the add-in assembly location and returns an ImageSource.
            public static BitmapImage LoadPngImage(string relativePath)
            {
                var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var fullPath = Path.Combine(asmDir ?? "", relativePath);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze(); // recommended for WPF images in Revit
                return bmp;
            }
        }
    }
}


