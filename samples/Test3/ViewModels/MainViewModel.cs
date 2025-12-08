using Caliburn.Micro;

namespace Test3.ViewModels {
	public class MainViewModel : Screen {
		private string _title;

		public string Title {
			get => _title;
			set {
				_title = value;
				NotifyOfPropertyChange("Title");
				LogManager.GetLog(typeof(Screen)).Info("Mainviewmodel");
			}
		}
		private readonly ILog _log = LogManager.GetLog(typeof(MainViewModel));

		private IEventAggregator _eventAggregator;
		public BindableCollection<string> collections = new ();

		public MainViewModel(IEventAggregator eventAgg) {
			Title = "Welcome to Caliburn Micro in WPF";
			_eventAggregator = eventAgg;
		}


		protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
			await _eventAggregator.PublishOnUIThreadAsync("Closing");
		}

		protected override async Task OnActivateAsync(CancellationToken cancellationToken) {
			await _eventAggregator.PublishOnUIThreadAsync("Loading");
			await Task.Delay(2000);
			await _eventAggregator.PublishOnUIThreadAsync(string.Empty);
		}
		// 自动调用方法: Button x:Name="SendMessage"
		public async Task SendMessage() {
			await _eventAggregator.PublishOnUIThreadAsync("来自 Main 的消息");
		}

		public void ShowText(object num) {
			DisplayName = "你点击了按钮！";
		}

	}
}