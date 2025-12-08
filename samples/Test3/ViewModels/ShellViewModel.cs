using System.Windows;
using Caliburn.Micro;

namespace Test3.ViewModels {
	public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<string> {
		private string _message;

		public string Message {
			get => _message;
			set {
				_message = value;
				NotifyOfPropertyChange("Message");
			}
		}


		private IEventAggregator _eventAggregator;
		public ShellViewModel() {
			_eventAggregator = new EventAggregator();
			_eventAggregator.SubscribeOnUIThread(this);
			Task.Run(async () => {
				await ActivateItemAsync(new MainViewModel(_eventAggregator));
			});
		}


		protected override void OnViewLoaded(object view) {
			base.OnViewLoaded(view);
			Application.Current.MainWindow.WindowState = WindowState.Maximized;
		}
		public Task HandleAsync(string message, CancellationToken cancellationToken) {
			this.Message = message.ToString();
			return Task.CompletedTask;
		}
		public void ShowMain() {
			Task.Run(async () => {
				await ActivateItemAsync(new MainViewModel(_eventAggregator));
			});
		}

		public void ShowSettings() {
			Task.Run(async () => {
				await ActivateItemAsync(new SettingsViewModel(_eventAggregator));
			});
		}

	}
}