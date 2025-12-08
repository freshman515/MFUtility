using MFUtility.Ioc.Enums;
using MFUtility.Mvvm.Avalonia.Services;

namespace MFUtility.Mvvm.Avalonia.Interfaces;

public interface INavigator {
	void Navigate<TViewModel>(string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton);
	//void Navigate<TViewModel>(string region, NavigationParameters? parameter = null, Lifetime lifetime = Lifetime.Singleton);
	
	void Navigate(Type viewModelType, string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton);
	void Navigate(string viewModelTypeString, string region, object? parameter = null, Lifetime lifetime = Lifetime.Singleton);
	// 尝试导航
	bool TryNavigate<TViewModel>(string region, object? parameter = null);
	bool TryNavigate(string viewModelTypeString, string region, object? parameter = null);

	// BackStack 支持	
	bool CanGoBack(string region);
	void GoBack(string region);

	// 关闭/清理区域
	void Close(string region);
	void Clear(string region);

	// 查询当前 VM
	object? GetCurrent(string region);
	TViewModel? GetCurrent<TViewModel>(string region) where TViewModel : class;
}