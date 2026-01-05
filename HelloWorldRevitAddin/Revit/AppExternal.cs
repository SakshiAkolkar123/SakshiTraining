using Autodesk.Revit.UI;

using System;

namespace HelloWorldRevitAddin.Revit
{
    public class AppExternal : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            const string tabName = "HelloWorld";
            try { app.CreateRibbonTab(tabName); } catch { /* tab may exist */ }

            var panel = app.CreateRibbonPanel(tabName, "Demo");
            var buttonData = new PushButtonData(
                "HelloWorldButton",
                "Hello World",
                System.Reflection.Assembly.GetExecutingAssembly().Location,
                typeof(HelloWorldCommand).FullName);

            panel.AddItem(buttonData);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication app) => Result.Succeeded;
    }
}
