using RevitElementInfoEditable.MVVM.Models;
using RevitElementInfoEditable.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace RevitElementInfoEditable.MVVM.ViewModels
{
    // the list of rows + commands
    public class ElementInfoViewModel
    {
        public ObservableCollection<ElementParameterRow> Rows { get; } = new();

        public string XmlPath { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ElementInfo.xml");
        //Serializes Rows to XML using XmlService.
        public ICommand SaveXmlCommand => new RelayCommand(_ => XmlService.Save(XmlPath, Rows));
        //Deserializes XML back into Rows.
        public ICommand LoadXmlCommand => new RelayCommand(_ =>
        {
            if (File.Exists(XmlPath))
            {
                var loaded = XmlService.Load(XmlPath);
                Rows.Clear();
                foreach (var r in loaded) Rows.Add(r);
            }
        });
        //write edited values back to Revit 
        public ICommand ApplyChangesCommand => new RelayCommand(_ =>
        {
            RevitElementInfoEditable.App.Bridge.RaiseApply(Rows);
        });
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> exec) => _execute = exec;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
    }
}
