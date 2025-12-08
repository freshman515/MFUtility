using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MFUtility.Ioc;
using MFUtility.Mvvm.Wpf.Interfaces;

namespace MFUtility.Mvvm.Wpf.Core;

public class  ViewModelBase: ObservableObject {
	public object? NavigationParameter { get; internal set; }
	public ViewModelBase? PreviousViewModel { get; internal set; }
	public virtual string DisplayName {
		get {
			var name = GetType().Name;
	
			// 去掉 ViewModel 后缀
			if (name.EndsWith("ViewModel"))
				name = name.Substring(0, name.Length - "ViewModel".Length);
	
			return name;
		}
	}
	public string? RegionName { get; internal set; }
	private WeakReference<FrameworkElement>? _viewRef;
	internal void AttachView(FrameworkElement view) {
		_viewRef = new WeakReference<FrameworkElement>(view);
	}
	protected FrameworkElement? View {
		get {
			if (_viewRef != null && _viewRef.TryGetTarget(out var v))
				return v;
	
			return null;
		}
	}
	private bool _isActive;
	public bool IsActive {
		get => _isActive;
		internal set => SetProperty(ref _isActive, value);
	}
	protected T Resolve<T>() => IoC.Default!.Resolve<T>();
	protected INavigator Navigator => Resolve<INavigator>();
	private bool _isBusy;
	public bool IsBusy {
		get => _isBusy;
		set => SetProperty(ref _isBusy, value);
	}
	protected async Task RunBusy(Func<Task> action) {
		if (IsBusy) return;
	
		try {
			IsBusy = true;
			await action();
		} finally {
			IsBusy = false;
		}
	}
	/// <summary>
	/// 即将导航到此 VM（页面还未显示，可提前准备参数）
	/// </summary>
	public virtual void OnNavigatingTo(object? parameter) {
	}
	
	/// <summary>
	/// 即将离开当前 VM（返回 false 可阻止导航）
	/// </summary>
	public virtual bool OnNavigatingFrom() => true;
	
	public virtual void OnNavigatedTo(object? parameter) {
	}
	
	public virtual void OnNavigatedFrom() {
	}
	protected void SafeExecute(Action action) {
		try { action(); } catch (Exception ex) {
			// 你可以调用 Logger，也可以留空让使用者 override
			OnError(ex);
		}
	}
	protected virtual void OnError(Exception ex) {
		// 默认不做事
		// 用户可在子类重写，比如：Logger.Error(ex)
	}
	protected virtual void ShowMessage(string msg) {
		MessageBox.Show(msg);
	}
	public object GetCurrentViewModel(string region) => Navigator.GetCurrent(region);
	public object? GetCurrentViewModel() {
		// 如果这个 VM 没有 RegionName，则无法从导航器获取当前 VM
		if (string.IsNullOrEmpty(RegionName))
			return null;
	
		return Navigator.GetCurrent(RegionName);
	}
	
	public TViewModel GetCurrentViewModel<TViewModel>(string region) where TViewModel : class => Navigator.GetCurrent<TViewModel>(region);
}