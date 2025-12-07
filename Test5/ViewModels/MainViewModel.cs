using System.Windows.Input;
using MFUtility.Mvvm.Wpf;
using MFUtility.Mvvm.Wpf.Bases;

namespace Test5.ViewModels {
	public class MainViewModel:ViewModelBase {
		private string _hello;
		public string Hello {
			get => _hello;
			set => SetProperty(ref _hello, value);
		}
		
		public ICommand GoHomeCommand => new RelayCommand(() => {
			Navigator.Navigate<HomeViewModel>("Main");
		});
		public ICommand GoSettingsCommand => new RelayCommand(() => {
			Navigator.Navigate<SettingsViewModel>("Main",123);
		});

		public ICommand GoBackCommand => new RelayCommand(() => {
			Navigator.GoBack("Main");
		});

		public MainViewModel() {
			Hello = "Hello";
		}
	}	
}