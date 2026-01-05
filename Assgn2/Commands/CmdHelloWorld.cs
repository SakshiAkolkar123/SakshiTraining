
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace Assgn2
{
    [Transaction(TransactionMode.Manual)]
    public class CmdHelloWorld : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            MessageBox.Show("Hello World", "Revit Training", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return Result.Succeeded;
        }
    }
}
