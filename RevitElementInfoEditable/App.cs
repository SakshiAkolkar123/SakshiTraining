using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using RevitElementInfoEditable.MVVM.ViewModels;

namespace RevitElementInfoEditable
{
    public class App : IExternalApplication
    {
        //A stable identifier for your Dockable Pane.
        public static readonly Guid PaneGuid = new("A0B9E609-22D1-4B3C-9D6D-3D58C0B1F7E9");

        public static Services.ExternalEventBridge Bridge = null!;
        public static ElementInfoViewModel ViewModel { get; } = new ElementInfoViewModel();

        public Result OnStartup(UIControlledApplication application)
        {
            const string TabName = "Information";
            application.CreateRibbonTab(TabName);
            var panel = application.CreateRibbonPanel(TabName, "Element Data Viewer");

            string asmPath = Assembly.GetExecutingAssembly().Location;
            var pbd = new PushButtonData(
                "ElementButton",
                "Element Data",
                asmPath,
                "RevitElementInfoEditable.Commands.ElementCommand");

            var btn = panel.AddItem(pbd) as PushButton;
            btn.ToolTip = "List all instance parameters for the selected element.";

            // Optional icon (file-system example)
            btn.LargeImage = LoadBitmap(@"D:\SakshiTraining\RevitElementInfoEditable\Resources\images\RevitBtn.jpg", 32, 32);
            

            // Register dockable pane once UI is ready
            application.ControlledApplication.ApplicationInitialized += (s, e) =>
            {
                var id = new DockablePaneId(PaneGuid);
                var title = "Element Info";
                var page = new MVVM.Views.ElementInfoPane();

                var data = new DockablePaneProviderData
                {
                    FrameworkElement = page,
                    InitialState = new DockablePaneState { DockPosition = DockPosition.Tabbed }
                };

                application.RegisterDockablePane(id, title, page as IDockablePaneProvider);
            };

            Bridge = new Services.ExternalEventBridge();
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        private static BitmapImage LoadBitmap(string path, int w, int h)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.DecodePixelWidth = w;
            bmp.DecodePixelHeight = h;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze(); // thread-safe
            return bmp;
        }
    }
}
