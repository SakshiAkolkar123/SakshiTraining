using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace Assignment2
{
    [Transaction(TransactionMode.Manual)]
    public class CmdSecondTool : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,ref string message,ElementSet elements)
        {
            MessageBox.Show("Second Tool", "Revit Training", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return Result.Succeeded;
        }
    }
}
