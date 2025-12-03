using System.Windows;
using Caliburn.Micro;
using Test3.ViewModels;

namespace Test3 {
	public class Bootstrapper : BootstrapperBase {
		private SimpleContainer _container;

		public Bootstrapper() {
			Initialize();
		}

		protected override void Configure() {
			LogManager.GetLog = type => new DebugLogger(type);
			_container = new SimpleContainer();

			_container.Singleton<IWindowManager, WindowManager>();
			_container.PerRequest<ShellViewModel>();
		}

		protected override async void OnStartup(object sender, StartupEventArgs e) {
			await DisplayRootViewForAsync<ShellViewModel>();
		}

		protected override object GetInstance(Type service, string key) {
			return _container.GetInstance(service, key);
		}

		protected override IEnumerable<object> GetAllInstances(Type service) {
			return _container.GetAllInstances(service);
		}

		protected override void BuildUp(object instance) {
			_container.BuildUp(instance);
		}
	}
}