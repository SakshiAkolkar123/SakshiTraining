
using System.ComponentModel;

namespace RevitElementInfoEditable.MVVM.Models
{
    public class ElementParameterRow : INotifyPropertyChanged
    {
        public int ElementId { get; set; }                 // target element id
        public string ElementName { get; set; } = "";      // display name
        public string ParameterName { get; set; } = "";    // parameter display name
        public string StorageType { get; set; } = "";      // String | Double | Integer | ElementId

        private string _value = "";
        public string Value
        {
            get => _value;
            set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
