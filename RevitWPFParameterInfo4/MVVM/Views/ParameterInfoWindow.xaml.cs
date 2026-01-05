
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RevitWPFParameterInfo4.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ParameterInfoWindow.xaml
    /// </summary>
    public partial class ParameterInfoWindow : Window
    {
        /// <summary>
        /// Default constructor. You can set DataContext manually after InitializeComponent.
        /// </summary>
        public ParameterInfoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Convenient constructor to set the ElementName and Parameters in one go.
        /// </summary>
        /// <param name="elementName">Name of the Revit element to display in the header.</param>
        /// <param name="parameters">Collection of parameter items to show in the grid.</param>
        public ParameterInfoWindow(string elementName, ObservableCollection<ParameterInfoItem> parameters)
            : this()
        {
            // InitializeComponent() is already called by the default constructor chained above.
            this.DataContext = new ParameterInfoViewModel
            {
                ElementName = elementName ?? string.Empty,
                Parameters = parameters ?? new ObservableCollection<ParameterInfoItem>()
            };
        }

        /// <summary>
        /// Click handler for the Close button.
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// ViewModel matching the bindings in XAML:
    /// - ElementName   -> TextBlock in header
    /// - Parameters    -> DataGrid ItemsSource
    /// </summary>
    public class ParameterInfoViewModel : INotifyPropertyChanged
    {
        private string _elementName;
        private ObservableCollection<ParameterInfoItem> _parameters = new ObservableCollection<ParameterInfoItem>();

        public string ElementName
        {
            get => _elementName;
            set
            {
                if (_elementName != value)
                {
                    _elementName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ParameterInfoItem> Parameters
        {
            get => _parameters;
            set
            {
                if (_parameters != value)
                {
                    _parameters = value ?? new ObservableCollection<ParameterInfoItem>();
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Model representing a single parameter row in the DataGrid.
    /// Matches the XAML bindings:
    /// - Name         -> Parameter Name column
    /// - Value        -> Value column
    /// - StorageType  -> Storage Type column
    /// - IsReadOnly   -> Read-only checkbox column
    /// </summary>
    public class ParameterInfoItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string StorageType { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
