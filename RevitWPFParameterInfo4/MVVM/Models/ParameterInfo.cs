using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitWPFParameterInfo4.MVVM.Models
{
    //represents a single Revit parameter for display and editing in the UI.
    public class ParameterInfo :INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private string _storageType;
        private bool _isReadOnly;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public string StorageType
        {
            get => _storageType;
            set { _storageType = value; OnPropertyChanged(nameof(StorageType)); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
