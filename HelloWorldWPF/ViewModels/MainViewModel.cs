
using HelloWorldWPF.Helpers;

namespace HelloWorldWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _message = "Hello World!";

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        // Optional: command to change the message (MVVM interaction demo)
        public RelayCommand ChangeMessageCommand { get; }

        public MainViewModel()
        {
            ChangeMessageCommand = new RelayCommand(_ =>
            {
                Message = "Hello from MVVM!";
            });
        }
    }
}
