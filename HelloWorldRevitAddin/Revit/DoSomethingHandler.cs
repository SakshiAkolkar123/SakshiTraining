
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace HelloWorldRevitAddin.Revit
{
    public class DoSomethingHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (var tx = new Transaction(doc, "Demo"))
            {
                tx.Start();
                // TODO: Revit API work here
                tx.Commit();
            }
        }

        public string GetName() => "HelloWorld External Event Handler";
    }
}
