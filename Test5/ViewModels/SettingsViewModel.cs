using MFUtility.Extensions;
using MFUtility.Mvvm.Wpf;
using MFUtility.Notifications.Services;
using MFUtility.WPF.Notifications.Services;

namespace Test5.ViewModels {
	public class SettingsViewModel :ViewModelBase {
		public override void OnNavigatedFrom() {
			base.OnNavigatedFrom();
			ToastService.Show(IsActive.ToString());
		}
		public override void OnNavigatedTo(object parameter) {
			base.OnNavigatedTo(parameter);
			
			ToastService.Show(IsActive.ToString());
		}
	}
}