using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Windows;
using System.Windows.Interop;
using HelloWorldRevitAddin.Views;
using HelloWorldRevitAddin.ViewModels;

namespace HelloWorldRevitAddin.Revit
    {
        public class HelloWorldCommand : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                try
                {
                    // Create window and bind the MVVM ViewModel
                    var window = new MainWindow
                    {
                        DataContext = new MainViewModel()
                    };

                    // Set Revit's main window as owner to keep proper modality and focus
                    var uiapp = commandData.Application;
                    IntPtr revitHandle = uiapp.MainWindowHandle; // Revit 2021+ exposes this
                    var helper = new WindowInteropHelper(window) { Owner = revitHandle };

                    // Show modal or modeless as needed
                    // Modal (blocks Revit until closed):
                    window.ShowDialog();

                    // For modeless, use window.Show(); and consider ExternalEvent for Revit API calls
                    // window.Show();

                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Failed;
                }
                
            }
        }
    }

