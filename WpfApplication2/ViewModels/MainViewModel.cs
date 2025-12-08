using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MFUtility.Ioc;
using MFUtility.Ioc.Core;
using MFUtility.Ioc.Enums;
using MFUtility.Logging;
using MFUtility.Mvvm.Wpf.Framework;
using MFUtility.Mvvm.Wpf.Framework.Bases;

namespace WpfApplication2.ViewModels {
	public class MainViewModel : ViewModelBase {
		public ICommand GoHomeCommand => new RelayCommand(() => {
			Navigator.Navigate<HomeViewModel>("Main", "Hello");
			// var userSerivce = IoC.Default.Resolve<IUserSerivce>();
		});
		public ICommand GoSettingsCommand => new RelayCommand(() => {
			Navigator.Navigate<SettingsViewModel>("Main2");
		});
		public ICommand GoBackCommand => new RelayCommand(() => {
			Navigator.GoBack("Main");
		});
		public ICommand NavigateCommand => new RelayCommand<string>(param => {
			Navigator.Navigate(param,"Main");
		});
	}
}