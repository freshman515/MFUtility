using MFUtility.Mvvm.Wpf.Framework;
using MFUtility.Notifications.Services;

namespace WpfApplication2.ViewModels {
	public class HomeViewModel :ViewModelBase{
		public HomeViewModel(IUserSerivce userSerivice) {
			userSerivice.Load();
		}
		public override void OnNavigatedTo(object parameter) {
			base.OnNavigatedTo(parameter);
			if (parameter == null) return;
			ToastService.Show(parameter.ToString());
			
		}
	}
}