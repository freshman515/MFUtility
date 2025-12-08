using System.Windows.Input;
using MFUtility.Mvvm.Wpf.Framework;
using MFUtility.Mvvm.Wpf.Framework.Bases;

namespace WpfApplication2.ViewModels {
	public class SettingsViewModel:ViewModelBase {
		public override void OnNavigatedTo(object parameter) {
			base.OnNavigatedTo(parameter);
			
		}
		public override void OnFirstActivated() {
			base.OnFirstActivated();
			
		}
		private string _hello;
		public string Hello {
			get => _hello;
			set => SetProperty(ref _hello, value);
		}
		public ICommand HeCommand => new RelayCommand(() => {
			
		});

	}
}