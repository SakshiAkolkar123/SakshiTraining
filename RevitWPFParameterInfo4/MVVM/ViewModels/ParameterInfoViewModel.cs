using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitWPFParameterInfo4.MVVM.ViewModels
{
    internal class ParameterInfoViewModel : INotifyPropertyChanged
    {
        private string _elementName;
        private ObservableCollection<Models.ParameterInfo> _parameters;

        public string ElementName
        {
            get => _elementName;
            set { _elementName = value; OnPropertyChanged(nameof(ElementName)); }
        }

        public ObservableCollection<Models.ParameterInfo> Parameters
        {
            get => _parameters;
            set { _parameters = value; OnPropertyChanged(nameof(Parameters)); }
        }

        public ICommand CloseCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}

