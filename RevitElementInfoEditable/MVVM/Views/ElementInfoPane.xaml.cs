
using Autodesk.Revit.UI;

using System.Windows.Controls;

namespace RevitElementInfoEditable.MVVM.Views
{
    //hosts the runtime behavior of that view
    public partial class ElementInfoPane : Page, IDockablePaneProvider
    {
        public ElementInfoPane()
        {
            InitializeComponent();
            DataContext = RevitElementInfoEditable.App.ViewModel; // shared VM
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState { DockPosition = DockPosition.Tabbed };
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
